using System;
using System.Collections.Generic;

namespace FrameworkAPI.Models;

public struct MaterialLotsFilter
{
    public MaterialLotsFilter()
    {
    }
    public int Skip { get; set; } = 0;
    public int Take { get; set; } = 20;
    public DateTime? From { get; set; } = null;
    public DateTime? To { get; set; } = null;
    public string? RegexFilter { get; set; } = null;
    public List<string> MachineIdsFilter { get; set; } = new();
}