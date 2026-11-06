using System;
using System.Collections.Generic;

namespace VideoGamePriceAggregator.Models;

public partial class Game
{
    public int GameId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public DateOnly? ReleaseDate { get; set; }

    public int? DeveloperId { get; set; }

    public int? PublisherId { get; set; }

    public string? AgeRating { get; set; }

    public string? CoverUrl { get; set; }

    public virtual Developer? Developer { get; set; }

    public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();

    public virtual ICollection<Offer> Offers { get; set; } = new List<Offer>();

    public virtual ICollection<PriceAlert> PriceAlerts { get; set; } = new List<PriceAlert>();

    public virtual Publisher? Publisher { get; set; }

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual ICollection<Genre> Genres { get; set; } = new List<Genre>();

    public virtual ICollection<Platform> Platforms { get; set; } = new List<Platform>();
}
