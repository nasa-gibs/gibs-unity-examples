using System;
using System.IO;
using UnityEngine;

namespace EVRTH.Scripts.Utility
{
    public static class DDSTextureLoader
    {
        public static Texture2D LoadTextureDxt(string ddsFilename, TextureFormat textureFormat)
        {
            byte[] ddsBytes;

            try
            {
                ddsBytes = File.ReadAllBytes(ddsFilename);
            }
            catch (Exception e)
            {
                Debug.Log("Error occurred during file load!");
                Debug.Log(e.ToString());

                return (new Texture2D(2,2));
            }
        

            if (textureFormat != TextureFormat.DXT1 && textureFormat != TextureFormat.DXT5)
            { 
                throw new Exception("Invalid TextureFormat. Only DXT1 and DXT5 formats are supported by this method.");
            }

            byte ddsSizeCheck = ddsBytes[4];
            if (ddsSizeCheck != 124)
            { 
                throw new Exception("Invalid DDS DXTn texture. Unable to read");  //this header byte should be 124 for DDS image files
            }

            int height = ddsBytes[13] * 256 + ddsBytes[12];
            int width = ddsBytes[17] * 256 + ddsBytes[16];

            const int ddsHeaderSize = 128;
            byte[] dxtBytes = new byte[ddsBytes.Length - ddsHeaderSize];
            Buffer.BlockCopy(ddsBytes, ddsHeaderSize, dxtBytes, 0, ddsBytes.Length - ddsHeaderSize);

            Texture2D texture = new Texture2D(width, height, textureFormat, true);
        
            texture.LoadRawTextureData(dxtBytes);
            texture.Apply();

            return (texture);
        }
    }
}
