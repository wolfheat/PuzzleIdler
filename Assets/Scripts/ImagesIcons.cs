using UnityEngine;
public enum IconType{CPS,GPS,Coins,Gems, Multiply,Research,Buildings,Upgrades}

public class ImagesIcons : MonoBehaviour
{
    [SerializeField] Sprite[] icons;
    [SerializeField] Sprite[] tetrisBoxes;
    [SerializeField] Color[] gemsColors;
    public static ImagesIcons Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public Color GetGemColor(int index) => gemsColors[index];
    public Sprite GetIcon(int index) => icons[index];    
    public Sprite GetTetrisIcon(int index) => tetrisBoxes[index];


}
