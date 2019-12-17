using System;
using System.Collections;
using System.Collections.Generic;
using EVRTH.Scripts.GlobeNS;
using EVRTH.Scripts.Utility;
using EVRTH.Scripts.Visualization;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

namespace EVRTH.Scripts.GIBS
{
    /// <summary>
    /// Controls the application of layers through the dropdown ui.
    /// Just an example of how to interact with the main GIBS Data source
    /// </summary>
    public class GlobeControls : MonoBehaviour
    {

        private Globe globe;
        private LayerApplier layerApplier;

        public Dropdown flatLayerDropdown;

        public Dropdown extrudedlayerDropdown;

        private IEnumerator Start()
        {
            globe = GetComponent<Globe>();
            layerApplier = GetComponent<LayerApplier>();
            WaitForEndOfFrame wait = new WaitForEndOfFrame();
            if (flatLayerDropdown == null)
            {
                flatLayerDropdown = FindObjectOfType<Dropdown>();
            }
            while (!globe.parsedAvailableLayers || globe.availableLayers == null)
            {
                yield return wait;
            }
            PopulateDropdownItems();
        }

        private void PopulateDropdownItems()
        {
            //Add layers from globe dict
            List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();
            foreach (string dataset in globe.availableLayers.Keys)
            {
                Dropdown.OptionData newOption = new Dropdown.OptionData(dataset);
                options.Add(newOption);
            }

            flatLayerDropdown.options = options;

            if (extrudedlayerDropdown != null)
            {
                extrudedlayerDropdown.options =
                    new List<Dropdown.OptionData> {new Dropdown.OptionData("AMSR2_Surface_Rain_Rate_Day"), new Dropdown.OptionData("AIRS_CO_Total_Column_Day") };
            }
        }

        public void OnFlatLayerDropdownChange()
        {
            UpdateGlobeLayer(0, flatLayerDropdown, false);
        }


        public void OnExtrudedLayerDropdownChange()
        {
            UpdateGlobeLayer(0, extrudedlayerDropdown, true);
        }


        private void UpdateGlobeLayer(int layerIndex, Dropdown dropdown, bool isExtruded)
        {
            string layerName = dropdown.options[dropdown.value].text;
            GlobeLayerInfo globeLayer = globe.layers[layerIndex];

            if (globeLayer.name == null || layerName != globeLayer.name)
            {
                if (isExtruded)
                {
                    Debug.LogFormat("Setting extruded layer {0} to {1}", layerIndex, layerName);
                    layerApplier.ApplyLayer(layerName, new DateTime(2016, 8, 16), LayerApplier.LayerVisualizationStyle.Volumetric, layerIndex);
                }
                else
                {
                    Debug.LogFormat("Setting flat layer {0} to {1}", layerIndex, layerName);
                    layerApplier.ApplyLayer(layerName, new DateTime(2016, 8, 16), LayerApplier.LayerVisualizationStyle.Flat, layerIndex);
                }
            }
        }
    }
}
