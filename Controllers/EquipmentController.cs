using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MiniEquipmentMarketplace.Data;
using MiniEquipmentMarketplace.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;


namespace MiniEquipmentMarketplace.Controllers
{   
    [Authorize(Roles = "Admin,Shopper,Vendor")]
    public class EquipmentController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailSender _emailSender;

        public EquipmentController(
            AppDbContext context,
            UserManager<IdentityUser> userManager,
            IEmailSender emailSender)
        {
            _context = context;
            _userManager = userManager;
            _emailSender = emailSender;
        }

        // GET: Equipment
        public async Task<IActionResult> Index()
        {
            // var appDbContext = _context.Equipment.Include(e => e.Vendor);
            // return View(await appDbContext.ToListAsync());

            ViewBag.Vendors = new SelectList(
            await _context.Vendors.ToListAsync(), 
            "VendorId", 
            "Name"
            );

            var list = await _context.Equipment
                .Include(e => e.Vendor)
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();
            return View(list);
        }

        // GET: Equipment/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var equipment = await _context.Equipment
                .Include(e => e.Vendor)
                .FirstOrDefaultAsync(m => m.EquipmentId == id);
            if (equipment == null)
            {
                return NotFound();
            }

            return View(equipment);
        }

        // GET: Equipment/Create
        public async Task<IActionResult> Create()
        {
            if (User.IsInRole("Vendor") && !User.IsInRole("Admin"))
            {
                // Get the current user
                var user = await _userManager.GetUserAsync(User);
                
                // Get the vendor associated with this user's email
                var userVendor = await _context.Vendors.FirstOrDefaultAsync(v => v.Email == user.Email);
                
                if (userVendor != null)
                {
                    // For vendors, only show their own vendor ID
                    ViewData["VendorId"] = new SelectList(new List<Vendor> { userVendor }, "VendorId", "Name");
                }
                else
                {
                    TempData["StatusMessage"] = "Vendor profile not found. Please contact support.";
                    TempData["StatusType"] = "alert-danger";
                    return RedirectToAction(nameof(Index));
                }
            }
            else
            {
                // For admins, show all vendors
                ViewData["VendorId"] = new SelectList(_context.Vendors, "VendorId", "Name");
            }
            return View();
        }

        // POST: Equipment/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EquipmentId,Title,Description,Price,VendorId,CreatedAt")] Equipment equipment)
        {
            if (User.IsInRole("Vendor") && !User.IsInRole("Admin"))
            {
                // Get the current user
                var user = await _userManager.GetUserAsync(User);
                
                // Get the vendor associated with this user's email
                var userVendor = await _context.Vendors.FirstOrDefaultAsync(v => v.Email == user.Email);
                
                // Check if the vendor exists and matches the equipment's vendor
                if (userVendor == null || userVendor.VendorId != equipment.VendorId)
                {
                    TempData["StatusMessage"] = "You don't have permission to create equipment for other vendors.";
                    TempData["StatusType"] = "alert-danger";
                    return RedirectToAction(nameof(Index));
                }
            }

            if (ModelState.IsValid)
            {
                equipment.CreatedAt = DateTime.UtcNow; // Ensure this is always set to current time
                _context.Add(equipment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            
            // If we get here, something failed, redisplay form
            if (User.IsInRole("Vendor") && !User.IsInRole("Admin"))
            {
                var user = await _userManager.GetUserAsync(User);
                var userVendor = await _context.Vendors.FirstOrDefaultAsync(v => v.Email == user.Email);
                ViewData["VendorId"] = new SelectList(new List<Vendor> { userVendor }, "VendorId", "Name", equipment.VendorId);
            }
            else
            {
                ViewData["VendorId"] = new SelectList(_context.Vendors, "VendorId", "Name", equipment.VendorId);
            }
            return View(equipment);
        }

        // GET: Equipment/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var equipment = await _context.Equipment
                .Include(e => e.Vendor)
                .FirstOrDefaultAsync(m => m.EquipmentId == id);
        
            if (equipment == null)
            {
                return NotFound();
            }
        
            // Check if the user is a vendor but not an admin
            if (User.IsInRole("Vendor") && !User.IsInRole("Admin"))
            {
                // Get the current user
                var user = await _userManager.GetUserAsync(User);
            
                // Get the vendor associated with this user's email
                var userVendor = await _context.Vendors.FirstOrDefaultAsync(v => v.Email == user.Email);
            
                // Check if the vendor exists and matches the equipment's vendor
                if (userVendor == null || userVendor.VendorId != equipment.VendorId)
                {
                    TempData["StatusMessage"] = "You don't have permission to edit equipment from other vendors.";
                    TempData["StatusType"] = "alert-danger";
                    return RedirectToAction(nameof(Index));
                }
            }
        
            ViewData["VendorId"] = new SelectList(_context.Vendors, "VendorId", "Name", equipment.VendorId);
            return View(equipment);
        }

        // POST: Equipment/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("EquipmentId,Title,Description,Price,VendorId")] Equipment equipment)
        {
            if (id != equipment.EquipmentId)
            {
                return NotFound();
            }
        
            // Check if the user is a vendor but not an admin
            if (User.IsInRole("Vendor") && !User.IsInRole("Admin"))
            {
                // Get the current user
                var user = await _userManager.GetUserAsync(User);
            
                // Get the vendor associated with this user's email
                var userVendor = await _context.Vendors.FirstOrDefaultAsync(v => v.Email == user.Email);
            
                // Check if the vendor exists and matches the equipment's vendor
                if (userVendor == null || userVendor.VendorId != equipment.VendorId)
                {
                    TempData["StatusMessage"] = "You don't have permission to edit equipment from other vendors.";
                    TempData["StatusType"] = "alert-danger";
                    return RedirectToAction(nameof(Index));
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Get the existing entity to preserve CreatedAt
                    var existingEquipment = await _context.Equipment.AsNoTracking()
                        .FirstOrDefaultAsync(e => e.EquipmentId == id);
                    
                    if (existingEquipment != null)
                    {
                        equipment.CreatedAt = existingEquipment.CreatedAt;
                    }
                    else
                    {
                        equipment.CreatedAt = DateTime.UtcNow;
                    }
                    
                    _context.Update(equipment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EquipmentExists(equipment.EquipmentId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["VendorId"] = new SelectList(_context.Vendors, "VendorId", "VendorId", equipment.VendorId);
            return View(equipment);
        }

        // GET: Equipment/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var equipment = await _context.Equipment
                .Include(e => e.Vendor)
                .FirstOrDefaultAsync(m => m.EquipmentId == id);
        
            if (equipment == null)
            {
                return NotFound();
            }
        
            // Check if the user is a vendor but not an admin
            if (User.IsInRole("Vendor") && !User.IsInRole("Admin"))
            {
                // Get the current user
                var user = await _userManager.GetUserAsync(User);
            
                // Get the vendor associated with this user's email
                var userVendor = await _context.Vendors.FirstOrDefaultAsync(v => v.Email == user.Email);
            
                // Check if the vendor exists and matches the equipment's vendor
                if (userVendor == null || userVendor.VendorId != equipment.VendorId)
                {
                    TempData["StatusMessage"] = "You don't have permission to delete equipment from other vendors.";
                    TempData["StatusType"] = "alert-danger";
                    return RedirectToAction(nameof(Index));
                }
            }

            return View(equipment);
        }

        // POST: Equipment/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var equipment = await _context.Equipment
                .Include(e => e.Vendor)
                .FirstOrDefaultAsync(m => m.EquipmentId == id);
        
            if (equipment == null)
            {
                return NotFound();
            }
        
            // Check if the user is a vendor but not an admin
            if (User.IsInRole("Vendor") && !User.IsInRole("Admin"))
            {
                // Get the current user
                var user = await _userManager.GetUserAsync(User);
            
                // Get the vendor associated with this user's email
                var userVendor = await _context.Vendors.FirstOrDefaultAsync(v => v.Email == user.Email);
            
                // Check if the vendor exists and matches the equipment's vendor
                if (userVendor == null || userVendor.VendorId != equipment.VendorId)
                {
                    TempData["StatusMessage"] = "You don't have permission to delete equipment from other vendors.";
                    TempData["StatusType"] = "alert-danger";
                    return RedirectToAction(nameof(Index));
                }
            }
        
            _context.Equipment.Remove(equipment);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // POST: Equipment/RequestQuote
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestQuote(int id)
        {
            // Get the equipment
            var equipment = await _context.Equipment
                .Include(e => e.Vendor)
                .FirstOrDefaultAsync(m => m.EquipmentId == id);

            if (equipment == null)
            {
                return NotFound();
            }

            // Get the current user's email
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var userEmail = await _userManager.GetEmailAsync(user);

            // Compose email
            var subject = $"Quote for {equipment.Title}";
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
                            <h1>Your Quote is Ready</h1>
                        </div>
                        <div class='content'>
                            <p>Thank you for your interest in our equipment.</p>
                            <p>Your quote for <strong>{equipment.Title}</strong> is <strong>${equipment.Price:C}</strong>.</p>
                            <p>This equipment is provided by {equipment.Vendor?.Name}.</p>
                            <a href='{Url.Action("Buy", "Equipment", new { id = equipment.EquipmentId }, Request.Scheme)}' class='btn'>Buy Now</a>
                        </div>
                        <div class='footer'>
                            <p>© {DateTime.Now.Year} Equipment Marketplace. All rights reserved.</p>
                        </div>
                    </div>
                </body>
                </html>";

            // Send email
            await _emailSender.SendEmailAsync(userEmail, subject, htmlBody);

            // Redirect with success message
            TempData["StatusMessage"] = "Quote sent to your email successfully!";
            TempData["StatusType"] = "alert-success";
            
            return RedirectToAction(nameof(Index));
        }

        private bool EquipmentExists(int id)
        {
            return _context.Equipment.Any(e => e.EquipmentId == id);
        }
    }
}
