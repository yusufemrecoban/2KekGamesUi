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

    private void Start()
    {
        // Hedef parent'ı bul
        GameObject targetObject = GameObject.Find("Canvas/pages/page1/cards1");
        if (targetObject != null)
        {
            targetParent = targetObject.transform;
            targetRectTransform = targetObject.GetComponent<RectTransform>();
        }
        else
        {
            Debug.LogError("Target object 'cards1' not found. Please check the hierarchy path.");
        }

        // Orijinal parent ve pozisyonu kaydet
        originalParent = this.transform.parent;
        originalPosition = this.transform.localPosition;

        // Pozisyonları yükle
        LoadPosition();
        // PlayerPrefs.DeleteAll();
        // PlayerPrefs.Save();
        // Debug.Log("All PlayerPrefs data deleted.");
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
        if (targetParent != null)
        {
            this.transform.SetParent(targetParent, false);

            RectTransform rectTransform = GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = Vector2.zero;

            Debug.Log("Item moved to target parent and centered.");

            // Pozisyonu kaydet
            SavePosition();
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
        Vector3 position = this.transform.localPosition;
        string parentName = this.transform.parent != null ? this.transform.parent.name : "None";

        string data = $"{position.x},{position.y},{position.z},{parentName}";
        PlayerPrefs.SetString(key, data);
        PlayerPrefs.Save();

        Debug.Log($"Position saved for {uniqueID}: {data}");
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

            if (values.Length >= 6)
            {
                // x ve y'nin tam sayıları birleşiyor, z doğrudan alınıyor
                string xString = values[0] + "." + values[1];
                string yString = values[2] + "." + values[3];
                string zString = values[4];
                string parentName = values[5];

                // Koordinatları float'a dönüştür ve işle
                if (float.TryParse(xString, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float x) &&
                    float.TryParse(yString, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float y) &&
                    float.TryParse(zString, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float z))
                {
                    // Transform pozisyonunu ayarla
                    this.transform.localPosition = new Vector3(x, y, z);

                    // Parent nesnesini bul ve ata
                    Transform parentTransform = GameObject.Find(parentName)?.transform;

                    if (parentTransform != null)
                    {
                        this.transform.SetParent(parentTransform, false);
                    }
                    else
                    {
                        Debug.LogWarning($"Parent '{parentName}' not found for {uniqueID}. Keeping original parent.");
                    }

                    Debug.Log($"Position and parent loaded for {uniqueID}: {data}");
                }
                else
                {
                    Debug.LogWarning($"Invalid position data for {uniqueID}. Resetting to default position.");
                }
            }
            else
            {
                Debug.LogWarning($"Incomplete data for {uniqueID}. Expected 6 elements, got {values.Length}: {data}");
            }
        }
        else
        {
            Debug.LogWarning($"No saved position found for {uniqueID}. Applying default position.");
        }
    }

}
