using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AmongUsRevamped;

[HarmonyPatch(typeof(AmongUsDateTime), nameof(AmongUsDateTime.UtcNow), MethodType.Getter)]
public static class AmongUsDateTime_UtcNow
{
    public static bool Prefix(ref Il2CppSystem.DateTime __result)
    {
        if (!CreateOptionsPickerPatch.SetDleks) return true;

        var managedDate = new DateTime(DateTime.UtcNow.Year, 4, 2, 7, 1, 0, DateTimeKind.Utc);
        __result = new Il2CppSystem.DateTime(managedDate.Ticks);
            
        return false;
    }
}

class CreateOptionsPickerPatch
{
    public static bool SetDleks = false;
    public static bool SetDleks2;
    private static MapSelectButton DleksButton;
    [HarmonyPatch]
    public static class GameOptionsMapPickerPatch
    {
        [HarmonyPatch(typeof(GameOptionsMapPicker), nameof(GameOptionsMapPicker.SetupMapButtons))]
        [HarmonyPostfix]
        public static void Postfix_Initialize(CreateGameMapPicker __instance)
        {
            if (SceneManager.GetActiveScene().name == "FindAGame") return;
            if (SetDleks2 && SceneManager.GetActiveScene().name == "MainMenu") return;

            new LateTask(() =>
            {
            SetDleks2 = true;
            int DleksPos = 3;
            MapSelectButton[] AllMapButton = __instance.transform.GetComponentsInChildren<MapSelectButton>();

            if (AllMapButton != null)
            {
                GameObject dlekS_ehT = UnityEngine.Object.Instantiate(AllMapButton[0].gameObject, __instance.transform);
                dlekS_ehT.transform.position = AllMapButton[DleksPos].transform.position;
                dlekS_ehT.transform.SetSiblingIndex(DleksPos + 2);
                MapSelectButton dlekS_ehT_MapButton = dlekS_ehT.GetComponent<MapSelectButton>();
                DleksButton = dlekS_ehT_MapButton;
                foreach (var icon in dlekS_ehT_MapButton.MapIcon)
                {
                    if (icon == null || icon.transform == null) continue;
                    icon.flipX = true;
                }
                dlekS_ehT_MapButton.Button.OnClick.RemoveAllListeners();
                dlekS_ehT_MapButton.Button.OnClick.AddListener((UnityEngine.Events.UnityAction)(() =>
                {
                    __instance.SelectMap(__instance.AllMapIcons[0]);

                    if (__instance.selectedButton)
                    {
                        __instance.selectedButton.Button.SelectButton(false);
                    }
                    __instance.selectedButton = dlekS_ehT_MapButton;
                    __instance.selectedButton.Button.SelectButton(true);
                    __instance.selectedMapId = 3;

                    if (!Utils.isHideNSeek)
                        Main.NormalOptions.MapId = 0;
                    else if (Utils.isHideNSeek)
                        Main.HideNSeekOptions.MapId = 0;

                    __instance.MapImage.sprite = Utils.LoadSprite($"AmongUsRevamped.Resources.Images.DleksBanner.png", 100f);
                    __instance.MapName.sprite = Utils.LoadSprite($"AmongUsRevamped.Resources.Images.DleksBanner-Wordart.png", 100f);
                }));

                for (int i = DleksPos; i < AllMapButton.Length; i++)
                {
                    AllMapButton[i].transform.localPosition += new Vector3(0.625f, 0f, 0f);
                }

                if (DleksButton != null)
                {
                    if (SetDleks)
                    {
                        if (__instance.selectedButton)
                        {
                            __instance.selectedButton.Button.SelectButton(false);
                        }
                        DleksButton.Button.SelectButton(true);
                        __instance.selectedButton = DleksButton;
                        __instance.selectedMapId = 3;

                        __instance.MapImage.sprite = Utils.LoadSprite($"AmongUsRevamped.Resources.Images.DleksBanner.png", 100f);
                        __instance.MapName.sprite = Utils.LoadSprite($"AmongUsRevamped.Resources.Images.DleksBanner-Wordart.png", 100f);
                    }
                    else
                    {
                        DleksButton.Button.SelectButton(false);
                    }
                }
            }

            }, 0.2f, "ApplyDleks");
        }

        [HarmonyPatch(typeof(GameOptionsMapPicker), nameof(GameOptionsMapPicker.FixedUpdate))]
        [HarmonyPrefix]
        public static bool Prefix_FixedUpdate(GameOptionsMapPicker __instance)
        {
            if (__instance == null) return true;

            if (__instance.MapName == null) return false;

            if (DleksButton != null) SetDleks = __instance.selectedMapId == 3;

            if (__instance.selectedMapId == 3)
            {
                if (SceneManager.GetActiveScene().name == "FindAGame")
                {
                    __instance.SelectMap(0);
                    SetDleks = false;
                }
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(CreateOptionsPicker), nameof(CreateOptionsPicker.Awake))]
    class MenuMapPickerPatch
    {
        public static void Postfix(CreateOptionsPicker __instance)
        {
            Transform mapPickerTransform = __instance.transform.Find("MapPicker");
            MapPickerMenu mapPickerMenu = mapPickerTransform.Find("Map Picker Menu").GetComponent<MapPickerMenu>();

            MapFilterButton airhipIconInMenu = __instance.MapMenu.MapButtons[3];
            MapFilterButton fungleIconInMenu = __instance.MapMenu.MapButtons[4];
            MapFilterButton skeldIconInMenu = __instance.MapMenu.MapButtons[0];
            MapFilterButton dleksIconInMenuCopy = UnityEngine.Object.Instantiate(airhipIconInMenu, airhipIconInMenu.transform.parent);

            Transform skeldMenuButton = mapPickerMenu.transform.Find("Skeld");
            Transform polusMenuButton = mapPickerMenu.transform.Find("Polus");
            Transform airshipMenuButton = mapPickerMenu.transform.Find("Airship");
            Transform fungleMenuButton = mapPickerMenu.transform.Find("Fungle");
            Transform dleksMenuButtonCopy = UnityEngine.Object.Instantiate(airshipMenuButton, airshipMenuButton.parent);

            // Set mapid for Dleks button
            PassiveButton dleksButton = dleksMenuButtonCopy.GetComponent<PassiveButton>();
            dleksButton.OnClick.m_PersistentCalls.m_Calls._items[0].arguments.intArgument = (int)MapNames.Dleks;

            SpriteRenderer dleksImage = dleksMenuButtonCopy.Find("Image").GetComponent<SpriteRenderer>();
            dleksImage.sprite = skeldMenuButton.Find("Image").GetComponent<SpriteRenderer>().sprite;

            dleksIconInMenuCopy.name = "Dleks";
            dleksIconInMenuCopy.transform.localPosition = new Vector3(0.8f, airhipIconInMenu.transform.localPosition.y, airhipIconInMenu.transform.localPosition.z);
            dleksIconInMenuCopy.MapId = MapNames.Dleks;
            dleksIconInMenuCopy.Button = dleksButton;
            dleksIconInMenuCopy.ButtonCheck = dleksMenuButtonCopy.Find("selectedCheck").GetComponent<SpriteRenderer>();
            dleksIconInMenuCopy.ButtonImage = dleksImage;
            dleksIconInMenuCopy.ButtonOutline = dleksImage.transform.parent.GetComponent<SpriteRenderer>();
            dleksIconInMenuCopy.Icon.sprite = skeldIconInMenu.Icon.sprite;

            dleksMenuButtonCopy.name = "Dleks";
            dleksMenuButtonCopy.position = new Vector3(dleksMenuButtonCopy.position.x, 2f * dleksMenuButtonCopy.position.y - polusMenuButton.transform.position.y, dleksMenuButtonCopy.position.z);
            fungleMenuButton.position = new Vector3(fungleMenuButton.position.x, dleksMenuButtonCopy.transform.position.y - 0.6f, fungleMenuButton.position.z);

            __instance.MapMenu.MapButtons = HarmonyLib.CollectionExtensions.AddItem(__instance.MapMenu.MapButtons, dleksIconInMenuCopy).ToArray();

            float xPos = -1f;
            for (int index = 0; index < 6; ++index)
            {
                __instance.MapMenu.MapButtons[index].transform.SetLocalX(xPos);
                xPos += 0.34f;
            }

            if (__instance.mode == SettingsMode.Host)
            {
                mapPickerMenu.transform.localPosition = new Vector3(mapPickerMenu.transform.localPosition.x, 0.85f, mapPickerMenu.transform.localPosition.z);

                mapPickerTransform.localScale = new Vector3(0.86f, 0.85f, 1f);
                mapPickerTransform.transform.localPosition = new Vector3(mapPickerTransform.transform.localPosition.x + 0.05f, mapPickerTransform.transform.localPosition.y + 0.03f, mapPickerTransform.transform.localPosition.z);
            }

            SwapIconOrButtomsPositions(airhipIconInMenu, dleksIconInMenuCopy);
            SwapIconOrButtomsPositions(fungleIconInMenu, airhipIconInMenu);

            SwapIconOrButtomsPositions(airshipMenuButton, dleksMenuButtonCopy);

            __instance.MapMenu.MapButtons[5].SetFlipped(true);

            mapPickerMenu.transform.Find("Backdrop").localScale *= 5;
        }
        private static void SwapIconOrButtomsPositions(Component one, Component two)
        {
            Transform transform1 = one.transform;
            Transform transform2 = two.transform;
            Vector3 position1 = two.transform.position;
            Vector3 position2 = one.transform.position;
            transform1.position = position1;
            transform2.position = position2;
        }
    }
}