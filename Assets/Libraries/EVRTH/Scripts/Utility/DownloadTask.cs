using System;
using UnityEngine;

namespace EVRTH.Scripts.Utility
{
    /// <summary>
    /// Simple data structure that facilitates downloads
    /// </summary>
    public class DownloadTask
    {
        public string url;
        public string layerName;
        public DateTime date;
        public TextureDownloadHandler handler;
        public bool prepareTextureForRendering;
        public Texture2D texture;
        public string localFilePath;
        public bool usingDxtFormat;
    }
}
