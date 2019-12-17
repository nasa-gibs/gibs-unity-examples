using System;
using System.Collections;
using System.Collections.Generic;
using EVRTH.Scripts.GIBS;
using EVRTH.Scripts.GlobeNS;
using EVRTH.Scripts.Visualization;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace EVRTH.Scripts.Utility
{
    public class FeatureWalkthroughManager : MonoBehaviour
    {
        public LayerApplier layerApplier;
        public Globe globe;
        public List<UnityEvent> actions;
        public int currAction;
        public float cooldown;
        public Text headerText;
        public Text stepDescriptionDisplay;
        public Text dateLabelText;
        public bool nextStepTrigger;
        public bool prepAnimationTrigger;
        public bool playAnimationTrigger;
        public bool stopAnimationTrigger;
        public float animLayer;
        public DateTime startDate = new DateTime(2016, 10, 1);
        public DateTime endDate = new DateTime(2016, 11, 1);
        private float lastAction;
        private bool isReady;
        private GlobeAnimationController animationController;


        private IEnumerator Start()
        {
            WaitForEndOfFrame wait = new WaitForEndOfFrame();
            lastAction = float.MinValue;
            SetupLayerEvents();
            while (!globe.parsedAvailableLayers || globe.availableLayers == null)
            {
                yield return wait;
            }
            NextAction();
            isReady = true;
            animationController = globe.GetComponent<GlobeAnimationController>();
        }

        //sets up pre-scripted layer changes
        //done in this way so that it is done in addition to any other things already on the events such as turning on/off gameobjects
        //or triggering methods on other GameObjects
        private void SetupLayerEvents()
        {
            if (actions == null)
            {
                actions = new List<UnityEvent>();
            }
            if(actions.Count == 0)
                actions.Add(new UnityEvent());
            (actions[0] ?? (actions[0] = new UnityEvent())).AddListener(() =>
            {
                UpdateGlobeLayer(0, "BlueMarble_NextGeneration");
            });

            if (actions.Count == 1)
                actions.Add(new UnityEvent());
            (actions[1] ?? (actions[1] = new UnityEvent())).AddListener(() =>
            {
                UpdateGlobeLayer(1, "MODIS_Fires_All");
                headerText.text = "Fires";
                stepDescriptionDisplay.text = "Here are the fires started over just a few months in 2016 as recorded by the infrared sensor on a camera called MODIS.";
            });

            if (actions.Count == 2)
                actions.Add(new UnityEvent());
            (actions[2] ?? (actions[2] = new UnityEvent())).AddListener(() =>
            {
                headerText.text = "Carbon Monoxide";
                UpdateGlobeLayer(0, "AIRS_CO_Total_Column_Day",true);
                stepDescriptionDisplay.text = "These fires release life-threatening pollutants which can be seen by an atmospheric sounder called AIRS.";
            });

            if (actions.Count == 3)
                actions.Add(new UnityEvent());
            (actions[3] ?? (actions[3] = new UnityEvent())).AddListener(() =>
            {
                headerText.text = "Perfect Storm";
                //ClearVolumetricLayer();
                UpdateGlobeLayer(1, "MODIS_Fires_All");
                UpdateGlobeLayer(2, "AMSR2_Surface_Rain_Rate_Day");
                stepDescriptionDisplay.text =
                    "In 2015, the El Niño phenomenon pushed rains offshore and the fires burned out of control";
            });

        }

        private void Update()
        {
            if (!isReady) return;
            if (nextStepTrigger)
            {
                nextStepTrigger = false;
                //and enough time has elapsed
                if (Time.realtimeSinceStartup - lastAction > cooldown)
                {
                    //do the next action in the time-line
                    NextAction();
                    lastAction = Time.realtimeSinceStartup;
                }
            }

            if (prepAnimationTrigger)
            {
                prepAnimationTrigger = false;
                PrepAnimation();
            }

            if (playAnimationTrigger)
            {
                playAnimationTrigger = false;
                PlayAnimation();
            }

            if (stopAnimationTrigger)
            {
                stopAnimationTrigger = false;
                StopAnimation();
            }
        }


        public void NextAction()
        {
            if (currAction == actions.Count)
                return;

            //perform the next event in the series as long as its not null
            if (actions[currAction] != null)
                actions[currAction].Invoke();

            //increment the current action
            currAction++;
        }

        public void PrepAnimation()
        {
            Debug.LogFormat("Preparing animation from {0} to {1}", startDate, endDate);
            Layer layer = globe.layers[1].wmsLayer;
            animationController.PrepareAnimation(startDate, endDate, 31, new List<Layer> { layer });
            animationController.OnAnimationStep += OnAnimationStep;
        }

        public void PlayAnimation(float duration = 30f)
        {
            animationController.StartAnimation(duration);
        }

        public void StopAnimation()
        {
            animationController.StopAnimation();
        }

        private void UpdateGlobeLayer(int layerIndex,string layerName, bool isExtruded = false)
        {
            GlobeLayerInfo globeLayer = globe.layers[layerIndex];
            var date = new DateTime(2015, 9, 16);
            if (globeLayer.name == null || layerName != globeLayer.name || isExtruded)
            {
                if (isExtruded)
                {
                    Debug.LogFormat("Setting extruded layer {0} to {1}", layerIndex, layerName);
                        dateLabelText.text = string.Format("Current Date: {0:MM/dd/yyyy}", date);
                    layerApplier.ApplyLayer(layerName, date, LayerApplier.LayerVisualizationStyle.Volumetric, layerIndex);
                }
                else
                {
                    dateLabelText.text = string.Format("Current Date: {0:MM/dd/yyyy}", date);
                    Debug.LogFormat("Setting flat layer {0} to {1}", layerIndex, layerName);
                    layerApplier.ApplyLayer(layerName, date, LayerApplier.LayerVisualizationStyle.Flat, layerIndex);
                }
            }
        }

        private void OnAnimationStep(DateTime date, float animProgress)
        {
            dateLabelText.text = string.Format("Current Date: {0:MM/dd/yyyy}", date);
        }
    }
}
