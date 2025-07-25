﻿using BepInEx;
using SongOrganizer.Data;

namespace SongOrganizer.Utils;

public static class FilterQueryParser
{
    public static bool ShowTrack(Track track)
    {
        return ShowTrack(Plugin.Options.ShowCustom.Value, Plugin.Options.ShowDefault.Value, track.custom)
            && ShowTrack(Plugin.Options.ShowPlayed.Value, Plugin.Options.ShowUnplayed.Value, track.letterScore != "-")
            && ShowTrack(Plugin.Options.ShowSRank.Value, Plugin.Options.ShowNotSRank.Value, track.letterScore == "S")
            && ShowTrack(Plugin.Options.ShowRated.Value, Plugin.Options.ShowUnrated.Value, track.rated)
            && ShowTrack(Plugin.Options.MinStar.Value, Plugin.Options.MaxStar.Value, track.stars)
            && ShowTrack(Plugin.Options.SearchValue.Value, track)
            && FilterFavorites(Plugin.Options.ShowOnlyFavorites.Value, track.trackref)
            && track.collections.Contains(Plugin.Options.CollectionIndex.Value);
    }

    private static bool ShowTrack(bool optionToggle, bool oppositeOptionToggle, bool option) =>
        optionToggle == oppositeOptionToggle ? true : optionToggle == option;

    // stars != stars is intentional because it's sometimes NaN
    private static bool ShowTrack(float minStar, float maxStar, float stars) =>
        (maxStar > Plugin.Options.MaxStarSlider.Value && stars != stars) || (stars > minStar && (maxStar > Plugin.Options.MaxStarSlider.Value || stars <= maxStar));

    private static bool FilterFavorites(bool onlyFavorites, string trackRef) =>
        !onlyFavorites || (onlyFavorites && Plugin.Options.ContainsFavorite(trackRef));

    private static bool ShowTrack(string query, Track track)
    {
        if (query.IsNullOrWhiteSpace()) return true;
        string search = query.ToLower().Trim().Replace(".", "");
        return track.trackname_long.ToLower().Replace(".", "").Contains(search)
            || track.trackname_short.ToLower().Replace(".", "").Contains(search)
            || track.artist.ToLower().Replace(".", "").Contains(search)
            || track.genre.ToLower().Replace(".", "").Contains(search)
            || track.desc.ToLower().Replace(".", "").Contains(search);
    }
}
