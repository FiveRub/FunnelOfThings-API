using System;
using System.Collections.Generic;

namespace FunnelOfThingsAPI.Models;

public partial class SearchHistory
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string Query { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
