using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VideoGamePriceAggregator.Models;

namespace VideoGamePriceAggregator.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class OffersController : Controller
    {
        private readonly VideoGamePriceAggregatorContext _context;

        public OffersController(VideoGamePriceAggregatorContext context)
        {
            _context = context;
        }

        
        // Список предложений
        // /Admin/Offers
        

        [HttpGet]
        public async Task<IActionResult> Index(string? search)
        {
            var offersQuery = _context.Offers
                .Include(o => o.Game)
                .Include(o => o.Store)
                .Include(o => o.Platform)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                offersQuery = offersQuery.Where(o =>
                    o.Game.Title.Contains(search) ||
                    o.Store.Name.Contains(search) ||
                    o.Platform.Name.Contains(search));
            }

            var offers = await offersQuery
                .OrderBy(o => o.Game.Title)
                .ThenBy(o => o.Price)
                .ToListAsync();

            ViewBag.Search = search;

            return View(offers);
        }


        
        // Добавление предложения - GET
        

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadSelectLists();

            return View(new Offer
            {
                Currency = "RUB",
                IsAvailable = true,
                DiscountPercent = 0
            });
        }


        
        // Добавление предложения - POST
        

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Offer offer)
        {
            // Эти свойства не вводятся через форму.
            // Они будут загружены Entity Framework по внешним ключам.
            ModelState.Remove(nameof(offer.Game));
            ModelState.Remove(nameof(offer.Store));
            ModelState.Remove(nameof(offer.Platform));

            // Валюта в проекте всегда RUB.
            ModelState.Remove(nameof(offer.Currency));
            offer.Currency = "RUB";

            // Удаляем возможную ошибку валидации
            // LastUpdated, так как оно устанавливается сервером.
            ModelState.Remove(nameof(offer.LastUpdated));

            if (!ModelState.IsValid)
            {
                await LoadSelectLists(offer);

                return View(offer);
            }

            // Проверяем игру
            bool gameExists = await _context.Games
                .AnyAsync(g => g.GameId == offer.GameId);

            if (!gameExists)
            {
                ModelState.AddModelError(
                    nameof(offer.GameId),
                    "Выбранная игра не существует.");

                await LoadSelectLists(offer);

                return View(offer);
            }

            // Проверяем магазин
            bool storeExists = await _context.Stores
                .AnyAsync(s => s.StoreId == offer.StoreId);

            if (!storeExists)
            {
                ModelState.AddModelError(
                    nameof(offer.StoreId),
                    "Выбранный магазин не существует.");

                await LoadSelectLists(offer);

                return View(offer);
            }

            // Проверяем платформу
            bool platformExists = await _context.Platforms
                .AnyAsync(p => p.PlatformId == offer.PlatformId);

            if (!platformExists)
            {
                ModelState.AddModelError(
                    nameof(offer.PlatformId),
                    "Выбранная платформа не существует.");

                await LoadSelectLists(offer);

                return View(offer);
            }

            // Проверяем, нет ли дубликата
            bool duplicate = await _context.Offers
                .AnyAsync(o =>
                    o.GameId == offer.GameId &&
                    o.PlatformId == offer.PlatformId &&
                    o.StoreId == offer.StoreId);

            if (duplicate)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Для этой игры, платформы и магазина предложение уже существует.");

                await LoadSelectLists(offer);

                return View(offer);
            }

            offer.LastUpdated = DateTime.Now;

            _context.Offers.Add(offer);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Предложение успешно добавлено.";

            return RedirectToAction(nameof(Index));
        }


        
        // Редактирование - GET
        

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var offer = await _context.Offers
                .Include(o => o.Game)
                .Include(o => o.Store)
                .Include(o => o.Platform)
                .FirstOrDefaultAsync(o => o.OfferId == id);

            if (offer == null)
            {
                return NotFound();
            }

            await LoadSelectLists(offer);

            return View(offer);
        }


        
        // Редактирование - POST
        

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
    int id,
    Offer offer)
        {
            if (id != offer.OfferId)
            {
                return NotFound();
            }

            // Эти свойства не передаются из формы.
            // Они являются навигационными свойствами EF.
            ModelState.Remove(nameof(offer.Game));
            ModelState.Remove(nameof(offer.Store));
            ModelState.Remove(nameof(offer.Platform));

            // Валюта всегда RUB.
            ModelState.Remove(nameof(offer.Currency));
            offer.Currency = "RUB";

            // Устанавливается сервером.
            ModelState.Remove(nameof(offer.LastUpdated));

            // Получаем существующую запись
            var existingOffer = await _context.Offers
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.OfferId == id);

            if (existingOffer == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                await LoadSelectLists(offer);

                return View(offer);
            }

            // Проверяем игру
            bool gameExists = await _context.Games
                .AnyAsync(g => g.GameId == offer.GameId);

            if (!gameExists)
            {
                ModelState.AddModelError(
                    nameof(offer.GameId),
                    "Выбранная игра не существует.");

                await LoadSelectLists(offer);

                return View(offer);
            }

            // Проверяем магазин
            bool storeExists = await _context.Stores
                .AnyAsync(s => s.StoreId == offer.StoreId);

            if (!storeExists)
            {
                ModelState.AddModelError(
                    nameof(offer.StoreId),
                    "Выбранный магазин не существует.");

                await LoadSelectLists(offer);

                return View(offer);
            }

            // Проверяем платформу
            bool platformExists = await _context.Platforms
                .AnyAsync(p => p.PlatformId == offer.PlatformId);

            if (!platformExists)
            {
                ModelState.AddModelError(
                    nameof(offer.PlatformId),
                    "Выбранная платформа не существует.");

                await LoadSelectLists(offer);

                return View(offer);
            }

            // Проверяем дубликат
            bool duplicate = await _context.Offers
                .AnyAsync(o =>
                    o.OfferId != id &&
                    o.GameId == offer.GameId &&
                    o.PlatformId == offer.PlatformId &&
                    o.StoreId == offer.StoreId);

            if (duplicate)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Для этой игры, платформы и магазина предложение уже существует.");

                await LoadSelectLists(offer);

                return View(offer);
            }

            offer.LastUpdated = DateTime.Now;

            try
            {
                _context.Offers.Update(offer);

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                bool exists = await _context.Offers
                    .AnyAsync(o => o.OfferId == id);

                if (!exists)
                {
                    return NotFound();
                }

                throw;
            }

            TempData["SuccessMessage"] =
                "Предложение успешно изменено.";

            return RedirectToAction(nameof(Index));
        }


        
        // Удаление - GET
        

        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var offer = await _context.Offers
                .Include(o => o.Game)
                .Include(o => o.Store)
                .Include(o => o.Platform)
                .FirstOrDefaultAsync(o => o.OfferId == id);

            if (offer == null)
            {
                return NotFound();
            }

            return View(offer);
        }


        
        // Удаление - POST
        

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var offer = await _context.Offers
                .FindAsync(id);

            if (offer == null)
            {
                return NotFound();
            }

            _context.Offers.Remove(offer);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Предложение успешно удалено.";

            return RedirectToAction(nameof(Index));
        }


        
        // Формирование списков
        

        private async Task LoadSelectLists(Offer? offer = null)
        {
            var games = await _context.Games
                .OrderBy(g => g.Title)
                .ToListAsync();

            var stores = await _context.Stores
                .OrderBy(s => s.Name)
                .ToListAsync();

            var platforms = await _context.Platforms
                .OrderBy(p => p.Name)
                .ToListAsync();


            ViewData["GameId"] = new SelectList(
                games,
                "GameId",
                "Title",
                offer?.GameId);


            ViewData["StoreId"] = new SelectList(
                stores,
                "StoreId",
                "Name",
                offer?.StoreId);


            ViewData["PlatformId"] = new SelectList(
                platforms,
                "PlatformId",
                "Name",
                offer?.PlatformId);
        }


        
        // Проверка предложения
        

        private void ValidateOffer(Offer offer)
        {
            if (offer.Price < 0)
            {
                ModelState.AddModelError(
                    nameof(offer.Price),
                    "Цена не может быть отрицательной.");
            }

            if (offer.DiscountPercent < 0 ||
                offer.DiscountPercent > 100)
            {
                ModelState.AddModelError(
                    nameof(offer.DiscountPercent),
                    "Скидка должна быть от 0 до 100%.");
            }

            if (!string.IsNullOrWhiteSpace(offer.ProductUrl))
            {
                bool validUrl = Uri.TryCreate(
                    offer.ProductUrl,
                    UriKind.Absolute,
                    out Uri? uri)
                    && (uri.Scheme == Uri.UriSchemeHttp ||
                        uri.Scheme == Uri.UriSchemeHttps);

                if (!validUrl)
                {
                    ModelState.AddModelError(
                        nameof(offer.ProductUrl),
                        "Введите корректную ссылку на страницу игры.");
                }
            }
        }
    }
}