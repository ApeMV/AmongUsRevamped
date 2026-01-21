using System;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

// https://github.com/Yumenopai/TownOfHost_Y/blob/main/Patches/GameSettingMenuPatch.cs
namespace AmongUsRevamped
{
    [HarmonyPatch(typeof(GameSettingMenu))]
    public class GameSettingMenuPatch
    {
        private static GameOptionsMenu templateGameOptionsMenu;
        private static PassiveButton templateGameSettingsButton;
        static Dictionary<TabGroup, PassiveButton> ModSettingsButtons = new();
        static Dictionary<TabGroup, GameOptionsMenu> ModSettingsTabs = new();

        [HarmonyPatch(nameof(GameSettingMenu.Start)), HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        public static void StartPostfix(GameSettingMenu __instance)
        {
            ModSettingsButtons = new();
            int tabNum = 0;
            foreach (var tab in Enum.GetValues<TabGroup>())
            {

                var button = Object.Instantiate(templateGameSettingsButton, __instance.GameSettingsButton.transform.parent);
                button.gameObject.SetActive(true);
                button.name = "Button_" + Utils.GetTabName(tab);
                var label = button.GetComponentInChildren<TextMeshPro>();
                label.DestroyTranslator();
                label.text = Utils.GetTabName(tab);

                Vector3 offset = new (0.0f, 0.5f * (tabNum / 2), 0.0f);
                button.transform.localPosition = ((tabNum % 2 == 0) ? new Vector3(-3.9f, -0.9f, 0f) : new Vector3(-2.4f, -0.9f, 0f)) - offset;
                button.transform.localScale = new(0.45f, 0.6f, 1f);

                var buttonComponent = button.GetComponent<PassiveButton>();
                buttonComponent.OnClick = new();
                buttonComponent.OnClick.AddListener((Action)(() => __instance.ChangeTab((int)tab + 3, false)));
                ModSettingsButtons.Add(tab, button);
                ++tabNum;
            }

            ModGameOptionsMenu.OptionList = new();
            ModGameOptionsMenu.BehaviourList = new();
            ModGameOptionsMenu.CategoryHeaderList = new();

            ModSettingsTabs = new();
            foreach (var tab in Enum.GetValues<TabGroup>())
            {
                var setTab = Object.Instantiate(templateGameOptionsMenu, __instance.GameSettingsTab.transform.parent);
                setTab.name = Utils.GetTabName(tab);
                setTab.gameObject.SetActive(false);
                ModSettingsTabs.Add(tab, setTab);
            }
            foreach (var tab in Enum.GetValues<TabGroup>())
            {
                if (ModSettingsButtons.TryGetValue(tab, out var button))
                    __instance.ControllerSelectable.Add(button);
            }
        }
        private static void SetDefaultButton(GameSettingMenu __instance)
        {
            var gamepreset = __instance.GamePresetsButton;
            gamepreset.gameObject.SetActive(false);

            var gamesettings = __instance.GameSettingsButton;
            gamesettings.transform.localScale = new Vector3(0.45f, 0.6f, 1f);
            gamesettings.transform.localPosition = new Vector3(-3.9f, -0.4f, 0f);

            var rolesettings = __instance.RoleSettingsButton;
            rolesettings.transform.localScale = new Vector3(0.45f, 0.6f, 1f);
            rolesettings.transform.localPosition = new(-2.4f, -0.4f, 0f);
            var label = rolesettings.GetComponentInChildren<TextMeshPro>();
            label.DestroyTranslator();
            label.text = "Vanilla Roles";
        }

        [HarmonyPatch(nameof(GameSettingMenu.ChangeTab)), HarmonyPrefix]
        public static bool ChangeTabPrefix(GameSettingMenu __instance, ref int tabNum, [HarmonyArgument(1)] bool previewOnly)
        {
            if (!previewOnly)
                ModGameOptionsMenu.TabIndex = tabNum;
            GameOptionsMenu settingsTab;
            PassiveButton button;

            if ((previewOnly && Controller.currentTouchType == Controller.TouchType.Joystick) || !previewOnly)
            {
                foreach (var tab in Enum.GetValues<TabGroup>())
                {
                    if (ModSettingsTabs.TryGetValue(tab, out settingsTab) && settingsTab != null)
                        settingsTab.gameObject.SetActive(false);
                }
                foreach (var tab in Enum.GetValues<TabGroup>())
                {
                    if (ModSettingsButtons.TryGetValue(tab, out button) && button != null)
                        button.SelectButton(false);
                }
            }
            if (tabNum < 3) return true;
            if ((previewOnly && Controller.currentTouchType == Controller.TouchType.Joystick) || !previewOnly)
            {
                __instance.PresetsTab.gameObject.SetActive(false);
                __instance.GameSettingsTab.gameObject.SetActive(false);
                __instance.RoleSettingsTab.gameObject.SetActive(false);
                __instance.GamePresetsButton.SelectButton(false);
                __instance.GameSettingsButton.SelectButton(false);
                __instance.RoleSettingsButton.SelectButton(false);

                if (ModSettingsTabs.TryGetValue((TabGroup)(tabNum - 3), out settingsTab) && settingsTab != null)
                {
                    settingsTab.gameObject.SetActive(true);
                    __instance.MenuDescriptionText.DestroyTranslator();
                    switch ((TabGroup)(tabNum - 3))
                    {
                        case TabGroup.SystemSettings:
                            __instance.MenuDescriptionText.text = "Quality settings to improve comfort and testing.";
                            break;
                        case TabGroup.CustomRoleSettings:
                            __instance.MenuDescriptionText.text = "Custom Role enabling and settings.";
                            break;
                        case TabGroup.ModSettings:
                            __instance.MenuDescriptionText.text = "Gameplay Settings to improve the in-game experience.";
                            break;
                        case TabGroup.GamemodeSettings:
                            __instance.MenuDescriptionText.text = "Situational settings to apply for specific gamemodes.";
                            break;
                    }
                }
            }
            if (previewOnly)
            {
                __instance.ToggleLeftSideDarkener(false);
                __instance.ToggleRightSideDarkener(true);
                return false;
            }
            __instance.ToggleLeftSideDarkener(true);
            __instance.ToggleRightSideDarkener(false);
            if (ModSettingsButtons.TryGetValue((TabGroup)(tabNum - 3), out button) && button != null)
                button.SelectButton(true);
            return false;
        }

        [HarmonyPatch(nameof(GameSettingMenu.OnEnable)), HarmonyPrefix]
        private static bool OnEnablePrefix(GameSettingMenu __instance)
        {
            if (templateGameOptionsMenu == null)
            {
                templateGameOptionsMenu = Object.Instantiate(__instance.GameSettingsTab, __instance.GameSettingsTab.transform.parent);
                templateGameOptionsMenu.gameObject.SetActive(false);
            }
            if (templateGameSettingsButton == null)
            {
                templateGameSettingsButton = Object.Instantiate(__instance.GameSettingsButton, __instance.GameSettingsButton.transform.parent);
                templateGameSettingsButton.gameObject.SetActive(false);
            }
            SetDefaultButton(__instance);
            ControllerManager.Instance.OpenOverlayMenu(__instance.name, __instance.BackButton, __instance.DefaultButtonSelected, __instance.ControllerSelectable, false);
            DestroyableSingleton<HudManager>.Instance.menuNavigationPrompts.SetActive(false);
            if (Controller.currentTouchType != Controller.TouchType.Joystick)
                __instance.ChangeTab(1, Controller.currentTouchType == Controller.TouchType.Joystick);
            __instance.StartCoroutine(__instance.CoSelectDefault());
            return false;
        }

        [HarmonyPatch(nameof(GameSettingMenu.Close)), HarmonyPostfix]
        private static void ClosePostfix(GameSettingMenu __instance)
        {
            foreach (var button in ModSettingsButtons.Values)
                Object.Destroy(button);
            foreach (var tab in ModSettingsTabs.Values)
                Object.Destroy(tab);
            ModSettingsButtons = new();
            ModSettingsTabs = new();
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSyncSettings))]
    public class RpcSyncSettingsPatch
    {
        public static void Postfix()
        {
            OptionItem.SyncAllOptions();
        }
    }
}