using System.ComponentModel;

namespace SongOrganizer.Data;

public enum FilterOption
{
    DEFAULT,
    CUSTOM,
    UNPLAYED,
    PLAYED,
    [Description("Non S Rank")]
    NOT_S_RANK,
    [Description("S Rank")]
    S_RANK,
    UNRATED,
    RATED,
    [Description("Only Fav")]
    ONLY_FAVORITES,
}
