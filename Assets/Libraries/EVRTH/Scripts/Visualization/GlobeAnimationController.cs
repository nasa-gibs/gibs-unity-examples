using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EVRTH.Scripts.GIBS;
using EVRTH.Scripts.GlobeNS;
using EVRTH.Scripts.Utility;
using UnityEngine;

namespace EVRTH.Scripts.Visualization
{
    [RequireComponent(typeof(Globe))]
    [RequireComponent(typeof(DownloadManager))]
    public class GlobeAnimationController : MonoBehaviour
    {
        private class AnimationLayer
        {
            public Layer wmsLayer;
            public List<List<string>> urls;

            public override string ToString()
            {
                return wmsLayer.identifier;
            }
        }

        public event Action<DateTime, float> OnAnimationStep;

        private Globe globe;
        private DownloadManager downloadManager;

        public DateTime StartTime { get; private set; }
        public DateTime EndTime { get; private set; }
        public int NumberOfSteps { get; private set; }

        private List<DateTime> animationSteps;
        private List<AnimationLayer> layers;

        public bool IsPlaying { get; private set; }
        public bool loopAnimation = true;

        private int currentStepIndex;

        private int lastReadyStep;
        private int[] requestsCompletePerStep;
        internal List<string> layersOrder;

        private int precacheSteps = 20; // Somewhat arbitrary. Could be computed based on download speed and animation speed.

        public float PercentReady
        {
            get { return (float)lastReadyStep / NumberOfSteps; }
        }

        private int RequestsPerStep
        {
            get
            {
                int numRootTiles = globe.rootTiles.Length;
                return numRootTiles * layers.Count;
            }
        }

        internal void setPrecache(int steps)
        {
            precacheSteps = steps;
        }


        private void Start ()
        {
            globe = GetComponent<Globe>();
            downloadManager = GetComponent<DownloadManager>();
        }
	
        public void PrepareAnimation(DateTime start, DateTime end, int numSteps, List<Layer> animLayers)
        {
            if (start >= end)
            {
                throw new ArgumentException("Animation start time must be before end time");
            }

            StartTime = start;
            EndTime = end;
            NumberOfSteps = numSteps;
            requestsCompletePerStep = new int[NumberOfSteps];

            IEnumerable<DateTime> steps = GenerateTimeSteps(start, end, numSteps);
            animationSteps = steps.ToList();

            layers = new List<AnimationLayer>(animLayers.Count);
            foreach (Layer layer in animLayers)
            {
                AnimationLayer newLayer = new AnimationLayer();
                layers.Add(newLayer);

                newLayer.wmsLayer = layer;
                newLayer.urls = new List<List<string>>(numSteps);

                for (int i = 0; i < numSteps; i++)
                {
                    newLayer.urls.Add(GenerateUrLs(layer, animationSteps[i]));
                }
            }
        }

        public void StartAnimation(float durationSeconds)
        {
            if (durationSeconds <= 0)
            {
                throw new ArgumentException("Duration must be a positive number");
            }
            if (animationSteps == null)
            {
                throw new InvalidOperationException("Must call PrepareAnimation before calling StartAnimation");
            }

            IsPlaying = true;
            StartCoroutine(PlayAnimation(durationSeconds));
        }

        public void StopAnimation()
        {
            StopAllCoroutines();
            IsPlaying = false;
            currentStepIndex = 0;
        }

        private IEnumerator PlayAnimation(float duration)
        {
            float stepTime = duration / NumberOfSteps;

            // Pre-load data for the first few steps
            for (int i = 0; i < precacheSteps; i++)
            {
                LoadData(i);
            }

            // Wait for the first few steps of the animation to load
            bool firstStepsReady = false;
            while (!firstStepsReady)
            {
                yield return null;

                firstStepsReady = true;
                for (int i = 0; i < precacheSteps; i++)
                {
                    firstStepsReady = firstStepsReady && IsStepReady(i);
                }
            }

            // Play the animation one step at the time
            currentStepIndex = 0;
            while (currentStepIndex < animationSteps.Count)
            {
                // Wait for current step to be ready, in case the animation is overrunning the buffered data
                while (!IsStepReady(currentStepIndex))
                {
                    yield return null;
                }

                StepAnimation();

                // Restart if we've reached the end and we're playing a loop
                if (loopAnimation && currentStepIndex >= animationSteps.Count)
                {
                    currentStepIndex = 0;
                }

                // Pre-load data for the next steps. If we've reached the end start caching at the beginning.
                for (int i = 0; i < precacheSteps; i++)
                {
                    int stepToFetch = (currentStepIndex + i) % NumberOfSteps;
                    LoadData(stepToFetch);
                }

                yield return new WaitForSeconds(stepTime);
            }
        }

        private void StepAnimation()
        {
            DateTime timeStep = animationSteps[currentStepIndex];
            for (int i = 0; i < layers.Count; i++)
            {
                globe.LoadLayer(layersOrder != null && layersOrder.Contains(layers[i].ToString())
                    ? layersOrder.IndexOf(layers[i].ToString())
                    : 1, layers[i].wmsLayer.identifier, timeStep);
            }
            if (OnAnimationStep != null)
            {
                OnAnimationStep(timeStep, (float)(currentStepIndex + 1) / NumberOfSteps);
            }

            currentStepIndex += 1;
        }

        private bool IsStepReady(int step)
        {
            return LoadData(step);
        }

        private IEnumerable<DateTime> GenerateTimeSteps(DateTime start, DateTime end, int numSteps)
        {
            TimeSpan startToEnd = end.Subtract(start);
            double stepSeconds = startToEnd.TotalSeconds / numSteps;

            DateTime current = start;
            while (current < end)
            {
                yield return current;
                current = current.AddSeconds(stepSeconds);
            }
        }

        private int DateToStep(DateTime date)
        {
            TimeSpan startToEnd = EndTime.Subtract(StartTime);
            double stepSeconds = startToEnd.TotalSeconds / NumberOfSteps;

            TimeSpan startToStep = date.Subtract(StartTime);
            return (int)(startToStep.TotalSeconds / stepSeconds);
        }

        /// <summary>
        /// Load the data required for one step of the animation. Data that is already in the texture
        /// cache will not be requested again. This method is inexpensive to call repeatedly.
        /// </summary>
        /// <param name="step">Index of the step to load.</param>
        /// <returns>True if all the data for this step is already loaded.</returns>
        private bool LoadData(int step)
        {
            DateTime date = animationSteps[step];

            bool allTilesReady = true;
            for (int i = 0; i < layers.Count; i++)
            {
                AnimationLayer layer = layers[i];
                Debug.Assert(step < layer.urls.Count,step + " " + layer.urls.Count);
                List<string> requestsForThisDate = layer.urls[step];

                for (int j = 0; j < requestsForThisDate.Count; j++)
                {
                    // Check cache to see if this data is already available
                    string url = requestsForThisDate[j];
                    if (!globe.tileTextureCache.ExistsInCache(url))
                    {
                        // Request that the texture be downloaded. The DownloadManager
                        // does not submit duplicates if the request is already inflight,
                        // so it's safe to call this multiple times for the same URL.
                        downloadManager.RequestTexture(url, layer.wmsLayer.identifier, date, OnDataReady);
                        allTilesReady = false;
                    }
                }
            }
            return allTilesReady;
        }

        /// <summary>
        /// Generate all of the texture URLs for a layer at one date of the time step.
        /// Generating URLs is a moderately expensive operation, so we generate all
        /// of the URLs upfront and then cache them as the animation plays.
        /// </summary>
        /// <param name="layer">Layer for which to generate URLs.</param>
        /// <param name="date">Date step.</param>
        /// <returns>List of URLs for the textures required for one animation step
        /// of the specified layer.</returns>
        private List<string> GenerateUrLs(Layer layer, DateTime date)
        {
            List<string> urls = new List<string>(RequestsPerStep * NumberOfSteps);
            for (int tileId = 0; tileId < globe.rootTiles.Length; tileId++)
            {
                string url = layer.WmtsToUrl(globe.rootTiles[tileId].coords, date);
                urls.Add(url);
            }
            return urls;
        }

        private void OnDataReady(string layerNameToSet, DateTime dateTimeToSet, Texture myTexture)
        {
            //requestsComplete++;
            int step = DateToStep(dateTimeToSet);
            if (step <= requestsCompletePerStep.Length)
            {
                requestsCompletePerStep[step]++;
            }

            // Find the step furthest along the animation that is completely ready to play
            int completedStep = 0;
            while (completedStep < requestsCompletePerStep.Length && requestsCompletePerStep[completedStep] >= RequestsPerStep)
            {
                completedStep += 1;
            }
            lastReadyStep = completedStep;
        }
    }
}
