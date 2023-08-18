using System.Linq;
using System.Security.Cryptography;
using System.Text;
using BaboonAPI.Hooks.Tracks;
using UnityEngine;

namespace SongOrganizer.Utils;

public class Helpers
{
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

    public static string CalcSHA256(string input)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }
            return builder.ToString();
        }
    }
}