using System.Collections.Generic;
using System.Linq;
using System.Threading;
using EVRTH.Scripts.GIBS;
using EVRTH.Scripts.Utility;
using UnityEditor;
using UnityEngine;
using EGL =UnityEditor.EditorGUILayout;

namespace EVRTH.Editor
{
    [CustomEditor(typeof(LayerPresetLoader))]
    public class PresetBuilder : UnityEditor.Editor
    {
        private static List<string> layersList = new List<string>();
        private static bool isGettingLayers;
        private string filter;
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var pb = (LayerPresetLoader) target;
            if (isGettingLayers)
            {
                EGL.Space();
                EGL.Space();
                EGL.LabelField("Loading Layers...");
                return;
            }
            if (layersList.Count == 0)
            {
                isGettingLayers = true;
                LayerLoader.Init();
                new Thread(GetLayersList).Start();
            }
            EGL.Space();
            EGL.Space();
            EGL.LabelField("Presets");
            EGL.Space();
            for (int i = 0; i < pb.presets.Count; i++)
            {
                using (new HorizontalGroup())
                {
                    pb.presets[i].presetName =
                        EGL.TextField(
                            string.IsNullOrEmpty(pb.presets[i].presetName)
                                ? "Preset " + (i + 1)
                                : pb.presets[i].presetName, pb.presets[i].presetName);
                    if (GUILayout.Button("X"))
                    {
                        pb.presets[i] = null;
                        pb.presets.RemoveAt(i);
                        return;
                    }
                }
                EditorGUI.indentLevel++;
                EGL.LabelField("Flat Layers");
                EditorGUI.indentLevel++;
                filter = EGL.TextField("shared layer filter", filter);
                var filteredList = new List<string>(layersList);
                var preset = pb.presets[i];
                for (int j = 0; j < pb.presets[i].layersInPreset.Count; j++)
                {
                    using (new HorizontalGroup())
                    {
                        if(layersList.Contains(pb.presets[i].layersInPreset[j]))
                        {
                            if (!string.IsNullOrEmpty(filter))
                            {
                                filteredList = layersList.Where(entry => entry.ToLower().Contains(filter.ToLower()) || entry == pb.presets[i].layersInPreset[j]).ToList();
                            }
                            pb.presets[i].layersInPreset[j] =
                                filteredList[
                                EGL.Popup(j == 0 ? "Base Layer" : "Layer", filteredList.IndexOf(preset.layersInPreset[j]),
                                    filteredList.ToArray())];
                        }
                        else
                        {
                            return;
                        }
                        if (GUILayout.Button("X"))
                        {
                            pb.presets[i].RemoveLayer(j);
                            return;
                        }
                    }
                }
                if (pb.presets[i].layersInPreset.Count < 3)
                {
                    using (new HorizontalGroup())
                    {
                        //var nLayer = layersList[EGL.Popup("Layer", 0, layersList.ToArray())];
                        EGL.LabelField("New Overlay Layer");
                        if (GUILayout.Button("+"))
                        {
                            pb.presets[i].AddOverlayLayer(layersList[0]);
                            break;
                        } 
                    }
                }
                EditorGUI.indentLevel--;
                EGL.LabelField("Volumetric Layers");
                EditorGUI.indentLevel++;
                for (int j = 0; j < pb.presets[i].volumetricLayers.Count; j++)
                {
                    using (new HorizontalGroup())
                    {
                        if (!string.IsNullOrEmpty(pb.presets[i].volumetricLayers[j]))
                        {
                            if (layersList.Contains(pb.presets[i].volumetricLayers[j]))
                            {
                                if (!string.IsNullOrEmpty(filter))
                                {
                                    filteredList = layersList.Where(entry => entry.ToLower().Contains(filter.ToLower()) || entry == pb.presets[i].volumetricLayers[j]).ToList();
                                }
                                pb.presets[i].volumetricLayers[j] =
                                    filteredList[
                                        EGL.Popup("Layer " + j, filteredList.IndexOf(preset.volumetricLayers[j]),
                                            filteredList.ToArray())];
                            }
                            if (GUILayout.Button("X"))
                            {
                                pb.presets[i].volumetricLayers[j] = "";
                            }
                        }
                        else
                        {
                            if (GUILayout.Button("Activate"))
                                pb.presets[i].volumetricLayers[j] = "MODIS_Fires_All";
                        }
                    }
                }
                EditorGUI.indentLevel--;
                EditorGUI.indentLevel--;
                EGL.Space();

            }
            using (new HorizontalGroup())
            {
                if (GUILayout.Button("+"))
                {
                    pb.presets.Add(new Preset());
                }
            }
            if (GUI.changed)
            {
                Undo.RegisterCompleteObjectUndo(pb,"Preset change");
                EditorUtility.SetDirty(pb);
            }
        }

        private static void GetLayersList()
        {
            layersList = new List<string>(LayerLoader.GetLayers().Keys);
            isGettingLayers = false;
        }
    }
}
