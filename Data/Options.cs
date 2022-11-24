using System;
using BepInEx.Configuration;

namespace SongOrganizer.Data;

public class Options
{
    public ConfigEntry<bool> ShowDefault { get; set; }
    public ConfigEntry<bool> ShowCustom { get; set; }
    public ConfigEntry<bool> ShowUnplayed { get; set; }
    public ConfigEntry<bool> ShowPlayed { get; set; }
    public ConfigEntry<bool> ShowNotSRank { get; set; }
    public ConfigEntry<bool> ShowSRank { get; set; }
    public ConfigEntry<string> SortMode { get; set; }
}
