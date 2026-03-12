using System;
using System.Collections.Generic;

namespace FunnelOfThingsAPI.Models;

public partial class Filter
{
    public int Id { get; set; }

    public int CategoryId { get; set; }

    public string Name { get; set; } = null!;

    public string Type { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Category Category { get; set; } = null!;

    public virtual ICollection<FilterValue> FilterValues { get; set; } = new List<FilterValue>();
}
