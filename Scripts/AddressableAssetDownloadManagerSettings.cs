using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Insthync.AddressableAssetTools
{
    [CreateAssetMenu(fileName = "Addressable Asset Download Manager Settings", menuName = "Addressables/Addressable Asset Download Manager Settings")]
    public class AddressableAssetDownloadManagerSettings : ScriptableObject
    {
        [SerializeField]
        protected List<AssetReference> initialObjects = new List<AssetReference>();
        public virtual List<AssetReference> InitialObjects => initialObjects;
    }
}
