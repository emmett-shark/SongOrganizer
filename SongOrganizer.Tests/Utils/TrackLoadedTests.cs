using BepInEx.Logging;
using SongOrganizer.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SongOrganizer.Utils.Tests;

[TestClass]
public class TrackLoadedTests : TrackLoaded
{
    [TestMethod]
    public void AddTracks_Official()
    {
        var ratedTracks = new List<TootTally.SearchTrackResult>();
        var alltrackslist = new List<SingleTrackData>();
        var track = (SingleTrackData)FormatterServices.GetUninitializedObject(typeof(SingleTrackData));
        track.trackref = "hi";
        alltrackslist.Add(track);
        Plugin.Log = new ManualLogSource("a");

        var test = new TrackLoadedTests();
        test.AddTracks(ratedTracks, alltrackslist);
        Assert.IsTrue(Plugin.TrackDict[track.trackref].custom);
    }

    protected override bool IsCustomTrack(string trackref)
    {
        return true;
    }

    protected override string CalcSHA256(string trackref) => trackref;

    protected override void SetScore(string trackref, Track newTrack)
    {
        newTrack.letterScore = "-";
    }
}