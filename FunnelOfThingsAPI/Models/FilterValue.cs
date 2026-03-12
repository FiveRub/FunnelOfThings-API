using System;
using System.Collections.Generic;

namespace FunnelOfThingsAPI.Models;

public partial class FilterValue
{
    public int Id { get; set; }

    public int FilterId { get; set; }

    public string Value { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Filter Filter { get; set; } = null!;
}
