using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VideoGamePriceAggregator.Models;
using VideoGamePriceAggregator.ViewModels;
using System.Security.Claims;

namespace VideoGamePriceAggregator.Controllers
{
    public class GamesController : Controller
    {
        private readonly VideoGamePriceAggregatorContext _context;

        public GamesController(VideoGamePriceAggregatorContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? search)
        {
            var gamesQuery = _context.Games.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                gamesQuery = gamesQuery
                    .Where(g => g.Title.Contains(search));
            }

            var games = await gamesQuery
                .OrderBy(g => g.Title)
                .ToListAsync();

            ViewBag.Search = search;

            return View(games);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var game = await _context.Games
                .FirstOrDefaultAsync(g => g.GameId == id);

            if (game == null)
            {
                return NotFound();
            }

            var offers = await _context.Offers
                .Include(o => o.Store)
                .Include(o => o.Platform)
                .Where(o => o.GameId == id && o.IsAvailable)
                .OrderBy(o => o.Price)
                .ToListAsync();

            bool isFavorite = false;

            if (User.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (int.TryParse(userIdClaim, out int userId))
                {
                    isFavorite = await _context.Favorites
                        .AnyAsync(f =>
                            f.UserId == userId &&
                            f.GameId == game.GameId);
                }
            }

            var viewModel = new GameDetailsViewModel
            {
                Game = game,
                Offers = offers,
                IsFavorite = isFavorite
            };

            return View(viewModel);
        }
    }
}