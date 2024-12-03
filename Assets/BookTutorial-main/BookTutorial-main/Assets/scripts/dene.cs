using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class SpritData
{
    public string spriteName; // Sprite'ın dosya yolu
    public string materialName; // Material'in dosya yolu    
    public float posX;
    public float posY;
    public float width;
    public float height;
}

[System.Serializable]
public class TextData
{
    public string textContent; // Gösterilecek metin
    public string textMaterialName;// Material'in dosya yolu
    public string fontName; // Font'un dosya yolu
    public int fontSize; // Font boyutu
    public Color textColor; // Metin rengi
    public float posX;
    public float posY;
    public float width;
    public float height;
}

[System.Serializable]
public class PageData
{
    public string hierarchParentName; // Hiyerarşi parent ismini belirtecek alan
    public List<SpritData> Images; // Sayfa içindeki resimler
    public List<TextData> Texts; // Sayfa içindeki metinler
}

[System.Serializable]
public class BookData
{
    public List<PageData> bookPages; // Kitap sayfaları
}

public class dene : MonoBehaviour
{
    public string veri;

    private void Start()
    {
        PlayerPrefs.SetString("ObjeJson", veri);
        PlayerPrefs.Save();

        LoadDataAndCreateUIElements();
    }

    private void LoadDataAndCreateUIElements()
    {
        if (veri != null)
        {
            // JSON verisini BookData'ya çevir
            BookData bookData = JsonUtility.FromJson<BookData>(veri);

            Debug.Log("Veri yüklendi: " + veri);

            // Her sayfa için UI elementlerini oluştur
            foreach (var pageData in bookData.bookPages)
            {
                // Hiyerarşi parent objesini bul
                GameObject hierarchParent = GameObject.Find(pageData.hierarchParentName);

                if (hierarchParent != null)
                {
                    // Image nesnelerini oluştur
                    foreach (var imageData in pageData.Images)
                    {
                        CreateImage(imageData, hierarchParent);
                    }

                    // Text nesnelerini oluştur
                    foreach (var textData in pageData.Texts)
                    {
                        CreateText(textData, hierarchParent);
                    }
                }
                else
                {
                    Debug.LogWarning("Hiyerarşide parent objesi bulunamadı: " + pageData.hierarchParentName);
                }
            }
        }
        else
        {
            Debug.LogWarning("Veri bulunamadı!");
        }
    }

    private void CreateImage(SpritData data, GameObject hierarchParent)
    {
        // Yeni bir GameObject oluştur
        GameObject newImage = new GameObject("Image");

        // Hierarchy'de doğru parent objesine ekle
        newImage.transform.SetParent(hierarchParent.transform);

        // RectTransform ekle ve ayarlarını yap
        RectTransform rectTransform = newImage.AddComponent<RectTransform>();
        rectTransform.anchoredPosition = new Vector2(data.posX, data.posY);
        rectTransform.sizeDelta = new Vector2(data.width, data.height);

        Image imageComponent = newImage.AddComponent<Image>();

        // Sprite'ı yükle ve atama yap
        Sprite sprite = Resources.Load<Sprite>(data.spriteName);
        if (sprite != null)
        {
            imageComponent.sprite = sprite;
        }
        else
        {
            Debug.Log("Sprite yok: " + data.spriteName);
        }

        // Material'ı yükle ve atama yap
        Material material = Resources.Load<Material>(data.materialName);
        if (material != null)
        {
            imageComponent.material = material;
        }
        else
        {
            Debug.Log("Material yok: " + data.materialName);
        }
    }

    private void CreateText(TextData data, GameObject hierarchParent)
    {
        // Yeni bir GameObject oluştur
        GameObject newText = new GameObject("Text");

        // Hierarchy'de doğru parent objesine ekle
        newText.transform.SetParent(hierarchParent.transform);

        // RectTransform ekle ve ayarlarını yap
        RectTransform rectTransform = newText.AddComponent<RectTransform>();
        rectTransform.anchoredPosition = new Vector2(data.posX, data.posY);
        rectTransform.sizeDelta = new Vector2(data.width, data.height);

        Text textComponent = newText.AddComponent<Text>();

        // Metin içeriğini ayarla
        textComponent.text = data.textContent;

        // Font'u yükle ve atama yap
        Font font = Resources.Load<Font>(data.fontName);
        if (font != null)
        {
            textComponent.font = font;
        }
        else
        {
            Debug.Log("Font yok: " + data.fontName);
        }

        // Font boyutunu ayarla
        textComponent.fontSize = data.fontSize;

        // Metin rengini ayarla
        textComponent.color = data.textColor;

        // Material'ı yükle ve atama yap
        Material material = Resources.Load<Material>(data.textMaterialName);
        if (material != null)
        {
            textComponent.material = material;
        }
        else
        {
            Debug.Log("Material yok: " + data.textMaterialName);
        }
    }
}