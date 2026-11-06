using System;
using System.Collections.Generic;

namespace VideoGamePriceAggregator.Models;

public partial class User
{
    public int UserId { get; set; }

    public string Login { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public DateTime RegistrationDate { get; set; }

    public bool IsAdmin { get; set; }

    public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();

    public virtual ICollection<PriceAlert> PriceAlerts { get; set; } = new List<PriceAlert>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
}
