using UnityEngine;

public interface ISaveable
{
    string SaveID { get; }

    object FetchSaveData();

    void LoadSaveData(string jsonData);
}
