using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VideoGamePriceAggregator.Models;

namespace VideoGamePriceAggregator.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class GamesController : Controller
    {
        private readonly VideoGamePriceAggregatorContext _context;

        public GamesController(VideoGamePriceAggregatorContext context)
        {
            _context = context;
        }

        
        // Список игр
        
        

        [HttpGet]
        public async Task<IActionResult> Index(string? search)
        {
            var gamesQuery = _context.Games
                .Include(g => g.Developer)
                .Include(g => g.Publisher)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                gamesQuery = gamesQuery.Where(g =>
                    g.Title.Contains(search));
            }

            var games = await gamesQuery
                .OrderBy(g => g.Title)
                .ToListAsync();

            ViewBag.Search = search;

            return View(games);
        }


        
        // Добавление игры - GET
        

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadSelectLists();

            return View();
        }


        
        // Добавление игры - POST
        

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Title,Description,ReleaseDate,DeveloperId,PublisherId,AgeRating")]
            Game game,
            IFormFile? coverFile)
        {
            if (!ModelState.IsValid)
            {
                await LoadSelectLists(game);

                return View(game);
            }

            // Загружаем обложку
            if (coverFile != null && coverFile.Length > 0)
            {
                try
                {
                    game.CoverUrl = await SaveCoverAsync(coverFile);
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError(
                        "coverFile",
                        ex.Message);

                    await LoadSelectLists(game);

                    return View(game);
                }
            }

            _context.Games.Add(game);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Игра успешно добавлена.";

            return RedirectToAction(nameof(Index));
        }


        
        // Редактирование игры - GET
        

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var game = await _context.Games
                .FindAsync(id);

            if (game == null)
            {
                return NotFound();
            }

            await LoadSelectLists(game);

            return View(game);
        }


        
        // Редактирование игры - POST
        

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("GameId,Title,Description,ReleaseDate,DeveloperId,PublisherId,AgeRating")]
            Game game,
            IFormFile? coverFile)
        {
            if (id != game.GameId)
            {
                return NotFound();
            }

            // Получаем существующую игру из базы
            var existingGame = await _context.Games
                .AsNoTracking()
                .FirstOrDefaultAsync(g => g.GameId == id);

            if (existingGame == null)
            {
                return NotFound();
            }


            if (!ModelState.IsValid)
            {
                // Сохраняем старую обложку,
                // если форма содержит ошибку
                game.CoverUrl = existingGame.CoverUrl;

                await LoadSelectLists(game);

                return View(game);
            }


            // Если выбрана новая обложка
            if (coverFile != null && coverFile.Length > 0)
            {
                try
                {
                    game.CoverUrl =
                        await SaveCoverAsync(coverFile);
                }
                catch (InvalidOperationException ex)
                {
                    game.CoverUrl = existingGame.CoverUrl;

                    ModelState.AddModelError(
                        "coverFile",
                        ex.Message);

                    await LoadSelectLists(game);

                    return View(game);
                }
            }
            else
            {
                // Если новая обложка не выбрана,
                // оставляем старую
                game.CoverUrl = existingGame.CoverUrl;
            }


            try
            {
                _context.Games.Update(game);

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                bool exists = await _context.Games
                    .AnyAsync(g => g.GameId == id);

                if (!exists)
                {
                    return NotFound();
                }

                throw;
            }


            TempData["SuccessMessage"] =
                "Данные игры успешно изменены.";

            return RedirectToAction(nameof(Index));
        }


        
        // Удаление игры - GET
        

        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var game = await _context.Games
                .Include(g => g.Developer)
                .Include(g => g.Publisher)
                .FirstOrDefaultAsync(g => g.GameId == id);

            if (game == null)
            {
                return NotFound();
            }

            return View(game);
        }


        
        // Удаление игры - POST
        

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var game = await _context.Games
                .FindAsync(id);

            if (game == null)
            {
                return NotFound();
            }

            // Сохраняем путь к обложке,
            // чтобы удалить файл после удаления записи
            string? coverUrl = game.CoverUrl;


            _context.Games.Remove(game);

            await _context.SaveChangesAsync();


            // Удаляем файл обложки
            if (!string.IsNullOrWhiteSpace(coverUrl))
            {
                DeleteCoverFile(coverUrl);
            }


            TempData["SuccessMessage"] =
                "Игра и связанные с ней данные успешно удалены.";

            return RedirectToAction(nameof(Index));
        }


        
        // Списки разработчиков и издателей
        

        private async Task LoadSelectLists(Game? game = null)
        {
            var developers = await _context.Developers
                .OrderBy(d => d.Name)
                .ToListAsync();

            var publishers = await _context.Publishers
                .OrderBy(p => p.Name)
                .ToListAsync();


            ViewData["DeveloperId"] = new SelectList(
                developers,
                "DeveloperId",
                "Name",
                game?.DeveloperId);


            ViewData["PublisherId"] = new SelectList(
                publishers,
                "PublisherId",
                "Name",
                game?.PublisherId);
        }


        
        // Сохранение обложки
        

        private async Task<string> SaveCoverAsync(
            IFormFile coverFile)
        {
            string[] allowedExtensions =
            {
                ".jpg",
                ".jpeg",
                ".png",
                ".webp"
            };

            string extension =
                Path.GetExtension(
                    coverFile.FileName)
                .ToLowerInvariant();


            if (!allowedExtensions.Contains(extension))
            {
                throw new InvalidOperationException(
                    "Поддерживаются только JPG, JPEG, PNG и WEBP.");
            }


            // Ограничение размера файла:
            // 5 MB
            const long maxFileSize = 5 * 1024 * 1024;


            if (coverFile.Length > maxFileSize)
            {
                throw new InvalidOperationException(
                    "Размер изображения не должен превышать 5 МБ.");
            }


            string uploadsFolder = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "images",
                "games");


            Directory.CreateDirectory(
                uploadsFolder);


            // Генерируем уникальное имя файла
            string fileName =
                $"{Guid.NewGuid():N}{extension}";


            string filePath =
                Path.Combine(
                    uploadsFolder,
                    fileName);


            await using var stream =
                new FileStream(
                    filePath,
                    FileMode.Create);


            await coverFile.CopyToAsync(stream);


            return $"/images/games/{fileName}";
        }


        
        // Удаление файла обложки
        

        private void DeleteCoverFile(string coverUrl)
        {
            if (!coverUrl.StartsWith(
                    "/images/games/",
                    StringComparison.OrdinalIgnoreCase))
            {
                return;
            }


            string relativePath =
                coverUrl.TrimStart('/')
                    .Replace(
                        '/',
                        Path.DirectorySeparatorChar);


            string filePath =
                Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    relativePath);


            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }
    }
}