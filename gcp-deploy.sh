#!/usr/bin/env bash
set -euo pipefail

# One-shot deploy to Google Cloud Run with Cloud SQL (Postgres)
#
# Requirements:
# - gcloud CLI installed and authenticated (gcloud auth login)
# - Docker installed
# - A GCP project (create manually in the console or with gcloud; see notes below)
#
# Usage:
#   export PROJECT_ID="your-project-id"
#   export REGION="us-central1"              # optional, defaults to us-central1
#   export SERVICE_NAME="equipment-marketplace-app"  # optional
#   export REPO="equipment-marketplace-repo" # optional Artifact Registry repo name
#   export DB_INSTANCE_NAME="equipment-marketplace-db" # optional
#   export DB_USER="equipmentadmin"          # optional
#   export DB_NAME="equipmentmarketplace"    # optional
#   # DB_PASSWORD optional; will be generated if not provided
#   # POSTMARK_SERVER_TOKEN will be prompted if unset
#   ./gcp-deploy.sh

# Inputs with defaults
# Default PROJECT_ID to the user's provided project if not set
PROJECT_ID="${PROJECT_ID:-equipment-marketplace-468623}"
REGION="${REGION:-us-central1}"
SERVICE_NAME="${SERVICE_NAME:-equipment-marketplace-app}"
REPO="${REPO:-equipment-marketplace-repo}"
DB_INSTANCE_NAME="${DB_INSTANCE_NAME:-equipment-marketplace-db}"
DB_USER="${DB_USER:-equipmentadmin}"
DB_NAME="${DB_NAME:-equipmentmarketplace}"
DB_PASSWORD="${DB_PASSWORD:-}"
POSTMARK_SERVER_TOKEN="${POSTMARK_SERVER_TOKEN:-}"

if [[ -z "${POSTMARK_SERVER_TOKEN}" ]]; then
  echo -n "Enter POSTMARK_SERVER_TOKEN (input hidden): "
  read -r -s POSTMARK_SERVER_TOKEN
  echo
fi

if [[ -z "${PROJECT_ID}" ]]; then
  echo "ERROR: PROJECT_ID is required. Set it with: export PROJECT_ID=your-project-id" >&2
  exit 1
fi

if [[ -z "${DB_PASSWORD}" ]]; then
  echo "==> No DB_PASSWORD provided; generating a strong random password"
  if command -v openssl >/dev/null 2>&1; then
    DB_PASSWORD=$(openssl rand -base64 24 | tr -d '\n')
  else
    DB_PASSWORD=$(LC_ALL=C tr -dc 'A-Za-z0-9!@#$%^&*()_+' </dev/urandom | head -c 24)
  fi
  echo "    Generated DB_PASSWORD (save this securely): ${DB_PASSWORD}"
fi

INSTANCE_CONNECTION_NAME="${PROJECT_ID}:${REGION}:${DB_INSTANCE_NAME}"
REGISTRY_HOST="${REGION}-docker.pkg.dev"
IMAGE_URI="${REGISTRY_HOST}/${PROJECT_ID}/${REPO}/${SERVICE_NAME}:$(git rev-parse --short HEAD 2>/dev/null || echo latest)"

echo "==> Using settings:"
echo "    PROJECT_ID=${PROJECT_ID}"
echo "    REGION=${REGION}"
echo "    SERVICE_NAME=${SERVICE_NAME}"
echo "    REPO=${REPO}"
echo "    IMAGE_URI=${IMAGE_URI}"
echo "    INSTANCE_CONNECTION_NAME=${INSTANCE_CONNECTION_NAME}"
echo "    DB_USER=${DB_USER} | DB_NAME=${DB_NAME}"

echo "==> Setting gcloud project"
gcloud config set project "${PROJECT_ID}" >/dev/null

echo "==> Enabling required services (this is idempotent)"
gcloud services enable \
  run.googleapis.com \
  sqladmin.googleapis.com \
  artifactregistry.googleapis.com \
  cloudbuild.googleapis.com >/dev/null

echo "==> Ensuring minimal-cost Cloud SQL (Postgres) instance exists"
if ! gcloud sql instances describe "${DB_INSTANCE_NAME}" --project "${PROJECT_ID}" >/dev/null 2>&1; then
  # Note: Cloud SQL is not free. This uses a shared-core tier with small storage to minimize cost.
  gcloud sql instances create "${DB_INSTANCE_NAME}" \
    --project "${PROJECT_ID}" \
    --database-version=POSTGRES_15 \
    --region="${REGION}" \
    --tier=db-f1-micro \
    --storage-size=10 \
    --no-storage-auto-increase \
    --backup-start-time=03:00
fi

echo "==> Ensuring database ${DB_NAME} exists"
if ! gcloud sql databases describe "${DB_NAME}" --instance "${DB_INSTANCE_NAME}" >/dev/null 2>&1; then
  gcloud sql databases create "${DB_NAME}" --instance "${DB_INSTANCE_NAME}"
fi

echo "==> Ensuring user ${DB_USER} exists (and setting password)"
if ! gcloud sql users list --instance "${DB_INSTANCE_NAME}" --format="value(name)" | grep -qx "${DB_USER}"; then
  gcloud sql users create "${DB_USER}" --instance "${DB_INSTANCE_NAME}" --password "${DB_PASSWORD}"
else
  gcloud sql users set-password "${DB_USER}" --instance "${DB_INSTANCE_NAME}" --password "${DB_PASSWORD}"
fi

echo "==> Granting Cloud SQL Client role to the default compute service account"
PROJECT_NUMBER=$(gcloud projects describe "${PROJECT_ID}" --format='value(projectNumber)')
DEFAULT_SA="${PROJECT_NUMBER}-compute@developer.gserviceaccount.com"
gcloud projects add-iam-policy-binding "${PROJECT_ID}" \
  --member="serviceAccount:${DEFAULT_SA}" \
  --role="roles/cloudsql.client" -q >/dev/null

echo "==> Creating Artifact Registry repo if it doesn't exist"
if ! gcloud artifacts repositories describe "${REPO}" --location="${REGION}" >/dev/null 2>&1; then
  gcloud artifacts repositories create "${REPO}" \
    --repository-format=docker \
    --location="${REGION}" \
    --description="Docker repo for ${SERVICE_NAME}"
fi

if docker info >/dev/null 2>&1; then
  echo "==> Ensuring Docker is authenticated to Artifact Registry"
  gcloud auth configure-docker "${REGISTRY_HOST}" -q

  echo "==> Building and pushing image with Docker: ${IMAGE_URI}"
  docker buildx build --platform linux/amd64 -t "${IMAGE_URI}" --push .
else
  echo "==> Docker is not running. Using Cloud Build to build and push: ${IMAGE_URI}"
  gcloud builds submit --tag "${IMAGE_URI}" .
fi

echo "==> Deploying to Cloud Run"
gcloud run deploy "${SERVICE_NAME}" \
  --image "${IMAGE_URI}" \
  --platform managed \
  --region "${REGION}" \
  --allow-unauthenticated \
  --port 8080 \
  --add-cloudsql-instances "${INSTANCE_CONNECTION_NAME}" \
  --set-env-vars "ASPNETCORE_ENVIRONMENT=Production" \
  --set-env-vars "DB_PASSWORD=${DB_PASSWORD}" \
  --set-env-vars "POSTMARK_SERVER_TOKEN=${POSTMARK_SERVER_TOKEN}" \
  --set-env-vars "DB_USER=${DB_USER},DB_NAME=${DB_NAME},INSTANCE_CONNECTION_NAME=${INSTANCE_CONNECTION_NAME}"

SERVICE_URL=$(gcloud run services describe "${SERVICE_NAME}" --region "${REGION}" --format='value(status.url)')
echo "==> Deployment complete"
echo "    URL: ${SERVICE_URL}"

cat <<'NOTE'

Notes:
- You do need a GCP project. You can create it in the console or with gcloud:
    gcloud projects create YOUR_PROJECT_ID --name="Your Project Name"
  Then link billing and set it as default:
    gcloud beta billing projects link YOUR_PROJECT_ID --billing-account=YOUR_BILLING_ACCOUNT_ID
    gcloud config set project YOUR_PROJECT_ID

- This script assumes you already created a Cloud SQL Postgres instance and user:
    gcloud sql instances create ${DB_INSTANCE_NAME} --database-version=POSTGRES_15 --region=${REGION}
    gcloud sql databases create ${DB_NAME} --instance=${DB_INSTANCE_NAME}
    gcloud sql users create ${DB_USER} --instance=${DB_INSTANCE_NAME} --password=

- Ensure the Cloud Run service account has Cloud SQL Client role (usually granted automatically on first attach). If needed:
    SA=$(gcloud run services describe ${SERVICE_NAME} --region ${REGION} --format='value(spec.template.spec.serviceAccountName)')
    gcloud projects add-iam-policy-binding ${PROJECT_ID} \
      --member="serviceAccount:${SA}" \
      --role="roles/cloudsql.client"

- The app reads DB_PASSWORD and POSTMARK_SERVER_TOKEN from env. You will be prompted for the Postmark token if not set.

NOTE


