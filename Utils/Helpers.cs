using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BaboonAPI.Hooks.Tracks;
using BepInEx;
using Newtonsoft.Json;
using UnityEngine;

namespace SongOrganizer.Utils;

public class Helpers
{
    public static string RatedTracksPath = Paths.ConfigPath + "/rated.json";

    public static string GetBestLetterScore(string trackRef, int bestScore)
    {
        if (bestScore == 0) return "-";
        float num = (float)bestScore / GetMaxScore(trackRef);
        return num < 1f ? (num < 0.8f ? (num < 0.6f ? (num < 0.4f ? (num < 0.2f ? "F" : "D") : "C") : "B") : "A") : "S";
    }

    public static int GetMaxScore(string trackRef)
    {
        return TrackLookup.lookup(trackRef).LoadChart().savedleveldata.Sum(noteData =>
            (int)Mathf.Floor(Mathf.Floor(noteData[1] * 10f * 100f * 1.3f) * 10f));
    }

    // key: track_ref, value: short_name
    public static Dictionary<string, string> GetRatedTracks()
    {
        var trackrefs = new Dictionary<string, string>();
        try
        {
            string responseText = File.ReadAllText(RatedTracksPath);
            var response = JsonConvert.DeserializeObject<SearchResponse>(responseText);
            foreach (var result in response.results)
            {
                trackrefs.TryAdd(result.track_ref, result.short_name);
            }
        }
        catch (Exception e)
        {
            Plugin.Log.LogError($"Error reading rated.json\n{e.Message}");
        }
        return trackrefs;
    }


    [Serializable]
    public class SearchResponse
    {
        public List<SearchTrackResult> results;
    }

    [Serializable]
    public class SearchTrackResult
    {
        public string track_ref;
        public string short_name;
    }
}