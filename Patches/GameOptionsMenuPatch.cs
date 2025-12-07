using System;
using Il2CppSystem.Collections.Generic;
using HarmonyLib;
using UnityEngine;

using Object = UnityEngine.Object;

// https://github.com/Yumenopai/TownOfHost_Y/blob/main/Patches/GameOptionsPatch.cs
namespace HNSRevamped
{
    public static class ModGameOptionsMenu
    {
        public static int TabIndex = 0;
        public static Il2CppSystem.Collections.Generic.Dictionary<OptionBehaviour, int> OptionList = new();
        public static Il2CppSystem.Collections.Generic.Dictionary<int, OptionBehaviour> BehaviourList = new();
        public static Il2CppSystem.Collections.Generic.Dictionary<int, CategoryHeaderMasked> CategoryHeaderList = new();
    }

    [HarmonyPatch(typeof(GameOptionsMenu))]
    public static class GameOptionsMenuPatch
    {
        [HarmonyPatch(nameof(GameOptionsMenu.Initialize)), HarmonyPrefix]
        private static bool InitializePrefix(GameOptionsMenu __instance)
        {
            if (ModGameOptionsMenu.TabIndex < 3) return true;
            if (__instance.Children == null || __instance.Children.Count == 0)
            {
                __instance.MapPicker.gameObject.SetActive(false);
                __instance.Children = new Il2CppSystem.Collections.Generic.List<OptionBehaviour>();
                __instance.CreateSettings();
                __instance.cachedData = GameOptionsManager.Instance.CurrentGameOptions;
                for (int i = 0; i < __instance.Children.Count; i++)
                {
                    OptionBehaviour optionBehaviour = __instance.Children[i];
                    optionBehaviour.OnValueChanged = new Action<OptionBehaviour>(__instance.ValueChanged);
                }
                __instance.InitializeControllerNavigation();
            }
            return false;
        }
        [HarmonyPatch(nameof(GameOptionsMenu.CreateSettings)), HarmonyPrefix]
        private static bool CreateSettingsPrefix(GameOptionsMenu __instance)
        {
            if (ModGameOptionsMenu.TabIndex < 3) return true;
            var modTab = (TabGroup)(ModGameOptionsMenu.TabIndex - 3);
            float num = 2.0f;
            const float pos_x = 0.952f;
            const float pos_z = -2.0f;
            for (int index = 0; index < OptionItem.AllOptions.Count; index++)
            {
                var option = OptionItem.AllOptions[index];
                if (option.Tab != modTab) continue;

                var enabled = !option.IsHiddenOn() && (option.Parent == null || (!option.Parent.IsHiddenOn() && option.Parent.GetBool()));
                if (option.IsHeader || option is TextOptionItem)
                {
                    CategoryHeaderMasked categoryHeaderMasked = Object.Instantiate(__instance.categoryHeaderOrigin, Vector3.zero, Quaternion.identity, __instance.settingsContainer);
                    categoryHeaderMasked.SetHeader(StringNames.RolesCategory, 20);
                    categoryHeaderMasked.Title.text = option.GetName();
                    categoryHeaderMasked.transform.localScale = Vector3.one * 0.63f;
                    categoryHeaderMasked.transform.localPosition = new Vector3(-0.903f, num, pos_z);
                    categoryHeaderMasked.transform.FindChild("HeaderText").GetComponent<TMPro.TextMeshPro>().fontStyle = TMPro.FontStyles.Bold;
                    categoryHeaderMasked.transform.FindChild("HeaderText").GetComponent<TMPro.TextMeshPro>().outlineWidth = 0.17f;
                    categoryHeaderMasked.gameObject.SetActive(enabled);
                    ModGameOptionsMenu.CategoryHeaderList.TryAdd(index, categoryHeaderMasked);
                    if (enabled) num -= 0.63f;
                }
                if (option is TextOptionItem) continue;
                var baseGameSetting = GetSetting(option);
                if (baseGameSetting == null) continue;

                OptionBehaviour optionBehaviour;
                switch (baseGameSetting.Type)
                {
                    case OptionTypes.Checkbox:
                        {
                            optionBehaviour = Object.Instantiate(__instance.checkboxOrigin, Vector3.zero, Quaternion.identity, __instance.settingsContainer);
                            optionBehaviour.transform.localPosition = new Vector3(pos_x, num, pos_z);
                            OptionBehaviourSetSizeAndPosition(optionBehaviour, option, baseGameSetting.Type);
                            optionBehaviour.SetClickMask(__instance.ButtonClickMask);
                            optionBehaviour.SetUpFromData(baseGameSetting, 20);
                            ModGameOptionsMenu.OptionList.TryAdd(optionBehaviour, index);
                            break;
                        }
                    case OptionTypes.String:
                        {
                            optionBehaviour = Object.Instantiate(__instance.stringOptionOrigin, Vector3.zero, Quaternion.identity, __instance.settingsContainer);
                            optionBehaviour.transform.localPosition = new Vector3(pos_x, num, pos_z);
                            OptionBehaviourSetSizeAndPosition(optionBehaviour, option, baseGameSetting.Type);
                            optionBehaviour.SetClickMask(__instance.ButtonClickMask);
                            optionBehaviour.SetUpFromData(baseGameSetting, 20);
                            ModGameOptionsMenu.OptionList.TryAdd(optionBehaviour, index);
                            break;
                        }
                    case OptionTypes.Float:
                    case OptionTypes.Int:
                        {
                            optionBehaviour = Object.Instantiate(__instance.numberOptionOrigin, Vector3.zero, Quaternion.identity, __instance.settingsContainer);
                            optionBehaviour.transform.localPosition = new Vector3(pos_x, num, pos_z);
                            OptionBehaviourSetSizeAndPosition(optionBehaviour, option, baseGameSetting.Type);
                            optionBehaviour.SetClickMask(__instance.ButtonClickMask);
                            optionBehaviour.SetUpFromData(baseGameSetting, 20);
                            ModGameOptionsMenu.OptionList.TryAdd(optionBehaviour, index);
                            break;
                        }
                    default:
                        continue;
                }
                optionBehaviour.transform.localPosition = new Vector3(0.952f, num, -2f);
                optionBehaviour.SetClickMask(__instance.ButtonClickMask);
                optionBehaviour.SetUpFromData(baseGameSetting, 20);
                ModGameOptionsMenu.OptionList.TryAdd(optionBehaviour, index);
                ModGameOptionsMenu.BehaviourList.TryAdd(index, optionBehaviour);
                optionBehaviour.gameObject.SetActive(enabled);
                __instance.Children.Add(optionBehaviour);
                if (enabled) num -= 0.45f;
            }
            __instance.ControllerSelectable.Clear();
            foreach (var x in __instance.scrollBar.GetComponentsInChildren<UiElement>())
                __instance.ControllerSelectable.Add(x);
            __instance.scrollBar.SetYBoundsMax(-num - 1.65f);
            return false;
        }
        private static void OptionBehaviourSetSizeAndPosition(OptionBehaviour optionBehaviour, OptionItem option, OptionTypes type)
        {
            Vector3 positionOffset = new(0f, 0f, 0f);
            Vector3 scaleOffset = new(0f, 0f, 0f);
            Color color = new(0.7f, 0.7f, 0.7f);
            float sizeDelta_x = 5.7f;
            if (option.Parent?.Parent?.Parent != null)
            {
                scaleOffset = new(-0.18f, 0, 0);
                positionOffset = new(0.3f, 0f, 0f);
                color = new(0.7f, 0.5f, 0.5f);
                sizeDelta_x = 5.1f;
            }
            else if (option.Parent?.Parent != null)
            {
                scaleOffset = new(-0.12f, 0, 0);
                positionOffset = new(0.2f, 0f, 0f);
                color = new(0.5f, 0.5f, 0.7f);
                sizeDelta_x = 5.3f;
            }
            else if (option.Parent != null)
            {
                scaleOffset = new(-0.05f, 0, 0);
                positionOffset = new(0.1f, 0f, 0f);
                color = new(0.5f, 0.7f, 0.5f);
                sizeDelta_x = 5.5f;
            }
            optionBehaviour.transform.FindChild("LabelBackground").GetComponent<SpriteRenderer>().color = color;
            optionBehaviour.transform.FindChild("LabelBackground").localScale += new Vector3(0.9f, -0.2f, 0f) + scaleOffset;
            optionBehaviour.transform.FindChild("LabelBackground").localPosition += new Vector3(-0.4f, 0f, 0f) + positionOffset;
            optionBehaviour.transform.FindChild("Title Text").localPosition += new Vector3(-0.4f, 0f, 0f) + positionOffset; ;
            optionBehaviour.transform.FindChild("Title Text").GetComponent<RectTransform>().sizeDelta = new Vector2(sizeDelta_x, 0.37f);
            optionBehaviour.transform.FindChild("Title Text").GetComponent<TMPro.TextMeshPro>().alignment = TMPro.TextAlignmentOptions.MidlineLeft;
            optionBehaviour.transform.FindChild("Title Text").GetComponent<TMPro.TextMeshPro>().fontStyle = TMPro.FontStyles.Bold;
            optionBehaviour.transform.FindChild("Title Text").GetComponent<TMPro.TextMeshPro>().outlineWidth = 0.17f;
            switch (type)
            {
                case OptionTypes.Checkbox:
                    optionBehaviour.transform.FindChild("Toggle").localPosition = new Vector3(1.46f, -0.042f);
                    break;
                case OptionTypes.String:
                    optionBehaviour.transform.FindChild("PlusButton").localPosition += new Vector3(1.7f, 0f, 0f);
                    optionBehaviour.transform.FindChild("MinusButton").localPosition += new Vector3(0.9f, 0f, 0f);
                    optionBehaviour.transform.FindChild("Value_TMP (1)").localPosition += new Vector3(1.3f, 0f, 0f);
                    optionBehaviour.transform.FindChild("Value_TMP (1)").GetComponent<RectTransform>().sizeDelta = new Vector2(2.3f, 0.4f);
                    goto default;
                case OptionTypes.Float:
                case OptionTypes.Int:
                    optionBehaviour.transform.FindChild("PlusButton").localPosition += new Vector3(1.7f, 0f, 0f);
                    optionBehaviour.transform.FindChild("MinusButton").localPosition += new Vector3(0.9f, 0f, 0f);
                    optionBehaviour.transform.FindChild("Value_TMP").localPosition += new Vector3(1.3f, 0f, 0f);
                    goto default;
                default:
                    optionBehaviour.transform.FindChild("ValueBox").localScale += new Vector3(0.2f, 0f, 0f);
                    optionBehaviour.transform.FindChild("ValueBox").localPosition += new Vector3(1.3f, 0f, 0f);
                    break;
            }
        }

        [HarmonyPatch(nameof(GameOptionsMenu.ValueChanged)), HarmonyPrefix]
        private static bool ValueChangedPrefix(GameOptionsMenu __instance, OptionBehaviour option)
        {
            if (ModGameOptionsMenu.TabIndex < 3) return true;
            if (ModGameOptionsMenu.OptionList.TryGetValue(option, out var index))
            {
                var item = OptionItem.AllOptions[index];
                if (item != null)
                {
                    if (item is BooleanOptionItem)
                        GameManager.Instance.RpcAddCustomSettingsChangeMessage(item, item.GetBool() ? "On" : "Off", true);
                    else
                        GameManager.Instance.RpcAddCustomSettingsChangeMessage(item, item.GetString(), true);
                    if (item.Id == 0)
                    {
                        ReOpenSettingMenu();
                        var viewSettingsPane = Object.FindObjectOfType<LobbyViewSettingsPane>();
                        if (viewSettingsPane != null)
                            LobbyViewPatch.ReCreateButtons(viewSettingsPane);
                    }
                    else if (item.Id == 2)
                    {
                        ReOpenSettingMenu();
                        var viewSettingsPane = Object.FindObjectOfType<LobbyViewSettingsPane>();
                        if (viewSettingsPane != null)
                            LobbyViewPatch.ReCreateButtons(viewSettingsPane);
                    }
                    else if (item.Children.Count > 0)
                        ReCreateSettings(__instance);
                }
            }
            return false;
        }
        private static void ReCreateSettings(GameOptionsMenu __instance)
        {
            if (ModGameOptionsMenu.TabIndex < 3) return;
            var modTab = (TabGroup)(ModGameOptionsMenu.TabIndex - 3);
            float num = 2.0f;
            for (int index = 0; index < OptionItem.AllOptions.Count; index++)
            {
                var option = OptionItem.AllOptions[index];
                if (option.Tab != modTab) continue;
                var enabled = !option.IsHiddenOn() && (option.Parent == null || (!option.Parent.IsHiddenOn() && option.Parent.GetBool()));

                if (ModGameOptionsMenu.CategoryHeaderList.TryGetValue(index, out var categoryHeaderMasked))
                {
                    categoryHeaderMasked.transform.localPosition = new Vector3(-0.903f, num, -2f);
                    categoryHeaderMasked.gameObject.SetActive(enabled);
                    if (enabled) num -= 0.63f;
                }
                if (ModGameOptionsMenu.BehaviourList.TryGetValue(index, out var optionBehaviour))
                {
                    optionBehaviour.transform.localPosition = new Vector3(0.952f, num, -2f);
                    optionBehaviour.gameObject.SetActive(enabled);
                    if (enabled) num -= 0.45f;
                }
            }
            __instance.ControllerSelectable.Clear();
            foreach (var x in __instance.scrollBar.GetComponentsInChildren<UiElement>())
                __instance.ControllerSelectable.Add(x);
            __instance.scrollBar.SetYBoundsMax(-num - 1.65f);
        }

        public static BaseGameSetting GetSetting(OptionItem item)
        {
            BaseGameSetting baseGameSetting = null;
            if (item is BooleanOptionItem)
            {
                baseGameSetting = new CheckboxGameSetting
                {
                    Type = OptionTypes.Checkbox,
                };
            }
            else if (item is IntegerOptionItem)
            {
                IntegerOptionItem intItem = item as IntegerOptionItem;
                baseGameSetting = new IntGameSetting
                {
                    Type = OptionTypes.Int,
                    Value = intItem.GetInt(),
                    Increment = intItem.Rule.Step,
                    ValidRange = new IntRange(intItem.Rule.MinValue, intItem.Rule.MaxValue),
                    ZeroIsInfinity = false,
                    SuffixType = NumberSuffixes.Multiplier,
                    FormatString = string.Empty,
                };
            }
            else if (item is FloatOptionItem)
            {
                FloatOptionItem floatItem = item as FloatOptionItem;
                baseGameSetting = new FloatGameSetting
                {
                    Type = OptionTypes.Float,
                    Value = floatItem.GetFloat(),
                    Increment = floatItem.Rule.Step,
                    ValidRange = new FloatRange(floatItem.Rule.MinValue, floatItem.Rule.MaxValue),
                    ZeroIsInfinity = false,
                    SuffixType = NumberSuffixes.Multiplier,
                    FormatString = string.Empty,
                };
            }
            else if (item is StringOptionItem)
            {
                StringOptionItem stringItem = item as StringOptionItem;
                baseGameSetting = new StringGameSetting
                {
                    Type = OptionTypes.String,
                    Values = new StringNames[stringItem.Selections.Length],
                    Index = stringItem.GetInt(),
                };
            }
            if (baseGameSetting != null)
                baseGameSetting.Title = StringNames.Accept;
            return baseGameSetting;
        }

        public static void ReOpenSettingMenu()
        {
            var tab = ModGameOptionsMenu.TabIndex;
            if (GameSettingMenu.Instance == null) return;
            GameSettingMenu.Instance.Close();
            OptionsConsole optionsConsole = null;
            foreach (var console in Object.FindObjectsOfType<OptionsConsole>())
            {
                if (console.HostOnly)
                    optionsConsole = console;
            }
            if (optionsConsole == null) return;
            GameObject gameObject = Object.Instantiate(optionsConsole.MenuPrefab);
		    gameObject.transform.SetParent(Camera.main.transform, false);
		    gameObject.transform.localPosition = optionsConsole.CustomPosition;
            new LateTask(() => {
		        if (GameSettingMenu.Instance == null) return;
                GameSettingMenu.Instance.ChangeTab(tab, false);
            }, 0f);
        }
    }

    [HarmonyPatch(typeof(ToggleOption))]
    public static class ToggleOptionPatch
    {
        [HarmonyPatch(nameof(ToggleOption.Initialize)), HarmonyPrefix]
        private static bool InitializePrefix(ToggleOption __instance)
        {
            if (ModGameOptionsMenu.OptionList.TryGetValue(__instance, out var index))
            {
                var item = OptionItem.AllOptions[index];
                __instance.TitleText.text = item.GetName();
                __instance.CheckMark.enabled = item.GetBool();
                return false;
            }
            return true;
        }
        [HarmonyPatch(nameof(ToggleOption.Toggle)), HarmonyPrefix]
        private static bool TogglePrefix(ToggleOption __instance)
        {
            if (ModGameOptionsMenu.OptionList.TryGetValue(__instance, out var index))
            {
                __instance.CheckMark.enabled = !__instance.CheckMark.enabled;
                var item = OptionItem.AllOptions[index];
                item.SetValue(__instance.GetBool() ? 1 : 0);
                __instance.OnValueChanged.Invoke(__instance);
                return false;
            }
            return true;
        }
    }
    [HarmonyPatch(typeof(NumberOption))]
    public static class NumberOptionPatch
    {
        [HarmonyPatch(nameof(NumberOption.Initialize)), HarmonyPrefix]
        private static bool InitializePrefix(NumberOption __instance)
        {
            if (ModGameOptionsMenu.OptionList.TryGetValue(__instance, out var index))
            {
                var item = OptionItem.AllOptions[index];
                __instance.TitleText.text = item.GetName();
                return false;
            }
            return true;
        }
        [HarmonyPatch(nameof(NumberOption.UpdateValue)), HarmonyPrefix]
        private static bool UpdateValuePrefix(NumberOption __instance)
        {
            if (ModGameOptionsMenu.OptionList.TryGetValue(__instance, out var index))
            {
                var item = OptionItem.AllOptions[index];
                if (item is IntegerOptionItem integerOptionItem)
                {
                    integerOptionItem.SetValue(integerOptionItem.Rule.GetNearestIndex(__instance.GetInt()));
                }
                else if (item is FloatOptionItem floatOptionItem)
                {
                    floatOptionItem.SetValue(floatOptionItem.Rule.GetNearestIndex(__instance.GetFloat()));
                }
                return false;
            }
            return true;
        }
        [HarmonyPatch(nameof(NumberOption.FixedUpdate)), HarmonyPrefix]
        private static bool FixedUpdatePrefix(NumberOption __instance)
        {
            if (ModGameOptionsMenu.OptionList.TryGetValue(__instance, out var index))
            {
                var item = OptionItem.AllOptions[index];
                if (__instance.oldValue != __instance.Value)
                {
                    __instance.oldValue = __instance.Value;
                    __instance.ValueText.text = item.GetString();
                }
                return false;
            }
            return true;
        }
        [HarmonyPatch(nameof(NumberOption.Increase)), HarmonyPrefix]
        public static bool IncreasePrefix(NumberOption __instance)
        {
            if (__instance.Value == __instance.ValidRange.max)
            {
                __instance.Value = __instance.ValidRange.min;
                __instance.UpdateValue();
                __instance.OnValueChanged.Invoke(__instance);
                return false;
            }
            return true;
        }
        [HarmonyPatch(nameof(NumberOption.Decrease)), HarmonyPrefix]
        public static bool DecreasePrefix(NumberOption __instance)
        {
            if (__instance.Value == __instance.ValidRange.min)
            {
                __instance.Value = __instance.ValidRange.max;
                __instance.UpdateValue();
                __instance.OnValueChanged.Invoke(__instance);
                return false;
            }
            return true;
        }
        [HarmonyPatch(nameof(NumberOption.AdjustButtonsActiveState)), HarmonyPrefix]
        public static bool AdjustButtonsActiveStatePrefix(NumberOption __instance)
        {
            __instance.MinusBtn.SetInteractable(true);
	    	__instance.PlusBtn.SetInteractable(true);
            return false;
        }
    }
    [HarmonyPatch(typeof(StringOption))]
    public static class StringOptionPatch
    {
        [HarmonyPatch(nameof(StringOption.Initialize)), HarmonyPrefix]
        private static bool InitializePrefix(StringOption __instance)
        {
            if (ModGameOptionsMenu.OptionList.TryGetValue(__instance, out var index))
            {
                var item = OptionItem.AllOptions[index];
                __instance.TitleText.text = item.GetName();
                return false;
            }
            return true;
        }
        [HarmonyPatch(nameof(StringOption.UpdateValue)), HarmonyPrefix]
        private static bool UpdateValuePrefix(StringOption __instance)
        {
            if (ModGameOptionsMenu.OptionList.TryGetValue(__instance, out var index))
            {
                var item = OptionItem.AllOptions[index];
                item.SetValue(__instance.GetInt());
                return false;
            }
            return true;
        }
        [HarmonyPatch(nameof(StringOption.FixedUpdate)), HarmonyPrefix]
        private static bool FixedUpdatePrefix(StringOption __instance)
        {
            if (ModGameOptionsMenu.OptionList.TryGetValue(__instance, out var index))
            {
                var item = OptionItem.AllOptions[index];
                if (__instance.oldValue != __instance.Value)
                {
                    __instance.oldValue = __instance.Value;
                    __instance.ValueText.text = item.GetString();
                }
                return false;
            }
            return true;
        }
        [HarmonyPatch(nameof(StringOption.Increase)), HarmonyPrefix]
        public static bool IncreasePrefix(StringOption __instance)
        {
            if (__instance.Value == __instance.Values.Length - 1)
            {
                __instance.Value = 0;
                __instance.UpdateValue();
                __instance.OnValueChanged.Invoke(__instance);
                return false;
            }
            return true;
        }
        [HarmonyPatch(nameof(StringOption.Decrease)), HarmonyPrefix]
        public static bool DecreasePrefix(StringOption __instance)
        {
            if (__instance.Value == 0)
            {
                __instance.Value = __instance.Values.Length - 1;
                __instance.UpdateValue();
                __instance.OnValueChanged.Invoke(__instance);
                return false;
            }
            return true;
        }
        [HarmonyPatch(nameof(StringOption.AdjustButtonsActiveState)), HarmonyPrefix]
        public static bool AdjustButtonsActiveStatePrefix(StringOption __instance)
        {
            __instance.MinusBtn.SetInteractable(true);
    		__instance.PlusBtn.SetInteractable(true);
            return false;
        }
    }
    [HarmonyPatch(typeof(PlayerOption))]
    public static class PlayerOptionPatch
    {
        [HarmonyPatch(nameof(PlayerOption.Increase)), HarmonyPrefix]
        public static bool IncreasePrefix(PlayerOption __instance)
        {
            if (__instance.Value == __instance.Values.Count - 1)
            {
                __instance.Value = -1;
                __instance.UpdateValue();
                __instance.OnValueChanged.Invoke(__instance);
                return false;
            }
            return true;
        }
        [HarmonyPatch(nameof(PlayerOption.Decrease)), HarmonyPrefix]
        public static bool DecreasePrefix(PlayerOption __instance)
        {
            if (__instance.Value < 0)
            {
                __instance.Value = __instance.Values.Count - 1;
                __instance.UpdateValue();
                __instance.OnValueChanged.Invoke(__instance);
                return false;
            }
            return true;
        }
        [HarmonyPatch(nameof(PlayerOption.AdjustButtonsActiveState)), HarmonyPrefix]
        public static bool AdjustButtonsActiveStatePrefix(PlayerOption __instance)
        {
            __instance.MinusBtn.SetInteractable(true);
	    	__instance.PlusBtn.SetInteractable(true);
            return false;
        }
    }
}