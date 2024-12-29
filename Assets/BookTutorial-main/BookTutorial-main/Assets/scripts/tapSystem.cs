using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;

public class LongPressHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private book bookScript; // Book scriptine referans
    public GameObject newTabPrefab; // Oluşturulacak prefab
    public Transform defaultParentTransform; // Varsayılan ebeveyn (Canvas gibi)
    private RectTransform canvasRect; // Dinamik olarak atanacak canvas

    private bool isPointerDown = false;
    private float pointerDownTimer = 0f;
    private const float longPressDuration = 1.0f;
    private const float creationCooldown = 1.0f; // Obje oluşturma arasındaki bekleme süreci
    private float lastCreationTime = 0f;

    private int totalObjectCount = 0;
    private Dictionary<int, GameObject> savedTabs = new Dictionary<int, GameObject>(); // Kaydedilen objeler
    private Dictionary<int, string> savedTexts = new Dictionary<int, string>(); // Objelerin text verilerini kaydetmek için

    private const string SaveKey = "SavedTabsData";

    public static bool isPerformingAction = false; // Yeni işlem kontrol değişkeni
    private float saveInterval = 0.5f; // 5 saniyede bir kayıt
    private float saveTimer = 0f;

    private void Start()
    {
        totalObjectCount = PlayerPrefs.GetInt("Total_Object_Count", 0); // Toplam obje sayısını yükle
        AssignCanvasRect(); // İlk aktif canvas'ı ata
        LoadObjects(); // Kaydedilmiş objeleri yükle
    }

    void Update()
    {
        saveTimer += Time.deltaTime;
        if (saveTimer >= saveInterval)
        {
            SaveObjects();
            Debug.Log("Otomatik kayıt alındı.");
            saveTimer = 0f;
        }
        
        int currentIndex = bookScript.GetPageIndex() + 2;
        if (canvasRect == null || canvasRect.name != $"page{currentIndex}")
        {
            AssignCanvasRect();
            CheckVisibilityForObjects(); // Objelerin görünürlüğünü kontrol et
        }

        if (Input.GetMouseButtonDown(0))
        {
            isPointerDown = true;
            pointerDownTimer = 0f;
        }

        if (Input.GetMouseButton(0))
        {
            pointerDownTimer += Time.deltaTime;

            if (pointerDownTimer >= longPressDuration && Time.time - lastCreationTime >= creationCooldown && !isPerformingAction)
            {
                OnLongPress();
                ResetPointerState(); // Uzun basma algılandı, tekrar tetiklenmesini önle
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            ResetPointerState();
        }
    }

    private void CheckVisibilityForObjects()
    {
        string currentParentPath = GetFullPath(canvasRect);

        foreach (var kvp in savedTabs)
        {
            int objectID = kvp.Key;
            GameObject tab = kvp.Value;

            if (tab == null) continue;

            RectTransform rect = tab.GetComponent<RectTransform>();
            if (rect == null) continue;

            string savedParentPath = GetFullPath(rect.parent);

            // Eğer kaydedilen parent path, aktif canvas path ile eşleşmiyorsa objeyi görünmez yap
            if (savedParentPath != currentParentPath)
            {
                tab.SetActive(false);
                Debug.Log($"Tab {objectID} is hidden (Saved Path: {savedParentPath}, Current Path: {currentParentPath})");
            }
            else
            {
                tab.SetActive(true);
                Debug.Log($"Tab {objectID} is visible (Saved Path: {savedParentPath}, Current Path: {currentParentPath})");
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData) {}

    public void OnPointerUp(PointerEventData eventData) {}

    private void OnLongPress()
    {
        Debug.Log("OnLongPress çağrıldı.");
        isPerformingAction = true;
        OpenNewTab();
        isPerformingAction = false;
    }

    private void OpenNewTab()
    {
        if (canvasRect == null || newTabPrefab == null) return;

        GameObject newTab = Instantiate(newTabPrefab, canvasRect);
        if (newTab == null) return;

        newTab.SetActive(true);

        int objectID = totalObjectCount;
        newTab.name = "Object_" + objectID;

        TMP_InputField inputField = newTab.GetComponentInChildren<TMP_InputField>();
        if (inputField != null)
        {
            inputField.onValueChanged.AddListener((text) => 
            {
                savedTexts[objectID] = text;
                SaveObjects(); // Metin değişikliğinde kaydı güncelle
            });
        }

        EnableDrag(newTab);
        EnableDelete(newTab);

        savedTabs[objectID] = newTab;
        SaveObjects();

        totalObjectCount++;
        PlayerPrefs.SetInt("Total_Object_Count", totalObjectCount);
        PlayerPrefs.Save();

        lastCreationTime = Time.time;
    }

    private void AssignCanvasRect()
    {
        int currentIndex = bookScript.GetPageIndex() + 2;
        string targetPath = $"Canvas/pages/page{currentIndex}/cards{currentIndex}";
        GameObject targetObject = GameObject.Find(targetPath);

         if (targetObject != null)
        {
            canvasRect = targetObject.GetComponent<RectTransform>();
            Debug.Log($"Canvas atanmış: {canvasRect.name}");
        }
        else
        {
            Debug.LogError($"Canvas bulunamadı: {targetPath}");
            canvasRect = defaultParentTransform as RectTransform;
        }
    }

    private void SaveObjects()
    {
        List<string> objectData = new List<string>();

        // Koleksiyonun bir kopyasını oluştur ve üzerinde iterasyon yap
        var savedTabsCopy = new Dictionary<int, GameObject>(savedTabs);

        foreach (var kvp in savedTabsCopy)
        {
            int objectID = kvp.Key;
            GameObject tab = kvp.Value;

            if (tab == null)
            {
                Debug.LogWarning($"Tab {objectID} null, kayıttan kaldırılıyor.");
                savedTabs.Remove(objectID);
                continue; // Null objeyi atla
            }

            RectTransform rect = tab.GetComponent<RectTransform>();
            if (rect == null) continue;

            int posX = Mathf.FloorToInt(rect.anchoredPosition.x);
            int posY = Mathf.FloorToInt(rect.anchoredPosition.y);

            string parentPath = GetFullPath(rect.parent);
            string text = savedTexts.ContainsKey(objectID) ? savedTexts[objectID] : "";

            objectData.Add($"{objectID},{posX},{posY},{text},{parentPath}");
        }

        string serializedData = string.Join(";", objectData);
        PlayerPrefs.SetString(SaveKey, serializedData);
        PlayerPrefs.Save();

        Debug.Log("Objeler kaydedildi: " + serializedData);
    }

    private void LoadObjects()
    {
        if (!PlayerPrefs.HasKey(SaveKey)) return;

        string serializedData = PlayerPrefs.GetString(SaveKey);
        string[] positions = serializedData.Split(';');

        foreach (string pos in positions)
        {
            string[] coords = pos.Split(',');
            if (coords.Length >= 5)
            {
                if (int.TryParse(coords[0], out int objectID) &&
                    int.TryParse(coords[1], out int x) &&
                    int.TryParse(coords[2], out int y))
                {
                    string text = coords[3];
                    string parentPath = coords[4];
                    Transform parentTransform = GameObject.Find(parentPath)?.transform;

                    if (parentTransform == null)
                    {
                        Debug.LogWarning($"Parent path '{parentPath}' bulunamadı. Varsayılan parent kullanıldı.");
                        parentTransform = canvasRect;
                    }

                    // Yeni obje oluşturma
                    Vector2 position = new Vector2(x, y);
                    GameObject instance = Instantiate(newTabPrefab, parentTransform);
                    RectTransform rect = instance.GetComponent<RectTransform>();
                    rect.anchoredPosition = position;

                    // TMP_InputField ile metin güncellemelerini takip et
                    TMP_InputField inputField = instance.GetComponentInChildren<TMP_InputField>();
                    if (inputField != null)
                    {
                        inputField.text = text; // Kaydedilmiş metni yükle
                        savedTexts[objectID] = text; // Kaydedilen metni dictionary'e ekle

                        // Metin değişikliklerini kaydetmek için dinleyici ekle
                        inputField.onValueChanged.AddListener((updatedText) =>
                        {
                            savedTexts[objectID] = updatedText;
                            SaveObjects(); // Her değişiklikte kaydı güncelle
                        });
                    }

                    // Obje kaydı ve işlevlerini etkinleştir
                    savedTabs[objectID] = instance;
                    EnableDrag(instance);
                    EnableDelete(instance);
                }
            }
        }

        Debug.Log("Objeler yüklendi: " + serializedData);
    }

    private string GetFullPath(Transform transform)
    {
        if (transform == null) return "None";

        string path = transform.name;
        while (transform.parent != null)
        {
            transform = transform.parent;
            path = transform.name + "/" + path;
        }

        return path;
    }

    private void ResetPointerState()
    {
        isPointerDown = false;
        pointerDownTimer = 0f;
    }

   public void EnableDrag(GameObject tab)
    {
        var dragHandler = tab.GetComponent<DragHandler>() ?? tab.AddComponent<DragHandler>();
        dragHandler.OnDragStart += () => isPerformingAction = true;
        dragHandler.OnDragEnd += () =>
        {
            isPerformingAction = false;

            // Silinmiş bir objeye işlem yapılmaması için kontrol
            if (tab == null)
            {
                Debug.LogWarning("Sürükleme bitişinde obje null, işlem yapılmadı.");
                return;
            }

            // Pozisyon değişikliğini kaydet
            RectTransform rect = tab.GetComponent<RectTransform>();
            if (rect != null)
            {
                int objectID;
                if (int.TryParse(tab.name.Replace("Object_", ""), out objectID))
                {
                    savedTabs[objectID] = tab;
                    SaveObjects();
                }
            }
        };
    }

    public void EnableDelete(GameObject tab)
    {
        var deleteHandler = tab.GetComponent<LongPressDeleteHandler>() ?? tab.AddComponent<LongPressDeleteHandler>();
        deleteHandler.OnDelete += () =>
        {
            if (!isPerformingAction)
            {
                OnTabDeleted(tab); // Silme işlemi başlatılır
                SaveObjects(); // Silme işleminden sonra kayıt al
            }
        };
        deleteHandler.OnDeleteStart += () => isPerformingAction = true;
        deleteHandler.OnDeleteEnd += () => isPerformingAction = false;
    }

    public void OnTabDeleted(GameObject tab)
    {
        if (tab == null) return;

        Debug.Log($"OnTabDeleted called for: {tab.name}");

        // Obje ID'sini adından çıkar
        if (int.TryParse(tab.name.Replace("Object_", ""), out int objectID))
        {
            // Objeyi kayıtlardan kaldır
            if (savedTabs.ContainsKey(objectID))
            {
                savedTabs.Remove(objectID);
                Debug.Log($"Tab removed from savedTabs: Object_{objectID}");
            }

            if (savedTexts.ContainsKey(objectID))
            {
                savedTexts.Remove(objectID);
                Debug.Log($"Text removed from savedTexts: Object_{objectID}");
            }
        }
        else
        {
            Debug.LogWarning($"Failed to parse Object ID from tab name: {tab.name}");
        }

        // Obje yok ediliyor
        Destroy(tab);
        Debug.Log($"Tab destroyed: {tab.name}");

        // Silme işleminden sonra kayıt al
        SaveObjects();
    }
}

public class DragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;

    public event System.Action OnDragStart;
    public event System.Action OnDragEnd;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = false;
        OnDragStart?.Invoke();
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
        canvasGroup.blocksRaycasts = true;
        OnDragEnd?.Invoke();
    }
}

public class LongPressDeleteHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private bool isPointerDown = false;
    private float pointerDownTimer = 0f;
    private const float longPressDuration = 1.0f;

    public event System.Action OnDelete;
    public event System.Action OnDeleteStart;
    public event System.Action OnDeleteEnd;

    public void OnPointerDown(PointerEventData eventData)
    {
        isPointerDown = true;
        pointerDownTimer = 0f;
        OnDeleteStart?.Invoke();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPointerDown = false;
        pointerDownTimer = 0f;
        OnDeleteEnd?.Invoke();
    }

    private void Update()
    {
        if (isPointerDown)
        {
            pointerDownTimer += Time.deltaTime;
            if (pointerDownTimer >= longPressDuration)
            {
                OnDelete?.Invoke();
                Destroy(gameObject);
                isPointerDown = false;
            }
        }
    }
}