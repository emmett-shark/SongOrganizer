using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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

    // working on copying osu's homework https://github.com/ppy/osu/blob/master/osu.Game/Screens/Select/FilterQueryParser.cs
    /*
## Search
By default, search is matched by long name, short name, artist, genre, and description. Queries are case-insensitive. For more granular filtering, use the following:

Filter          | Description
------          | -----------
`star`, `stars` | Star rating
`bpm`           | Song tempo
`status`        | Chart status. Value can be `r`/`u` (rated/unrated)

Comparison | Description
---------- | -----------
`=`        | Equal to
`!=`       | Not equal to
`<`        | Less than
`>`        | Greater than
`<=`       | Less than or equal to
`>=`       | Greater than or equal to

### Examples:

Find charts with difficulty [4, 6)
```
stars>=4 stars<6
```
Find rated charts
```
status=r
```
     */
    private static readonly Regex QUERY_SYNTAX_REGEX = new Regex(
            @"\b(?<key>\w+)(?<op>(=|!=|(<|>)=?))(?<value>("".*""[!]?)|(\S*))",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);
    public const float tolerance = 0.05f;

    private static string ApplyQueries(string query, Track track)
    {
        foreach (Match match in QUERY_SYNTAX_REGEX.Matches(query))
        {
            string key = match.Groups["key"].Value;
            var op = ParseOperator(match.Groups["op"].Value);
            string value = match.Groups["value"].Value;

            if (TryParseKeywordCriteria(key, value, op, track))
                query = query.Replace(match.ToString(), "");
        }
        return query;
    }

    private static bool TryParseKeywordCriteria(string key, string value, Operator op, Track track)
    {
        return key switch
        { 
            "star" or "stars" => true,
            "bpm" => true,
            "status" => true,
            _ => false,
        };
    }

    private static Operator ParseOperator(string value)
    {
        return value switch
        {
            "=" => Operator.Equal,
            "!=" => Operator.NotEqual,
            "<" => Operator.Less,
            "<=" => Operator.LessOrEqual,
            ">" => Operator.Greater,
            ">=" => Operator.GreaterOrEqual,
            _ => throw new ArgumentOutOfRangeException(nameof(value), $"Unsupported operator {value}"),
        };
    }
}
