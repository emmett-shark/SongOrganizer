using System;
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

        Task.Run(() =>
        {
            Directory.GetFiles(Globals.GetCustomSongsPath(), "song.tmb", SearchOption.AllDirectories)
                .Concat(Directory.GetFiles(BepInEx.Paths.PluginPath, "song.tmb", SearchOption.AllDirectories))
                .Select(i => Path.GetDirectoryName(i))
                .AsParallel()
                .WithDegreeOfParallelism(Plugin.MAX_PARALLELISM)
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
                .WithDegreeOfParallelism(Plugin.MAX_PARALLELISM)
                .ForAll(trackref => CalculateBaseStars(trackref));
            Plugin.Log.LogDebug($"Base star calculation elapsed: {DateTime.Now - start}");
        });
    }

    private static void CalculateBaseStars(string trackref)
    {
        using var chart = ChartReader.ReadBaseGame(trackref);
        ProcessChart(chart);
    }

    private static void CalculateCustomStars(string songFolder)
    {
        var chartPath = Path.Combine(songFolder, Globals.defaultChartName);
        if (!File.Exists(chartPath)) return;
        using var chart = ChartReader.ReadCustomChart(chartPath);
        ProcessChart(chart);
    }

    // build note string based on how toottally is doing it :skull:
    public static string BuildNoteString(string tmb)
    {
        var noteArrayStart = tmb.IndexOf('[', tmb.IndexOf("notes\""));
        var notes = new StringBuilder();
        var endCount = 0;
        for (int i = noteArrayStart; i < tmb.Length && endCount < 2; i++)
        {
            if (char.IsWhiteSpace(tmb[i])) continue;
            notes.Append(tmb[i]);
            if (tmb[i] == '[')
            {
                endCount = 0;
            }
            else if (tmb[i] == ']')
            {
                endCount++;
            }
            else if (tmb[i] == ',')
            {
                notes.Append(' ');
            }
        }
        return notes.ToString();
    }

    private static void ProcessChart(Chart chart)
    {
        chart.ProcessLite();
        Plugin.StarDict[chart.trackRef] = chart.performances.starRatingDict[0];
    }

    public static string CalcHash(string str)
    {
        var data = Encoding.UTF8.GetBytes(str);
        using SHA256 sha256 = SHA256.Create();
        byte[] hashArray = sha256.ComputeHash(data);
        var sb = new StringBuilder();
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
