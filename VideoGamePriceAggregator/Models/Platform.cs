using System;
using System.Collections.Generic;

namespace VideoGamePriceAggregator.Models;

public partial class Platform
{
    public int PlatformId { get; set; }

    public string Name { get; set; } = null!;

    public string? Manufacturer { get; set; }

    public virtual ICollection<Offer> Offers { get; set; } = new List<Offer>();

    public virtual ICollection<Game> Games { get; set; } = new List<Game>();
}
