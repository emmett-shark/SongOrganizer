using BaboonAPI.Hooks.Tracks;

namespace SongOrganizer.Data;

public class Track : TromboneTrack
{
    public int trackindex { get; set; }
    public string trackref { get; set; }
    public string trackname_long { get; set; }
    public string trackname_short { get; set; }
    public string year { get; set; }
    public string artist { get; set; }
    public string desc { get; set; }
    public string genre { get; set; }
    public int difficulty { get; set; }
    public int tempo { get; set; }
    public int length { get; set; }

    public bool custom { get; set; }
    public bool rated { get; set; }
    public string letterScore { get; set; }
    public int[] scores { get; set; }

    public Track() { }

    public Track(SingleTrackData singleTrackData)
    {
        trackindex = singleTrackData.trackindex;
        trackref = singleTrackData.trackref;
        trackname_long = singleTrackData.trackname_long;
        trackname_short = singleTrackData.trackname_short;
        year = singleTrackData.year;
        artist = singleTrackData.artist;
        desc = singleTrackData.desc;
        genre = singleTrackData.genre;
        difficulty = singleTrackData.difficulty;
        tempo = singleTrackData.tempo;
        length = singleTrackData.length;
    }

    public bool IsVisible()
    {
        throw new System.NotImplementedException();
    }

    public SavedLevel LoadChart()
    {
        throw new System.NotImplementedException();
    }

    public LoadedTromboneTrack LoadTrack()
    {
        throw new System.NotImplementedException();
    }
}