using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;

namespace HomeGenie
{
    public class AppSettings
    {
        private IsolatedStorageSettings settings;

        public AppSettings()
        {
            settings = IsolatedStorageSettings.ApplicationSettings;
        }


        /// <summary>
        /// Update a setting value for our application. If the setting does not
        /// exist, then add the setting.
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool AddOrUpdateValue(string Key, Object value)
        {
            bool valueChanged = false;

            // If the key exists
            if (settings.Contains(Key))
            {
                // If the value has changed
                if (settings[Key] != value)
                {
                    // Store the new value
                    settings[Key] = value;
                    valueChanged = true;
                }
            }
            // Otherwise create the key.
            else
            {
                settings.Add(Key, value);
                valueChanged = true;
            }
            return valueChanged;
        }

        /// <summary>
        /// Get the current value of the setting, or if it is not found, set the 
        /// setting to the default setting.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public T GetValueOrDefault<T>(string Key, T defaultValue)
        {
            T value;

            // If the key exists, retrieve the value.
            if (settings.Contains(Key))
            {
                value = (T)settings[Key];
            }
            // Otherwise, use the default value.
            else
            {
                value = defaultValue;
            }
            return value;
        }

        /// <summary>
        /// Save the settings.
        /// </summary>
        public void Save()
        {
            settings.Save();
        }

        /// <summary>
        /// Property to get and set Server Setting Key.
        /// </summary>
        public string RemoteServerAddress
        {
            get
            {
                return GetValueOrDefault<string>("RemoteServerAddress", "127.0.0.1:80");
            }
            set
            {
                if (AddOrUpdateValue("RemoteServerAddress", value))
                {
                    Save();
                }
            }
        }

        /// <summary>
        /// Property to get and set a Port Setting Key.
        /// </summary>
        public string RemoteServerUsername
        {
            get
            {
                return GetValueOrDefault<string>("RemoteServerUsername", "");
            }
            set
            {
                if (AddOrUpdateValue("RemoteServerUsername", value))
                {
                    Save();
                }
            }
        }
        /// <summary>
        /// Property to get and set a Password Setting Key.
        /// </summary>
        public string RemoteServerPassword
        {
            get
            {
                return GetValueOrDefault<string>("RemoteServerPassword", "");
            }
            set
            {
                if (AddOrUpdateValue("RemoteServerPassword", value))
                {
                    Save();
                }
            }
        }
        /// <summary>
        /// Property to get and set a Password Setting Key.
        /// </summary>
        public string RemoteServerUpdateInterval
        {
            get
            {
                return GetValueOrDefault<string>("RemoteServerUpdateInterval", "30");
            }
            set
            {
                if (AddOrUpdateValue("RemoteServerUpdateInterval", value))
                {
                    Save();
                }
            }
        }
        /// <summary>
        /// Property to get and set a Password Setting Key.
        /// </summary>
        public bool EnableNotifications
        {
            get
            {
                return GetValueOrDefault<bool>("EnableNotifications", false);
            }
            set
            {
                if (AddOrUpdateValue("EnableNotifications", value))
                {
                    Save();
                }
            }
        }

    }
}
