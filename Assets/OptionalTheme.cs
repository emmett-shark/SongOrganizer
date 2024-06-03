using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace SongOrganizer.Assets;

public class OptionalTheme
{
    public static SongOrganizerColors colors;

    public static void Setup()
    {
        colors = getTootTallyThemeColors();
        colors.replayButton.colors.colorMultiplier = 1;
        colors.playButton.background.a = 1;
        colors.playButton.shadow.a = 0.8f;
    }

    private static SongOrganizerColors getTootTallyThemeColors()
    {
        try
        {
            Type theme = Type.GetType("TootTallyCore.Theme, TootTallyCore");
            if (theme == null)
            {
                return new SongOrganizerColors();
            }
            var colorField = theme.GetField("colors", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            var colors = colorField.GetValue(colorField);

            return new SongOrganizerColors
            {
                scrollSpeedSlider = new ScrollSpeedSliderColors
                {
                    handle = Get<Color?>(colors, "scrollSpeedSlider", "handle") ?? new Color(1, 1, 0),
                    text = Get<Color?>(colors, "scrollSpeedSlider", "text") ?? Color.black,
                    background = Get<Color?>(colors, "scrollSpeedSlider", "background") ?? new Color(1, 1, 0),
                    fill = Get<Color?>(colors, "scrollSpeedSlider", "fill") ?? new Color(1, 1, 0),
                },
                playButton = new PlayButtonColors
                {
                    background = Get<Color?>(colors, "playButton", "background") ?? new Color(0.149f, 1, 1),
                    text = Get<Color?>(colors, "playButton", "text") ?? new Color(0.021f, 0.292f, 0.302f),
                    shadow = Get<Color?>(colors, "playButton", "shadow") ?? new Color(0.094f, 0.682f, 0.706f),
                },
                leaderboard = new LeaderboardColors
                {
                    text = Get<Color?>(colors, "leaderboard", "text") ?? Color.white,
                    textOutline = Get<Color?>(colors, "leaderboard", "textOutline") ?? new Color(0, 0, 0, .5f),
                },
                replayButton = new ReplayButtonColors
                {
                    text = Get<Color?>(colors, "replayButton", "text") ?? Color.white,
                    colors = Get<ColorBlock?>(colors, "replayButton", "colors") ?? new ColorBlock()
                    {
                        normalColor = new Color(0.95f, 0.22f, 0.35f),
                        highlightedColor = new Color(0.77f, 0.18f, 0.29f),
                        pressedColor = new Color(1, 1, 0)
                    },
                },
                songName = Get<Color?>(colors, "title", "songName") ?? Color.white,
            };
        }
        catch (Exception e)
        {
            Plugin.Log.LogError("Exception trying to get theme colors. Reporting TootTallyCore as not found.");
            Plugin.Log.LogError(e.Message);
            Plugin.Log.LogError(e.StackTrace);
            return new SongOrganizerColors();
        }
    }

    private static T Get<T>(object colors, params string[] fields)
    {
        var flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
        var fieldInfo = colors.GetType().GetField(fields[0], flags);
        var subField = fieldInfo.GetValue(colors);
        if (subField == null) return default(T);
        var subFieldInfo = subField.GetType().GetField(fields[1], flags);
        var subsubField = subFieldInfo.GetValue(subField);
        return (T)subsubField;
    }

    public class SongOrganizerColors
    {
        public ScrollSpeedSliderColors scrollSpeedSlider = new();
        public PlayButtonColors playButton = new();
        public LeaderboardColors leaderboard = new();
        public ReplayButtonColors replayButton = new();
        public Color songName = Color.white;
    }

    public class ScrollSpeedSliderColors
    {
        public Color handle = new Color(1, 1, 0);
        public Color text = Color.black;
        public Color background = Color.black;
        public Color fill = new Color(0.95f, 0.22f, 0.35f);
    }

    public class PlayButtonColors
    {
        public Color text = new Color(0.021f, 0.292f, 0.302f);
        public Color background = new Color(0.149f, 1, 1);
        public Color shadow = new Color(0.094f, 0.682f, 0.706f);
    }

    public class LeaderboardColors
    {
        public Color text = Color.white;
        public Color textOutline = new Color(0, 0, 0, .5f);
    }

    public class ReplayButtonColors
    {
        public Color text = Color.white;
        public ColorBlock colors = new ColorBlock()
        {
            normalColor = new Color(0.95f, 0.22f, 0.35f),
            highlightedColor = new Color(0.77f, 0.18f, 0.29f),
            pressedColor = new Color(1, 1, 0)
        };
    }
}
