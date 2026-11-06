using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VideoGamePriceAggregator.Models;

namespace VideoGamePriceAggregator.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class StoresController : Controller
    {
        private readonly VideoGamePriceAggregatorContext _context;

        public StoresController(VideoGamePriceAggregatorContext context)
        {
            _context = context;
        }

        
        // Список магазинов
        // /Admin/Stores
        

        [HttpGet]
        public async Task<IActionResult> Index(string? search)
        {
            var storesQuery = _context.Stores
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                storesQuery = storesQuery.Where(s =>
                    s.Name.Contains(search) ||
                    s.Website.Contains(search) ||
                    (s.Country != null && s.Country.Contains(search)));
            }

            var stores = await storesQuery
                .OrderBy(s => s.Name)
                .ToListAsync();

            ViewBag.Search = search;

            return View(stores);
        }

        
        // Добавление магазина - GET
        

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        
        // Добавление магазина - POST
        

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Store store)
        {
            if (!ModelState.IsValid)
            {
                return View(store);
            }

            bool exists = await _context.Stores
                .AnyAsync(s => s.Name == store.Name);

            if (exists)
            {
                ModelState.AddModelError(
                    nameof(store.Name),
                    "Магазин с таким названием уже существует.");

                return View(store);
            }

            _context.Stores.Add(store);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Магазин успешно добавлен.";

            return RedirectToAction(nameof(Index));
        }

        
        // Редактирование магазина - GET
        

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var store = await _context.Stores
                .FindAsync(id);

            if (store == null)
            {
                return NotFound();
            }

            return View(store);
        }

        
        // Редактирование магазина - POST
        

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Store store)
        {
            if (id != store.StoreId)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(store);
            }

            bool duplicate = await _context.Stores
                .AnyAsync(s =>
                    s.Name == store.Name &&
                    s.StoreId != store.StoreId);

            if (duplicate)
            {
                ModelState.AddModelError(
                    nameof(store.Name),
                    "Магазин с таким названием уже существует.");

                return View(store);
            }

            try
            {
                _context.Stores.Update(store);

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                bool exists = await _context.Stores
                    .AnyAsync(s => s.StoreId == id);

                if (!exists)
                {
                    return NotFound();
                }

                throw;
            }

            TempData["SuccessMessage"] =
                "Магазин успешно изменён.";

            return RedirectToAction(nameof(Index));
        }

        
        // Удаление магазина - GET
        

        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var store = await _context.Stores
                .FirstOrDefaultAsync(s => s.StoreId == id);

            if (store == null)
            {
                return NotFound();
            }

            bool hasOffers = await _context.Offers
                .AnyAsync(o => o.StoreId == id);

            ViewBag.HasOffers = hasOffers;

            return View(store);
        }

        
        // Удаление магазина - POST
        

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var store = await _context.Stores
                .FindAsync(id);

            if (store == null)
            {
                return NotFound();
            }

            bool hasOffers = await _context.Offers
                .AnyAsync(o => o.StoreId == id);

            if (hasOffers)
            {
                TempData["ErrorMessage"] =
                    "Нельзя удалить магазин, для которого существуют предложения.";

                return RedirectToAction(nameof(Index));
            }

            _context.Stores.Remove(store);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Магазин успешно удалён.";

            return RedirectToAction(nameof(Index));
        }
    }
}