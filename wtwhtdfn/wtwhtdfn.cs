using Quintessential;
using Quintessential.Settings;
using System;

namespace wtwhtdfn
{
    internal class wtwhtdfn : QuintessentialMod, IDataMod
    {
        public static wtwhtdfn Instance { get; }

        
        public override string ModId => "wtwhtdfn";

        public override void Load() { }

        public override void LoadContent()
        {

            Logger.Log("am i sane load");
        }
        public override void LoadCompatContent() { }
        public override void FinaliseContent() { }

        public override void PostLoad() { }
        public override void Unload() { }

        public override Type SettingsType => typeof(MQSettings);

        public class MQSettings
        {
            public static MQSettings Instance => wtwhtdfn.Instance.Settings as MQSettings;
            [SettingsLabelKey("wtwhtdfn.settings.enableOverlap")]
            public bool enableOverlap = false;

            [SettingsLabelKey("wtwhtdfn.settings.disableUnique")]
            public bool disableUnique = false;

            [SettingsLabelKey("wtwhtdfn.settings.keybinds.disjointSplit")]
            public Keybinding disjointSplit = new ("\\");

            [SettingsLabelKey("wtwhtdfn.settings.keybinds.regularSplit")]
            public Keybinding regularSplit = new("/");

            [SettingsLabelKey("wtwhtdfn.settings.keybinds.reverseTrack")]
            public Keybinding mirrorTrack = new(";");
            [SettingsLabelKey("wtwhtdfn.settings.keybinds.mergeTrack")]
            public Keybinding mergeTrack = new("'");
        }


        public static bool HexSide(HexIndex a, HexIndex b)
        {
            int x = a.Q - b.Q;
            int y = a.R - b.R;
            return Math.Abs(x) <= 1 && Math.Abs(y) <= 1 && x != y;
        }
    }
}
