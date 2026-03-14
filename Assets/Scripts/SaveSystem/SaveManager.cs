using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[System.Serializable]
public class SaveMetadata
{
    public string saveName;
    public string lastArea;
    public string saveTime;
    public int playerLevel;
}

public class SaveManager : MonoBehaviour
{
    private int _currentSlot = 0;

    // --- Path Helpers ---
    private string GetSlotFolder(int slot) => Path.Combine(Application.persistentDataPath, $"Slot_{slot}");
    private string GetDataPath(int slot) => Path.Combine(GetSlotFolder(slot), "save.dat");
    private string GetMetaPath(int slot) => Path.Combine(GetSlotFolder(slot), "meta.json");
    private string GetImgPath(int slot) => Path.Combine(GetSlotFolder(slot), "thumbnail.jpg");

    // --- Public Save Methods ---

    // Thumbnail
    public IEnumerator SaveGameWithScreenshot(int slot, SaveMetadata meta)
    {
        _currentSlot = slot;
        yield return new WaitForEndOfFrame();

        int thumbSize = 200;
        RenderTexture rt = new(thumbSize, thumbSize, 24);
        Texture2D screenShot = new(thumbSize, thumbSize, TextureFormat.RGB24, false);

        Camera cam = Camera.main;

        RenderTexture oldRT = cam.targetTexture; // Save old setting
        cam.targetTexture = rt;
        cam.Render();

        // Read the pixels
        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, thumbSize, thumbSize), 0, 0);
        screenShot.Apply();

        // CLEAN UP
        cam.targetTexture = oldRT; // Restore camera
        RenderTexture.active = null;
        rt.Release();
        Destroy(rt);

        byte[] bytes = screenShot.EncodeToJPG(85);
        Destroy(screenShot);

        string folder = GetSlotFolder(slot);
        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
        File.WriteAllBytes(GetImgPath(slot), bytes);

        SaveGame(meta);
    }

    // Standard save logic
    public void SaveGame(SaveMetadata meta)
    {
        string folder = GetSlotFolder(_currentSlot);
        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

        string dataPath = GetDataPath(_currentSlot);
        string tempPath = dataPath + ".tmp"; // The staging file

        try
        {
            //Gather all ISaveable data
            var saveables = FindObjectsByType<MonoBehaviour>(sortMode: FindObjectsSortMode.None).OfType<ISaveable>();
            Dictionary<string, object> saveData = new();

            foreach (var s in saveables)
            {
                saveData[s.SaveID] = s.FetchSaveData();
            }

            //Encrypt and write to the TEMP file first
            string json = JsonConvert.SerializeObject(saveData);
            string encryptedData = EncryptionHelper.Encrypt(json);
            File.WriteAllText(tempPath, encryptedData);

            //Only replace the real file if the temp write finished
            if (File.Exists(dataPath))
            {
                File.Delete(dataPath);
            }
            File.Move(tempPath, dataPath);

            //Update and save Metadata
            meta.saveTime = System.DateTime.Now.ToString("dd-MM-yyyy HH:mm");
            string metaJson = JsonUtility.ToJson(meta, true);
            File.WriteAllText(GetMetaPath(_currentSlot), metaJson);

            Debug.Log($"Save Success: Slot {_currentSlot}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Save Failed! Error: {e.Message}");

            // Clean up temp file if it exists so it doesn't clutter AppData
            if (File.Exists(tempPath)) File.Delete(tempPath);
        }
    }

    // --- Loading Logic ---

    public void LoadGame(int slot)
    {
        _currentSlot = slot;
        string path = GetDataPath(slot);

        if (!File.Exists(path))
        {
            Debug.LogWarning("No save file found at " + path);
            return;
        }

        string encryptedData = File.ReadAllText(path);
        string json = EncryptionHelper.Decrypt(encryptedData);
        var allData = JsonConvert.DeserializeObject<Dictionary<string, JToken>>(json);

        var saveables = FindObjectsByType<MonoBehaviour>(sortMode: FindObjectsSortMode.None).OfType<ISaveable>();
        foreach (var s in saveables)
        {
            if (allData.TryGetValue(s.SaveID, out JToken token))
            {
                s.LoadSaveData(token.ToString());
            }
        }
        Debug.Log($"Loaded Slot {slot}");
    }

    // --- UI Helpers ---

    public SaveMetadata GetMetaData(int slot)
    {
        string path = GetMetaPath(slot);
        if (!File.Exists(path)) return null;

        string json = File.ReadAllText(path);
        return JsonUtility.FromJson<SaveMetadata>(json);
    }

    public Sprite GetScreenshot(int slot)
    {
        string path = GetImgPath(slot);
        if (!File.Exists(path)) return null;

        byte[] bytes = File.ReadAllBytes(path);
        Texture2D tex = new(2, 2);
        tex.LoadImage(bytes);
        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
    }

#if UNITY_EDITOR
    [ContextMenu("Debug Print Decrypted Save")]
    public void DebugPrintSave()
    {
        string path = GetDataPath(_currentSlot);
        if (File.Exists(path))
        {
            string decrypted = EncryptionHelper.Decrypt(File.ReadAllText(path));
            Debug.Log($"--- DECRYPTED SLOT {_currentSlot} ---\n{decrypted}");
        }
    }
#endif
}