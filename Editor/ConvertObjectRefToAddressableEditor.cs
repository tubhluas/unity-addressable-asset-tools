using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets;
using System.Reflection;

namespace Insthync.AddressableAssetTools
{
    public class ConvertObjectRefToAddressableEditor : EditorWindow
    {
        private AddressableAssetSettings _settings;
        private AddressableAssetGroup _selectedGroup;
        private string _groupName;
        private List<Object> _selectedAssets = new List<Object>();
        private Vector2 _assetsScrollPosition;

        [MenuItem("Tools/Addressables/Convert Object Ref To Addressable")]
        public static void ShowWindow()
        {
            GetWindow<ConvertObjectRefToAddressableEditor>("Convert Object Ref To Addressable");
        }

        private void OnGUI()
        {
            GUILayout.Label("Convert Object Ref To Addressable", EditorStyles.boldLabel);

            _settings = AddressableAssetSettingsDefaultObject.Settings;
            if (_settings == null)
            {
                EditorGUILayout.HelpBox("Addressable Asset Settings not found!", MessageType.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(_groupName))
                _selectedGroup = (AddressableAssetGroup)EditorGUILayout.ObjectField("Target Group", _selectedGroup, typeof(AddressableAssetGroup), false);
            _groupName = EditorGUILayout.TextField("Target Group Name", _groupName);

            EditorGUILayout.Space();

            GUILayout.Label("Selected Assets:", EditorStyles.boldLabel);

            // Scrollable list of selected assets
            _assetsScrollPosition = EditorGUILayout.BeginScrollView(_assetsScrollPosition, GUILayout.Height(150));
            for (int i = 0; i < _selectedAssets.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                _selectedAssets[i] = EditorGUILayout.ObjectField(_selectedAssets[i], typeof(Object), false);
                if (GUILayout.Button("Remove", GUILayout.Width(60)))
                {
                    _selectedAssets.RemoveAt(i);
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Add Asset"))
            {
                _selectedAssets.Add(null);
            }

            if (GUILayout.Button("Add Selected Assets (In Project Tab)"))
            {
                _selectedAssets.AddRange(Selection.objects);
            }

            if (GUILayout.Button("Clear Assets"))
            {
                _selectedAssets.Clear();
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("Convert"))
            {
                ConvertSelectedAssets();
            }
        }

        private void ConvertSelectedAssets()
        {
            for (int i = 0; i < _selectedAssets.Count; ++i)
            {
                Convert(_selectedAssets[i]);
            }
        }

        private void Convert(Object asset)
        {
            if (asset == null)
                return;
            if (string.IsNullOrWhiteSpace(_groupName) && _selectedGroup != null)
                _groupName = _selectedGroup.Name;
            System.Type objectType = asset.GetType();
            List<FieldInfo> fields = new List<FieldInfo>(objectType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance));
            foreach (FieldInfo field in fields)
            {
                object[] foundAttr = field.GetCustomAttributes(typeof(AddressableAssetConversionAttribute), false);
                if (foundAttr.Length <= 0)
                    continue;
                AddressableAssetConversionAttribute attr = foundAttr[0] as AddressableAssetConversionAttribute;
                if (!string.IsNullOrWhiteSpace(attr.ConvertFunctionName))
                {
                    MethodInfo methodInfo = objectType.GetMethod(attr.ConvertFunctionName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (methodInfo != null)
                    {
                        methodInfo.Invoke(asset, new object[]
                        {
                            field.GetValue(asset),
                            attr.AddressableVarName,
                        });
                    }
                    continue;
                }
                AddressableEditorUtils.ConvertObjectRefToAddressable(asset, field.Name, attr.AddressableVarName, _groupName);
            }
            EditorUtility.SetDirty(asset);
        }
    }
}
