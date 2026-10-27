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

        public static bool overlapEnabled = false;

        public static Keybinding splitTracks = new ("M");
        public override void ApplySettings()
        {
            overlapEnabled = MQSettings.Instance.enableOverlap;
            splitTracks = MQSettings.Instance.splitTracks;
            base.ApplySettings();
            
        }
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

            [SettingsLabelKey("wtwhtdfn.setting.keybinds")]
            public Keybinding splitTracks = new ("M");
        }

    }
}
