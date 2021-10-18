using System.Collections.Generic;
using System.Reflection;

namespace NeosTweaks.Settings
{
    static class SettingsManager
    {
        public static List<SettingsPanel> SettingsPanels = new List<SettingsPanel>();

        /// <summary>
        /// Retrieves a setting by its nice name.
        /// </summary>
        public static Setting GetSettingByName<T>(string Name) where T : SettingsPanel
        {
            foreach (SettingsPanel settingsPanel in SettingsPanels)
            {
                if (settingsPanel is T)
                {
                    foreach (Setting setting in settingsPanel.Settings)
                    {
                        if (setting.Name == Name)
                        {
                            return setting;
                        }
                    }
                }
            }

            return null;
        }
        /// <summary>
        /// Retrieves a setting by its literal name.
        /// </summary>
        public static Setting GetSettingByLiteralName<T>(string Name) where T : SettingsPanel
        {
            foreach (SettingsPanel settingsPanel in SettingsPanels)
            {
                if (settingsPanel is T)
                {
                    foreach (FieldInfo fieldInfo in settingsPanel.RawSettings)
                    {
                        if (fieldInfo.Name == Name)
                        {
                            return (Setting)fieldInfo.GetValue(settingsPanel);
                        }
                    }
                }
            }

            return null;
        }
    }
}
