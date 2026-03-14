using UnityEngine;

public class TestSaveable : SaveableEntity
{
    private int currentHealth = 100;

    public override object FetchSaveData()
    {
        return new TestSaveData
        {
            health = this.currentHealth,
            // Convert Vector3 to a simple float array
            position = new float[] { transform.position.x, transform.position.y, transform.position.z }
        };
    }

    public override void LoadSaveData(string jsonData)
    {
        TestSaveData data = JsonUtility.FromJson<TestSaveData>(jsonData);
        this.currentHealth = data.health;
        this.transform.position = new Vector3(data.position[0], data.position[1], data.position[2]);
    }
}

[System.Serializable]
public class TestSaveData
{
    public float[] position;
    public int health;
}
