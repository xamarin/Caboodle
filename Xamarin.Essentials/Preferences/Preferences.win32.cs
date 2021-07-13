using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using PreferencesDictionary = System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, object>>;

namespace Xamarin.Essentials
{
    public static partial class Preferences
    {
        static readonly string settingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), GetCleanAppName(), "settings.dat");

        static readonly PreferencesDictionary preferences = new PreferencesDictionary();
        static readonly DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(PreferencesDictionary));

        static Preferences()
        {
            if (File.Exists(settingsPath))
            {
                using (var stream = File.OpenRead(settingsPath))
                {
                    try
                    {
                        var readPreferences = (PreferencesDictionary)serializer.ReadObject(stream);

                        if (readPreferences != null)
                        {
                            preferences = readPreferences;
                        }
                    }
                    catch (SerializationException)
                    {
                        // if deserialization fails proceed with empty settings
                    }
                }
            }
            else
            {
                if (!Directory.Exists(Path.GetDirectoryName(settingsPath)))
                {
                    // create folder for app settings
                    Directory.CreateDirectory(Path.GetDirectoryName(settingsPath));
                }
            }

            if (!preferences.ContainsKey(string.Empty))
            {
                preferences.Add(string.Empty, new Dictionary<string, object>());
            }
        }

        static string GetCleanAppName()
        {
            var appName = AppInfo.Name;

            if (string.IsNullOrEmpty(appName))
            {
                return Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().CodeBase);
            }

            return string.Concat(appName.Split(Path.GetInvalidFileNameChars()));
        }

        static void Save()
        {
            using (var stream = File.OpenWrite(settingsPath))
            {
                serializer.WriteObject(stream, preferences);
            }
        }

        static string CleanSharedName(string sharedName)
        {
            if (string.IsNullOrEmpty(sharedName))
            {
                return string.Empty;
            }

            return sharedName;
        }

        static bool PlatformContainsKey(string key, string sharedName)
        {
            if (preferences.TryGetValue(CleanSharedName(sharedName), out var inner))
            {
                return inner.ContainsKey(key);
            }

            return false;
        }

        static void PlatformRemove(string key, string sharedName)
        {
            if (preferences.TryGetValue(CleanSharedName(sharedName), out var inner))
            {
                inner.Remove(key);
                Save();
            }
        }

        static void PlatformClear(string sharedName)
        {
            if (preferences.TryGetValue(CleanSharedName(sharedName), out var inner))
            {
                inner.Clear();
                Save();
            }
        }

        static void PlatformSet<T>(string key, T value, string sharedName)
        {
            if (!preferences.TryGetValue(CleanSharedName(sharedName), out var inner))
            {
                inner = new Dictionary<string, object>();
                preferences.Add(CleanSharedName(sharedName), inner);
            }

            inner[key] = value;

            Save();
        }

        static T PlatformGet<T>(string key, T defaultValue, string sharedName)
        {
            if (preferences.TryGetValue(CleanSharedName(sharedName), out var inner))
            {
                if (inner.TryGetValue(key, out var value))
                {
                    return (T)value;
                }
            }

            return defaultValue;
        }
    }
}
