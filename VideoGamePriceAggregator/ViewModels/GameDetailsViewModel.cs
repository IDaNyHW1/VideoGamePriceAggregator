using VideoGamePriceAggregator.Models;

namespace VideoGamePriceAggregator.ViewModels
{
    public class GameDetailsViewModel
    {
        public Game Game { get; set; } = null!;

        public List<Offer> Offers { get; set; } = new();

        public bool IsFavorite { get; set; }
    }
}