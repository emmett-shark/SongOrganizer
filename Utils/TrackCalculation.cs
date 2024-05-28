using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TootTallyDiffCalcLibs;
using TrombLoader.Helpers;
using UnityEngine;
using static SongOrganizer.Utils.TootTallyWebClient;

namespace SongOrganizer.Utils;

public class TrackCalculation
{
    public static void CalculateStars()
    {
        var start = DateTime.Now;
        Plugin.Log.LogDebug($"Starting star calculations {DateTime.Now}");
        int maxParallelism = 4;

        Task.Run(() =>
        {
            Directory.GetFiles(Globals.GetCustomSongsPath(), "song.tmb", SearchOption.AllDirectories)
                .Concat(Directory.GetFiles(BepInEx.Paths.PluginPath, "song.tmb", SearchOption.AllDirectories))
                .Select(i => Path.GetDirectoryName(i))
                .AsParallel()
                .WithDegreeOfParallelism(maxParallelism)
                .ForAll(songFolder => CalculateCustomStars(songFolder));
            Plugin.Log.LogDebug($"Custom star calculation elapsed: {DateTime.Now - start}");
        });
        Task.Run(() =>
        {
            var trackassetDir = $"{Application.streamingAssetsPath}/trackassets/";
            Directory.GetDirectories(trackassetDir)
                .Select(i => i.Substring(trackassetDir.Length))
                .Where(i => i != "freeplay")
                .AsParallel()
                .WithDegreeOfParallelism(maxParallelism)
                .ForAll(trackref => CalculateBaseStars(trackref));
            Plugin.Log.LogDebug($"Base star calculation elapsed: {DateTime.Now - start}");
        });
    }

    public static void CalculateBaseStars(string trackref)
    {
        using var chart = ChartReader.ReadBaseGame(trackref);
        ProcessStars(chart);
    }

    public static void CalculateCustomStars(string songFolder)
    {
        var chartPath = Path.Combine(songFolder, Globals.defaultChartName);
        if (!File.Exists(chartPath)) return;
        using var chart = ChartReader.ReadCustomChart(chartPath);
        ProcessStars(chart);
    }

    private static void ProcessStars(Chart chart)
    {
        chart.ProcessLite();
        var stars = chart.performances.starRatingDict[0];
        Plugin.StarDict[chart.trackRef] = stars;
    }

    public static string CalcFileHash(string str)
    {
        var data = Encoding.UTF8.GetBytes(str);
        using SHA256 sha256 = SHA256.Create();
        byte[] hashArray = sha256.ComputeHash(data);
        StringBuilder sb = new StringBuilder();
        foreach (byte b in hashArray)
        {
            sb.Append(b.ToString("x2"));
        }
        return sb.ToString();
    }

    public static void ReadRatedTracksFromFile()
    {
        if (Plugin.RatedTracks.Count > 0) return;
        var start = DateTime.Now;
        try
        {
            string responseText = File.ReadAllText(Plugin.RatedTracksPath);
            var response = JsonConvert.DeserializeObject<SearchResponse>(responseText);
            Plugin.RatedTracks = response.results.Where(i => i.is_rated).ToList();
        }
        catch (Exception e)
        {
            Plugin.Log.LogError($"Error reading rated.json\n{e.Message}");
        }
    }
}
