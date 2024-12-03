using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIDrawAndErase : MonoBehaviour
{
    public RectTransform canvasRect; // Çizimin yapılacağı UI Canvas
    public GameObject brush; // Çizim yaparken kullanılacak fırça nesnesi (UI Image)
    private RectTransform currentBrushRect; // Şu anda çizim yapmakta olduğumuz fırçanın RectTransform'u
    private Vector2 lastPos; // Fare imlecinin en son pozisyonunu saklar
    private bool isDrawing = false; // Çizim modu açık mı?
    private bool isErasing = false; // Silme modu açık mı?

    private List<GameObject> allBrushInstances = new List<GameObject>(); // Tüm çizimleri saklar

    public static bool isDrawingModeActive = false; // Çizim modu durumu

    void Update()
    {
        if (isDrawing)
        {
            Draw();
        }
        else if (isErasing)
        {
            Erase();
        }
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
        Debug.Log("Çizim ve silme durduruldu.");
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
                for (int i = allBrushInstances.Count - 1; i >= 0; i--)
                {
                    GameObject brushInstance = allBrushInstances[i];
                    RectTransform brushRect = brushInstance.GetComponent<RectTransform>();
                    if (brushRect != null && RectTransformUtility.RectangleContainsScreenPoint(brushRect, Input.mousePosition, null))
                    {
                        allBrushInstances.RemoveAt(i);
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
        allBrushInstances.Add(brushInstance); // Yeni çizimi listeye ekle
        currentBrushRect = brushInstance.GetComponent<RectTransform>();

        Vector2 mousePos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, Input.mousePosition, null, out mousePos);

        if (RectTransformUtility.RectangleContainsScreenPoint(canvasRect, Input.mousePosition, null))
        {
            currentBrushRect.anchoredPosition = mousePos;
            Debug.Log("Yeni bir fırça oluşturuldu ve pozisyonlandı.");
        }
        else
        {
            Debug.Log("Fırça oluşturulamadı çünkü fare Canvas alanının dışında.");
        }
    }

    void AddAPoint(Vector2 pointPos)
    {
        GameObject brushInstance = Instantiate(brush, canvasRect);
        allBrushInstances.Add(brushInstance); // Yeni çizimi listeye ekle
        RectTransform brushRect = brushInstance.GetComponent<RectTransform>();

        brushRect.anchoredPosition = pointPos;
        Debug.Log("Yeni bir nokta eklendi: " + pointPos);
    }

    public void ClearAllDrawings()
    {
        foreach (GameObject brushInstance in allBrushInstances)
        {
            Destroy(brushInstance);
        }
        allBrushInstances.Clear();
        Debug.Log("Tüm çizimler silindi.");
    }
}
