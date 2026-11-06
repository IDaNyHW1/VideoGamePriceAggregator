using System;
using System.Collections.Generic;

namespace VideoGamePriceAggregator.Models;

public partial class Favorite
{
    public int UserId { get; set; }

    public int GameId { get; set; }

    public DateTime AddedAt { get; set; }

    public virtual Game Game { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
