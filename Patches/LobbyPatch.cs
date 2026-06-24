using Il2CppSystem.Collections;
using InnerNet;
using System;
using UnityEngine;

// https://github.com/SuperNewRoles/SuperNewRoles/blob/master/SuperNewRoles/Patches/LobbyBehaviourPatch.cs
namespace AmongUsRevamped;

[HarmonyPatch(typeof(LobbyBehaviour), nameof(LobbyBehaviour.Update))]
internal static class LobbyBehaviourUpdatePatch
{
    public static void Postfix(LobbyBehaviour __instance)
    {
        // ReSharper disable once ConvertToLocalFunction
        Func<ISoundPlayer, bool> lobbybgm = x => x.Name.Equals("MapTheme");
        ISoundPlayer mapThemeSound = SoundManager.Instance.soundPlayers.Find(lobbybgm);

        if (!Main.LobbyMusic.Value)
        {
            if (mapThemeSound == null) return;
            SoundManager.Instance.StopNamedSound("MapTheme");
        }
        else
        {
            if (mapThemeSound != null) return;
            SoundManager.Instance.CrossFadeSound("MapTheme", __instance.MapTheme, 0.1f);
        }
    }
}

[HarmonyPatch(typeof(LobbyBehaviour), nameof(LobbyBehaviour.Start))]
public class LobbyStartPatch
{
    private static GameObject LobbyPaintObject;
    private static Sprite LobbyPaintSprite;

    public static void Prefix()
    {
        LobbyPaintSprite = Utils.LoadSprite("AmongUsRevamped.Resources.Images.doger.png", 290f);
    }

    public static void Postfix(LobbyBehaviour __instance)
    {
        _ = new LateTask(() =>
        {
            var LeftBox = GameObject.Find("Leftbox");
            if (LeftBox != null)
            {
                LobbyPaintObject = UnityEngine.Object.Instantiate(LeftBox, LeftBox.transform.parent);
                LobbyPaintObject.name = "Lobby Paint";
                LobbyPaintObject.transform.localPosition = new Vector3(0.042f, -2.59f, -10.5f);

                SpriteRenderer renderer = LobbyPaintObject.GetComponent<SpriteRenderer>();
                renderer.sprite = LobbyPaintSprite;
            }
        }, 0.25f, "Co Load Dropship Decorations");
    }
}