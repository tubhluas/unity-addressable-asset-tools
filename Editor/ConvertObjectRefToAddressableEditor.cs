using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets;
using System.Reflection;
using System.Collections;

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
        private bool IsListOrArray(System.Type type, out System.Type itemType)
        {
            if (type.IsArray)
            {
                itemType = type.GetElementType();
                return true;
            }
            foreach (System.Type interfaceType in type.GetInterfaces())
            {
                if (interfaceType.IsGenericType &&
                    interfaceType.GetGenericTypeDefinition() == typeof(IList<>))
                {
                    itemType = type.GetGenericArguments()[0];
                    return true;
                }
            }
            itemType = null;
            return false;
        }

        private void Convert(Object asset)
        {
            if (asset == null)
                return;
            if (string.IsNullOrWhiteSpace(_groupName) && _selectedGroup != null)
                _groupName = _selectedGroup.Name;
            System.Type objectType = asset.GetType();
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            do
            {
                List<FieldInfo> fields = new List<FieldInfo>(objectType.GetFields(flags));
                foreach (FieldInfo field in fields)
                {
                    object[] foundAttr = field.GetCustomAttributes(typeof(AddressableAssetConversionAttribute), false);
                    if (foundAttr.Length > 0)
                    {
                        AddressableAssetConversionAttribute attr = foundAttr[0] as AddressableAssetConversionAttribute;
                        AddressableEditorUtils.ConvertObjectRefToAddressable(asset, field.Name, attr.AddressableVarName, _groupName);
                        continue;
                    }
                    System.Type interfaceType = field.FieldType.GetInterface(nameof(IAddressableAssetConversable));
                    if (interfaceType != null)
                    {
                        MethodInfo methodInfo = interfaceType.GetMethod("ProceedAddressableAssetConversion", flags);
                        methodInfo.Invoke(asset, new object[0]);
                        continue;
                    }
                    if (IsListOrArray(field.FieldType, out System.Type elementType))
                    {
                        interfaceType = elementType.GetInterface(nameof(IAddressableAssetConversable));
                        if (interfaceType != null)
                        {
                            IList list = field.GetValue(asset) as IList;
                            for (int i = 0; i < list.Count; ++i)
                            {
                                object entry = list[i];
                                MethodInfo methodInfo = interfaceType.GetMethod("ProceedAddressableAssetConversion", flags);
                                methodInfo.Invoke(entry, new object[0]);
                                list[i] = entry;
                            }
                        }
                    }
                }
                objectType = objectType.BaseType;
            } while (objectType.BaseType != null);
            EditorUtility.SetDirty(asset);
        }
    }
}
