using System;
using System.Collections.Generic;
using EVRTH.Scripts.GIBS;
using EVRTH.Scripts.GlobeNS;
using UnityEngine;
using UnityEngine.UI;

namespace EVRTH.Scripts.Visualization
{
    /// <summary>
    /// Controller to drive UI for animation test scene.
    /// </summary>
    public class AnimationUIController : MonoBehaviour
    {
        public Text dateLabel;
        public Text playButtonLabel;
        public Slider progress;
        public RectTransform bufferRect;

        public DateTime startDate = new DateTime(2016, 10, 1);
        public DateTime endDate = new DateTime(2016, 11, 1);

        public Globe globe;
        private GlobeAnimationController animationController;

        private bool animationSetup;

        void Start ()
        {
            Debug.Assert(globe != null, "Globe object is not set");
            Debug.Assert(dateLabel != null, "DateLabel object is not set");
            Debug.Assert(playButtonLabel != null, "PlayButtonLabel object is not set");
            Debug.Assert(progress != null, "Progress object is not set");
            Debug.Assert(bufferRect != null, "Buffer rect object is not set");

            animationController = globe.GetComponent<GlobeAnimationController>();
        }

        private void Update()
        {
            if (animationController.PercentReady <= 1f)
            {
                Vector3 scale = bufferRect.localScale;
                scale.x = animationController.PercentReady;
                bufferRect.localScale = scale;
            }
        }
	
        public void ToggleAnimation()
        {
            if (!animationSetup)
            {
                Debug.LogFormat("Preparing animation from {0} to {1}", startDate, endDate);
                Layer layer = globe.layers[0].wmsLayer;
                animationController.PrepareAnimation(startDate, endDate, 31, new List<Layer> { layer });
                animationController.OnAnimationStep += OnAnimationStep;
                animationSetup = true;
            }

            if (animationController.IsPlaying)
            {
                animationController.StopAnimation();
                playButtonLabel.text = "Play";
            }
            else
            {
                animationController.StartAnimation(30f);
                playButtonLabel.text = "Stop";
            }
        }

        private void OnAnimationStep(DateTime date, float animProgress)
        {
            dateLabel.text = string.Format("Current Date: {0:MM/dd/yyyy}", date);
            progress.value = animProgress;
        }
    }
}
