using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BaboonAPI.Hooks.Tracks;
using BaboonAPI.Internal.BaseGame;
using Microsoft.FSharp.Collections;
using Newtonsoft.Json;
using SongOrganizer.Data;
using TrombLoader.CustomTracks;
using TrombLoader.Helpers;
using static SongOrganizer.Utils.TootTallyWebClient;
using static SongOrganizer.Utils.TrackCalculation;

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
        AddTracks();
        FilterTracks(__instance);
    }

    private void AddTracks()
    {
        var start = DateTime.Now;
        var allTracks = GlobalVariables.all_track_collections.Last().all_tracks;
        Plugin.Log.LogDebug($"Loading tracks: {allTracks.Count} total, {Plugin.StarDict.Count} star calcs");

        CalculateRatedTracks(allTracks.Select(i => i.trackref));
        Plugin.TrackDict.Clear();
        allTracks.ForEach(track =>
        {
            bool isBaseGame = TrackLookup.lookup(track.trackref) is BaseGameTrack;
            track.difficulty = track.difficulty > 10 ? 10 : track.difficulty;
            track.difficulty = track.difficulty < 0 ? 0 : track.difficulty;
            Track newTrack = new(track);
            newTrack.custom = !isBaseGame;
            newTrack.rated = Plugin.RatedTrackrefs.Contains(track.trackref);
            newTrack.stars = track.difficulty;
            if (Plugin.StarDict.ContainsKey(track.trackref))
                newTrack.stars = Plugin.StarDict[track.trackref];
            var scores = TrackLookup.lookupScore(track.trackref);
            newTrack.letterScore = scores != null ? scores.Value.highestRank : "-";
            newTrack.scores = scores != null ? scores.Value.highScores.ToArray() : new int[Plugin.TRACK_SCORE_LENGTH];
            newTrack.isFavorite = Plugin.Options.ContainsFavorite(track.trackref);
            Plugin.TrackDict.TryAdd(track.trackref, newTrack);
        });
        for (var i = 0; i < GlobalVariables.all_track_collections.Count; i++)
        {
            GlobalVariables.all_track_collections[i].all_tracks.ForEach(track =>
                Plugin.TrackDict[track.trackref].collections.Add(i));
        }

        var missingRatedTracks = ratedTracks.Where(track => !foundNoteHashes.Contains(track.note_hash) && !track.is_official).ToList();
        //missingRatedTracks.ForEach(track => Plugin.Log.LogDebug($"{track.short_name} {track.mirror}"));
        Plugin.Log.LogDebug($"{missingRatedTracks.Count} / {Plugin.RatedTrackrefs.Count} rated tracks missing. Loading {Plugin.TrackDict.Count} tracks elapsed: {DateTime.Now - start}");
    }

    private static List<SearchTrackResult> ratedTracks;
    private static ILookup<string, SearchTrackResult> ratedTrackFileHashes;
    private static ILookup<string, SearchTrackResult> ratedTrackNoteHashes;
    private static HashSet<string> ratedTrackRefs;
    private static ConcurrentBag<string> foundNoteHashes;
    private static void CalculateRatedTracks(IEnumerable<string> trackrefs)
    {
        if (Plugin.RatedTrackrefs.Count > 0) return;
        try
        {
            string responseText = File.ReadAllText(Plugin.RatedTracksPath);
            var response = JsonConvert.DeserializeObject<SearchResponse>(responseText);
            ratedTracks = response.results.Where(i => i.is_rated).ToList();
            foundNoteHashes = new();
            ratedTrackFileHashes = ratedTracks.ToLookup(i => i.file_hash);
            ratedTrackNoteHashes = ratedTracks.ToLookup(i => i.note_hash);
            ratedTrackRefs = new HashSet<string>(ratedTracks.Select(i => i.track_ref));
            Plugin.RatedTrackrefs = new ConcurrentBag<string>(trackrefs.AsParallel()
                .WithDegreeOfParallelism(Plugin.MAX_PARALLELISM)
                .Where(trackref => IsRated(trackref))
                .ToList());
        }
        catch (Exception e)
        {
            Plugin.Log.LogError($"Error calculating rated tracks\n{e.Message}");
        }
    }

    private static bool IsRated(string trackref)
    {
        if (trackref == null) return false;
        if (TrackLookup.lookup(trackref) is BaseGameTrack) return ratedTrackRefs.Contains(trackref);
        var track = TrackLookup.lookup(trackref);
        if (track is not CustomTrack) return false;
        var customTrack = track as CustomTrack;
        var chartPath = Path.Combine(customTrack.folderPath, Globals.defaultChartName);
        var tmb = File.ReadAllText(chartPath);
        if (ratedTrackRefs.Contains(trackref))
        {
            var fileHash = CalcHash(tmb);
            if (ratedTrackFileHashes.Contains(fileHash))
            {
                foundNoteHashes.Add(ratedTrackFileHashes[fileHash].FirstOrDefault().note_hash);
                return true;
            }
        }
        var notes = BuildNoteString(tmb);
        var noteHash = CalcHash(notes);
        if (ratedTrackNoteHashes.Contains(noteHash))
        {
            foundNoteHashes.Add(noteHash);
            return true;
        }
        return false;
    }

    public static void FilterTracks(LevelSelectController __instance)
    {
        __instance.alltrackslist = new List<SingleTrackData>(__instance.alltrackslist);
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
