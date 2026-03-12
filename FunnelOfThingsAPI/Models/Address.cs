using System;
using System.Collections.Generic;

namespace FunnelOfThingsAPI.Models;

public partial class Address
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string? Label { get; set; }

    public string Country { get; set; } = null!;

    public string City { get; set; } = null!;

    public string Street { get; set; } = null!;

    public string? Building { get; set; }

    public string? Apartment { get; set; }

    public string? PostalCode { get; set; }

    public bool IsDefault { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual User User { get; set; } = null!;
}
