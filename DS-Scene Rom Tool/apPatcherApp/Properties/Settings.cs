namespace apPatcherApp.Properties
{
    using System;
    using System.CodeDom.Compiler;
    using System.Configuration;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    [CompilerGenerated, GeneratedCode("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "10.0.0.0")]
    internal sealed class Settings : ApplicationSettingsBase
    {
        private static Settings defaultInstance = ((Settings) SettingsBase.Synchronized(new Settings()));

        [DebuggerNonUserCode, DefaultSettingValue("0"), UserScopedSetting]
        public int Collection_Col1
        {
            get { return ((int) this["Collection_Col1"]); }
            set
            {
                this["Collection_Col1"] = value;
            }
        }

        [DefaultSettingValue("0"), UserScopedSetting, DebuggerNonUserCode]
        public int Collection_Col2
        {
            get { return ((int) this["Collection_Col2"]); }
            set
            {
                this["Collection_Col2"] = value;
            }
        }

        [DebuggerNonUserCode, DefaultSettingValue("0"), UserScopedSetting]
        public int Collection_Col3
        {
            get { return ((int) this["Collection_Col3"]); }
            set
            {
                this["Collection_Col3"] = value;
            }
        }

        [DefaultSettingValue("0"), UserScopedSetting, DebuggerNonUserCode]
        public int Collection_Col4
        {
            get { return ((int) this["Collection_Col4"]); }
            set
            {
                this["Collection_Col4"] = value;
            }
        }

        [DebuggerNonUserCode, DefaultSettingValue("0"), UserScopedSetting]
        public int Collection_H
        {
            get { return ((int) this["Collection_H"]); }
            set
            {
                this["Collection_H"] = value;
            }
        }

        [UserScopedSetting, DefaultSettingValue("0"), DebuggerNonUserCode]
        public int Collection_W
        {
            get { return ((int) this["Collection_W"]); }
            set
            {
                this["Collection_W"] = value;
            }
        }

        public static Settings Default =>
            defaultInstance;

        [DebuggerNonUserCode, UserScopedSetting, DefaultSettingValue("")]
        public string NFOSize
        {
            get { return ((string) this["NFOSize"]); }
            set
            {
                this["NFOSize"] = value;
            }
        }

        [DefaultSettingValue(""), UserScopedSetting, DebuggerNonUserCode]
        public string NFOTheme
        {
            get { return ((string) this["NFOTheme"]); }
            set
            {
                this["NFOTheme"] = value;
            }
        }
    }
}

