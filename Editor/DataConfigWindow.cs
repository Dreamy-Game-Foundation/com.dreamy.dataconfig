using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace Dreamy.DataConfig.Editor
{
    public sealed class DataConfigWindow : EditorWindow
    {
        private const string WindowTitle = "Data Config";
        private const string FavoritePathsKey =
            "Dreamy.DataConfig.Editor.FavoritePaths";
        private const float SidebarWidth = 260f;
        private const float RowHeight = 22f;

        private readonly List<string> jsonPaths = new();
        private readonly HashSet<string> favoritePaths =
            new(StringComparer.Ordinal);

        private Vector2 fileScroll;
        private Vector2 contentScroll;
        private string searchText = string.Empty;
        private string selectedPath;
        private string textContent = string.Empty;
        private JObject visualDocument;
        private ViewMode viewMode;
        private bool isDirty;
        private string statusMessage = string.Empty;

        public static void Open()
        {
            DataConfigWindow window = GetWindow<DataConfigWindow>();
            window.titleContent = new GUIContent(WindowTitle);
            window.minSize = new Vector2(800f, 420f);
            window.Show();
        }

        private void OnEnable()
        {
            LoadFavorites();
            RefreshFiles();
        }

        private void OnGUI()
        {
            DrawToolbar();

            EditorGUILayout.BeginHorizontal();
            DrawSidebar();
            DrawContent();
            EditorGUILayout.EndHorizontal();

            DrawStatusBar();
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            if (GUILayout.Button("Refresh", EditorStyles.toolbarButton))
            {
                RefreshFiles();
            }

            using (new EditorGUI.DisabledScope(string.IsNullOrEmpty(selectedPath)))
            {
                if (GUILayout.Button("Save", EditorStyles.toolbarButton))
                {
                    SaveSelectedFile();
                }
            }

            GUILayout.Space(12f);
            viewMode = (ViewMode)GUILayout.Toolbar(
                (int)viewMode,
                new[] { "Text", "Table" },
                EditorStyles.toolbarButton,
                GUILayout.Width(130f));

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawSidebar()
        {
            EditorGUILayout.BeginVertical(
                GUILayout.Width(SidebarWidth),
                GUILayout.ExpandHeight(true));

            searchText = EditorGUILayout.TextField(
                searchText,
                EditorStyles.toolbarSearchField);

            fileScroll = EditorGUILayout.BeginScrollView(fileScroll);
            foreach (string path in FilteredPaths())
            {
                DrawFileRow(path);
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawFileRow(string path)
        {
            EditorGUILayout.BeginHorizontal(GUILayout.Height(RowHeight));

            bool isFavorite = favoritePaths.Contains(path);
            bool nextFavorite = GUILayout.Toggle(
                isFavorite,
                isFavorite ? "★" : "☆",
                EditorStyles.miniButton,
                GUILayout.Width(28f));
            if (nextFavorite != isFavorite)
            {
                SetFavorite(path, nextFavorite);
            }

            string label = Path.GetFileNameWithoutExtension(path);
            GUIStyle style = string.Equals(
                selectedPath,
                path,
                StringComparison.Ordinal)
                ? EditorStyles.miniButtonMid
                : EditorStyles.label;

            if (GUILayout.Button(
                new GUIContent(label, path),
                style,
                GUILayout.ExpandWidth(true)))
            {
                SelectFile(path);
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawContent()
        {
            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));

            if (string.IsNullOrEmpty(selectedPath))
            {
                EditorGUILayout.HelpBox(
                    "Select a JSON config file.",
                    MessageType.Info);
                EditorGUILayout.EndVertical();
                return;
            }

            EditorGUILayout.LabelField(selectedPath, EditorStyles.miniLabel);
            contentScroll = EditorGUILayout.BeginScrollView(contentScroll);

            if (viewMode == ViewMode.Text)
            {
                DrawTextView();
            }
            else
            {
                DrawTableView();
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawTextView()
        {
            EditorGUI.BeginChangeCheck();
            textContent = EditorGUILayout.TextArea(
                textContent,
                GUILayout.ExpandHeight(true));
            if (EditorGUI.EndChangeCheck())
            {
                SetDirty(true);
                TryParseVisualDocument();
            }
        }

        private void DrawTableView()
        {
            if (visualDocument == null)
            {
                EditorGUILayout.HelpBox(
                    "JSON must be valid before using Table view.",
                    MessageType.Error);
                return;
            }

            if (visualDocument["rows"] is not JArray rows)
            {
                DrawObjectView();
                return;
            }

            List<string> columns = CollectColumns(rows);
            DrawTableHeader(columns);

            for (int index = 0; index < rows.Count; index++)
            {
                if (rows[index] is JObject row)
                {
                    DrawTableRow(rows, row, index, columns);
                }
            }

            if (GUILayout.Button("Add Row", GUILayout.Width(100f)))
            {
                JObject row = new();
                foreach (string column in columns)
                {
                    row[column] = string.Empty;
                }

                rows.Add(row);
                MarkVisualChanged();
            }
        }

        private void DrawObjectView()
        {
            List<JProperty> properties =
                visualDocument.Properties().ToList();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(
                "Property",
                EditorStyles.boldLabel,
                GUILayout.Width(180f));
            EditorGUILayout.LabelField("Value", EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();

            foreach (JProperty property in properties)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(
                    property.Name,
                    GUILayout.Width(180f));

                string value = TokenToEditorText(property.Value);
                EditorGUI.BeginChangeCheck();
                string nextValue = EditorGUILayout.TextField(value);
                if (EditorGUI.EndChangeCheck())
                {
                    property.Value = ParseEditorValue(
                        nextValue,
                        property.Value);
                    MarkVisualChanged();
                }

                EditorGUILayout.EndHorizontal();
            }
        }

        private static void DrawTableHeader(IReadOnlyList<string> columns)
        {
            EditorGUILayout.BeginHorizontal();
            foreach (string column in columns)
            {
                EditorGUILayout.LabelField(
                    column,
                    EditorStyles.boldLabel,
                    GUILayout.MinWidth(100f));
            }

            GUILayout.Space(54f);
            EditorGUILayout.EndHorizontal();
        }

        private void DrawTableRow(
            JArray rows,
            JObject row,
            int rowIndex,
            IReadOnlyList<string> columns)
        {
            EditorGUILayout.BeginHorizontal();
            foreach (string column in columns)
            {
                string value = TokenToEditorText(row[column]);
                EditorGUI.BeginChangeCheck();
                string nextValue = EditorGUILayout.TextField(
                    value,
                    GUILayout.MinWidth(100f));
                if (EditorGUI.EndChangeCheck())
                {
                    row[column] = ParseEditorValue(nextValue, row[column]);
                    MarkVisualChanged();
                }
            }

            if (GUILayout.Button("Delete", GUILayout.Width(54f)))
            {
                rows.RemoveAt(rowIndex);
                MarkVisualChanged();
                GUIUtility.ExitGUI();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawStatusBar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            EditorGUILayout.LabelField(
                isDirty ? "Modified" : "Saved",
                GUILayout.Width(60f));
            EditorGUILayout.LabelField(statusMessage);
            EditorGUILayout.EndHorizontal();
        }

        private void RefreshFiles()
        {
            jsonPaths.Clear();
            string[] guids = AssetDatabase.FindAssets("t:TextAsset");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                {
                    jsonPaths.Add(path);
                }
            }

            jsonPaths.Sort(ComparePaths);
            Repaint();
        }

        private IEnumerable<string> FilteredPaths()
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                return jsonPaths;
            }

            return jsonPaths.Where(path =>
                path.IndexOf(
                    searchText,
                    StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private void SelectFile(string path)
        {
            if (isDirty &&
                !EditorUtility.DisplayDialog(
                    WindowTitle,
                    "Discard unsaved changes?",
                    "Discard",
                    "Cancel"))
            {
                return;
            }

            selectedPath = path;
            textContent = File.ReadAllText(path);
            SetDirty(false);
            statusMessage = string.Empty;
            TryParseVisualDocument();
        }

        private void SaveSelectedFile()
        {
            try
            {
                string json = viewMode == ViewMode.Table &&
                              visualDocument != null
                    ? visualDocument.ToString(Formatting.Indented)
                    : textContent;

                DataConfigEditorMenu.ValidateJson(json);
                File.WriteAllText(selectedPath, json);
                textContent = json;
                visualDocument = JObject.Parse(json);
                SetDirty(false);
                statusMessage = "Saved successfully.";
                AssetDatabase.ImportAsset(selectedPath);
            }
            catch (Exception exception)
            {
                statusMessage = exception.Message;
                Debug.LogException(exception);
            }
        }

        private void TryParseVisualDocument()
        {
            try
            {
                visualDocument = JObject.Parse(textContent);
                statusMessage = string.Empty;
            }
            catch (JsonException exception)
            {
                visualDocument = null;
                statusMessage = exception.Message;
            }
        }

        private void MarkVisualChanged()
        {
            textContent = visualDocument.ToString(Formatting.Indented);
            SetDirty(true);
        }

        private void SetDirty(bool value)
        {
            isDirty = value;
            hasUnsavedChanges = value;
        }

        private void SetFavorite(string path, bool isFavorite)
        {
            if (isFavorite)
            {
                favoritePaths.Add(path);
            }
            else
            {
                favoritePaths.Remove(path);
            }

            SaveFavorites();
            jsonPaths.Sort(ComparePaths);
        }

        private int ComparePaths(string left, string right)
        {
            int favoriteComparison = favoritePaths.Contains(right)
                .CompareTo(favoritePaths.Contains(left));
            return favoriteComparison != 0
                ? favoriteComparison
                : string.Compare(left, right, StringComparison.OrdinalIgnoreCase);
        }

        private void LoadFavorites()
        {
            favoritePaths.Clear();
            string value = EditorPrefs.GetString(FavoritePathsKey, string.Empty);
            foreach (string path in value.Split(
                new[] { '\n' },
                StringSplitOptions.RemoveEmptyEntries))
            {
                favoritePaths.Add(path);
            }
        }

        private void SaveFavorites()
        {
            EditorPrefs.SetString(
                FavoritePathsKey,
                string.Join("\n", favoritePaths));
        }

        private static List<string> CollectColumns(JArray rows)
        {
            List<string> columns = new();
            foreach (JObject row in rows.OfType<JObject>())
            {
                foreach (JProperty property in row.Properties())
                {
                    if (!columns.Contains(property.Name))
                    {
                        columns.Add(property.Name);
                    }
                }
            }

            if (columns.Count == 0)
            {
                columns.Add("id");
            }

            return columns;
        }

        private static string TokenToEditorText(JToken token)
        {
            if (token == null || token.Type == JTokenType.Null)
            {
                return string.Empty;
            }

            return token is JValue value
                ? Convert.ToString(value.Value)
                : token.ToString(Formatting.None);
        }

        private static JToken ParseEditorValue(
            string value,
            JToken previousToken)
        {
            if (previousToken?.Type == JTokenType.Integer &&
                long.TryParse(value, out long integer))
            {
                return integer;
            }

            if (previousToken?.Type == JTokenType.Float &&
                double.TryParse(value, out double number))
            {
                return number;
            }

            if (previousToken?.Type == JTokenType.Boolean &&
                bool.TryParse(value, out bool boolean))
            {
                return boolean;
            }

            if (previousToken is JObject || previousToken is JArray)
            {
                try
                {
                    return JToken.Parse(value);
                }
                catch (JsonException)
                {
                    return value;
                }
            }

            return value;
        }

        private enum ViewMode
        {
            Text,
            Table
        }
    }
}
