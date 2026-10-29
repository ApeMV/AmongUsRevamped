using System;
using TMPro;
using UnityEngine;

// https://github.com/Rabek009/MoreGamemodes/blob/master/Patches/LobbyViewSettingsPatch.cs
namespace AmongUsRevamped
{
    [HarmonyPatch(typeof(LobbyViewSettingsPane))]
    public class LobbyViewPatch
    {
        private static Dictionary<TabGroup, GameObject> ViewSettingsButtons = new();
        
        [HarmonyPatch(nameof(LobbyViewSettingsPane.Awake))]
        [HarmonyPostfix]
        public static void AwakePostfix(LobbyViewSettingsPane __instance)
        {
            __instance.gameModeText.DestroyTranslator();
            ViewSettingsButtons = new();
        }

        [HarmonyPatch(nameof(LobbyViewSettingsPane.ChangeTab))]
        [HarmonyPrefix]
        public static bool ChangeTabPrefix(LobbyViewSettingsPane __instance, [HarmonyArgument(0)] StringNames category)
        {
            if (category == StringNames.OverviewCategory || category == StringNames.RolesCategory)
            {
                foreach (var button in ViewSettingsButtons.Values)
                    button.GetComponent<PassiveButton>().SelectButton(false);
                return true;
            }
            __instance.currentTab = category;
		    for (int i = 0; i < __instance.settingsInfo.Count; i++)
		    {
			    Object.Destroy(__instance.settingsInfo[i].gameObject);
		    }
		    __instance.settingsInfo.Clear();
            __instance.taskTabButton.SelectButton(false);
		    __instance.rolesTabButton.SelectButton(false);
            foreach (var tab in ViewSettingsButtons.Keys)
            {
                if (tab == (TabGroup)__instance.currentTab)
                    ViewSettingsButtons[tab].GetComponent<PassiveButton>().SelectButton(true);
                else
                    ViewSettingsButtons[tab].GetComponent<PassiveButton>().SelectButton(false);
            }
            DrawOptions(__instance, (TabGroup)__instance.currentTab);
            __instance.scrollBar.ScrollToTop();
            return false;
        }

        public static void DrawOptions(LobbyViewSettingsPane __instance, TabGroup tab)
        {
            float num = 1.44f;
            int settingsCount = 0;
            foreach (var option in OptionItem.AllOptions)
            {
                if (option.IsHiddenOn() || option.Tab != tab || (option.Parent != null && (option.Parent.IsHiddenOn() || !option.Parent.GetBool()))) continue;
                if (option.IsHeader || option is TextOptionItem)
                {
                    num -= 0.85f;
                    CategoryHeaderMasked categoryHeaderMasked = Object.Instantiate(__instance.categoryHeaderOrigin);
			        categoryHeaderMasked.SetHeader(StringNames.None, 61);
			        categoryHeaderMasked.transform.SetParent(__instance.settingsContainer);
			        categoryHeaderMasked.transform.localScale = Vector3.one;
			        categoryHeaderMasked.transform.localPosition = new Vector3(-9.77f, num, -2f);
			        __instance.settingsInfo.Add(categoryHeaderMasked.gameObject);
			        num -= 1f;
                    categoryHeaderMasked.DestroyTranslator();
                    categoryHeaderMasked.Title.text = option.GetName();
                    settingsCount = 0;
                }
                if (option is TextOptionItem) continue;

                ViewSettingsInfoPanel viewSettingsInfoPanel = Object.Instantiate(__instance.infoPanelOrigin);
				viewSettingsInfoPanel.transform.SetParent(__instance.settingsContainer);
				viewSettingsInfoPanel.transform.localScale = Vector3.one;
				float num2;
				if (settingsCount % 2 == 0)
				{
					num2 = -8.95f;
					if (settingsCount > 0)
					{
						num -= 0.85f;
					}
				}
				else
				{
					num2 = -3f;
				}
				viewSettingsInfoPanel.transform.localPosition = new Vector3(num2, num, -2f);

                if (option is BooleanOptionItem)
                {
                    viewSettingsInfoPanel.SetInfoCheckbox(StringNames.None, 61, option.GetBool());
                }
                else
                {
                    viewSettingsInfoPanel.SetInfo(StringNames.None, option.GetString(), 61);
                }
                viewSettingsInfoPanel.titleText.text = option.GetName();

                if (option.Parent?.Parent?.Parent != null)
                {
                    viewSettingsInfoPanel.transform.FindChild("LabelBackground").GetComponent<SpriteRenderer>().color = new(0.7f, 0.5f, 0.5f);
                }
                else if (option.Parent?.Parent != null)
                {
                    viewSettingsInfoPanel.transform.FindChild("LabelBackground").GetComponent<SpriteRenderer>().color = new(0.5f, 0.5f, 0.7f);
                }
                else if (option.Parent != null)
                {
                    viewSettingsInfoPanel.transform.FindChild("LabelBackground").GetComponent<SpriteRenderer>().color = new(0.5f, 0.7f, 0.5f);
                }

                __instance.settingsInfo.Add(viewSettingsInfoPanel.gameObject);
                ++settingsCount;
            }
            __instance.scrollBar.SetYBoundsMax(-num - 1.44f);
        }

        public static void ReCreateButtons(LobbyViewSettingsPane __instance)
        {
            foreach (var button in ViewSettingsButtons.Values)
                Object.Destroy(button.gameObject);
            ViewSettingsButtons = new();
        }

        [HarmonyPatch(nameof(LobbyViewSettingsPane.OnEnable))]
        [HarmonyPostfix]
        public static void OnEnablePostfix(LobbyViewSettingsPane __instance)
        {
            ReCreateButtons(__instance);
        }
    }
}
