using System;
using System.Collections.Generic;
using System.Linq;
using EVRTH.Scripts.GIBS;
using EVRTH.Scripts.GlobeNS;
using EVRTH.Scripts.Visualization;
using UnityEngine;
using UnityEngine.UI;

namespace EVRTH.Scripts.Utility
{
    public class AnimationRangeSelector : MonoBehaviour
    {
        [Header("Start Date (included)")]
        public Date startDate;
        [Space]
        [Header("End Date (excluded)")]
        public Date endDate;
        [Space]
        [Space]
        public bool autoPlay;
        public LayerPresetLoader presetLoader;
        public Globe globe;
        public Image progressBar;
        public Image bufferBar;
        public Text dateLabel;

        private GlobeAnimationController animationController;
        private int steps;

        private void Start()
        {
            Debug.Assert(globe != null, "Globe object is not set");
            Debug.Assert(dateLabel != null, "DateLabel object is not set");
            Debug.Assert(progressBar != null, "Progress object is not set");
            Debug.Assert(bufferBar != null, "Buffer rect object is not set");

            animationController = globe.GetComponent<GlobeAnimationController>();
            steps = (int)(endDate.ToDateTime - startDate.ToDateTime).TotalDays;
            print(steps);
            if (autoPlay)
            {
                Invoke("PrepareAnimation",1f);
                Invoke("PlayAnimation",10f);
            }
        }

        private void Update()
        {
            bufferBar.fillAmount = animationController.PercentReady;
        }

        public void PrepareAnimation()
        {
            List<Layer> toAnimate = globe.availableLayers.Where(kv => presetLoader.presets[presetLoader.currentPreset]
                .layersInPreset
                .Contains(kv.Key)).Select(k => k.Value).ToList();
            animationController.layersOrder = presetLoader.presets[presetLoader.currentPreset].layersInPreset;
            animationController.PrepareAnimation(startDate.ToDateTime, endDate.ToDateTime, steps, toAnimate);
            animationController.OnAnimationStep += OnAnimationStep;
            animationController.setPrecache((int)(steps * 0.66f));
        }

        private void OnAnimationStep(DateTime date, float animProgress)
        {
            dateLabel.text = string.Format("Current Date: {0:MM/dd/yyyy}", date);
            progressBar.fillAmount = animProgress;
        }

        public void PlayAnimation()
        {
            animationController.StartAnimation(30f);
        }

        public void StopAnimation()
        {
            animationController.StopAnimation();
        }
    }
}
