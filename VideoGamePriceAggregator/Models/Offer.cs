using System;
using System.Collections.Generic;

namespace VideoGamePriceAggregator.Models;

public partial class Offer
{
    public int OfferId { get; set; }

    public int GameId { get; set; }

    public int PlatformId { get; set; }

    public int StoreId { get; set; }

    public decimal Price { get; set; }

    public string Currency { get; set; } = null!;

    public decimal DiscountPercent { get; set; }

    public bool IsAvailable { get; set; }

    public string? ProductUrl { get; set; }

    public DateTime LastUpdated { get; set; }

    public virtual Game Game { get; set; } = null!;

    public virtual Platform Platform { get; set; } = null!;

    public virtual ICollection<PriceHistory> PriceHistories { get; set; } = new List<PriceHistory>();

    public virtual Store Store { get; set; } = null!;
}
