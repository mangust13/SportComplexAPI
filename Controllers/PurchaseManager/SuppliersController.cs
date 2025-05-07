using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportComplexAPI.Data;
using SportComplexAPI.Models;

namespace SportComplexAPI.Controllers.PurchaseManager
{
    [ApiController]
    [Route("api/[controller]")]
    public class SuppliersController : ControllerBase
    {
        private readonly SportComplexContext _context;

        public SuppliersController(SportComplexContext context)
        {
            _context = context;
        }

        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<SupplierFullDto>>> GetAllSuppliers()
        {
            var suppliers = await _context.Suppliers
                .Select(s => new SupplierFullDto
                {
                    SupplierId = s.supplier_id,
                    SupplierName = s.supplier_name,
                    SupplierPhoneNumber = s.supplier_phone_number,
                    SupplierLicense = s.supplier_license
                })
                .ToListAsync();
            return Ok(suppliers);
        }

        [HttpPost]
        public async Task<IActionResult> AddSupplier([FromBody] SupplierFullDto dto)
        {
            var supplier = new Supplier
            {
                supplier_name = dto.SupplierName,
                supplier_phone_number = dto.SupplierPhoneNumber,
                supplier_license = dto.SupplierLicense
            };
            _context.Suppliers.Add(supplier);
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Постачальника додано!", SupplierId = supplier.supplier_id });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSupplier(int id)
        {
            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier == null)
                return NotFound("Постачальника не знайдено.");

            _context.Suppliers.Remove(supplier);
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Постачальника видалено!" });
        }

        public class SupplierFullDto
        {
            public int SupplierId { get; set; }
            public string SupplierName { get; set; } = null!;
            public string SupplierPhoneNumber { get; set; } = null!;
            public string SupplierLicense { get; set; } = null!;
        }
    }   

}
