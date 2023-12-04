using System.ComponentModel;

namespace SongOrganizer.Data;

public enum FilterOption
{
    DEFAULT,
    CUSTOM,
    UNPLAYED,
    PLAYED,
    [Description("Non S rank")]
    NOT_S_RANK,
    [Description("S rank")]
    S_RANK,
    UNRATED,
    RATED,
}
