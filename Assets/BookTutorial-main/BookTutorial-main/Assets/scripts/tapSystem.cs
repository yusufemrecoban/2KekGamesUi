using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class LongPressHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public float longPressDuration = 1.0f;
    private bool isPointerDown = false;
    private float pointerDownTimer = 0f;
    private bool isDragging = false;

    public GameObject newTabPrefab;
    public Transform parentTransform;
    public Transform referenceObject;
    private bool isPrefabCreated = false;

    private int totalObjectCount = 0;

    void Start()
    {
        LoadObjects(); // Oyunun başlangıcında kaydedilmiş objeleri yükle
    }

    void Update()
    {
        if (isPointerDown && !isDragging && !isPrefabCreated)
        {
            pointerDownTimer += Time.deltaTime;

            if (pointerDownTimer >= longPressDuration)
            {
                OnLongPress();
                Reset();
            }
            
        }
        
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPointerDown = true;
        pointerDownTimer = 0f;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Reset();
    }

    private void Reset()
    {
        isPointerDown = false;
        pointerDownTimer = 0f;
        isPrefabCreated = false;
    }

    private void OnLongPress()
    {
        if (UIDrawAndErase.isDrawingModeActive)
        {
            Debug.Log("Çizim modu aktif, prefab oluşturulmadı.");
            return;
        }

        OpenNewTab();
    }

    private void OpenNewTab()
    {
        if (referenceObject == null)
        {
            Debug.LogError("Referans objesi atanmamış!");
            return;
        }

        float yRotation = referenceObject.rotation.eulerAngles.y;
        GameObject newTab = Instantiate(newTabPrefab, parentTransform);

        Material newMaterial = null;
        if (yRotation >= -10 && yRotation <= 10)
        {
            newMaterial = Resources.Load<Material>("shader/Front");
            newTab.transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        else if (yRotation >= 150 && yRotation <= 200)
        {
            newMaterial = Resources.Load<Material>("shader/Back");
            newTab.transform.rotation = Quaternion.Euler(0, 180, 0);
        }

        if (newMaterial != null)
        {
            newTab.GetComponentInChildren<TMP_InputField>().textComponent.fontMaterial = newMaterial;
        }

        isPrefabCreated = true;
        newTab.SetActive(true);

        int objectID = totalObjectCount;
        newTab.name = "Object_" + objectID;

        // Yeni oluşturulan objeye LongPressDeleteHandler ve DragHandler ekle
        LongPressDeleteHandler longPressDeleteHandler = newTab.AddComponent<LongPressDeleteHandler>();
        DragHandler dragHandler = newTab.AddComponent<DragHandler>();
        dragHandler.longPressDeleteHandler = longPressDeleteHandler;

        SaveObjectData(newTab, objectID);
        totalObjectCount++;
        SaveAllObjects();
    }

    public void SaveObjectData(GameObject newTab, int index)
    {
        Vector3 position = newTab.transform.position;
        Quaternion rotation = newTab.transform.rotation;

        PlayerPrefs.SetFloat($"Object_{index}_PosX", position.x);
        PlayerPrefs.SetFloat($"Object_{index}_PosY", position.y);
        PlayerPrefs.SetFloat($"Object_{index}_PosZ", position.z);

        PlayerPrefs.SetFloat($"Object_{index}_RotX", rotation.eulerAngles.x);
        PlayerPrefs.SetFloat($"Object_{index}_RotY", rotation.eulerAngles.y);
        PlayerPrefs.SetFloat($"Object_{index}_RotZ", rotation.eulerAngles.z);

        PlayerPrefs.SetInt($"Object_{index}_Exists", 1);
    }


    private void SaveAllObjects()
    {
        PlayerPrefs.SetInt("Object_Count", totalObjectCount);
        PlayerPrefs.Save();
    }

    private void LoadObjects()
    {
        int totalObjects = PlayerPrefs.GetInt("Object_Count", 0);

        for (int i = 0; i < totalObjects; i++)
        {
            if (PlayerPrefs.GetInt($"Object_{i}_Exists", 0) == 1)
            {
                Vector3 position = new Vector3(
                    PlayerPrefs.GetFloat($"Object_{i}_PosX"),
                    PlayerPrefs.GetFloat($"Object_{i}_PosY"),
                    PlayerPrefs.GetFloat($"Object_{i}_PosZ")
                );

                Quaternion rotation = Quaternion.Euler(
                    PlayerPrefs.GetFloat($"Object_{i}_RotX"),
                    PlayerPrefs.GetFloat($"Object_{i}_RotY"),
                    PlayerPrefs.GetFloat($"Object_{i}_RotZ")
                );

                GameObject newTab = Instantiate(newTabPrefab, position, rotation, parentTransform);
                newTab.name = "Object_" + i;

                Material newMaterial = null;
                float yRotation = rotation.eulerAngles.y;
                if (yRotation >= -10 && yRotation <= 10)
                {
                    newMaterial = Resources.Load<Material>("shader/Front");
                    newTab.transform.rotation = Quaternion.Euler(0, 0, 0);
                }
                else if (yRotation >= 150 && yRotation <= 200)
                {
                    newMaterial = Resources.Load<Material>("shader/Back");
                    newTab.transform.rotation = Quaternion.Euler(0, 180, 0);
                }

                if (newMaterial != null)
                {
                    newTab.GetComponentInChildren<TMP_InputField>().textComponent.fontMaterial = newMaterial;
                }

                newTab.AddComponent<LongPressDeleteHandler>();
                DragHandler dragHandler = newTab.AddComponent<DragHandler>();
                dragHandler.longPressDeleteHandler = newTab.GetComponent<LongPressDeleteHandler>();
            }
        }
    }

    public void DeleteObject(int index)
    {
        PlayerPrefs.SetInt($"Object_{index}_Exists", 0);
        PlayerPrefs.Save();
    }
}

public class LongPressDeleteHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public float longPressDuration = 1.0f;
    private bool isPointerDown = false;
    private float pointerDownTimer = 0f;
    private bool isDragging = false;

    public void SetDragging(bool dragging)
    {
        isDragging = dragging;
    }

    void Update()
    {
        if (isPointerDown && !isDragging)
        {
            pointerDownTimer += Time.deltaTime;

            if (pointerDownTimer >= longPressDuration)
            {
                OnLongPress();
                Reset();
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPointerDown = true;
        pointerDownTimer = 0f;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Reset();
    }

    private void Reset()
    {
        isPointerDown = false;
        pointerDownTimer = 0f;
    }

    private void OnLongPress()
    {
        Debug.Log("Obje silindi!");
        int objectID = int.Parse(gameObject.name.Split('_')[1]);
        LongPressHandler longPressHandler = FindObjectOfType<LongPressHandler>();
        longPressHandler.DeleteObject(objectID);
        Destroy(gameObject);
    }
}

public class DragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Canvas canvas;
    public LongPressDeleteHandler longPressDeleteHandler;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("DragHandler, parent Canvas'ı bulamadı. Lütfen objenin bir Canvas içinde olduğundan emin olun.");
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        longPressDeleteHandler.SetDragging(true);
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (canvas != null)
        {
            rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        longPressDeleteHandler.SetDragging(false);
        canvasGroup.blocksRaycasts = true;

        // Obje konumunu kaydet
        int objectID = int.Parse(gameObject.name.Split('_')[1]);
        LongPressHandler longPressHandler = FindObjectOfType<LongPressHandler>();
        longPressHandler.SaveObjectData(gameObject, objectID);
    }
}