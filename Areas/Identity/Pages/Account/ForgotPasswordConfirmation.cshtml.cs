// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace MiniEquipmentMarketplace.Areas.Identity.Pages.Account
{
    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    [AllowAnonymous]
    public class ForgotPasswordConfirmationModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        
        public ForgotPasswordConfirmationModel(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }
        
        [BindProperty]
        public InputModel Input { get; set; }
        
        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
            
            public string Code { get; set; }
            
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "New password")]
            public string Password { get; set; }
            
            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }
        
        public IActionResult OnGet()
        {
            // Generate a reset token directly for the user without requiring email
            // This is just for UX improvement in a development environment
            
            // Most recently submitted email (from TempData set in the Forgot Password page)
            string email = TempData["ResetEmail"] as string;
            
            if (string.IsNullOrEmpty(email))
            {
                // For testing purposes, use a default email
                email = "subramanyam.duggirala@gmail.com";
            }
            
            Input = new InputModel
            {
                Email = email
            };
            
            // Generate a reset token
            var user = _userManager.FindByEmailAsync(email).Result;
            if (user != null)
            {
                var code = _userManager.GeneratePasswordResetTokenAsync(user).Result;
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                Input.Code = code;
            }
            
            return Page();
        }
        
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            
            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToPage("./Login");
            }
            
            var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(Input.Code));
            var result = await _userManager.ResetPasswordAsync(user, code, Input.Password);
            
            if (result.Succeeded)
            {
                TempData["StatusMessage"] = "Your password has been reset successfully. You can now log in with your new password.";
                TempData["StatusType"] = "alert-success";
                return RedirectToPage("./Login");
            }
            
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            
            return Page();
        }
    }
}
