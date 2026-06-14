using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace Dreamy.DataConfig.Editor
{
    public static class DataConfigEditorMenu
    {
        private const string ConfigFolder = "Assets/Resources/DataConfig";
        private const string JsonSearchPattern = "*.json";

        [MenuItem("Tools/Dreamy/Data Config/Open Editor")]
        public static void OpenEditor()
        {
            DataConfigWindow.Open();
        }

        [MenuItem("Tools/Dreamy/Data Config/Validate All")]
        public static void ValidateAll()
        {
            if (!Directory.Exists(ConfigFolder))
            {
                Debug.LogWarning(
                    $"Dreamy DataConfig: folder does not exist: {ConfigFolder}");
                return;
            }

            string[] paths = Directory.GetFiles(
                ConfigFolder,
                JsonSearchPattern,
                SearchOption.AllDirectories);

            int errorCount = 0;
            foreach (string path in paths)
            {
                try
                {
                    ValidateFile(path);
                }
                catch (Exception exception)
                {
                    errorCount++;
                    Debug.LogError(
                        $"Dreamy DataConfig: {path}: {exception.Message}");
                }
            }

            if (errorCount == 0)
            {
                Debug.Log(
                    $"Dreamy DataConfig: validated {paths.Length} JSON file(s).");
            }
            else
            {
                Debug.LogError(
                    $"Dreamy DataConfig: validation failed with {errorCount} error(s).");
            }
        }

        [MenuItem("Tools/Dreamy/Data Config/Open Config Folder")]
        public static void OpenConfigFolder()
        {
            Directory.CreateDirectory(ConfigFolder);
            AssetDatabase.Refresh();

            UnityEngine.Object folder =
                AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(ConfigFolder);
            Selection.activeObject = folder;
            EditorGUIUtility.PingObject(folder);
        }

        [MenuItem("Tools/Dreamy/Data Config/Create Missing JSON")]
        public static void CreateMissingJson()
        {
            Directory.CreateDirectory(ConfigFolder);

            int createdCount = 0;
            int skippedCount = 0;
            foreach (Type type in FindConfigTypes())
            {
                ConfigBase config;
                try
                {
                    config = Activator.CreateInstance(type) as ConfigBase;
                }
                catch (Exception exception)
                {
                    Debug.LogError(
                        $"Dreamy DataConfig: cannot create {type.FullName}: " +
                        exception.Message);
                    continue;
                }

                if (config == null)
                {
                    continue;
                }

                string path = Path.Combine(
                    ConfigFolder,
                    config.DocumentName + ".json");
                if (File.Exists(path))
                {
                    skippedCount++;
                    continue;
                }

                string json = JsonConvert.SerializeObject(
                    config,
                    Formatting.Indented,
                    DataConfigJson.Settings);
                File.WriteAllText(path, json);
                createdCount++;
            }

            AssetDatabase.Refresh();
            Debug.Log(
                $"Dreamy DataConfig: created {createdCount} JSON file(s), " +
                $"skipped {skippedCount} existing file(s).");
        }

        internal static void ValidateFile(string path)
        {
            ValidateJson(File.ReadAllText(path));
        }

        internal static void ValidateJson(string json)
        {
            JObject root = JObject.Parse(json);
            JToken rowsToken = root["rows"];
            if (rowsToken == null)
            {
                return;
            }

            if (rowsToken is not JArray rows)
            {
                throw new JsonException("'rows' must be an array.");
            }

            HashSet<string> ids = new(StringComparer.Ordinal);
            for (int index = 0; index < rows.Count; index++)
            {
                string id = rows[index]?["id"]?.Value<string>();
                if (string.IsNullOrWhiteSpace(id))
                {
                    throw new JsonException(
                        $"Row {index} has a missing or empty 'id'.");
                }

                if (!ids.Add(id))
                {
                    throw new JsonException(
                        $"Row {index} contains duplicate id '{id}'.");
                }
            }
        }

        private static IEnumerable<Type> FindConfigTypes()
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in GetLoadableTypes(assembly))
                {
                    if (!type.IsAbstract &&
                        !type.IsGenericTypeDefinition &&
                        (type.IsPublic || type.IsNestedPublic) &&
                        typeof(ConfigBase).IsAssignableFrom(type))
                    {
                        yield return type;
                    }
                }
            }
        }

        private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException exception)
            {
                return exception.Types.Where(type => type != null);
            }
        }
    }
}
