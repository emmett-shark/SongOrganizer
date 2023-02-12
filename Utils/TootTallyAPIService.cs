using System.Collections.Generic;
using UnityEngine.Networking;
using System.IO;

namespace SongOrganizer.Utils;

public static class TootTallyAPIService
{
    public const string API_URL = "https://toottally.com";
    public const string RATED_ENDPOINT = "/api/search/?rated=1&page=1&page_size=100000";
    
    public static IEnumerator<UnityWebRequestAsyncOperation> GetRatedTracks()
    {
        string apiLink = $"{API_URL}{RATED_ENDPOINT}";
        Plugin.Log.LogDebug($"Getting rated tracks {apiLink}");
        var webRequest = UnityWebRequest.Get(apiLink);
        yield return webRequest.SendWebRequest();

        if (!webRequest.isNetworkError && !webRequest.isHttpError)
        {
            File.WriteAllText(Helpers.RatedTracksPath, webRequest.downloadHandler.GetText());
            Plugin.Log.LogDebug($"Finished writing rated tracks to {Helpers.RatedTracksPath}");
        }
        else
        {
            Plugin.Log.LogError($"ERROR: {webRequest.error}");
        }
    }
}
