using UnityEngine;


public enum IconType{CPS,GPS,Coins,Gems, Multiply,Research,Buildings,Upgrades}

public class ImagesIcons : MonoBehaviour
{
    [SerializeField] Sprite[] icons;
    public static ImagesIcons Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }


    public Sprite GetIcon(int index) => icons[index];


}
