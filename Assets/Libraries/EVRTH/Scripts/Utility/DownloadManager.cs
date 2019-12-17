using System;
using System.Collections.Generic;
using EVRTH.Scripts.GlobeNS;
using EVRTH.Scripts.Visualization;
using UnityEngine;
using UnityEngine.Networking;

namespace EVRTH.Scripts.Utility
{
    public class DownloadManager : MonoBehaviour
    {
        public int tileTextureLoadsPerFrame = 2;

        public readonly int numQueues = Globe.MaxLayers + 1;

        private readonly Queue<DownloadTask> globeTileTexturesToLoadQueue = new Queue<DownloadTask>();

        private readonly Dictionary<string, DownloadStatus> downloadRequests = new Dictionary<string, DownloadStatus>();
        private readonly Dictionary<string, DownloadTask> globeTileQueueLookup = new Dictionary<string, DownloadTask>();

        private Globe globe;
        private TileTextureCache textureCache;

        public float globalMipMapBias;
        public int globalAnisoLevel;

        public bool initialized;
        public bool linearTextures = true;

        private void Awake()
        {
            if (!initialized)
            {
                Init();
            }
        }

        private void Init()
        {
            globe = GetComponent<Globe>();
            textureCache = globe != null
                ? globe.tileTextureCache ?? GetComponent<TileTextureCache>()
                : GetComponent<TileTextureCache>();

            initialized = true;
        }

        private void Update()
        {
            ProcessTextureQueue();
        }

        /// <summary>
        /// Request a texture download. The download will be processed asynchronously. If there is
        /// already a request in progress for this URL a second request will *not* be queued.
        /// </summary>
        /// <param name="url">URL to download.</param>
        /// <param name="layerNameToSet">Layer identifier for this request.</param>
        /// <param name="dateTimeToSet">Date of this request.</param>
        /// <param name="handler">Callback that will be invoked when the texture is ready.</param>
        /// <param name="prepareForRendering">True if the texture will be used for rendering. If
        /// true mipmaps will be generated and an aniso bias set. If false the texture will be
        /// loaded and returned with no further processing.</param>
        /// <param name="queueNum">The download queue to use for this request. Queues are identified
        /// by index, and all downloads in a queue can be cancelled without affecting other
        /// queues.</param>
        public void RequestTexture(string url, string layerNameToSet, DateTime dateTimeToSet,
            TextureDownloadHandler handler, bool prepareForRendering = true, int queueNum = 0)
        {
            DownloadStatus status;
            bool haveEntry = downloadRequests.TryGetValue(url, out status);

            bool requestInProgress = haveEntry && status == DownloadStatus.InProgress;
            if (!requestInProgress)
            {
                DownloadTask globeTileLayerSet = new DownloadTask
                {
                    url = url,
                    layerName = layerNameToSet,
                    date = dateTimeToSet,
                    handler = handler,
                    prepareTextureForRendering = prepareForRendering
                };

                downloadRequests[url] = DownloadStatus.InProgress;
                SwarmManager.Instance.EnqueueRequest(new DownloadRequest
                {
                    url = url,
                    callbackAction = t =>
                    {
                        Texture2D myTexture = new Texture2D(2, 2, TextureFormat.RGB24, true, linearTextures);
                        myTexture.LoadImage(t.downloadHandler.data, false);
                        if (globeTileLayerSet.prepareTextureForRendering)
                        {
                            myTexture.wrapMode = TextureWrapMode.Clamp;
                            myTexture.mipMapBias = globalMipMapBias;
                            myTexture.anisoLevel = globalAnisoLevel;
                            myTexture.Apply(true, true);
                        }
                        globeTileLayerSet.texture = myTexture;
                        globeTileTexturesToLoadQueue.Enqueue(globeTileLayerSet);
                    }
                });

                if (!globeTileQueueLookup.ContainsKey(url))
                {
                    globeTileQueueLookup.Add(url, globeTileLayerSet);
                }
            }
            else
            {
                DownloadTask globeTileLayerSet = globeTileQueueLookup[url];
                globeTileLayerSet.handler += handler;
                globeTileLayerSet.prepareTextureForRendering =
                    globeTileLayerSet.prepareTextureForRendering || prepareForRendering;
            }
        }

        /// <summary>
        /// Clear all active downloads.
        /// </summary>
        public void Clear()
        {
            if (!initialized)
            {
                Init();
            }

            Debug.Log("Clearing download content");
            StopAllCoroutines();

            globeTileTexturesToLoadQueue.Clear();
            globeTileQueueLookup.Clear();
        }

        private void ProcessTextureQueue()
        {
            for (int i = 0; i < tileTextureLoadsPerFrame; i++)
            {
                if (globeTileTexturesToLoadQueue.Count > 0)
                {
                    DownloadTask task = globeTileTexturesToLoadQueue.Dequeue();

                    // Invoke download handler
                    InvokeHandler(task);

                    downloadRequests.Remove(task.url);
                    globeTileQueueLookup.Remove(task.url);
                }
                else
                {
                    break;
                }
            }
        }

        private void InvokeHandler(DownloadTask task)
        {
            // Add this texture to the cache
            textureCache.AddTexture(task.url, task.texture);

            if (task.handler != null)
            {
                task.handler(task.layerName, task.date, task.texture);
            }
        }
    }
}
