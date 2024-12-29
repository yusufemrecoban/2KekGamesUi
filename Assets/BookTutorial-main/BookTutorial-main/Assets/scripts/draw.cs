using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIDrawAndErase : MonoBehaviour
{
    [SerializeField] private book bookScript; // Book scriptini referans al
    public RectTransform canvasRect; // Çizimin yapılacağı UI Canvas
    public GameObject brush; // Çizim yaparken kullanılacak fırça nesnesi (UI Image)
    private RectTransform currentBrushRect; // Şu anda çizim yapmakta olduğumuz fırçanın RectTransform'u
    private Vector2 lastPos; // Fare imlecinin en son pozisyonunu saklar
    private bool isDrawing = false; // Çizim modu açık mı?
    private bool isErasing = false; // Silme modu açık mı?

    private Dictionary<int, List<GameObject>> pageDrawings = new Dictionary<int, List<GameObject>>(); // Sayfa bazlı çizimleri saklar
    private int lastPageIndex = -1; // Son kontrol edilen sayfa index'i

    public static bool isDrawingModeActive = false; // Çizim modu durumu

    private const string SaveKey = "DrawingsData";

    void Start()
    {
        lastPageIndex = bookScript.GetPageIndex() + 2; // İlk sayfa index'ini sakla
        AssignCanvasRect(); // Aktif canvas'ı ayarla
        LoadDrawings(); // Önceki çizimleri yükle
        CheckVisibilityForDrawing(); // Çizim görünürlüğünü güncelle
    }

    void Update()
    {
        int currentIndex = bookScript.GetPageIndex() + 2;

        // Eğer sayfa index'i değiştiyse işlemleri güncelle
        if (currentIndex != lastPageIndex)
        {
            lastPageIndex = currentIndex; // Yeni index'i sakla
            AssignCanvasRect(); // Yeni aktif sayfanın canvasRect'ini ayarla
            CheckVisibilityForDrawing(); // Görünürlük kontrolü yap
        }

        if (isDrawing)
        {
            Draw();
        }
        else if (isErasing)
        {
            Erase();
        }
    }

    void AssignCanvasRect()
    {
        int currentIndex = bookScript.GetPageIndex() + 2; // İlk sayfa 1 olarak başlasın

        string targetPath = $"Canvas/pages/page{currentIndex}/cards{currentIndex}";
        GameObject targetObject = GameObject.Find(targetPath);

        if (targetObject != null)
        {
            canvasRect = targetObject.GetComponent<RectTransform>();
            Debug.Log($"Canvas rect updated to: {targetPath}");
        }
        else
        {
            Debug.LogError($"Target path not found: {targetPath}");
        }
    }

    void CheckVisibilityForDrawing()
    {
        int currentIndex = bookScript.GetPageIndex() + 2;

        foreach (var kvp in pageDrawings)
        {
            int pageIndex = kvp.Key;
            List<GameObject> drawings = kvp.Value;

            // Aktif sayfa dışındakileri gizle
            bool shouldBeVisible = (pageIndex == currentIndex);
            foreach (GameObject drawing in drawings)
            {
                var image = drawing.GetComponent<Image>();
                if (image != null)
                {
                    image.enabled = shouldBeVisible;
                }
            }

            Debug.Log($"Drawings on page {pageIndex} set to visible: {shouldBeVisible}");
        }
    }

    void Draw()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            CreateBrush();
        }

        if (Input.GetKey(KeyCode.Mouse0))
        {
            Vector2 mousePos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, Input.mousePosition, null, out mousePos);

            if (RectTransformUtility.RectangleContainsScreenPoint(canvasRect, Input.mousePosition, null))
            {
                if (mousePos != lastPos)
                {
                    AddAPoint(mousePos);
                    lastPos = mousePos;
                }
            }
            else
            {
                Debug.Log("Fare Canvas alanının dışında.");
            }
        }
        else
        {
            currentBrushRect = null;
        }
    }

    void Erase()
    {
        if (Input.GetKey(KeyCode.Mouse0))
        {
            Vector2 mousePos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, Input.mousePosition, null, out mousePos);

            if (RectTransformUtility.RectangleContainsScreenPoint(canvasRect, Input.mousePosition, null))
            {
                for (int i = pageDrawings[lastPageIndex].Count - 1; i >= 0; i--)
                {
                    GameObject brushInstance = pageDrawings[lastPageIndex][i];
                    RectTransform brushRect = brushInstance.GetComponent<RectTransform>();
                    if (brushRect != null && RectTransformUtility.RectangleContainsScreenPoint(brushRect, Input.mousePosition, null))
                    {
                        pageDrawings[lastPageIndex].RemoveAt(i);
                        Destroy(brushInstance);
                        Debug.Log("Bir çizim silindi.");
                    }
                }
            }
            else
            {
                Debug.Log("Fare Canvas alanının dışında.");
            }
        }
    }

    void CreateBrush()
    {
        GameObject brushInstance = Instantiate(brush, canvasRect);
        Vector2 mousePos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, Input.mousePosition, null, out mousePos);

        if (RectTransformUtility.RectangleContainsScreenPoint(canvasRect, Input.mousePosition, null))
        {
            brushInstance.GetComponent<RectTransform>().anchoredPosition = mousePos;

            // Aktif sayfanın index'ini al ve çizimi listeye ekle
            int currentIndex = bookScript.GetPageIndex() + 2;
            if (!pageDrawings.ContainsKey(currentIndex))
            {
                pageDrawings[currentIndex] = new List<GameObject>();
            }
            pageDrawings[currentIndex].Add(brushInstance);

            Debug.Log($"New drawing added to page {currentIndex}.");
        }
        else
        {
            Debug.Log("Fırça oluşturulamadı çünkü fare Canvas alanının dışında.");
        }
    }

    void AddAPoint(Vector2 pointPos)
    {
        GameObject brushInstance = Instantiate(brush, canvasRect);
        RectTransform rect = brushInstance.GetComponent<RectTransform>();
        rect.anchoredPosition = pointPos;

        // Aktif sayfanın index'ini al ve çizimi listeye ekle
        int currentIndex = bookScript.GetPageIndex() + 2;
        if (!pageDrawings.ContainsKey(currentIndex))
        {
            pageDrawings[currentIndex] = new List<GameObject>();
        }
        pageDrawings[currentIndex].Add(brushInstance);

        Debug.Log($"Yeni bir nokta eklendi: {pointPos} (Page {currentIndex}).");
    }

    public void StartDrawing()
    {
        isDrawingModeActive = true;
        isDrawing = true;
        isErasing = false;
        Debug.Log("Çizim modu aktif.");
    }

    public void StartErasing()
    {
        isDrawingModeActive = true;
        isErasing = true;
        isDrawing = false;
        Debug.Log("Silme modu aktif.");
    }

    public void StopDrawingOrErasing()
    {
        isDrawingModeActive = false;
        isDrawing = false;
        isErasing = false;
        SaveDrawings();
        Debug.Log("Çizim ve silme durduruldu.");
    }

    public void ClearAllDrawings()
    {
        foreach (var kvp in pageDrawings)
        {
            foreach (GameObject drawing in kvp.Value)
            {
                Destroy(drawing);
            }
        }
        pageDrawings.Clear();
        Debug.Log("Tüm çizimler silindi.");
    }

    private void SaveDrawings()
    {
        List<string> drawingData = new List<string>();
        foreach (var kvp in pageDrawings)
        {
            int pageIndex = kvp.Key;
            foreach (GameObject brushInstance in kvp.Value)
            {
                RectTransform rect = brushInstance.GetComponent<RectTransform>();
                int posX = Mathf.FloorToInt(rect.anchoredPosition.x);
                int posY = Mathf.FloorToInt(rect.anchoredPosition.y);

                // Parent'ın tam path bilgisini kaydet
                string parentPath = GetFullPath(rect.parent);
                drawingData.Add($"{pageIndex},{posX},{posY},{parentPath}");
            }
        }
        string serializedData = string.Join(";", drawingData);
        PlayerPrefs.SetString(SaveKey, serializedData);
        PlayerPrefs.Save();
        Debug.Log("Çizimler kaydedildi: " + serializedData);
    }

    private void LoadDrawings()
    {
        if (!PlayerPrefs.HasKey(SaveKey)) return;
        string serializedData = PlayerPrefs.GetString(SaveKey);
        string[] positions = serializedData.Split(';');

        foreach (string pos in positions)
        {
            string[] coords = pos.Split(',');
            if (coords.Length == 4)
            {
                if (int.TryParse(coords[0], out int pageIndex) && int.TryParse(coords[1], out int x) && int.TryParse(coords[2], out int y))
                {
                    string parentPath = coords[3];
                    Transform parentTransform = GameObject.Find(parentPath)?.transform;

                    if (parentTransform == null)
                    {
                        Debug.LogWarning($"Parent path '{parentPath}' bulunamadı. Varsayılan parent kullanıldı.");
                        parentTransform = canvasRect; // Varsayılan parent
                    }

                    Vector2 position = new Vector2(x, y);
                    GameObject brushInstance = Instantiate(brush, parentTransform);
                    RectTransform rect = brushInstance.GetComponent<RectTransform>();
                    rect.anchoredPosition = position;

                    if (!pageDrawings.ContainsKey(pageIndex))
                    {
                        pageDrawings[pageIndex] = new List<GameObject>();
                    }
                    pageDrawings[pageIndex].Add(brushInstance);
                }
            }
        }
        Debug.Log("Çizimler yüklendi: " + serializedData);
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

    private Transform FindParentByName(string parentName)
    {
        Transform[] allTransforms = GameObject.FindObjectsOfType<Transform>();
        foreach (Transform t in allTransforms)
        {
            if (t.name == parentName)
                return t;
        }
        return null;
    }
}
