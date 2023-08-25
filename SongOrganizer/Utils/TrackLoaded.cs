using System;
using System.Collections.Generic;
using System.Linq;
using BaboonAPI.Hooks.Tracks;
using BepInEx;
using Microsoft.FSharp.Collections;
using SongOrganizer.Data;
using TrombLoader.Helpers;

namespace SongOrganizer.Utils;

public class TrackLoaded : TracksLoadedEvent.Listener
{
    private LevelSelectController __instance;
    private List<TootTally.SearchTrackResult> ratedTracks;

    public TrackLoaded() { }

    public TrackLoaded(LevelSelectController instance, List<TootTally.SearchTrackResult> ratedTracks)
    {
        __instance = instance;
        this.ratedTracks = ratedTracks;
    }

    public void OnTracksLoaded(FSharpList<TromboneTrack> tracks)
    {
        var start = DateTime.Now;
        var alltrackslist = __instance.alltrackslist;
        Plugin.Log.LogDebug($"Loading tracks: {alltrackslist.Count} total");
        AddTracks(ratedTracks, alltrackslist);
        FilterTracks(alltrackslist);
        __instance.sortTracks(Plugin.Options.SortMode.Value.ToLower(), false);
        Plugin.Log.LogDebug($"Elapsed {DateTime.Now - start}");
    }

    protected virtual bool IsCustomTrack(string trackref) => Globals.IsCustomTrack(trackref);
    protected virtual string CalcSHA256(string trackref) => Helpers.CalcSHA256(trackref);
    protected virtual void SetScore(string trackref, Track newTrack)
    {
        var scores = TrackLookup.lookupScore(trackref);
        newTrack.letterScore = scores != null ? scores.Value.highestRank : "-";
        newTrack.scores = scores != null ? scores.Value.highScores.ToArray() : new int[Plugin.TRACK_SCORE_LENGTH];
    }

    public void AddTracks(List<TootTally.SearchTrackResult> ratedTracks, List<SingleTrackData> alltrackslist)
    {
        var ratedTrackFileHashes = ratedTracks?.Where(i => !i.is_official && i.download != null).ToDictionary(i => i.file_hash);
        var ratedTrackRefs = new HashSet<string>(ratedTracks.Select(i => i.track_ref));
        var foundRatedTrackNoteHashes = new HashSet<string>();
        Plugin.TrackDict.Clear();
        foreach (var track in alltrackslist)
        {
            bool custom = false, rated = true;
            if (IsCustomTrack(track.trackref))
            {
                if (ratedTrackRefs.Contains(track.trackref))
                {
                    var fileHash = CalcSHA256(track.trackref);
                    rated = ratedTrackFileHashes.ContainsKey(fileHash);
                    if (rated) foundRatedTrackNoteHashes.Add(ratedTrackFileHashes[fileHash].note_hash);
                }
                else
                {
                    rated = false;
                }
                custom = true;
            }
            Track newTrack = new Track(track)
            {
                custom = custom,
                rated = rated
            };
            SetScore(track.trackref, newTrack);
            if (!Plugin.TrackDict.ContainsKey(track.trackref)) Plugin.TrackDict.Add(track.trackref, newTrack);
        }
        var missingRatedTracks = new Dictionary<string, TootTally.SearchTrackResult>();
        foreach (var track in ratedTracks)
        {
            if (!foundRatedTrackNoteHashes.Contains(track.note_hash) && track.download != null)
            {
                if (!missingRatedTracks.ContainsKey(track.download)) missingRatedTracks.Add(track.download, track);
            }
        }
        Plugin.Log.LogInfo($"Rated tracks: {ratedTracks.Count} total. {missingRatedTracks.Count} missing");
        foreach (var track in missingRatedTracks.Values.OrderBy(i => i.short_name))
        {
            Plugin.Log.LogInfo($"{track.short_name} - {track.download}");
        }
    }

    public void FilterTracks(List<SingleTrackData> alltrackslist)
    {
        List<string[]> newTracktitles = new List<string[]>();
        List<SingleTrackData> newTrackData = new List<SingleTrackData>();
        int newTrackIndex = 0;
        foreach (Track track in Plugin.TrackDict.Values)
        {
            if (!ShowTrack(track)) continue;
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
            newTrackData.Add(TrackLookup.toTrackData(track));
            newTrackIndex++;
        }
        if (newTracktitles.Count > 0)
        {
            GlobalVariables.data_tracktitles = newTracktitles.ToArray();
            alltrackslist.Clear();
            alltrackslist.AddRange(newTrackData);
        }
        if (GlobalVariables.levelselect_index >= GlobalVariables.data_tracktitles.Length)
        {
            GlobalVariables.levelselect_index = 0;
        }
        Plugin.Log.LogDebug($"Filter result: {alltrackslist.Count} found of {Plugin.TrackDict.Count}");
    }

    private bool ShowTrack(Track track)
    {
        return ShowTrack(Plugin.Options.ShowCustom.Value, Plugin.Options.ShowDefault.Value, track.custom)
            && ShowTrack(Plugin.Options.ShowPlayed.Value, Plugin.Options.ShowUnplayed.Value, track.letterScore != "-")
            && ShowTrack(Plugin.Options.ShowSRank.Value, Plugin.Options.ShowNotSRank.Value, track.letterScore == "S")
            && ShowTrack(Plugin.Options.ShowRated.Value, Plugin.Options.ShowUnrated.Value, track.rated)
            && ShowTrack(Plugin.Options.SearchValue.Value, track);
    }

    private bool ShowTrack(bool optionToggle, bool oppositeOptionToggle, bool option) =>
        optionToggle == oppositeOptionToggle ? true : optionToggle == option;

    private bool ShowTrack(string searchVal, Track track)
    {
        if (searchVal.IsNullOrWhiteSpace()) return true;
        string search = searchVal.ToLower().Trim();
        return track.trackname_long.ToLower().Contains(search)
            || track.trackname_short.ToLower().Contains(search)
            || track.artist.ToLower().Contains(search)
            || track.desc.ToLower().Contains(search);
    }
}
