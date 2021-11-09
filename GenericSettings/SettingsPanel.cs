using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using FrooxEngine;

namespace NeosTweaks.Settings
{
    abstract class SettingsPanel
    {
        /// <summary>
        /// The displayed name of this settings panel.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// The cloud variable Neos Bot command that's needed to make the saving of this panel functional in its entirety.
        /// </summary>
        public abstract string CloudVariableCommand { get; }

        /// <summary>
        /// Specifies if the settings need to be refreshed in a special way.
        /// </summary>
        public virtual bool RefreshableSettings { get; }

        /// <summary>
        /// The string used for saving and loading settings.
        /// </summary>
        string CloudVariableDirectory { get; }

        /// <summary>
        /// The settings in their fancy form.
        /// </summary>
        public List<Setting> Settings { get; }

        /// <summary>
        /// The settings as their raw FieldInfo form with possible attribute information.
        /// </summary>
        public List<FieldInfo> RawSettings { get; }

        /// <summary>
        /// A centralized list of every settings panel currently being used.
        /// </summary>
        public static readonly List<SettingsPanel> SettingsPanels = new List<SettingsPanel>();

        static SettingsPanel()
        {
            IEnumerable<Type> AssemblySettingsPanels = AppDomain.CurrentDomain.GetAssemblies().SelectMany(assemblies => assemblies.GetTypes());

            foreach (Type SettingsPanelType in AppDomain.CurrentDomain.GetAssemblies().
                SelectMany(assemblies => assemblies.GetTypes()).OrderBy(o => o.Name).
                Where(assembly => assembly.IsSubclassOf(typeof(SettingsPanel))))
            {
                SettingsPanels.Add((SettingsPanel)Activator.CreateInstance(SettingsPanelType));
            }
        }

        public SettingsPanel()
        {
            Settings = new List<Setting>();
            RawSettings = new List<FieldInfo>();

            FieldInfo[] Fields = GetType().GetFields();

            foreach (FieldInfo fieldInfo in Fields)
            {
                if (fieldInfo.FieldType == typeof(Setting))
                {
                    Setting setting = (Setting)fieldInfo.GetValue(this);

                    Settings.Add(setting);
                    RawSettings.Add(fieldInfo);
                }
            }

            string[] items = CloudVariableCommand.Split(' ');
            CloudVariableDirectory = Engine.Current.Cloud.CurrentUser.Id + items[1].Insert(0, ".");

            try
            {
                LoadSettings();
            }
            catch
            {
                try
                {
                    SaveSettings();
                }
                catch
                {
                }
            }
        }

        /// <summary>
        /// Converts Settings to a simplified dictionary and uploads it to the cloud as a json object.
        /// </summary>
        public Dictionary<string, object> SaveSettings()
        {
            Dictionary<string, object> SerializableSettings = new Dictionary<string, object>();

            foreach (FieldInfo fieldInfo in RawSettings)
            {
                Setting setting = (Setting)fieldInfo.GetValue(this);

                SerializableSettings.Add(fieldInfo.Name, setting.Value);
            }

            Engine.Current.Cloud.WriteVariable(CloudVariableDirectory, JsonConvert.SerializeObject(SerializableSettings));

            return SerializableSettings;
        }
        /// <summary>
        /// Converts Settings to a simplified dictionary and returns it.
        /// </summary>
        public Dictionary<string, object> GetSettings()
        {
            Dictionary<string, object> SerializableSettings = new Dictionary<string, object>();

            foreach (FieldInfo fieldInfo in RawSettings)
            {
                Setting setting = (Setting)fieldInfo.GetValue(this);

                SerializableSettings.Add(fieldInfo.Name, setting.Value);
            }

            return SerializableSettings;
        }
        /// <summary>
        /// Deserializes the simplified dictionary form of Settings from the cloud and updates the current Settings.
        /// </summary>
        public Dictionary<string, object> LoadSettings()
        {
            Dictionary<string, object> SerializableSettings = JsonConvert.DeserializeObject<Dictionary<string, object>>(Engine.Current.Cloud.ReadVariable<string>(CloudVariableDirectory).Result.Entity);

            foreach (FieldInfo fieldInfo in RawSettings)
            {
                if (SerializableSettings.TryGetValue(fieldInfo.Name, out object Value))
                {
                    Setting setting = (Setting)fieldInfo.GetValue(this);

                    if (setting.Value.GetType() == Value.GetType())
                    {
                        setting.Value = Value;

                        if (setting.LinkedField != null)
                        {
                            setting.LinkedField.BoxedValue = Value;
                        }
                    }
                }
            }

            if (RefreshableSettings)
            {
                OnRefreshSettings();
            }

            return SerializableSettings;
        }
        /// <summary>
        /// Deserializes the given simplified dictionary form of Settings and updates the current Settings.
        /// </summary>
        public void LoadSettings(Dictionary<string, object> SerializableSettings)
        {
            foreach (FieldInfo fieldInfo in RawSettings)
            {
                if (SerializableSettings.TryGetValue(fieldInfo.Name, out object Value))
                {
                    Setting setting = (Setting)fieldInfo.GetValue(this);

                    if (setting.Value.GetType() == Value.GetType())
                    {
                        setting.Value = Value;

                        if (setting.LinkedField != null)
                        {
                            setting.LinkedField.BoxedValue = Value;
                        }
                    }
                }
            }

            if (RefreshableSettings)
            {
                OnRefreshSettings();
            }
        }
        /// <summary>
        /// A method for special case settings that can't be easily changed otherwise.
        /// </summary>
        public virtual void OnRefreshSettings()
        {

        }

        /// <summary>
        /// Retrieves a setting by its nice name.
        /// </summary>
        public Setting GetSettingByName(string Name)
        {
            foreach (Setting setting in Settings)
            {
                if (setting.Name == Name)
                {
                    return setting;
                }
            }

            return null;
        }
        /// <summary>
        /// Retrieves a setting by its literal name.
        /// </summary>
        public Setting GetSettingByLiteralName(string Name)
        {
            foreach (FieldInfo fieldInfo in RawSettings)
            {
                if (fieldInfo.Name == Name)
                {
                    return (Setting)fieldInfo.GetValue(this);
                }
            }

            return null;
        }
        /// <summary>
        /// Retrieves a setting by its nice name from the current list.
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
        /// Retrieves a setting by its literal name from the current list.
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

        public void SaveSettings_FieldHandler(IField field)
        {
            if ((bool)field.BoxedValue)
            {
                SaveSettings();
            }
        }
        public void LoadSettings_FieldHandler(IField field)
        {
            if ((bool)field.BoxedValue)
            {
                LoadSettings();
            }
        }
        public void RefreshSettings_FieldHandler(IField field)
        {
            if ((bool)field.BoxedValue)
            {
                OnRefreshSettings();
            }
        }
    }
}
