using System.Collections.Generic;
using UnityEngine;

public class Scroll : MonoBehaviour
{
    [SerializeField]
    private Transform scrollViewContent;

    [SerializeField]
    private GameObject scrollPrefab;

    [SerializeField]
    private List<Sprite> scrollImages;

    private string tagToImage = "ScrollImage";

    private int imageId = 0;

    private void Start()
    {
        foreach (Sprite scrollImage in scrollImages)
        {
            GameObject newScrollImage = Instantiate(scrollPrefab, scrollViewContent);

            if (newScrollImage.TryGetComponent<ScrollItem>(out ScrollItem item))
            {
                // Görseli değiştir
                item.ChangeImage(scrollImage);

                // Benzersiz ID için imageId kullanılıyor
                item.SetUniqueID($"{newScrollImage.name}_{imageId}");
            }

            // Tag ata
            newScrollImage.tag = tagToImage;

            // Benzersiz ID artır
            imageId++;
        }
    }
}
