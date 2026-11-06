using System;
using System.Collections.Generic;

namespace VideoGamePriceAggregator.Models;

public partial class Store
{
    public int StoreId { get; set; }

    public string Name { get; set; } = null!;

    public string Website { get; set; } = null!;

    public string? Country { get; set; }

    public virtual ICollection<Offer> Offers { get; set; } = new List<Offer>();
}
