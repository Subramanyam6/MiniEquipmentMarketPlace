using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MiniEquipmentMarketplace.Models;
using Microsoft.AspNetCore.Identity;

namespace MiniEquipmentMarketplace.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            using (var context = serviceProvider.GetRequiredService<AppDbContext>())
            {
                // Create database if it doesn't exist
                context.Database.Migrate();

                // Check if the database already has data
                if (context.Vendors.Any())
                {
                    return; // Database has been seeded already
                }

                // Seed Vendors
                var vendors = new[]
                {
                    new Vendor { Name = "Vendor 1", Email = "vendor1@example.com", CreatedAt = DateTime.UtcNow },
                    new Vendor { Name = "Vendor 2", Email = "vendor2@example.com", CreatedAt = DateTime.UtcNow }
                    // Add more vendors as needed to match your actual data
                };

                context.Vendors.AddRange(vendors);
                await context.SaveChangesAsync();

                // Seed Equipment
                var equipment = new[]
                {
                    new Equipment { 
                        Title = "Equipment 1", 
                        Description = "Description for equipment 1", 
                        Price = 199.99M, 
                        VendorId = vendors[0].VendorId, 
                        CreatedAt = DateTime.UtcNow 
                    },
                    new Equipment { 
                        Title = "Equipment 2", 
                        Description = "Description for equipment 2", 
                        Price = 299.99M, 
                        VendorId = vendors[0].VendorId, 
                        CreatedAt = DateTime.UtcNow 
                    },
                    new Equipment { 
                        Title = "Equipment 3", 
                        Description = "Description for equipment 3", 
                        Price = 399.99M, 
                        VendorId = vendors[1].VendorId, 
                        CreatedAt = DateTime.UtcNow 
                    }
                    // Add more equipment as needed to match your actual data
                };

                context.Equipment.AddRange(equipment);
                await context.SaveChangesAsync();
            }
            
            // Seed roles and admin user if needed
            using (var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>())
            using (var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>())
            {
                await SeedRolesAndUsersAsync(roleManager, userManager);
            }
        }

        private static async Task SeedRolesAndUsersAsync(
            RoleManager<IdentityRole> roleManager,
            UserManager<IdentityUser> userManager)
        {
            // Seed roles
            string[] roleNames = { "Admin", "Vendor", "Shopper" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Seed admin user if needed
            var adminEmail = "admin@example.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };
                
                // Replace with a strong password for production
                var result = await userManager.CreateAsync(adminUser, "Admin123!");
                
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // Add more user seeding here if needed
        }
    }
} 