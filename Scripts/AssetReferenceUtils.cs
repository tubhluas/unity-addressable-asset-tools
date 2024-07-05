using UnityEngine.AddressableAssets;

namespace Insthync.AddressableAssetTools
{
    public static class AssetReferenceUtils
    {
        public static bool IsDataValid(this AssetReference asset)
        {
            return asset != null && asset.RuntimeKeyIsValid();
        }
    }
}