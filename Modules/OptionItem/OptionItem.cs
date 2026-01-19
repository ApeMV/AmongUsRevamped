using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using UnityEngine;

// https://github.com/tukasa0001/TownOfHost/blob/main/Modules/OptionItems/OptionItem.cs
namespace AmongUsRevamped
{
    public abstract class OptionItem
    {
        public static IReadOnlyList<OptionItem> AllOptions => _allOptions;
        private static List<OptionItem> _allOptions = new();
        public static int CurrentPreset { get; set; }

        public int Id { get; }
        public string Name { get; }
        public int DefaultValue { get; }
        public TabGroup Tab { get; }
        public bool IsSingleValue { get; }

        public Color NameColor { get; protected set; }
        public OptionFormat ValueFormat { get; protected set; }
        public bool IsHeader { get; protected set; }
        public bool IsHidden { get; protected set; }
        public bool IsText { get; protected set; }
        public Dictionary<string, string> ReplacementDictionary
        {
            get => _replacementDictionary;
            set
            {
                if (value == null) _replacementDictionary?.Clear();
                else _replacementDictionary = value;
            }
        }
        private Dictionary<string, string> _replacementDictionary;

        public int CurrentValue
        {
            get => GetValue();
            set => SetValue(value);
        }

        public OptionItem Parent { get; private set; }
        public List<OptionItem> Children;

        public ConfigEntry<int> CurrentEntry =>
            IsSingleValue ? singleEntry : AllConfigEntries[CurrentPreset];
        private ConfigEntry<int>[] AllConfigEntries;
        private ConfigEntry<int> singleEntry;

        public OptionBehaviour OptionBehaviour;

        public event EventHandler<UpdateValueEventArgs> UpdateValueEvent;

        public OptionItem(int id, string name, int defaultValue, TabGroup tab, bool isSingleValue)
        {
            Id = id;
            Name = name;
            DefaultValue = defaultValue;
            Tab = tab;
            IsSingleValue = isSingleValue;

            NameColor = Color.white;
            ValueFormat = OptionFormat.None;
            IsHeader = false;
            IsHidden = false;
            IsText = false;

            Children = new();

            AllConfigEntries = new ConfigEntry<int>[10];
            if (Id == 0)
            {
                singleEntry = Main.Instance.Config.Bind("Current Preset", id.ToString(), DefaultValue);
                CurrentPreset = singleEntry.Value;
            }
            else if (IsSingleValue)
            {
                singleEntry = Main.Instance.Config.Bind("SingleEntryOptions", id.ToString(), DefaultValue);
            }
            else
            {
                for (int i = 0; i < 10; i++)
                {
                    AllConfigEntries[i] = Main.Instance.Config.Bind($"Preset{i + 1}", id.ToString(), DefaultValue);
                }
            }

            _allOptions.Add(this);
        }

        public OptionItem Do(Action<OptionItem> action)
        {
            action(this);
            return this;
        }

        public OptionItem SetColor(Color value) => Do(i => i.NameColor = value);
        public OptionItem SetValueFormat(OptionFormat value) => Do(i => i.ValueFormat = value);
        public OptionItem SetHeader(bool value) => Do(i => i.IsHeader = value);
        public OptionItem SetHidden(bool value) => Do(i => i.IsHidden = value);
        public OptionItem SetText(bool value) => Do(i => i.IsHidden = value);

        public OptionItem SetParent(OptionItem parent) => Do(i =>
        {
            i.Parent = parent;
            parent.SetChild(i);
        });
        public OptionItem SetChild(OptionItem child) => Do(i => i.Children.Add(child));
        public OptionItem RegisterUpdateValueEvent(EventHandler<UpdateValueEventArgs> handler)
            => Do(i => UpdateValueEvent += handler);

        public OptionItem AddReplacement((string key, string value) kvp)
            => Do(i =>
            {
                ReplacementDictionary ??= new();
                ReplacementDictionary.Add(kvp.key, kvp.value);
            });
        public OptionItem RemoveReplacement(string key)
            => Do(i => ReplacementDictionary?.Remove(key));

        public virtual string GetName(bool disableColor = false)
        {
            return disableColor ?
                Name :
                Utils.ColorString(NameColor, Name);
        }
        public virtual bool GetBool() => CurrentValue != 0 && (Parent == null || Parent.GetBool());
        public virtual int GetInt() => CurrentValue;
        public virtual float GetFloat() => CurrentValue;
        public virtual string GetString()
        {
            return ApplyFormat(CurrentValue.ToString());
        }
        public virtual int GetValue() => CurrentEntry.Value;

        public virtual bool IsHiddenOn()
        {
            return IsHidden;
        }

        public string ApplyFormat(string value)
        {
            if (ValueFormat == OptionFormat.Players) return value + " players";
            if (ValueFormat == OptionFormat.Seconds) return value + "s";
            if (ValueFormat == OptionFormat.Percent) return value + "%";
            if (ValueFormat == OptionFormat.Multiplier) return value + "x";
            return value;
        }

        public virtual void Refresh()
        {
            if (OptionBehaviour is not null and StringOption opt)
            {
                opt.TitleText.text = GetName();
                opt.ValueText.text = GetString();
                opt.oldValue = opt.Value = CurrentValue;
            }
        }
        public virtual void SetValue(int value)
        {
            int beforeValue = CurrentEntry.Value;
            int afterValue = CurrentEntry.Value = value;

            CallUpdateValueEvent(beforeValue, afterValue);
            Refresh();
            SyncAllOptions();
        }

        public static OptionItem operator ++(OptionItem item)
            => item.Do(item => item.SetValue(item.CurrentValue + 1));
        public static OptionItem operator --(OptionItem item)
            => item.Do(item => item.SetValue(item.CurrentValue - 1));

        public static void SwitchPreset(int newPreset)
        {
            CurrentPreset = Math.Clamp(newPreset, 0, 4);

            foreach (var op in AllOptions)
                op.Refresh();

            SyncAllOptions();
        }
        public static void SyncAllOptions()
        {
            if (
                PlayerControl.AllPlayerControls.Count <= 1 ||
                AmongUsClient.Instance.AmHost == false ||
                PlayerControl.LocalPlayer == null
            ) return;

            GameManager.Instance.RpcSyncCustomOptions();
        }


        private void CallUpdateValueEvent(int beforeValue, int currentValue)
        {
            if (UpdateValueEvent == null) return;
            UpdateValueEvent(this, new UpdateValueEventArgs(beforeValue, currentValue));
        }

        public class UpdateValueEventArgs : EventArgs
        {
            public int CurrentValue { get; set; }
            public int BeforeValue { get; set; }
            public UpdateValueEventArgs(int beforeValue, int currentValue)
            {
                CurrentValue = currentValue;
                BeforeValue = beforeValue;
            }
        }
    }

    public enum TabGroup
    {
        SystemSettings,
        ModSettings,
        GamemodeSettings,

    }
    
    public enum OptionFormat
    {
        None,
        Players,
        Seconds,
        Percent,
        Multiplier,
        Level,
        Times,
    }
}