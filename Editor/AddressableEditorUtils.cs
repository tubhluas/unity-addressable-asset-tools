using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.AddressableAssets;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Reflection;

namespace Insthync.AddressableAssetTools
{
    public static partial class AddressableEditorUtils
    {
        private static void CreateSettings()
        {
            if (!AddressableAssetSettingsDefaultObject.SettingsExists)
            {
                AddressableAssetSettingsDefaultObject.Settings = AddressableAssetSettingsDefaultObject.GetSettings(true);
            }
        }

        private static AddressableAssetGroup CreateGroup(string name)
        {
            CreateSettings();

            AddressableAssetGroup addressableAssetGroup = AddressableAssetSettingsDefaultObject.Settings.FindGroup(name);

            if (addressableAssetGroup == null)
            {
                addressableAssetGroup = AddressableAssetSettingsDefaultObject.Settings.CreateGroup(name, false, false, false, new List<AddressableAssetGroupSchema>(), new System.Type[0]);
            }

            // Make sure we are using the default schemas with the default settings
            if (addressableAssetGroup != null)
            {
                addressableAssetGroup.RemoveSchema(typeof(BundledAssetGroupSchema));
                addressableAssetGroup.RemoveSchema(typeof(ContentUpdateGroupSchema));
                addressableAssetGroup.AddSchema(typeof(BundledAssetGroupSchema));
                addressableAssetGroup.AddSchema(typeof(ContentUpdateGroupSchema));
            }
            return addressableAssetGroup;
        }

        public static void ConvertObjectRefToAddressable<TObject, TAssetRef>(ref TObject obj, ref TAssetRef aa, string groupName = "Default Local Group")
            where TObject : Object
            where TAssetRef : AssetReference
        {
            if (obj == null)
                return;
            if (string.IsNullOrWhiteSpace(groupName))
                groupName = "Default Local Group";
            string objPath = AssetDatabase.GetAssetPath(obj);
            if (string.IsNullOrWhiteSpace(objPath))
            {
                Debug.LogWarning($"Skipping {obj.name}: Not an assets.");
                return;
            }
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                EditorGUILayout.HelpBox("Addressable Asset Settings not found!", MessageType.Error);
                return;
            }
            AddressableAssetGroup targetGroup = AddressableAssetSettingsDefaultObject.Settings.FindGroup(groupName);
            if (targetGroup == null)
                targetGroup = CreateGroup(groupName);
            string guid = AssetDatabase.AssetPathToGUID(objPath);
            settings.CreateOrMoveEntry(guid, targetGroup, false, false);
            Debug.Log($"{obj.name} is moved to group: {groupName}.");
            aa = (TAssetRef)System.Activator.CreateInstance(typeof(TAssetRef), new object[]
            {
                guid,
            });
            obj = null;
        }

        public static void ConvertObjectRefToAddressable(Object asset, string objFieldName, string aaFieldName, string groupName = "Default Local Group")
        {
            if (asset == null)
                return;
            if (string.IsNullOrWhiteSpace(groupName))
                groupName = "Default Local Group";
            System.Type objectType = asset.GetType();
            FieldInfo objFieldInfo = objectType.GetField(objFieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo aaFieldInfo = objectType.GetField(aaFieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            Object obj = objFieldInfo.GetValue(asset) as Object;
            if (obj == null)
            {
                Debug.LogWarning($"Skipping object: {objFieldName} not an assets.");
                return;
            }
            string objPath = AssetDatabase.GetAssetPath(obj);
            if (string.IsNullOrWhiteSpace(objPath))
            {
                Debug.LogWarning($"Skipping {obj.name}: Not an assets.");
                return;
            }
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                EditorGUILayout.HelpBox("Addressable Asset Settings not found!", MessageType.Error);
                return;
            }
            AddressableAssetGroup targetGroup = AddressableAssetSettingsDefaultObject.Settings.FindGroup(groupName);
            if (targetGroup == null)
                targetGroup = CreateGroup(groupName);
            string guid = AssetDatabase.AssetPathToGUID(objPath);
            settings.CreateOrMoveEntry(guid, targetGroup, false, false);
            Debug.Log($"{obj.name} is moved to group: {groupName}.");
            aaFieldInfo.SetValue(asset, System.Activator.CreateInstance(aaFieldInfo.FieldType, new object[]
            {
                guid,
            }));
            objFieldInfo.SetValue(asset, null);
        }
    }
}
