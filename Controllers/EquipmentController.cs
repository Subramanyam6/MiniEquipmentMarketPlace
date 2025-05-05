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
    [Authorize(Roles = "Admin,Shopper")]
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

            var list = await _context.Equipment.Include(e => e.Vendor).ToListAsync();
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
        public IActionResult Create()
        {
            ViewData["VendorId"] = new SelectList(_context.Vendors, "VendorId", "Name");
            return View();
        }

        // POST: Equipment/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EquipmentId,Title,Description,Price,VendorId")] Equipment equipment)
        {

            Console.WriteLine("Hit EquipmentController.Create POST");

            Console.WriteLine("ModelState.IsValid = " + ModelState.IsValid);
            foreach (var kv in ModelState)
                foreach (var err in kv.Value.Errors)
                    Console.WriteLine($"  {kv.Key}: {err.ErrorMessage}");


            if (ModelState.IsValid)
            {
                _context.Add(equipment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["VendorId"] = new SelectList(_context.Vendors, "VendorId", "VendorId", equipment.VendorId);
            return View(equipment);
        }

        // GET: Equipment/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var equipment = await _context.Equipment.FindAsync(id);
            if (equipment == null)
            {
                return NotFound();
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

            if (ModelState.IsValid)
            {
                try
                {
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

            return View(equipment);
        }

        // POST: Equipment/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var equipment = await _context.Equipment.FindAsync(id);
            if (equipment != null)
            {
                _context.Equipment.Remove(equipment);
            }

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
