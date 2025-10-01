using UnityEngine;
using UnityEngine.UI;

public class PlayerColorView : MonoBehaviour
{
    [SerializeField] private Color whiteColor;
    [SerializeField] private Color blackColor;

    [SerializeField] private Image image;
    [SerializeField] private Image oponentImage;

    public void SetColor(int color)
    {
        image.color = color == 0 ? whiteColor : blackColor;
        oponentImage.color = color == 0 ? blackColor : whiteColor;
    }
}
