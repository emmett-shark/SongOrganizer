using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using BepInEx;
using SimpleJSON;
using TrombLoader.Data;
using TrombLoader.Helpers;
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
        string baseTmb = Application.streamingAssetsPath + "/leveldata/" + trackRef + ".tmb";
        List<float[]> levelData = !File.Exists(baseTmb)
            ? GetCustomLevelData(trackRef)
            : GetSavedLevel(baseTmb).savedleveldata;

        return levelData.Sum(noteData =>
            (int)Mathf.Floor(Mathf.Floor(noteData[1] * 10f * 100f * 1.3f) * 10f));
    }

    private static List<float[]> GetCustomLevelData(string trackRef)
    {
        if (!Globals.ChartFolders.TryGetValue(trackRef, out string customChartPath))
        {
            return new List<float[]>();
        }
        using (var streamReader = new StreamReader(customChartPath + "/song.tmb"))
        {
            string baseChartName = Application.streamingAssetsPath + "/leveldata/ballgame.tmb";
            SavedLevel savedLevel = GetSavedLevel(baseChartName);
            CustomSavedLevel customLevel = new CustomSavedLevel(savedLevel);
            string jsonString = streamReader.ReadToEnd();
            var jsonObject = JSON.Parse(jsonString);
            customLevel.Deserialize(jsonObject);
            return customLevel.savedleveldata;
        }
    }

    private static SavedLevel GetSavedLevel(string baseTmb)
    {
        using (FileStream fileStream = File.Open(baseTmb, FileMode.Open))
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            return (SavedLevel)binaryFormatter.Deserialize(fileStream);
        }
    }

    // key: track_ref, value: short_name
    public static Dictionary<string, string> GetRatedTracks()
    {
        var trackrefs = new Dictionary<string, string>();
        try
        {
            using (var streamReader = new StreamReader(RatedTracksPath))
            {
                string jsonString = streamReader.ReadToEnd();
                var jsonObject = JSON.Parse(jsonString);
                var results = jsonObject.GetValueOrDefault("results", null);
                foreach (var result in results?.AsArray)
                {
                    trackrefs.TryAdd(result.Value["track_ref"], result.Value["short_name"]);
                }
            }
        }
        catch (Exception e)
        {
            Plugin.Log.LogError($"Error reading rated.json\n{e.Message}");
        }
        return trackrefs;
    }
}