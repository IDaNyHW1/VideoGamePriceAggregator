using System;
using System.Collections.Generic;

namespace VideoGamePriceAggregator.Models;

public partial class Review
{
    public int ReviewId { get; set; }

    public int UserId { get; set; }

    public int GameId { get; set; }

    public byte Rating { get; set; }

    public string? ReviewText { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Game Game { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
