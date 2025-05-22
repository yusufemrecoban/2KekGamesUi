using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class SaveOnChangeWithDeletion : MonoBehaviour
{
    [System.Serializable]
    public class ObjectData
    {
        public string name;
        public string path;
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
    }

    [System.Serializable]
    public class SceneData
    {
        public List<ObjectData> objects = new List<ObjectData>();
    }

    private Dictionary<GameObject, ObjectData> trackedObjects = new Dictionary<GameObject, ObjectData>();

    void Start()
    {
        InitializeTrackedObjects();
    }

    void Update()
    {
        bool anyChanged = false;

        // Şu anki tüm aktif objeleri topla
        HashSet<GameObject> currentObjects = new HashSet<GameObject>(FindObjectsOfType<GameObject>());

        // Silinen objeleri kontrol et
        List<GameObject> toRemove = new List<GameObject>();
        foreach (var tracked in trackedObjects)
        {
            if (!currentObjects.Contains(tracked.Key))
            {
                toRemove.Add(tracked.Key);
                anyChanged = true;
            }
        }
        foreach (var obj in toRemove)
        {
            trackedObjects.Remove(obj);
        }

        // Yeni veya değişmiş objeleri kontrol et
        foreach (GameObject obj in currentObjects)
        {
            if (!obj.activeInHierarchy)
                continue;

            if (!trackedObjects.ContainsKey(obj))
            {
                // Yeni obje
                trackedObjects[obj] = CreateObjectData(obj);
                anyChanged = true;
            }
            else
            {
                // Mevcut obje, değişiklik var mı kontrol et
                ObjectData prev = trackedObjects[obj];
                Transform t = obj.transform;

                if (t.position != prev.position || t.rotation != prev.rotation || t.localScale != prev.scale)
                {
                    trackedObjects[obj] = CreateObjectData(obj);
                    anyChanged = true;
                }
            }
        }

        if (anyChanged)
        {
            SaveAllObjects();
        }
    }

    void InitializeTrackedObjects()
    {
        trackedObjects.Clear();
        foreach (GameObject obj in FindObjectsOfType<GameObject>())
        {
            if (obj.activeInHierarchy)
                trackedObjects[obj] = CreateObjectData(obj);
        }
    }

    ObjectData CreateObjectData(GameObject obj)
    {
        return new ObjectData
        {
            name = obj.name,
            path = GetHierarchyPath(obj.transform),
            position = obj.transform.position,
            rotation = obj.transform.rotation,
            scale = obj.transform.localScale
        };
    }

    void SaveAllObjects()
    {
        SceneData sceneData = new SceneData();

        foreach (var pair in trackedObjects)
        {
            sceneData.objects.Add(pair.Value);
        }

        string json = JsonUtility.ToJson(sceneData, true);

        string folderPath = Application.dataPath + "/Resources";
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        string filePath = folderPath + "/scene_objects.json";
        File.WriteAllText(filePath, json);

        Debug.Log("✅ Scene objects updated (including deletions) and saved to: " + filePath);
    }

    string GetHierarchyPath(Transform obj)
    {
        string path = obj.name;
        while (obj.parent != null)
        {
            obj = obj.parent;
            path = obj.name + "/" + path;
        }
        return path;
    }
}
