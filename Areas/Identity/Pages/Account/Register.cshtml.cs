// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace MiniEquipmentMarketplace.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IUserStore<IdentityUser> _userStore;
        private readonly IUserEmailStore<IdentityUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;

        public RegisterModel(
            UserManager<IdentityUser> userManager,
            IUserStore<IdentityUser> userStore,
            SignInManager<IdentityUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public string UserType { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null, string userType = null)
        {
            ReturnUrl = returnUrl;
            UserType = userType;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            
            // UserType is now bound from the form

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                // Check if user already exists
                var existingUser = await _userManager.FindByEmailAsync(Input.Email);
                
                if (existingUser != null)
                {
                    // User exists, check if they're trying to register with a role they already have
                    var userRoles = await _userManager.GetRolesAsync(existingUser);
                    
                    if (userRoles.Contains(UserType))
                    {
                        ModelState.AddModelError(string.Empty, $"You are already registered as a {UserType}.");
                        return Page();
                    }
                    
                    // Add the new role to the existing user
                    await _userManager.AddToRoleAsync(existingUser, UserType);
                    
                    // Set success message
                    TempData["StatusMessage"] = $"Your account has been updated to include {UserType} privileges!";
                    TempData["StatusType"] = "alert-success";
                    
                    // Try to send welcome email for the new role, but don't fail if email fails
                    try
                    {
                        await SendWelcomeEmailAsync(Input.Email, UserType);
                        _logger.LogInformation("Welcome email sent for role addition");
                    }
                    catch (Exception emailEx)
                    {
                        _logger.LogWarning(emailEx, "Failed to send welcome email for role addition, but role was added successfully");
                    }
                    
                    // Sign the user in with their existing account
                    await _signInManager.SignInAsync(existingUser, isPersistent: false);
                    return LocalRedirect(returnUrl);
                }
                
                // Create new user if they don't exist
                var user = CreateUser();

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    // Add user to role with validation
                    if (!string.IsNullOrWhiteSpace(UserType))
                    {
                        await _userManager.AddToRoleAsync(user, UserType);
                    }
                    else
                    {
                        // Default to Shopper role if no role is specified
                        await _userManager.AddToRoleAsync(user, "Shopper");
                        UserType = "Shopper"; // Update UserType for the welcome email
                    }

                    var userId = await _userManager.GetUserIdAsync(user);
                    
                    // Try to send emails, but don't fail registration if email fails
                    bool emailSent = false;
                    try
                    {
                        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                        var callbackUrl = Url.Page(
                            "/Account/ConfirmEmail",
                            pageHandler: null,
                            values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                            protocol: Request.Scheme);

                        // Send confirmation email
                        await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                            $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                        // Send welcome email
                        await SendWelcomeEmailAsync(Input.Email, UserType);
                        
                        emailSent = true;
                        _logger.LogInformation("Registration and welcome emails sent successfully");
                    }
                    catch (Exception emailEx)
                    {
                        _logger.LogWarning(emailEx, "Failed to send registration emails, but user was created successfully");
                        // Continue with registration success even if email fails
                    }

                    // Set success message based on email status
                    if (emailSent)
                    {
                        TempData["StatusMessage"] = "Registration successful! Confirmation and welcome emails sent!";
                    }
                    else
                    {
                        TempData["StatusMessage"] = "Registration successful! (Email service temporarily unavailable)";
                    }
                    TempData["StatusType"] = "alert-success";

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

        private async Task SendWelcomeEmailAsync(string email, string userType)
        {
            var subject = "Welcome to Equipment Marketplace";
            var htmlBody = $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #1a472a; color: white; padding: 10px 20px; text-align: center; }}
                        .content {{ padding: 20px; background-color: #f9f9f9; }}
                        .btn {{ display: inline-block; background-color: #1a472a; color: white; padding: 10px 20px; 
                                text-decoration: none; border-radius: 5px; margin-top: 20px; }}
                        .footer {{ margin-top: 20px; font-size: 12px; color: #666; text-align: center; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>Welcome to Equipment Marketplace!</h1>
                        </div>
                        <div class='content'>
                            <p>Thank you for registering as a <strong>{userType}</strong>.</p>
                            <p>Your account has been created successfully and you can now:</p>
                            <ul>
                                {(userType == "Vendor" ? "<li>List your equipment for sale</li><li>Manage your inventory</li>" : "")}
                                {(userType == "Shopper" ? "<li>Browse available equipment</li><li>Request quotes from vendors</li>" : "")}
                                {(userType == "Admin" ? "<li>Manage all equipment listings</li><li>Administer user accounts</li>" : "")}
                                <li>Update your profile settings</li>
                                <li>Contact our support team if you need assistance</li>
                            </ul>
                            <p>We're excited to have you on board!</p>
                            <a href='{Url.Action("Index", "Home", new { }, Request.Scheme)}' class='btn'>Start Exploring</a>
                        </div>
                        <div class='footer'>
                            <p>© {DateTime.Now.Year} Equipment Marketplace. All rights reserved.</p>
                        </div>
                    </div>
                </body>
                </html>";

            await _emailSender.SendEmailAsync(email, subject, htmlBody);
        }

        private IdentityUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<IdentityUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(IdentityUser)}'. " +
                    $"Ensure that '{nameof(IdentityUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<IdentityUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<IdentityUser>)_userStore;
        }
    }
}
