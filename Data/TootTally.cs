using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using Newtonsoft.Json;
using UnityEngine.Networking;

namespace SongOrganizer.Data;

public class TootTally
{
    public static string RatedTracksPath = Paths.ConfigPath + "/rated.json";
    public const string API_URL = "https://toottally.com";
    public const string RATED_ENDPOINT = "/api/search/?rated=1&page=1&page_size=100000";

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
        public string name;
        public string note_hash;
        public string file_hash;
        public decimal difficulty;
        public string author;
        public string download;
        public string mirror;
        public bool is_official;
        public bool is_rated;
    }

    public static IEnumerator<UnityWebRequestAsyncOperation> GetRatedTracks()
    {
        string apiLink = $"{API_URL}{RATED_ENDPOINT}";
        Plugin.Log.LogDebug($"Getting rated tracks {apiLink}");
        var webRequest = UnityWebRequest.Get(apiLink);
        yield return webRequest.SendWebRequest();

        if (!webRequest.isNetworkError && !webRequest.isHttpError)
        {
            File.WriteAllText(RatedTracksPath, webRequest.downloadHandler.GetText());
            Plugin.Log.LogDebug($"Finished writing rated tracks to {RatedTracksPath}");
        }
        else
        {
            Plugin.Log.LogError($"ERROR: {webRequest.error}");
        }
    }

    public static IEnumerable<SearchTrackResult> ReadRatedTracks()
    {
        try
        {
            string responseText = File.ReadAllText(RatedTracksPath);
            var response = JsonConvert.DeserializeObject<SearchResponse>(responseText);
            return response.results.Where(i => i.is_rated);
        }
        catch (Exception e)
        {
            Plugin.Log.LogError($"Error reading rated.json\n{e.Message}");
        }
        return new List<SearchTrackResult>();
    }
}
