using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using SimpleJSON;
using TrombLoader.Data;
using TrombLoader.Helpers;
using UnityEngine;

namespace SongOrganizer.Data;

public class Utils
{
    public static string getBestLetterScore(string trackRef, int bestScore)
    {
        if (bestScore == 0) return "-";
        float num = (float)bestScore / getMaxScore(trackRef);
        return (double)num <= 1.0 ? ((double)num <= 0.800000011920929 ? ((double)num <= 0.600000023841858 ? ((double)num <= 0.400000005960464 ? ((double)num <= 0.200000002980232 ? "F" : "D") : "C") : "B") : "A") : "S";
    }

    public static int getMaxScore(string trackRef)
    {
        string baseTmb = Application.streamingAssetsPath + "/leveldata/" + trackRef + ".tmb";
        List<float[]> levelData = !File.Exists(baseTmb)
            ? GetCustomLevelData(trackRef)
            : GetSavedLevel(baseTmb).savedleveldata;
        return levelData.Sum(noteData =>
        {
            float num1 = Mathf.Floor(noteData[1] * 10f);
            float f = Mathf.Floor(num1 * 100f * 1.3f) * 10f;
            return Mathf.FloorToInt(f);
        });
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
}