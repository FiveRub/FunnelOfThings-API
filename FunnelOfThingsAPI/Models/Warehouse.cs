using System;
using System.Collections.Generic;

namespace FunnelOfThingsAPI.Models;

public partial class Warehouse
{
    public int Id { get; set; }

    public int SellerId { get; set; }

    public string Name { get; set; } = null!;

    public string Address { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual User Seller { get; set; } = null!;

    public virtual ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
}
