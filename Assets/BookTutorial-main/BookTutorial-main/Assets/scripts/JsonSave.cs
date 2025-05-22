using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class SceneManager : MonoBehaviour
{
    public static SceneManager Instance;

    private List<GameObject> trackedObjects = new List<GameObject>();
    public string fileName = "sceneData.json";

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void RegisterObject(GameObject obj)
    {
        if (!trackedObjects.Contains(obj))
        {
            trackedObjects.Add(obj);
            SaveScene(); // Objeyi eklediğinde kaydet
        }
    }

    public void UnregisterObject(GameObject obj)
    {
        if (trackedObjects.Contains(obj))
        {
            trackedObjects.Remove(obj);
            SaveScene(); // Objeyi kaldırdığında da kaydet
        }
    }

    [System.Serializable]
    public class GameObjectData
    {
        public string name;
        public Vector3 position;
        public Vector3 scale;
        public string parentName;
    }

    [System.Serializable]
    public class SceneData
    {
        public List<GameObjectData> objects = new List<GameObjectData>();
    }

    public void SaveScene()
    {
        SceneData sceneData = new SceneData();

        foreach (GameObject obj in trackedObjects)
        {
            if (obj == null) continue;

            GameObjectData data = new GameObjectData
            {
                name = obj.name,
                position = obj.transform.position,
                scale = obj.transform.localScale,
                parentName = obj.transform.parent ? obj.transform.parent.name : "None"
            };

            sceneData.objects.Add(data);
        }

        string json = JsonUtility.ToJson(sceneData, true);
        string path = Application.persistentDataPath + "/" + fileName;
        File.WriteAllText(path, json);

        Debug.Log("Sahne otomatik kaydedildi: " + path);
    }
}
