using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BaboonAPI.Hooks.Tracks;
using BaboonAPI.Internal.BaseGame;
using Microsoft.FSharp.Collections;
using SongOrganizer.Data;
using TrombLoader.CustomTracks;
using TrombLoader.Helpers;

namespace SongOrganizer.Utils;

public class RefreshLevelSelect : TracksLoadedEvent.Listener
{
    private LevelSelectController __instance;

    public RefreshLevelSelect(LevelSelectController instance)
    {
        __instance = instance;
    }

    public void OnTracksLoaded(FSharpList<TromboneTrack> tracks)
    {
        AddTracks(__instance.alltrackslist);
        FilterTracks(__instance);
    }

    public void AddTracks(List<SingleTrackData> singleTrackDatas)
    {
        var start = DateTime.Now;
        Plugin.Log.LogDebug($"Loading tracks: {singleTrackDatas} total, {Plugin.RatedTracks.Count} rated, {Plugin.StarDict.Count} star calcs");
        var ratedTrackHashes = Plugin.RatedTracks.ToLookup(i => i.file_hash);
        var ratedTrackRefs = new HashSet<string>(Plugin.RatedTracks.Select(i => i.track_ref));
        var foundRatedTrackNoteHashes = new HashSet<string>();
        Plugin.TrackDict.Clear();
        foreach (var track in singleTrackDatas)
        {
            bool isBaseGame = TrackLookup.lookup(track.trackref) is BaseGameTrack;
            bool rated = false;
            if (ratedTrackRefs.Contains(track.trackref))
            {
                if (!isBaseGame)
                {
                    var customTrack = TrackLookup.lookup(track.trackref) as CustomTrack;
                    var chartPath = Path.Combine(customTrack.folderPath, Globals.defaultChartName);
                    var hash = TrackCalculation.CalcFileHash(File.ReadAllText(chartPath));
                    rated = ratedTrackHashes.Contains(hash);
                    if (rated) foundRatedTrackNoteHashes.Add(ratedTrackHashes[hash].First().note_hash);
                }
                else
                {
                    rated = true;
                }
            }
            track.difficulty = track.difficulty > 10 ? 10 : track.difficulty;
            track.difficulty = track.difficulty < 0 ? 0 : track.difficulty;
            Track newTrack = new(track);
            newTrack.custom = !isBaseGame;
            newTrack.rated = rated;
            newTrack.stars = track.difficulty;
            if (Plugin.StarDict.ContainsKey(track.trackref))
                newTrack.stars = Plugin.StarDict[track.trackref];
            var scores = TrackLookup.lookupScore(track.trackref);
            newTrack.letterScore = scores != null ? scores.Value.highestRank : "-";
            newTrack.scores = scores != null ? scores.Value.highScores.ToArray() : new int[Plugin.TRACK_SCORE_LENGTH];
            Plugin.TrackDict.TryAdd(track.trackref, newTrack);
        }

        var missingRatedTracks = Plugin.RatedTracks.Where(track => !foundRatedTrackNoteHashes.Contains(track.note_hash) && !track.is_official).ToList();
        Plugin.Log.LogDebug($"{missingRatedTracks.Count} rated tracks missing. Loading tracks elapsed: {DateTime.Now - start}");
    }

    public static void FilterTracks(LevelSelectController __instance)
    {
        List<string[]> newTracktitles = new List<string[]>();
        List<Track> newTrackData = new List<Track>();
        int newTrackIndex = 0;
        foreach (Track track in Plugin.TrackDict.Values)
        {
            if (!FilterQueryParser.ShowTrack(track)) continue;
            track.trackindex = newTrackIndex;
            newTracktitles.Add(new string[] {
                track.trackname_long,
                track.trackname_short,
                track.trackref,
                track.year,
                track.artist,
                track.genre,
                track.desc,
                track.difficulty.ToString(),
                track.length.ToString(),
                track.tempo.ToString()
            });
            newTrackData.Add(track);
            newTrackIndex++;
        }
        if (newTracktitles.Count > 0)
        {
            GlobalVariables.data_tracktitles = newTracktitles.ToArray();
            __instance.alltrackslist.Clear();
            __instance.alltrackslist.AddRange(newTrackData);
        }
        if (GlobalVariables.levelselect_index >= GlobalVariables.data_tracktitles.Length)
        {
            GlobalVariables.levelselect_index = 0;
        }
        Plugin.Log.LogDebug($"Filter result: {__instance.alltrackslist.Count} found of {Plugin.TrackDict.Count}");

        __instance.sortTracks(Plugin.Options.SortMode.Value.ToLower(), false);
    }
}
