using System.Collections.Specialized;
using UnityEngine;

namespace EVRTH.Scripts.Visualization
{
    public class TileTextureCache : MonoBehaviour
    {
        // Dictionary mapping from TileMetaData to Texture
        public OrderedDictionary tileTextureDict = new OrderedDictionary();

        public int dictSize;
        public int maxDictSize;

        private void Update()
        {
            dictSize = tileTextureDict.Count;
        }

        public void AddTexture(string url, Texture tex)
        {
            // Only add if we don't already have this in our cache
            if (!ExistsInCache(url))
            {
                // Clear space for this entry, if necessary
                if (tileTextureDict.Count + 1 > maxDictSize)
                {
                    tileTextureDict.RemoveAt(0);
                    //Destroy(textureToRemove);
                }

                //Debug.Log("Registering tile texture for " + date.ToString("yyyy-MM-dd"));

                tileTextureDict.Add(url, tex);
            }
        }

        public bool ExistsInCache(string url)
        {
            return tileTextureDict.Contains(url);
        }

        public Texture GetTexture(string url)
        {
            return (Texture)tileTextureDict[url];
        }
    }
}
