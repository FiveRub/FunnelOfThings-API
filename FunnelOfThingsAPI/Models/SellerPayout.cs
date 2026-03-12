using System;
using System.Collections.Generic;

namespace FunnelOfThingsAPI.Models;

public partial class SellerPayout
{
    public int Id { get; set; }

    public int SellerId { get; set; }

    public int OrderId { get; set; }

    public decimal GrossAmount { get; set; }

    public decimal CommissionPct { get; set; }

    public decimal NetAmount { get; set; }

    public string Status { get; set; } = null!;

    public DateTime? PaidAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual User Seller { get; set; } = null!;
}
