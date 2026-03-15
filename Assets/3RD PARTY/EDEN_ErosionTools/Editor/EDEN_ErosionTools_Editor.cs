using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace GapperGames
{
    [CustomEditor(typeof(EDEN_ErosionTools))]
    public class EDEN_ErosionTools_Editor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EDEN_ErosionTools terrain = (EDEN_ErosionTools)target;

            if (GUILayout.Button("Hydraulic Erode"))
            {
                terrain.HydraulicErode();
            }

            if (GUILayout.Button("Wind Erode"))
            {
                terrain.WindErode();
            }

            if (GUILayout.Button("Terrace"))
            {
                terrain.Terrace();
            }

            if (GUILayout.Button("Smooth"))
            {
                terrain.Smooth(terrain.SmoothingWidth);
            }

            if (GUILayout.Button("Sharpen"))
            {
                terrain.Sharpen();
            }
        }
    }
}
