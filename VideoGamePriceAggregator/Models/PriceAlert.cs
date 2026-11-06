using System;
using System.Collections.Generic;

namespace VideoGamePriceAggregator.Models;

public partial class PriceAlert
{
    public int AlertId { get; set; }

    public int UserId { get; set; }

    public int GameId { get; set; }

    public decimal TargetPrice { get; set; }

    public string Currency { get; set; } = null!;

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Game Game { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
