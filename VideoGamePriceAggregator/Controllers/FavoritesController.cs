using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VideoGamePriceAggregator.Models;

namespace VideoGamePriceAggregator.Controllers
{
    [Authorize]
    public class FavoritesController : Controller
    {
        private readonly VideoGamePriceAggregatorContext _context;

        public FavoritesController(VideoGamePriceAggregatorContext context)
        {
            _context = context;
        }

        
        // Список желаемых игр
        

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userIdClaim = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Challenge();
            }

            var favorites = await _context.Favorites
                .Include(f => f.Game)
                .Where(f => f.UserId == userId)
                .OrderByDescending(f => f.AddedAt)
                .ToListAsync();

            return View(favorites);
        }

        
        // Добавление игры
        

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(
            int gameId,
            string? returnUrl = null)
        {
            var userIdClaim = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Challenge();
            }

            var gameExists = await _context.Games
                .AnyAsync(g => g.GameId == gameId);

            if (!gameExists)
            {
                return NotFound();
            }

            var alreadyExists = await _context.Favorites
                .AnyAsync(f =>
                    f.UserId == userId &&
                    f.GameId == gameId);

            if (!alreadyExists)
            {
                var favorite = new Favorite
                {
                    UserId = userId,
                    GameId = gameId,
                    AddedAt = DateTime.Now
                };

                _context.Favorites.Add(favorite);

                await _context.SaveChangesAsync();
            }

            if (!string.IsNullOrEmpty(returnUrl) &&
                Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction(
                "Details",
                "Games",
                new { id = gameId });
        }

        
        // Удаление игры
        

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(
            int gameId,
            string? returnUrl = null)
        {
            var userIdClaim = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Challenge();
            }

            var favorite = await _context.Favorites
                .FirstOrDefaultAsync(f =>
                    f.UserId == userId &&
                    f.GameId == gameId);

            if (favorite != null)
            {
                _context.Favorites.Remove(favorite);

                await _context.SaveChangesAsync();
            }

            if (!string.IsNullOrEmpty(returnUrl) &&
                Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction(
                "Index",
                "Favorites");
        }
    }
}