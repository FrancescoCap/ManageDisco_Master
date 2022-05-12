﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ManageDisco.Context;
using ManageDisco.Model;
using Microsoft.Extensions.Configuration;

namespace ManageDisco.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WarehousesController : BaseController
    {
        public WarehousesController(DiscoContext db, IConfiguration configuration) : base(db, configuration)
        {
        }

        // GET: api/Warehouses
        [HttpGet]
        public async Task<IActionResult> GetWarehouse()
        {

            List<WarehouseView> warehouses = await _db.Warehouse
                .Where(x => x.Product.ProductShopType.ProductShopTypeDescription == ProductShopTypeContants.PRODUCT_SHOP_TYPE_TABLE)
                .Select(x => new WarehouseView()
                {
                    ProductId = x.ProductId,
                    ProductName = x.Product.ProductName,
                    WarehouseQuantity = x.WarehouseQuantity
                }).ToListAsync();


            return Ok(warehouses);
        }

        // GET: api/Warehouses/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Warehouse>> GetWarehouse(int id)
        {
            return Ok();
        }

        // PUT: api/Warehouses/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut]
        [Route("Product/Add")]
        public async Task<IActionResult> PutWarehouse([FromBody] WarehousePut warehousePut)
        {
            if (warehousePut == null)
                return BadRequest(new GeneralReponse() { OperationSuccess = false, Message = "Dati non validi." });

            if (warehousePut.ProductId == 0)
                return BadRequest(new GeneralReponse() { OperationSuccess = false, Message = "Il prodotto non è valido." });

            if (warehousePut.WarehouseQuantity == 0)
                return BadRequest(new GeneralReponse() { OperationSuccess = false, Message = "Impossibile inserire quantità 0."});
            

            Product product = await _db.Product.FirstOrDefaultAsync(x => x.ProductId == warehousePut.ProductId);
            if (product == null)
                return BadRequest(new GeneralReponse() { OperationSuccess = false, Message = "Prodotto non trovato." });

            Warehouse warehouseRow = await _db.Warehouse.FirstOrDefaultAsync(x => x.ProductId == product.ProductId);
            if (warehouseRow == null)
                return BadRequest(new GeneralReponse() { OperationSuccess = false, Message = "Il prodotto non è presente a magazzino" });

            warehouseRow.WarehouseQuantity = warehouseRow.WarehouseQuantity + warehousePut.WarehouseQuantity;
            _db.Entry(warehouseRow).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/Warehouses
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<IActionResult> PostWarehouse(Warehouse warehouse)
        {
            return Ok();
        }

        // DELETE: api/Warehouses/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWarehouse(int id)
        {
           

            return NoContent();
        }
    }
}
