using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniEquipmentMarketplace.Data;
using MiniEquipmentMarketplace.Models;

namespace MiniEquipmentMarketplace.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class EquipmentApiController : ControllerBase
    {
        private readonly AppDbContext _context;
        public EquipmentApiController(AppDbContext context) => _context = context;

        // GET: api/EquipmentApi
        // [HttpGet]
        // public async Task<ActionResult<IEnumerable<Equipment>>> GetAll() =>
        //     await _context.Equipment.Include(e => e.Vendor).ToListAsync();

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Equipment>>> GetAll([FromQuery]int? vendorId)
        {
            var query = _context.Equipment.Include(e => e.Vendor).AsQueryable();
            if (vendorId.HasValue)
                query = query.Where(e => e.VendorId == vendorId.Value);
            return await query.ToListAsync();
        }


        // GET: api/EquipmentApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Equipment>> Get(int id)
        {
            var eq = await _context.Equipment.Include(e => e.Vendor)
                                            .FirstOrDefaultAsync(e => e.EquipmentId == id);
            if (eq == null) return NotFound();
            return eq;
        }

        // POST: api/EquipmentApi
        [HttpPost]
        public async Task<ActionResult<Equipment>> Create(Equipment equipment)
        {
            _context.Equipment.Add(equipment);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = equipment.EquipmentId }, equipment);
        }

        // PUT: api/EquipmentApi/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Equipment equipment)
        {
            if (id != equipment.EquipmentId) return BadRequest();
            _context.Entry(equipment).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/EquipmentApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var eq = await _context.Equipment.FindAsync(id);
            if (eq == null) return NotFound();
            _context.Equipment.Remove(eq);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
