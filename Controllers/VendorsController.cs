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

namespace MiniEquipmentMarketplace.Controllers
{

    [Authorize(Roles = "Admin,Vendor")]
    public class VendorsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public VendorsController(AppDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Vendors
        public async Task<IActionResult> Index()
        {
            // Remove user with specific email if exists
            var targetEmail = "subramanyam.duggirala@gmail.com";
            var userToRemove = await _userManager.FindByEmailAsync(targetEmail);
            
            if (userToRemove != null)
            {
                await _userManager.DeleteAsync(userToRemove);
            }
            
            return _context.Vendors != null ? 
                View(await _context.Vendors.OrderByDescending(v => v.CreatedAt).ToListAsync()) :
                Problem("Entity set 'ApplicationDbContext.Vendors' is null.");
        }

        // GET: Vendors/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vendor = await _context.Vendors
                .FirstOrDefaultAsync(m => m.VendorId == id);
            if (vendor == null)
            {
                return NotFound();
            }

            return View(vendor);
        }

        // GET: Vendors/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Vendors/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("VendorId,Name,Email,CreatedAt")] Vendor vendor)
        {
            if (ModelState.IsValid)
            {
                vendor.CreatedAt = DateTime.UtcNow; // Ensure this is always set to current time
                _context.Add(vendor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(vendor);
        }

        // GET: Vendors/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vendor = await _context.Vendors.FindAsync(id);
            if (vendor == null)
            {
                return NotFound();
            }
            
            // Check if the user is a vendor but not an admin
            if (User.IsInRole("Vendor") && !User.IsInRole("Admin"))
            {
                // Get the current user
                var user = await _userManager.GetUserAsync(User);
                
                // Check if the vendor email matches user email
                if (user != null && vendor.Email != user.Email)
                {
                    TempData["StatusMessage"] = "You don't have permission to edit vendor information that doesn't belong to you.";
                    TempData["StatusType"] = "alert-danger";
                    return RedirectToAction(nameof(Index));
                }
            }
            
            return View(vendor);
        }

        // POST: Vendors/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("VendorId,Name,Email")] Vendor vendor)
        {
            if (id != vendor.VendorId)
            {
                return NotFound();
            }
            
            // Check if the user is a vendor but not an admin
            if (User.IsInRole("Vendor") && !User.IsInRole("Admin"))
            {
                // Get the current user
                var user = await _userManager.GetUserAsync(User);
                
                // Get the original vendor to check email
                var originalVendor = await _context.Vendors.FindAsync(id);
                
                // Check if the vendor email matches user email
                if (user != null && originalVendor != null && originalVendor.Email != user.Email)
                {
                    TempData["StatusMessage"] = "You don't have permission to edit vendor information that doesn't belong to you.";
                    TempData["StatusType"] = "alert-danger";
                    return RedirectToAction(nameof(Index));
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Get the existing entity to preserve CreatedAt
                    var existingVendor = await _context.Vendors.AsNoTracking()
                        .FirstOrDefaultAsync(v => v.VendorId == id);
                    
                    if (existingVendor != null)
                    {
                        vendor.CreatedAt = existingVendor.CreatedAt;
                    }
                    else
                    {
                        vendor.CreatedAt = DateTime.UtcNow;
                    }
                    
                    _context.Update(vendor);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VendorExists(vendor.VendorId))
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
            return View(vendor);
        }

        // GET: Vendors/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vendor = await _context.Vendors
                .FirstOrDefaultAsync(m => m.VendorId == id);
            if (vendor == null)
            {
                return NotFound();
            }

            return View(vendor);
        }

        // POST: Vendors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var vendor = await _context.Vendors.FindAsync(id);
            if (vendor != null)
            {
                _context.Vendors.Remove(vendor);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool VendorExists(int id)
        {
            return _context.Vendors.Any(e => e.VendorId == id);
        }
    }
}
