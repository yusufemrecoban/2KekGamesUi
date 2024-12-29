using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;

public class ScrollItem : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [SerializeField]
    private Image childImage;

    [SerializeField]
    private float requiredHoldTime = 0.5f; // Basılı tutma süresi

    [SerializeField]
    private float cooldownTime = 1f; // İki işlem arası bekleme süresi

    private bool isPressing = false;
    private bool isDragging = false;
    private float pressTime = 0f;
    private float lastMoveTime = 0f;

    private Transform targetParent;
    private Transform originalParent;
    private Vector3 originalPosition;
    private RectTransform targetRectTransform;

    private string uniqueID; // Benzersiz ID

    [SerializeField] private book bookScript; // Aktif sayfayı almak için book script referansı

    private void Start()
    {
        if (bookScript == null)
        {
            bookScript = FindObjectOfType<book>(); // book scriptini otomatik bul
        }

        if (bookScript == null)
        {
            Debug.LogError("Book script is not assigned or not found in the scene!");
            return;
        }

        // Aktif sayfa indexini alın
        int currentIndex = bookScript.GetPageIndex() + 2; // İlk sayfa 1 olacak

        if (currentIndex <= 0)
        {
            Debug.LogError("Invalid currentIndex value. Ensure bookScript.GetPageIndex() returns a valid index.");
            return;
        }

        // Dinamik yol oluştur
        string targetPath = $"Canvas/pages/page{currentIndex}/cards{currentIndex}";
        GameObject targetObject = GameObject.Find(targetPath);

        if (targetObject != null)
        {
            targetParent = targetObject.transform;
            targetRectTransform = targetObject.GetComponent<RectTransform>();
            Debug.Log($"Target parent set to: {targetPath}");
        }
        else
        {
            Debug.LogError($"Target path not found: {targetPath}");
        }

        // Orijinal parent ve pozisyonu kaydet
        originalParent = this.transform.parent;
        originalPosition = this.transform.localPosition;

        // Pozisyonları yükle
        LoadPosition();
    }
    private void CheckVisibility()
{
    if (string.IsNullOrEmpty(uniqueID))
    {
        Debug.LogWarning("Unique ID is not set. Cannot check visibility.");
        return;
    }

    string key = $"{uniqueID}_Position";

    // PlayerPrefs'teki pozisyon ve parent bilgisini al
    if (PlayerPrefs.HasKey(key))
    {
        string savedData = PlayerPrefs.GetString(key);
        string[] values = savedData.Split(',');

        if (values.Length >= 4)
        {
            string savedParentPath = values[3];

            // Eğer parent yolu "Canvas/Scroll View/Viewport/Content" ise görünmezlik işlemi yapılmasın
            if (savedParentPath == "Canvas/Scroll View/Viewport/Content")
            {
                Debug.Log($"{uniqueID} is in the default parent (Canvas/Scroll View/Viewport/Content). Visibility check skipped.");
                return;
            }

            // Aktif sayfa index'i (bookScript'ten gelen currentIndex)
            int currentIndex = bookScript.GetPageIndex() + 2;
            string currentParentPath = $"Canvas/pages/page{currentIndex}/cards{currentIndex}";

            // Kaydedilmiş parent yoluyla aktif parent yolunu karşılaştır
            if (savedParentPath != currentParentPath)
            {
                // Farklıysa objeyi görünmez yap
                if (childImage != null)
                {
                    childImage.enabled = false;
                    Debug.Log($"{uniqueID} is now invisible because saved path ({savedParentPath}) is not the current path ({currentParentPath}).");
                }
                else
                {
                    Debug.LogWarning($"Child image is not assigned for {uniqueID}.");
                }
            }
            else
            {
                // Aynıysa objeyi görünür yap
                if (childImage != null)
                {
                    childImage.enabled = true;
                    Debug.Log($"{uniqueID} is visible because saved path matches the current path.");
                }
            }
        }
        else
        {
            Debug.LogWarning($"Invalid saved data for {uniqueID}: {savedData}");
        }
    }
    else
    {
        Debug.LogWarning($"No saved data found for {uniqueID}. Cannot check visibility.");
    }
}


    public void ChangeImage(Sprite image)
    {
        childImage.sprite = image;
    }

    public void SetUniqueID(string id)
    {
        uniqueID = id;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!isDragging)
        {
            isPressing = true;
            pressTime = 0f;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPressing = false;
        if (!isDragging)
            return;

        isDragging = false;

        // Sürükleme işlemi bittiğinde pozisyonu kaydet
        SavePosition();
    }

    private void Update()
    {
        CheckVisibility();
        if (isPressing && !isDragging)
        {
            pressTime += Time.deltaTime;
            if (pressTime >= requiredHoldTime && Time.time - lastMoveTime >= cooldownTime)
            {
                if (this.transform.parent == targetParent)
                {
                    MoveToOriginal();
                }
                else
                {
                    MoveToTarget();
                }
                lastMoveTime = Time.time;
            }
        }
    }
    

    private void MoveToTarget()
{
    // Aktif sayfa indexini al
    int currentIndex = bookScript.GetPageIndex() + 2; // Eğer mantıklıysa bırakın, yoksa +2 eklemeyin

    if (currentIndex <= 0)
    {
        Debug.LogError("Invalid currentIndex value in MoveToTarget. Ensure bookScript.GetPageIndex() returns a valid index.");
        return;
    }

    // Dinamik yol oluştur
    string targetPath = $"Canvas/pages/page{currentIndex}/cards{currentIndex}";
    Debug.Log($"MoveToTarget: Calculated target path: {targetPath}");

    GameObject targetObject = GameObject.Find(targetPath);

    if (targetObject != null)
    {
        targetParent = targetObject.transform;
        targetRectTransform = targetObject.GetComponent<RectTransform>();

        // Objeyi hedef parent'a taşı
        this.transform.SetParent(targetParent, false);

        RectTransform rectTransform = GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = Vector2.zero;

        Debug.Log($"Item moved to target: {targetPath}");

        // Pozisyonu kaydet
        SavePosition();
    }
    else
    {
        Debug.LogError($"MoveToTarget: Target path not found: {targetPath}");
    }
}



    private void MoveToOriginal()
    {
        this.transform.SetParent(originalParent);
        RectTransform rectTransform = GetComponent<RectTransform>();
        rectTransform.localPosition = originalPosition;

        Debug.Log("Item moved back to original parent and position.");

        // Pozisyonu kaydet
        SavePosition();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (this.transform.parent == targetParent)
        {
            isDragging = true;
            isPressing = false;

            Vector2 localPointerPosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(targetRectTransform, eventData.position, eventData.pressEventCamera, out localPointerPosition);

            RectTransform rectTransform = GetComponent<RectTransform>();
            Vector2 minPosition = targetRectTransform.rect.min;
            Vector2 maxPosition = targetRectTransform.rect.max;

            Vector2 clampedPosition = new Vector2(
                Mathf.Clamp(localPointerPosition.x, minPosition.x, maxPosition.x),
                Mathf.Clamp(localPointerPosition.y, minPosition.y, maxPosition.y)
            );

            rectTransform.anchoredPosition = clampedPosition;

            // Pozisyonu kaydet
            SavePosition();
        }
    }

    private void SavePosition()
    {
        if (string.IsNullOrEmpty(uniqueID))
        {
            Debug.LogWarning("Unique ID is not set. Cannot save position.");
            return;
        }

        string key = $"{uniqueID}_Position";

        // Pozisyon koordinatlarını yuvarla
        int x = Mathf.RoundToInt(this.transform.localPosition.x);
        int y = Mathf.RoundToInt(this.transform.localPosition.y);
        int z = 0;

        // Parent'ın tam yolunu al
        string parentPath = GetFullParentPath(this.transform.parent);

        // Yuvarlanmış pozisyon ve parent yolunu kaydet
        string data = $"{x},{y},{z},{parentPath}";
        PlayerPrefs.SetString(key, data);
        PlayerPrefs.Save();

        Debug.Log($"Position saved for {uniqueID}: {data}");
    }

    private string GetFullParentPath(Transform parent)
    {
        if (parent == null)
            return "None";

        string path = parent.name;
        while (parent.parent != null)
        {
            parent = parent.parent;
            path = $"{parent.name}/{path}";
        }
        return path;
    }

   private void LoadPosition()
    {
        if (string.IsNullOrEmpty(uniqueID))
        {
            Debug.LogWarning("Unique ID is not set. Cannot load position.");
            return;
        }

        string key = $"{uniqueID}_Position";

        if (PlayerPrefs.HasKey(key))
        {
            string data = PlayerPrefs.GetString(key);
            string[] values = data.Split(',');

            if (values.Length >= 4)
            {
                float.TryParse(values[0], out float x);
                float.TryParse(values[1], out float y);
                float.TryParse(values[2], out float z);
                string parentPath = values[3];

                Debug.Log($"Attempting to load parent: {parentPath}");

                // Pozisyonu uygula
                this.transform.localPosition = new Vector3(x, y, z);

                // Kaydedilen parent'ı bul ve objeyi oraya taşı
                Transform parentTransform = GameObject.Find(parentPath)?.transform;
                if (parentTransform != null)
                {
                    this.transform.SetParent(parentTransform, false);
                    Debug.Log($"Parent found and set for {uniqueID}: {parentPath}");
                }
                else
                {
                    Debug.LogWarning($"Parent '{parentPath}' not found for {uniqueID}. Keeping original parent.");
                }
            }
            else
            {
                Debug.LogWarning($"Invalid data for {uniqueID}: {data}");
            }
        }
        else
        {
            Debug.LogWarning($"No saved position found for {uniqueID}. Applying default position.");
        }
    }

}
