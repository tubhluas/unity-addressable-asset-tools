using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Insthync.AddressableAssetTools
{
    public static class AddressableAssetsManager
    {
        private static readonly Dictionary<object, Object> s_loadedAssets = new Dictionary<object, Object>();
        private static readonly Dictionary<object, AsyncOperationHandle> s_assetRefs = new Dictionary<object, AsyncOperationHandle>();

        public static async UniTask<TType> GetOrLoadObjectAsync<TType>(
            this AssetReference assetRef, System.Action<AsyncOperationHandle> handlerCallback = null)
            where TType : Object
        {
            if (!assetRef.IsDataValid())
                return null;

            if (s_loadedAssets.TryGetValue(assetRef.RuntimeKey, out Object result))
                return result as TType;

            // Check if the asset is actually marked as Addressable (has a valid GUID)
            if (string.IsNullOrEmpty(assetRef.AssetGUID))
            {
                Debug.LogWarning($"Asset is not marked as Addressable: {assetRef.RuntimeKey}. Ignoring Load.");
                return null;
            }

            var locations = await Addressables.LoadResourceLocationsAsync(assetRef.RuntimeKey).Task;
            if (locations == null || locations.Count == 0)
            {
                Debug.LogWarning($"Addressable asset not found: {assetRef.RuntimeKey}. Ignoring Load.");
                return default; // Return null or a default object
            }

            AsyncOperationHandle<TType> handler = Addressables.LoadAssetAsync<TType>(assetRef.RuntimeKey);
            handlerCallback?.Invoke(handler);

            TType handlerResult;
            try
            {
                handlerResult = await handler.ToUniTask();
            }
            catch
            {
                return null;
            }

            s_loadedAssets[assetRef.RuntimeKey] = handlerResult;
            s_assetRefs[assetRef.RuntimeKey] = handler;
            return handlerResult;
        }

        public static TType GetOrLoadObject<TType>(
            this AssetReference assetRef, System.Action<AsyncOperationHandle> handlerCallback = null)
            where TType : Object
        {
            if (s_loadedAssets.TryGetValue(assetRef.RuntimeKey, out Object result))
                return result as TType;

            // Check if the asset is actually marked as Addressable
            if (string.IsNullOrEmpty(assetRef.AssetGUID))
            {
                Debug.LogWarning($"Asset is not marked as Addressable: {assetRef.RuntimeKey}. Ignoring load.");
                return null;
            }

            // Check if the Addressable asset exists before loading
            var locations = Addressables.LoadResourceLocationsAsync(assetRef.RuntimeKey).WaitForCompletion();
            if (locations == null || locations.Count == 0)
            {
                Debug.LogWarning($"Addressable asset not found: {assetRef.RuntimeKey}. Ignoring load.");
                return null;
            }

            AsyncOperationHandle<TType> handler = Addressables.LoadAssetAsync<TType>(assetRef.RuntimeKey);
            handlerCallback?.Invoke(handler);

            TType handlerResult;
            try
            {
                handlerResult = handler.WaitForCompletion();
            }
            catch
            {
                return null;
            }

            s_loadedAssets[assetRef.RuntimeKey] = handlerResult;
            s_assetRefs[assetRef.RuntimeKey] = handler;
            return handlerResult;
        }

        public static async UniTask<TType> GetOrLoadAssetAsync<TType>(
            this AssetReference assetRef, System.Action<AsyncOperationHandle> handlerCallback = null)
            where TType : Component
        {
            // Check if the asset is actually Addressable
            if (string.IsNullOrEmpty(assetRef.AssetGUID))
            {
                Debug.LogWarning($"Asset is not marked as Addressable: {assetRef.RuntimeKey}. Ignoring load.");
                return null;
            }

            // Check if asset exists before loading
            var locations = await Addressables.LoadResourceLocationsAsync(assetRef.RuntimeKey);
            if (locations == null || locations.Count == 0)
            {
                Debug.LogWarning($"Addressable asset not found: {assetRef.RuntimeKey}. Ignoring load.");
                return null;
            }

            GameObject loadedObject = await assetRef.GetOrLoadAssetAsync(handlerCallback);
            return loadedObject != null ? loadedObject.GetComponent<TType>() : null;
        }

        public static TType GetOrLoadAsset<TType>(
            this AssetReference assetRef, System.Action<AsyncOperationHandle> handlerCallback = null)
            where TType : Component
        {
            // Check if the asset is actually Addressable
            if (string.IsNullOrEmpty(assetRef.AssetGUID))
            {
                Debug.LogWarning($"Asset is not marked as Addressable: {assetRef.RuntimeKey}. Ignoring load.");
                return null;
            }

            // Check if asset exists before loading
            var locations = Addressables.LoadResourceLocationsAsync(assetRef.RuntimeKey).WaitForCompletion();
            if (locations == null || locations.Count == 0)
            {
                Debug.LogWarning($"Addressable asset not found: {assetRef.RuntimeKey}. Ignoring load.");
                return null;
            }

            GameObject loadedObject = assetRef.GetOrLoadAsset(handlerCallback);
            return loadedObject != null ? loadedObject.GetComponent<TType>() : null;
        }

        public static async UniTask<GameObject> GetOrLoadAssetAsync(
            this AssetReference assetRef, System.Action<AsyncOperationHandle> handlerCallback = null)
        {
            // Check if the asset is actually Addressable
            if (string.IsNullOrEmpty(assetRef.AssetGUID))
            {
                Debug.LogWarning($"Asset is not marked as Addressable: {assetRef.RuntimeKey}. Ignoring load.");
                return null;
            }

            // Check if asset exists before loading
            var locations = await Addressables.LoadResourceLocationsAsync(assetRef.RuntimeKey);
            if (locations == null || locations.Count == 0)
            {
                Debug.LogWarning($"Addressable asset not found: {assetRef.RuntimeKey}. Ignoring load.");
                return null;
            }

            return await assetRef.GetOrLoadObjectAsync<GameObject>(handlerCallback);
        }

        public static GameObject GetOrLoadAsset(
            this AssetReference assetRef, System.Action<AsyncOperationHandle> handlerCallback = null)
        {
            // Ensure asset reference is actually Addressable
            if (string.IsNullOrEmpty(assetRef.AssetGUID))
            {
                Debug.LogWarning($"Asset is not marked as Addressable: {assetRef.RuntimeKey}. Ignoring load.");
                return null;
            }

            // Check if asset exists before loading
            var locations = Addressables.LoadResourceLocationsAsync(assetRef.RuntimeKey).WaitForCompletion();
            if (locations == null || locations.Count == 0)
            {
                Debug.LogWarning($"Addressable asset not found: {assetRef.RuntimeKey}. Ignoring load.");
                return null;
            }

            return assetRef.GetOrLoadObject<GameObject>(handlerCallback);
        }

        public static async UniTask<TType> GetOrLoadAssetAsyncOrUsePrefab<TType>(
            this AssetReference assetRef, TType prefab, System.Action<AsyncOperationHandle> handlerCallback = null)
            where TType : Component
        {
            TType tempPrefab = null;

            // Ensure asset reference is actually Addressable before loading
            if (!string.IsNullOrEmpty(assetRef.AssetGUID))
            {
                var locations = await Addressables.LoadResourceLocationsAsync(assetRef.RuntimeKey);
                if (locations != null && locations.Count > 0)
                {
                    tempPrefab = await assetRef.GetOrLoadAssetAsync<TType>(handlerCallback);
                }
            }

            return tempPrefab ?? prefab; // Use prefab as fallback if loading fails
        }

        public static TType GetOrLoadAssetOrUsePrefab<TType>(
            this AssetReference assetRef, TType prefab, System.Action<AsyncOperationHandle> handlerCallback = null)
            where TType : Component
        {
            TType tempPrefab = null;

            // Ensure asset reference is actually Addressable before loading
            if (!string.IsNullOrEmpty(assetRef.AssetGUID))
            {
                var locations = Addressables.LoadResourceLocationsAsync(assetRef.RuntimeKey).WaitForCompletion();
                if (locations != null && locations.Count > 0)
                {
                    tempPrefab = assetRef.GetOrLoadAsset<TType>(handlerCallback);
                }
            }

            return tempPrefab ?? prefab; // Use prefab as fallback if loading fails
        }

        public static async UniTask<TType> GetOrLoadObjectAsyncOrUseAsset<TType>(
            this AssetReference assetRef, TType asset, System.Action<AsyncOperationHandle> handlerCallback = null)
            where TType : Object
        {
            TType tempAsset = null;

            // Ensure asset reference is actually Addressable before loading
            if (!string.IsNullOrEmpty(assetRef.AssetGUID))
            {
                var locations = await Addressables.LoadResourceLocationsAsync(assetRef.RuntimeKey);
                if (locations != null && locations.Count > 0)
                {
                    tempAsset = await assetRef.GetOrLoadObjectAsync<TType>(handlerCallback);
                }
            }

            return tempAsset ?? asset; // Use fallback asset if loading fails
        }

        public static TType GetOrLoadObjectOrUseAsset<TType>(
            this AssetReference assetRef, TType asset, System.Action<AsyncOperationHandle> handlerCallback = null)
            where TType : Object
        {
            TType tempAsset = null;

            // Ensure asset reference is actually Addressable before loading
            if (!string.IsNullOrEmpty(assetRef.AssetGUID))
            {
                var locations = Addressables.LoadResourceLocationsAsync(assetRef.RuntimeKey).WaitForCompletion();
                if (locations != null && locations.Count > 0)
                {
                    tempAsset = assetRef.GetOrLoadObject<TType>(handlerCallback);
                }
            }

            return tempAsset ?? asset; // Use fallback asset if loading fails
        }

        public static async UniTask<GameObject> GetOrLoadAssetAsyncOrUsePrefab(
            this AssetReference assetRef, GameObject prefab, System.Action<AsyncOperationHandle> handlerCallback = null)
        {
            GameObject tempPrefab = null;

            // Ensure asset reference is actually Addressable before loading
            if (!string.IsNullOrEmpty(assetRef.AssetGUID))
            {
                var locations = await Addressables.LoadResourceLocationsAsync(assetRef.RuntimeKey);
                if (locations != null && locations.Count > 0)
                {
                    tempPrefab = await assetRef.GetOrLoadAssetAsync(handlerCallback);
                }
            }

            return tempPrefab ?? prefab; // Use prefab as fallback if loading fails
        }

        public static GameObject GetOrLoadAssetOrUsePrefab(
            this AssetReference assetRef, GameObject prefab, System.Action<AsyncOperationHandle> handlerCallback = null)
        {
            GameObject tempPrefab = null;

            // Ensure asset reference is actually Addressable before loading
            if (!string.IsNullOrEmpty(assetRef.AssetGUID))
            {
                var locations = Addressables.LoadResourceLocationsAsync(assetRef.RuntimeKey).WaitForCompletion();
                if (locations != null && locations.Count > 0)
                {
                    tempPrefab = assetRef.GetOrLoadAsset(handlerCallback);
                }
            }

            return tempPrefab ?? prefab; // Use prefab as fallback if loading fails
        }

        public static async UniTask<TType[]> GetOrLoadObjectsAsync<TType>(
            this IEnumerable<AssetReference> assetRefs, System.Action<AsyncOperationHandle> handlerCallback = null)
            where TType : Object
        {
            List<UniTask<TType>> tasks = new List<UniTask<TType>>();

            foreach (AssetReference assetRef in assetRefs)
            {
                // Skip lookup if asset isn't marked as Addressable
                if (!string.IsNullOrEmpty(assetRef.AssetGUID))
                {
                    var locations = await Addressables.LoadResourceLocationsAsync(assetRef.RuntimeKey);
                    if (locations != null && locations.Count > 0)
                    {
                        tasks.Add(assetRef.GetOrLoadObjectAsync<TType>(handlerCallback));
                    }
                    else
                    {
                        Debug.LogWarning($"Addressable asset not found: {assetRef.RuntimeKey}. Ignoring load.");
                    }
                }
            }

            return await UniTask.WhenAll(tasks);
        }

        public static TType[] GetOrLoadObjects<TType>(
            this IEnumerable<AssetReference> assetRefs, System.Action<AsyncOperationHandle> handlerCallback = null)
            where TType : Object
        {
            List<TType> results = new List<TType>();

            foreach (AssetReference assetRef in assetRefs)
            {
                // Skip lookup if asset isn't marked as Addressable
                if (!string.IsNullOrEmpty(assetRef.AssetGUID))
                {
                    var locations = Addressables.LoadResourceLocationsAsync(assetRef.RuntimeKey).WaitForCompletion();
                    if (locations != null && locations.Count > 0)
                    {
                        results.Add(assetRef.GetOrLoadObject<TType>(handlerCallback));
                    }
                    else
                    {
                        Debug.LogWarning($"Addressable asset not found: {assetRef.RuntimeKey}. Ignoring load.");
                    }
                }
            }

            return results.ToArray();
        }

        public static async UniTask<TType[]> GetOrLoadAssetsAsync<TType>(
            this IEnumerable<AssetReference> assetRefs, System.Action<AsyncOperationHandle> handlerCallback = null)
            where TType : Component
        {
            List<UniTask<TType>> tasks = new List<UniTask<TType>>();

            foreach (AssetReference assetRef in assetRefs)
            {
                // Skip lookup if asset isn't marked as Addressable
                if (!string.IsNullOrEmpty(assetRef.AssetGUID))
                {
                    var locations = await Addressables.LoadResourceLocationsAsync(assetRef.RuntimeKey);
                    if (locations != null && locations.Count > 0)
                    {
                        tasks.Add(assetRef.GetOrLoadAssetAsync<TType>(handlerCallback));
                    }
                    else
                    {
                        Debug.LogWarning($"Addressable asset not found: {assetRef.RuntimeKey}. Ignoring load.");
                    }
                }
            }

            return await UniTask.WhenAll(tasks);
        }

        public static TType[] GetOrLoadAssets<TType>(
            this IEnumerable<AssetReference> assetRefs, System.Action<AsyncOperationHandle> handlerCallback = null)
            where TType : Component
        {
            List<TType> results = new List<TType>();

            foreach (AssetReference assetRef in assetRefs)
            {
                // Skip lookup if asset isn't marked as Addressable
                if (!string.IsNullOrEmpty(assetRef.AssetGUID))
                {
                    var locations = Addressables.LoadResourceLocationsAsync(assetRef.RuntimeKey).WaitForCompletion();
                    if (locations != null && locations.Count > 0)
                    {
                        results.Add(assetRef.GetOrLoadAsset<TType>(handlerCallback));
                    }
                    else
                    {
                        Debug.LogWarning($"Addressable asset not found: {assetRef.RuntimeKey}. Ignoring load.");
                    }
                }
            }

            return results.ToArray();
        }

        public static async UniTask<GameObject[]> GetOrLoadAssetsAsync(
            this IEnumerable<AssetReference> assetRefs, System.Action<AsyncOperationHandle> handlerCallback = null)
        {
            List<UniTask<GameObject>> tasks = new List<UniTask<GameObject>>();

            foreach (AssetReference assetRef in assetRefs)
            {
                // Skip lookup if asset isn't marked as Addressable
                if (!string.IsNullOrEmpty(assetRef.AssetGUID))
                {
                    var locations = await Addressables.LoadResourceLocationsAsync(assetRef.RuntimeKey);
                    if (locations != null && locations.Count > 0)
                    {
                        tasks.Add(assetRef.GetOrLoadAssetAsync(handlerCallback));
                    }
                    else
                    {
                        Debug.LogWarning($"Addressable asset not found: {assetRef.RuntimeKey}. Ignoring load.");
                    }
                }
            }

            return await UniTask.WhenAll(tasks);
        }

        public static GameObject[] GetOrLoadAssets(
            this IEnumerable<AssetReference> assetRefs, System.Action<AsyncOperationHandle> handlerCallback = null)
        {
            List<GameObject> results = new List<GameObject>();

            foreach (AssetReference assetRef in assetRefs)
            {
                // Skip lookup if asset isn't marked as Addressable
                if (!string.IsNullOrEmpty(assetRef.AssetGUID))
                {
                    var locations = Addressables.LoadResourceLocationsAsync(assetRef.RuntimeKey).WaitForCompletion();
                    if (locations != null && locations.Count > 0)
                    {
                        results.Add(assetRef.GetOrLoadAsset(handlerCallback));
                    }
                    else
                    {
                        Debug.LogWarning($"Addressable asset not found: {assetRef.RuntimeKey}. Ignoring load.");
                    }
                }
            }

            return results.ToArray();
        }

        public static void Release<TAssetRef>(this TAssetRef assetRef)
            where TAssetRef : AssetReference
        {
            Release(assetRef.RuntimeKey);
        }

        public static void Release(object runtimeKey)
        {
            if (s_assetRefs.TryGetValue(runtimeKey, out AsyncOperationHandle handler))
                Addressables.Release(handler);
            s_assetRefs.Remove(runtimeKey);
            s_loadedAssets.Remove(runtimeKey);
        }

        public static void ReleaseAll()
        {
            List<object> keys = new List<object>(s_assetRefs.Keys);
            for (int i = 0; i < keys.Count; ++i)
            {
                Release(keys[i]);
            }
        }
    }
}
