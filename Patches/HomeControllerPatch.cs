using System;
using System.Linq;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SongOrganizer.Patches;

// idk how to create a toggle, so stealing one from the home screen. yoink
[HarmonyPatch(typeof(HomeController), nameof(HomeController.Start))]
public class HomeControllerStartPatch : MonoBehaviour
{
    static void Postfix(HomeController __instance)
    {
        Plugin.Toggle = Instantiate(__instance.set_tog_vsync);
        DontDestroyOnLoad(Plugin.Toggle);

        Plugin.Button = Instantiate(__instance.graphicspanel.GetComponentInChildren<Button>());
        Plugin.Button.onClick.RemoveAllListeners();
        DontDestroyOnLoad(Plugin.Button);

        SetInputFieldPrefab(); //thanks electro
    }

    private static void SetInputFieldPrefab()
    {
        float width = 250f;
        GameObject gameObject = new GameObject("InputFieldHolderSongOrganizer");
        RectTransform rectTransform = gameObject.AddComponent<RectTransform>();
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = new Vector2(width, 50f);
        GameObject image = Instantiate(gameObject, gameObject.transform);
        GameObject text = Instantiate(image, gameObject.transform);
        image.name = "Image";
        text.name = "Text";
        Plugin.InputFieldPrefab = gameObject.AddComponent<TMP_InputField>();
        rectTransform.anchorMax = rectTransform.anchorMin = Vector2.zero;
        Plugin.InputFieldPrefab.image = image.AddComponent<Image>();
        RectTransform component = image.GetComponent<RectTransform>();
        component.anchorMin = component.anchorMax = component.pivot = Vector2.zero;
        component.anchoredPosition = new Vector2(0f, 2f);
        component.sizeDelta = new Vector2(width, 2f);
        RectTransform textComponent = text.GetComponent<RectTransform>();
        textComponent.pivot = textComponent.anchorMax = textComponent.anchoredPosition = textComponent.anchorMin = Vector2.zero;
        textComponent.sizeDelta = new Vector2(width, 50f);
        Plugin.InputFieldPrefab.textComponent = CreateSingleText(text.transform, "TextLabel", "", Color.white);
        Plugin.InputFieldPrefab.textComponent.rectTransform.pivot = new Vector2(0f, 0.7f);
        Plugin.InputFieldPrefab.textComponent.alignment = TextAlignmentOptions.Left;
        Plugin.InputFieldPrefab.textComponent.margin = new Vector4(2f, 0f, 0f, 0f);
        Plugin.InputFieldPrefab.textComponent.enableWordWrapping = true;
        Plugin.InputFieldPrefab.textViewport = Plugin.InputFieldPrefab.textComponent.rectTransform;
        DontDestroyOnLoad(Plugin.InputFieldPrefab);
    }

    public static TMP_Text CreateSingleText(Transform canvasTransform, string name, string text, Color color)
    {
        return CreateSingleText(canvasTransform, name, text, new Vector2(0f, 1f), canvasTransform.GetComponent<RectTransform>().sizeDelta, color);
    }

    public static TMP_Text CreateSingleText(Transform canvasTransform, string name, string text, Vector2 pivot, Vector2 size, Color color)
    {
        GameObject gameObject = GameObject.Find("MainCanvas").gameObject;
        GameObject gameObject2 = gameObject.transform.Find("AdvancedInfoPanel/primary-content/intro/copy").gameObject;
        GameObject gameObject3 = Instantiate(gameObject2);
        gameObject3.name = "ComfortaaTextPrefab";
        gameObject3.SetActive(true);
        DestroyImmediate(gameObject3.GetComponent<Text>());
        var _comfortaaTextPrefab = gameObject3.AddComponent<TextMeshProUGUI>();
        _comfortaaTextPrefab.fontSize = 13f;
        _comfortaaTextPrefab.text = "DefaultText";
        _comfortaaTextPrefab.font = TMP_FontAsset.CreateFontAsset(gameObject2.GetComponent<Text>().font);

        var start = DateTime.Now;
        _comfortaaTextPrefab.font.fallbackFontAssetTable = Font.GetPathsToOSFonts()
            .Select(path => TMP_FontAsset.CreateFontAsset(new Font(path))).ToList();
        Plugin.Log.LogInfo($"Loaded {_comfortaaTextPrefab.font.fallbackFontAssetTable.Count} fallback fonts in {DateTime.Now - start}");
        _comfortaaTextPrefab.fontMaterial.SetFloat(ShaderUtilities.ID_FaceDilate, 0.25f);
        _comfortaaTextPrefab.fontMaterial.EnableKeyword(ShaderUtilities.Keyword_Outline);
        _comfortaaTextPrefab.fontMaterial.SetFloat(ShaderUtilities.ID_OutlineWidth, 0.25f);
        _comfortaaTextPrefab.fontMaterial.SetColor(ShaderUtilities.ID_OutlineColor, Color.black);
        _comfortaaTextPrefab.alignment = TextAlignmentOptions.Center;
        _comfortaaTextPrefab.GetComponent<RectTransform>().sizeDelta = gameObject3.GetComponent<RectTransform>().sizeDelta;
        _comfortaaTextPrefab.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        _comfortaaTextPrefab.richText = true;
        _comfortaaTextPrefab.enableWordWrapping = false;
        DontDestroyOnLoad(_comfortaaTextPrefab);
        TMP_Text tMP_Text = Instantiate(_comfortaaTextPrefab, canvasTransform);
        tMP_Text.name = name;
        tMP_Text.text = text;
        tMP_Text.color = color;
        tMP_Text.gameObject.GetComponent<RectTransform>().pivot = pivot;
        tMP_Text.gameObject.GetComponent<RectTransform>().sizeDelta = size;
        tMP_Text.enableWordWrapping = true;
        tMP_Text.gameObject.SetActive(true);
        return tMP_Text;
    }
}
