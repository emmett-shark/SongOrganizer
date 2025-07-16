using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using UnityEngine;

namespace SongOrganizer.Data;

public class Options
{
    public const char DELIMETER = '|';

    public ConfigEntry<bool> ShowDefault { get; set; }
    public ConfigEntry<bool> ShowCustom { get; set; }
    public ConfigEntry<bool> ShowUnplayed { get; set; }
    public ConfigEntry<bool> ShowPlayed { get; set; }
    public ConfigEntry<bool> ShowNotSRank { get; set; }
    public ConfigEntry<bool> ShowSRank { get; set; }
    public ConfigEntry<bool> ShowUnrated { get; set; }
    public ConfigEntry<bool> ShowRated { get; set; }
    public ConfigEntry<bool> ShowOnlyFavorites { get; set; }
    public ConfigEntry<bool> HideHearts { get; set; }
    public ConfigEntry<string> Favorites { get; set; }

    public ConfigEntry<string> SortMode { get; set; }

    public ConfigEntry<int> CollectionIndex { get; set; }
    public ConfigEntry<int> LastIndex { get; set; }

    public ConfigEntry<string> SearchValue { get; set; }
    public ConfigEntry<float> MinStar { get; set; }
    public ConfigEntry<float> MaxStar { get; set; }

    public ConfigEntry<KeyCode> ClearSearchKey { get; set; }
    public ConfigEntry<int> MaxStarSlider { get; set; }

    private SortedSet<string> FavoriteTrackrefs { get; set; }

    public void SetFavorites()
    {
        FavoriteTrackrefs = new SortedSet<string>();
        if (!Favorites.Value.IsNullOrWhiteSpace())
            FavoriteTrackrefs = [.. Favorites.Value.Split(DELIMETER)];
    }

    public bool ContainsFavorite(string trackref) => FavoriteTrackrefs.Contains(trackref);

    public void AddFavorite(string trackref)
    {
        FavoriteTrackrefs.Add(trackref);
        Favorites.Value = string.Join(DELIMETER + "", FavoriteTrackrefs);
    }

    public void RemoveFavorite(string trackref)
    {
        FavoriteTrackrefs.Remove(trackref);
        Favorites.Value = string.Join(DELIMETER + "", FavoriteTrackrefs);
    }

    public void ClearFilters()
    {
        ShowDefault.Value = false;
        ShowCustom.Value = false;
        ShowUnplayed.Value = false;
        ShowPlayed.Value = false;
        ShowNotSRank.Value = false;
        ShowSRank.Value = false;
        ShowUnrated.Value = false;
        ShowRated.Value = false;
        ShowOnlyFavorites.Value = false;
        SearchValue.Value = "";
        MinStar.Value = 0;
        MaxStar.Value = Plugin.Options.MaxStarSlider.Value + 1;
    }
}
