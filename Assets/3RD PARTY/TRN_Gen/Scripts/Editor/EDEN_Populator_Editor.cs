using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace GapperGames
{
    [CustomEditor(typeof(EDEN_Terrain_Populator))]
    public class EDEN_Populator_Editor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EDEN_Terrain_Populator terrain = (EDEN_Terrain_Populator)target;

            if (GUILayout.Button("Spawn Prefabs"))
            {
                terrain.SpawnTrees();
            }

            if (GUILayout.Button("Spawn Details"))
            {
                terrain.SpawnDetailTextures();
            }
        }
    }
}
