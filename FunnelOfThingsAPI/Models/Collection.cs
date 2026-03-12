using System;
using System.Collections.Generic;

namespace FunnelOfThingsAPI.Models;

public partial class Collection
{
    public int Id { get; set; }

    public int SellerId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string? CoverUrl { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();

    public virtual User Seller { get; set; } = null!;
}
