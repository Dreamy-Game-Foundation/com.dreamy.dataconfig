using System;
using System.Collections.Generic;
using System.IO;
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

        private static void ValidateFile(string path)
        {
            JObject root = JObject.Parse(File.ReadAllText(path));
            JToken rowsToken = root["rows"];
            if (rowsToken is not JArray rows)
            {
                throw new JsonException("Required array 'rows' is missing.");
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
    }
}
