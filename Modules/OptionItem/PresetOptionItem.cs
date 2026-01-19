// https://github.com/tukasa0001/TownOfHost/blob/main/Modules/OptionItems/PresetOptionItem.cs

namespace AmongUsRevamped
{
    public class PresetOptionItem : OptionItem
    {
        public IntegerValueRule Rule;

        public PresetOptionItem(int defaultValue, TabGroup tab)
        : base(0, "Preset", defaultValue, tab, true)
        {
            Rule = (0, 4, 1);
        }
        public static PresetOptionItem Create(int defaultValue, TabGroup tab)
        {
            return new PresetOptionItem(defaultValue, tab);
        }

        public override int GetInt() => Rule.GetValueByIndex(CurrentValue);
        public override float GetFloat() => Rule.GetValueByIndex(CurrentValue);
        public override string GetString()
        {
            return CurrentValue switch
            {   
                0 => Main.Preset1.Value == (string)Main.Preset1.DefaultValue ? "Preset 1" : Main.Preset1.Value,
                1 => Main.Preset2.Value == (string)Main.Preset2.DefaultValue ? "Preset 2" : Main.Preset2.Value,
                2 => Main.Preset3.Value == (string)Main.Preset3.DefaultValue ? "Preset 3" : Main.Preset3.Value,
                3 => Main.Preset4.Value == (string)Main.Preset4.DefaultValue ? "Preset 4" : Main.Preset4.Value,
                4 => Main.Preset5.Value == (string)Main.Preset5.DefaultValue ? "Preset 5" : Main.Preset5.Value,
                _ => null,
            };
        }
        public override int GetValue()
            => Rule.RepeatIndex(base.GetValue());

        public override void SetValue(int value)
        {
            base.SetValue(Rule.RepeatIndex(value));
            SwitchPreset(Rule.RepeatIndex(value));
        }
    }
}