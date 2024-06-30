using BepInEx;
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
            && ShowTrack(Plugin.Options.SearchValue.Value, track);
    }

    private static bool ShowTrack(bool optionToggle, bool oppositeOptionToggle, bool option) =>
        optionToggle == oppositeOptionToggle ? true : optionToggle == option;

    // stars != stars is intentional because it's sometimes NaN
    private static bool ShowTrack(float minStar, float maxStar, float stars) =>
        (maxStar >= Plugin.MAX_STARS && stars != stars) || (stars > minStar && (maxStar >= Plugin.MAX_STARS || stars <= maxStar));

    private static bool ShowTrack(string query, Track track)
    {
        if (query.IsNullOrWhiteSpace()) return true;
        query = query.ToLower();
        return Search(query, track);
    }

    private static bool Search(string query, Track track)
    {
        string search = query.Trim();
        return track.trackname_long.ToLower().Contains(search)
            || track.trackname_short.ToLower().Contains(search)
            || track.artist.ToLower().Contains(search)
            || track.genre.ToLower().Contains(search)
            || track.desc.ToLower().Contains(search);
    }
}
