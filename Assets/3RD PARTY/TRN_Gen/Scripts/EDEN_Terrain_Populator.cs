using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace GapperGames
{
    public class EDEN_Terrain_Populator : MonoBehaviour
    {
        [Header("Objects")]

        [SerializeField] private int seed;
        [SerializeField] private EDENTerrainObject[] prefabs;
        [SerializeField] private EDENDetaiTexture[] detailTextures;
        [HideInInspector] public bool spawnPrefabs = true;

        Terrain terrain;
        float[,] heightMap;

        public void SpawnTrees()
        {
            terrain = GetComponent<Terrain>();
            int width = (int)terrain.terrainData.size.x;
            int height = (int)terrain.terrainData.size.y;

            List<TreeInstance> trees = new List<TreeInstance>();

            TreePrototype[] prototypes = new TreePrototype[prefabs.Length];
            for (int i = 0; i < prefabs.Length; i++)
            {
                TreePrototype proto = new TreePrototype();
                proto.prefab = prefabs[i].prefab;
                prototypes[i] = proto;
            }
            terrain.terrainData.treePrototypes = prototypes;

            for (int i = 0; i < prefabs.Length; i++)
            {
                TreeInstance tree = new TreeInstance();
                for (int x = 0; x < width; x++)
                {
                    for (int z = 0; z < width; z++)
                    {
                        if (x % prefabs[i].spacing == 0 && z % prefabs[i].spacing == 0)
                        {
                            float xOffset = (random(new Vector2(z + seed * i, x + seed * i)) * 2) - 1;
                            float zOffset = (random(new Vector2(x + seed * i, z + seed * i)) * 2) - 1;

                            Vector2 offset = (new Vector2(xOffset, zOffset) * (prefabs[i].spacing / 2));

                            float _x = (float)(x + offset.x) / width;
                            float _z = (float)(z + offset.y) / width;

                            float _y = terrain.terrainData.GetHeight((int)(_x * terrain.terrainData.heightmapResolution), (int)(_z * terrain.terrainData.heightmapResolution)) / (float)height;
                            float grad = terrain.terrainData.GetSteepness(_x, _z);

                            tree.position = new Vector3(_x, _y, _z);
                            tree.heightScale = (random(new Vector2(z + seed, x + seed)) * (prefabs[i].sizeRange.y - prefabs[i].sizeRange.x)) + prefabs[i].sizeRange.x;
                            tree.widthScale = tree.heightScale;
                            tree.rotation = UnityEngine.Random.Range(0, 360);

                            tree.prototypeIndex = i;

                            float minSteep = min(prefabs[i].maxSteepness, prefabs[i].minSteepness);
                            float maxSteep = max(prefabs[i].maxSteepness, prefabs[i].minSteepness);
                            bool invertSteepness = prefabs[i].maxSteepness < prefabs[i].minSteepness;

                            bool steepnessMask = grad > minSteep && grad < maxSteep;
                            if (invertSteepness) { steepnessMask = !steepnessMask; }

                            if (_y > prefabs[i].heightRange.x / height && _y < prefabs[i].heightRange.y / height && steepnessMask)
                            {
                                trees.Add(tree);
                            }
                        }
                    }
                }
            }

            terrain.terrainData.treeInstances = trees.ToArray();

            terrain.terrainData.size = new Vector3(width - 1, height, width);
            terrain.terrainData.size = new Vector3(width, height, width);
        }

        public static float random(float2 uv)
        {
            float rnd = frac(sin(dot(uv.xy, float2(12.9898f, 78.233f))) * 43758.5453123f);
            return rnd;
        }

        public Vector3 DetailToWorld(int x, int y)
        {
            //XZ world position
            return new Vector3(
                terrain.GetPosition().x + (((float)x / (float)terrain.terrainData.detailWidth) * (terrain.terrainData.size.x)),
                0f,
                terrain.GetPosition().z + (((float)y / (float)terrain.terrainData.detailHeight) * (terrain.terrainData.size.z))
                );
        }

        public Vector2 GetNormalizedPosition(Vector3 worldPosition)
        {
            Vector3 localPos = terrain.transform.InverseTransformPoint(worldPosition);

            //Position relative to terrain as 0-1 value
            return new Vector2(
                localPos.x / terrain.terrainData.size.x,
                localPos.z / terrain.terrainData.size.z);
        }

        public void SampleHeight(Vector2 position, out float height, out float worldHeight, out float normalizedHeight)
        {
            height = terrain.terrainData.GetHeight(
                Mathf.CeilToInt(position.x * terrain.terrainData.heightmapTexture.width),
                Mathf.CeilToInt(position.y * terrain.terrainData.heightmapTexture.height)
                );

            worldHeight = height + terrain.transform.position.y;
            //Normalized height value (0-1)
            normalizedHeight = height / terrain.terrainData.size.y;
        }

        public void SpawnDetailTextures()
        {
            terrain = GetComponent<Terrain>();
            //terrain.terrainData.SetDetailResolution(512, 32);

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(terrain);
            UnityEditor.EditorUtility.SetDirty(terrain.terrainData);
#endif

            int width = (int)terrain.terrainData.size.x;
            int height = (int)terrain.terrainData.size.y;

            //Initialize Details

            List<DetailPrototype> prototypes = new List<DetailPrototype>();
            for (int i = 0; i < detailTextures.Length; i++)
            {
                DetailPrototype proto = new DetailPrototype();
                proto.usePrototypeMesh = false;
                proto.renderMode = DetailRenderMode.Grass;
                proto.healthyColor = Color.white;
                proto.dryColor = Color.white;
                proto.prototypeTexture = detailTextures[i].texture;
                proto.minWidth = detailTextures[i].sizeRange.x;
                proto.maxWidth = detailTextures[i].sizeRange.y;
                proto.minHeight = detailTextures[i].sizeRange.x;
                proto.maxHeight = detailTextures[i].sizeRange.y;
                proto.dryColor = detailTextures[i].color;
                proto.healthyColor = detailTextures[i].color;
                proto.useDensityScaling = true;
                proto.alignToGround = 1;
                proto.noiseSeed = 0;
                proto.noiseSpread = 0.1f;
                prototypes.Add(proto);
            }
            terrain.terrainData.detailPrototypes = prototypes.ToArray();

            //Spawn Details

            List<int[,]> detailMaps = new List<int[,]>();

            for (int i = 0; i < detailTextures.Length; i++)
            {
                int[,] map = new int[terrain.terrainData.detailWidth, terrain.terrainData.detailWidth];
                detailMaps.Add(map);
                //terrain.terrainData.detailPrototypes[i].Validate();
            }

            for (int x = 0; x < terrain.terrainData.detailWidth; x++)
            {
                for (int z = 0; z < terrain.terrainData.detailWidth; z++)
                {
                    Vector3 wPos = DetailToWorld(z, x);
                    Vector2 normPos = GetNormalizedPosition(wPos);
                    SampleHeight(normPos, out _, out wPos.y, out _);

                    float grad = terrain.terrainData.GetSteepness(normPos.x, normPos.y);

                    for (int i = 0; i < detailTextures.Length; i++)
                    {
                        float spacing = detailTextures[i].randomOffset;
                        int xPos = Mathf.Clamp(x + (int)UnityEngine.Random.Range(-spacing, spacing), 1, terrain.terrainData.detailWidth - 1);
                        int zPos = Mathf.Clamp(z + (int)UnityEngine.Random.Range(-spacing, spacing), 1, terrain.terrainData.detailWidth - 1);
                        //int xPos = Mathf.Clamp(x, 1, terrain.terrainData.detailWidth - 1);
                        //int zPos = Mathf.Clamp(z, 1, terrain.terrainData.detailWidth - 1);

                        float minSteep = min(detailTextures[i].maxSteepness, detailTextures[i].minSteepness);
                        float maxSteep = max(detailTextures[i].maxSteepness, detailTextures[i].minSteepness);
                        bool invertSteepness = detailTextures[i].maxSteepness < detailTextures[i].minSteepness;

                        bool steepnessMask = grad > minSteep && grad < maxSteep;
                        if (invertSteepness) { steepnessMask = !steepnessMask; }

                        bool spawnChance = UnityEngine.Random.Range(0, 100) > detailTextures[i].spawnChance;

                        if (wPos.y > detailTextures[i].heightRange.x && wPos.y < detailTextures[i].heightRange.y && steepnessMask && (x % detailTextures[i].spacing == 0 && z % detailTextures[i].spacing == 0) && spawnChance)
                        {
                            detailMaps[i][xPos, zPos] = detailTextures[i].instanceCoundPerPatch;
                            //detailMaps[i][xPos, zPos] = width / 1000;
                        }
                        else
                        {
                            detailMaps[i][xPos, zPos] = 0;
                        }
                    }
                }
            }

            for (int i = 0; i < detailMaps.Count; i++)
            {
                terrain.terrainData.SetDetailLayer(0, 0, i, detailMaps[i]);
            }

            //terrain.terrainData.SetDetailResolution(4032, 32);

            terrain.terrainData.size = new Vector3(width - 1, height, width);
            terrain.terrainData.size = new Vector3(width, height, width);

            terrain.Flush();
        }
    }

    [System.Serializable]
    public class EDENTerrainObject
    {
        public GameObject prefab;
        public int spacing = 5;
        [Range(0, 90)] public float minSteepness = 0;
        [Range(0, 90)] public float maxSteepness = 20;
        public Vector2 sizeRange = new Vector2(0.9f, 1.1f);
        public Vector2 heightRange = new Vector2(20, 500);
    }

    [System.Serializable]
    public class EDENDetaiTexture
    {
        public Texture2D texture;
        public int spacing = 1;
        public Color color = Color.white;
        [Range(0, 90)] public float minSteepness = 0;
        [Range(0, 90)] public float maxSteepness = 20;
        [Range(0, 10)] public int randomOffset = 0;
        [Range(0, 50)] public int instanceCoundPerPatch = 10;
        [Range(0, 100)] public int spawnChance = 50;
        public Vector2 sizeRange = new Vector2(0.9f, 1.1f);
        public Vector2 heightRange = new Vector2(20, 500);
    }
}
