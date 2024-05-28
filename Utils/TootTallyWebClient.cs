using System;
using System.Collections.Generic;
using BepInEx;
using Newtonsoft.Json;
using UnityEngine.Networking;

namespace SongOrganizer.Utils;

public class TootTallyWebClient
{
    public const string API_URL = "https://toottally.com";
    public const string RATED_ENDPOINT = "/api/search/?rated=1&fields=track_ref,short_name,note_hash,file_hash,difficulty,download,mirror,is_official,is_rated";
    public const int PAGE_SIZE = 100;

    [Serializable]
    public class SearchResponse
    {
        public int count;
        public List<SearchTrackResult> results;
    }

    [Serializable]
    public class SearchTrackResult
    {
        public string track_ref;
        public string short_name;
        public string note_hash;
        public string file_hash;
        public decimal difficulty;
        public string download;
        public string mirror;
        public bool is_official;
        public bool is_rated;
    }

    public static IEnumerator<UnityWebRequestAsyncOperation> GetRatedTracksRequest(int page, int pageSize, Action<SearchResponse> callback = null)
    {
        string apiLink = $"{API_URL}{RATED_ENDPOINT}&page_size={pageSize}&page={page}";
        var webRequest = UnityWebRequest.Get(apiLink);
        yield return webRequest.SendWebRequest();
        if (!webRequest.isNetworkError && !webRequest.isHttpError)
        {
            var data = webRequest.downloadHandler.GetText();
            var result = JsonConvert.DeserializeObject<SearchResponse>(data);
            Plugin.RatedTracksPaged.AddRange(result.results);
            if (callback != null) callback(result);
        }
        else
        {
            Plugin.Log.LogError($"ERROR: {webRequest.error}");
        }
    }
}
