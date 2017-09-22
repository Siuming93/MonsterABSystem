using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Assets.Editor.AssetBundleBuilder
{
    class AssetBundleBuilderWindow : EditorWindow
    {
        private const string DB_PATH = "Editor/AssetBundleBuilder/AssetBundleDataBase";
        private static AssetBundleBuilderWindow _instance;
        [MenuItem("Tools/Show AssetBundle Builder")]
        public static void Open()
        {
            _instance = (AssetBundleBuilderWindow)EditorWindow.GetWindow(typeof(AssetBundleBuilderWindow));
            _instance.Init();
        }

        private void Init()
        {
            title = "Builder";
            minSize = new Vector2(300f, 400f);
            _config = _config ?? new BuilderConfig();
        }

        private BuilderConfig _config;
        void OnGUI()
        {
            if (this._config == null)
                Init();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            _config.LoadPath= EditorGUILayout.TextField("Load Path", _config.LoadPath);
            if (GUILayout.Button("Select"))
            {
                var path = EditorUtility.OpenFolderPanel("Load", _config.LoadPath, "");
                _config.LoadPath = string.IsNullOrEmpty(path) ? _config.LoadPath : path;
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            _config.ExportPath = EditorGUILayout.TextField("Export Path", _config.ExportPath);
            if (GUILayout.Button("Select"))
            {
                var path = EditorUtility.OpenFolderPanel("Load", _config.ExportPath, "");
                _config.ExportPath = string.IsNullOrEmpty(path) ? _config.ExportPath : path;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("BuildAssetBundleOptions:");
            if (GUILayout.Button(_config.options.ToString()))
            {
                ShowTypeNamesMenu(
                     _config.options.ToString(), optionsList,
                    (string selectedTypeStr) =>
                    {
                        _config.options = optionsMap[selectedTypeStr];
                    }
                );
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("BuildTarget:");
            if (GUILayout.Button(_config.target.ToString()))
            {
                ShowTypeNamesMenu(
                     _config.target.ToString(), targetList,
                    (string selectedTypeStr) =>
                    {
                        _config.target = targetMap[selectedTypeStr];
                    }
                );
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Debug"))
            {
                new AssetBundleBuilder_5().Debug(_config);
            }
            if (GUILayout.Button("Build"))
            {
                new AssetBundleBuilder_5().Excute(_config);
            }
            EditorGUILayout.EndHorizontal();
        }

        private List<string> optionsList = new List<string>()
        {
            BuildAssetBundleOptions.None.ToString(),
            BuildAssetBundleOptions.DeterministicAssetBundle.ToString(),
            BuildAssetBundleOptions.IgnoreTypeTreeChanges.ToString(),
            BuildAssetBundleOptions.OmitClassVersions.ToString(),
            BuildAssetBundleOptions.UncompressedAssetBundle.ToString(),
        };

        private Dictionary<string, BuildAssetBundleOptions> optionsMap = new Dictionary<string, BuildAssetBundleOptions>()
        {
            {BuildAssetBundleOptions.None.ToString(),BuildAssetBundleOptions.None},
            {BuildAssetBundleOptions.DeterministicAssetBundle.ToString(),BuildAssetBundleOptions.DeterministicAssetBundle},
            {BuildAssetBundleOptions.IgnoreTypeTreeChanges.ToString(),BuildAssetBundleOptions.IgnoreTypeTreeChanges},
            {BuildAssetBundleOptions.OmitClassVersions.ToString(),BuildAssetBundleOptions.OmitClassVersions},
            {BuildAssetBundleOptions.UncompressedAssetBundle.ToString(),BuildAssetBundleOptions.UncompressedAssetBundle},
        };

        private List<string> targetList = new List<string>()
        {
            BuildTarget.StandaloneWindows.ToString(),
            BuildTarget.Android.ToString(),
        };

        private Dictionary<string, BuildTarget> targetMap = new Dictionary<string, BuildTarget>()
        {
            {BuildTarget.StandaloneWindows.ToString(),BuildTarget.StandaloneWindows},
            {BuildTarget.Android.ToString(),BuildTarget.Android},
        };

        void ShowTypeNamesMenu(string current, List<string> contents, Action<string> ExistSelected)
        {
            var menu = new GenericMenu();

            for (var i = 0; i < contents.Count; i++)
            {
                var type = contents[i];
                var selected = false;
                if (type == current) selected = true;

                menu.AddItem(
                    new GUIContent(type),
                    selected,
                    () =>
                    {
                        ExistSelected(type);
                    }
                );
            }
            menu.ShowAsContext();
        }
    }
}
