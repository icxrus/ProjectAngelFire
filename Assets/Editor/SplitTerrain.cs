using UnityEngine;
using UnityEditor;

public class SplitTerrain : EditorWindow
{
    public Terrain sourceTerrain;
    public int tileSize = 512;
    public int tileHeightmapResolution = 2049;

    [MenuItem("Tools/Terrain Tiler Fixed")]
    public static void ShowWindow()
    {
        GetWindow<SplitTerrain>("Terrain Tiler Fixed");
    }

    void OnGUI()
    {
        GUILayout.Label("Terrain Tiling Tool (Fixed)", EditorStyles.boldLabel);

        sourceTerrain = (Terrain)EditorGUILayout.ObjectField("Source Terrain", sourceTerrain, typeof(Terrain), true);
        tileSize = EditorGUILayout.IntField("Tile Size (world units)", tileSize);
        tileHeightmapResolution = EditorGUILayout.IntField("Tile Heightmap Resolution", tileHeightmapResolution);

        if (GUILayout.Button("Tile Terrain"))
        {
            if (sourceTerrain != null)
                TileTerrain();
            else
                Debug.LogError("Please assign a Source Terrain.");
        }
    }

    void TileTerrain()
    {
        TerrainData sourceData = sourceTerrain.terrainData;
        Vector3 terrainSize = sourceData.size;

        int tilesX = Mathf.CeilToInt(terrainSize.x / tileSize);
        int tilesZ = Mathf.CeilToInt(terrainSize.z / tileSize);

        // Loop through tiles
        for (int x = 0; x < tilesX; x++)
        {
            for (int z = 0; z < tilesZ; z++)
            {
                TerrainData newData = new TerrainData();
                newData.heightmapResolution = tileHeightmapResolution;
                newData.size = new Vector3(tileSize, terrainSize.y, tileSize);

                float[,] tileHeights = new float[tileHeightmapResolution, tileHeightmapResolution];

                // Fill the new tile heights by sampling the original terrain
                for (int i = 0; i < tileHeightmapResolution; i++)
                {
                    for (int j = 0; j < tileHeightmapResolution; j++)
                    {
                        // Calculate normalized position on the source terrain
                        float normX = (x * tileSize + ((float)i / (tileHeightmapResolution - 1)) * tileSize) / terrainSize.x;
                        float normZ = (z * tileSize + ((float)j / (tileHeightmapResolution - 1)) * tileSize) / terrainSize.z;

                        // Sample original heightmap
                        tileHeights[j, i] = sourceData.GetInterpolatedHeight(normX, normZ) / terrainSize.y;
                    }
                }

                newData.SetHeights(0, 0, tileHeights);

                GameObject tile = Terrain.CreateTerrainGameObject(newData);
                tile.transform.position = new Vector3(x * tileSize, 0, z * tileSize);
                tile.name = $"Tile_{x}_{z}";

                // Save TerrainData asset
                string folder = "Assets/TerrainTiles";
                if (!AssetDatabase.IsValidFolder(folder))
                    AssetDatabase.CreateFolder("Assets", "TerrainTiles");

                string path = $"{folder}/Tile_{x}_{z}.asset";
                AssetDatabase.CreateAsset(newData, path);
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"Created {tilesX * tilesZ} terrain tiles correctly.");
    }
}