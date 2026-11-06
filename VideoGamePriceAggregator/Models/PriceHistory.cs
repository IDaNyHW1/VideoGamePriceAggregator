using System;
using System.Collections.Generic;

namespace VideoGamePriceAggregator.Models;

public partial class PriceHistory
{
    public long PriceHistoryId { get; set; }

    public int OfferId { get; set; }

    public decimal Price { get; set; }

    public string Currency { get; set; } = null!;

    public decimal DiscountPercent { get; set; }

    public DateTime RecordedAt { get; set; }

    public virtual Offer Offer { get; set; } = null!;
}
