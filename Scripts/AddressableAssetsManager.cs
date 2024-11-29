using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Insthync.AddressableAssetTools
{
    public static class AddressableAssetsManager
    {
        public static async Task<TType> GetOrLoadObjectAsync<TType>(this AssetReference assetRef, System.Action<AsyncOperationHandle> handlerCallback = null)
            where TType : Object
        {
            AsyncOperationHandle<TType> handler = Addressables.LoadAssetAsync<TType>(assetRef.RuntimeKey);
            handlerCallback?.Invoke(handler);
            TType handlerResult;
            try
            {
                handlerResult = await handler.Task;
            }
            catch
            {
                return null;
            }
            return handlerResult;
        }

        public static TType GetOrLoadObject<TType>(this AssetReference assetRef, System.Action<AsyncOperationHandle> handlerCallback = null)
            where TType : Object
        {
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
            return handlerResult;
        }

        public static async Task<TType> GetOrLoadAssetAsync<TType>(this AssetReference assetRef, System.Action<AsyncOperationHandle> handlerCallback = null)
            where TType : Component
        {
            GameObject loadedObject = await assetRef.GetOrLoadAssetAsync(handlerCallback);
            if (loadedObject != null)
            {
                loadedObject.AddComponent<AssetReferenceReleaser>();
                return loadedObject.GetComponent<TType>();
            }
            return null;
        }

        public static TType GetOrLoadAsset<TType>(this AssetReference assetRef, System.Action<AsyncOperationHandle> handlerCallback = null)
            where TType : Component
        {
            GameObject loadedObject = assetRef.GetOrLoadAsset(handlerCallback);
            if (loadedObject != null)
            {
                loadedObject.AddComponent<AssetReferenceReleaser>();
                return loadedObject.GetComponent<TType>();
            }
            return null;
        }

        public static async Task<GameObject> GetOrLoadAssetAsync(this AssetReference assetRef, System.Action<AsyncOperationHandle> handlerCallback = null)
        {
            GameObject loadedObject = await assetRef.GetOrLoadObjectAsync<GameObject>(handlerCallback);
            if (loadedObject != null)
            {
                loadedObject.AddComponent<AssetReferenceReleaser>();
            }
            return loadedObject;
        }

        public static GameObject GetOrLoadAsset(this AssetReference assetRef, System.Action<AsyncOperationHandle> handlerCallback = null)
        {
            GameObject loadedObject = assetRef.GetOrLoadObject<GameObject>(handlerCallback);
            if (loadedObject != null)
            {
                loadedObject.AddComponent<AssetReferenceReleaser>();
            }
            return loadedObject;
        }

        public static async Task<TType> GetOrLoadAssetAsyncOrUsePrefab<TType>(this AssetReference assetRef, TType prefab, System.Action<AsyncOperationHandle> handlerCallback = null)
            where TType : Component
        {
            TType tempPrefab = null;
            if (assetRef.IsDataValid())
                tempPrefab = await assetRef.GetOrLoadAssetAsync<TType>(handlerCallback);
            if (tempPrefab == null)
                tempPrefab = prefab;
            return tempPrefab;
        }

        public static TType GetOrLoadAssetOrUsePrefab<TType>(this AssetReference assetRef, TType prefab, System.Action<AsyncOperationHandle> handlerCallback = null)
            where TType : Component
        {
            TType tempPrefab = null;
            if (assetRef.IsDataValid())
                tempPrefab = assetRef.GetOrLoadAsset<TType>(handlerCallback);
            if (tempPrefab == null)
                tempPrefab = prefab;
            return tempPrefab;
        }

        public static async Task<TType> GetOrLoadObjectAsyncOrUseAsset<TType>(this AssetReference assetRef, TType asset, System.Action<AsyncOperationHandle> handlerCallback = null)
            where TType : Object
        {
            TType tempAsset = null;
            if (assetRef.IsDataValid())
                tempAsset = await assetRef.GetOrLoadObjectAsync<TType>(handlerCallback);
            if (tempAsset == null)
                tempAsset = asset;
            return tempAsset;
        }
        
        public static TType GetOrLoadObjectOrUseAsset<TType>(this AssetReference assetRef, TType asset, System.Action<AsyncOperationHandle> handlerCallback = null)
            where TType : Object
        {
            TType tempAsset = null;
            if (assetRef.IsDataValid())
                tempAsset = assetRef.GetOrLoadObject<TType>(handlerCallback);
            if (tempAsset == null)
                tempAsset = asset;
            return tempAsset;
        }
        
        public static async Task<GameObject> GetOrLoadAssetAsyncOrUsePrefab(this AssetReference assetRef, GameObject prefab, System.Action<AsyncOperationHandle> handlerCallback = null)
        {
            GameObject tempPrefab = null;
            if (assetRef.IsDataValid())
                tempPrefab = await assetRef.GetOrLoadAssetAsync(handlerCallback);
            if (tempPrefab == null)
                tempPrefab = prefab;
            return tempPrefab;
        }
        
        public static GameObject GetOrLoadAssetOrUsePrefab(this AssetReference assetRef, GameObject prefab, System.Action<AsyncOperationHandle> handlerCallback = null)
        {
            GameObject tempPrefab = null;
            if (assetRef.IsDataValid())
                tempPrefab = assetRef.GetOrLoadAsset(handlerCallback);
            if (tempPrefab == null)
                tempPrefab = prefab;
            return tempPrefab;
        }

        public static async Task<TType[]> GetOrLoadObjectsAsync<TType>(this IEnumerable<AssetReference> assetRefs, System.Action<AsyncOperationHandle> handlerCallback = null)
            where TType : Object
        {
            List<Task<TType>> tasks = new List<Task<TType>>();
            foreach (AssetReference assetRef in assetRefs)
            {
                tasks.Add(assetRef.GetOrLoadObjectAsync<TType>(handlerCallback));
            }
            return await Task.WhenAll(tasks);
        }

        public static TType[] GetOrLoadObjects<TType>(this IEnumerable<AssetReference> assetRefs, System.Action<AsyncOperationHandle> handlerCallback = null)
            where TType : Object
        {
            List<TType> results = new List<TType>();
            foreach (AssetReference assetRef in assetRefs)
            {
                results.Add(assetRef.GetOrLoadObject<TType>(handlerCallback));
            }
            return results.ToArray();
        }

        public static async Task<TType[]> GetOrLoadAssetsAsync<TType>(this IEnumerable<AssetReference> assetRefs, System.Action<AsyncOperationHandle> handlerCallback = null)
            where TType : Component
        {
            List<Task<TType>> tasks = new List<Task<TType>>();
            foreach (AssetReference assetRef in assetRefs)
            {
                tasks.Add(assetRef.GetOrLoadAssetAsync<TType>(handlerCallback));
            }
            return await Task.WhenAll(tasks);
        }

        public static TType[] GetOrLoadAssets<TType>(this IEnumerable<AssetReference> assetRefs, System.Action<AsyncOperationHandle> handlerCallback = null)
            where TType : Component
        {
            List<TType> results = new List<TType>();
            foreach (AssetReference assetRef in assetRefs)
            {
                results.Add(assetRef.GetOrLoadAsset<TType>(handlerCallback));
            }
            return results.ToArray();
        }
        
        public static async Task<GameObject[]> GetOrLoadAssetsAsync(this IEnumerable<AssetReference> assetRefs, System.Action<AsyncOperationHandle> handlerCallback = null)
        {
            List<Task<GameObject>> tasks = new List<Task<GameObject>>();
            foreach (AssetReference assetRef in assetRefs)
            {
                tasks.Add(assetRef.GetOrLoadAssetAsync(handlerCallback));
            }
            return await Task.WhenAll(tasks);
        }

        public static GameObject[] GetOrLoadAssets(this IEnumerable<AssetReference> assetRefs, System.Action<AsyncOperationHandle> handlerCallback = null)
        {
            List<GameObject> results = new List<GameObject>();
            foreach (AssetReference assetRef in assetRefs)
            {
                results.Add(assetRef.GetOrLoadAsset(handlerCallback));
            }
            return results.ToArray();
        }
    }
}