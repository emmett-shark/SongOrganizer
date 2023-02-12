namespace SongOrganizer.Data;

public class Track : SingleTrackData
{
    public bool custom { get; set; }
    public bool rated { get; set; }
    public string letterScore { get; set; }
    public int[] scores { get; set; }

    public Track() { }

    public Track(SingleTrackData other)
    {
        trackname_long = other.trackname_long;
        trackname_short = other.trackname_short;
        year = other.year;
        artist = other.artist;
        desc = other.desc;
        genre = other.genre;
        difficulty = other.difficulty;
        tempo = other.tempo;
        length = other.length;
        trackindex = other.trackindex;
        trackref = other.trackref;
    }
}