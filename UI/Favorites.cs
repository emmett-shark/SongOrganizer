using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace SongOrganizer.UI;

//https://i.gyazo.com/148c5ec1a8562ed7714571bd5ae5efb4.png GameButtonsPanel hierarchy
//https://i.gyazo.com/083d2ca900201bb49eb34ef78a444ec3.png default colorblock
//https://forum.unity.com/threads/ui-button-create-by-script-c.285829/ create unity ui element programatically without prefab
public class Favorites : MonoBehaviour
{
    public const string GAME_BUTTONS_PANEL = $"{UnityPaths.FULLSCREENPANEL}/GameButtonsPanel";

    public static void Setup(LevelSelectController __instance)
    {
        if (Plugin.Options.HideHearts.Value) return;
        var gameButtonsPanel = GameObject.Find(GAME_BUTTONS_PANEL);
        for (int i = 0; i < 7; i++)
            Plugin.FavoriteButtons.Add(FavoriteButton(i, gameButtonsPanel.transform, __instance));
        ShowFavorites(__instance);
    }

    public static void Setup(PointSceneController __instance)
    {
        if (Plugin.Options.HideHearts.Value) return;
        var favButton = Instantiate(Plugin.Button, GameObject.Find("Canvas").transform);
        favButton.name = $"favbutton";
        favButton.GetComponentInChildren<Text>().text = "";
        var rectTransform = favButton.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x + 5, rectTransform.anchoredPosition.y);

        favButton.colors = Plugin.Options.ContainsFavorite(GlobalVariables.chosen_track) ? on : off;
        favButton.onClick.AddListener(() =>
        {
            if (Plugin.Options.ContainsFavorite(GlobalVariables.chosen_track))
            {
                Plugin.Options.RemoveFavorite(GlobalVariables.chosen_track);
                favButton.colors = off;
            }
            else
            {
                Plugin.Options.AddFavorite(GlobalVariables.chosen_track);
                favButton.colors = on;
            }
        });
        favButton.transform.SetAsLastSibling();
        var image = favButton.GetComponent<Image>();
        image.sprite = GetSprite($"{Path.GetDirectoryName(Plugin.Instance.Info.Location)}/Assets/white-heart.png");
        image.rectTransform.sizeDelta = new Vector2(36, 36);
    }

    private static Button FavoriteButton(int i, Transform parent, LevelSelectController __instance)
    {
        var favButton = Instantiate(Plugin.Button, parent);
        favButton.name = $"favbutton{i}";
        favButton.GetComponentInChildren<Text>().text = "";

        var songButton = parent.Find($"Button{i}");
        var rectTransform = songButton.GetComponent<RectTransform>();
        favButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(rectTransform.anchoredPosition.x - 246, rectTransform.anchoredPosition.y - 228);

        favButton.colors = off;
        favButton.onClick.AddListener(() =>
        {
            var newIndex = GetIndex(i, __instance);
            var track = __instance.alltrackslist[newIndex];
            if (Plugin.Options.ContainsFavorite(track.trackref))
            {
                Plugin.Options.RemoveFavorite(track.trackref);
            }
            else
            {
                Plugin.Options.AddFavorite(track.trackref);
            }
            ShowFavorites(__instance);
        });
        var image = favButton.GetComponent<Image>();
        image.sprite = GetSprite($"{Path.GetDirectoryName(Plugin.Instance.Info.Location)}/Assets/white-heart.png");
        image.rectTransform.sizeDelta = new Vector2(18, 18);
        image.rectTransform.rotation = new Quaternion(0, 0, -0.05f, 1);
        return favButton;
    }

    public static void ShowFavorites(LevelSelectController __instance)
    {
        int buttonIndex = 0;
        foreach (var button in Plugin.FavoriteButtons)
        {
            var newIndex = GetIndex(buttonIndex, __instance);
            var isFav = Plugin.Options.ContainsFavorite(__instance.alltrackslist[newIndex].trackref);
            button.colors = isFav ? on : off;
            buttonIndex++;
        }
    }

    private static int GetIndex(int index, LevelSelectController __instance)
    {
        index = __instance.songindex + indices[index];
        var length = __instance.alltrackslist.Count;
        if (length <= 1) return 0;
        if (index < length && index >= 0) return index;
        if (index >= length) return (index - length) % length;
        return ((index % length) + length) % length;
    }

    // 4, 5, 6, 0, 1, 2, 3 is how the dev ordered the song list :skull:
    private static readonly int[] indices = new int[] { 0, 1, 2, 3, -3, -2, -1 };

    private static readonly ColorBlock off = new ColorBlock
    {
        normalColor = Color.white,
        highlightedColor = new Color(0.8f, 0.8f, 0.8f),
        pressedColor = new Color(0.8f, 0.8f, 0.8f),
        colorMultiplier = 1,
    };

    private static readonly ColorBlock on = new ColorBlock
    {
        normalColor = new Color(0.95f, 0.22f, 0.35f),
        highlightedColor = new Color(0.74f, 0.15f, 0.25f),
        pressedColor = new Color(0.74f, 0.15f, 0.25f),
        colorMultiplier = 1,
    };

    private static Sprite GetSprite(string path)
    {
        var bytes = File.ReadAllBytes(path);
        var texture = new Texture2D(0, 0);
        texture.LoadImage(bytes);
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero, 300f);
    }
}
