using UnityEngine;

public class GreyScaleController : MonoBehaviour
{
    [SerializeField] private GameObject greyScaleVolume;


    public static GreyScaleController Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void SetGameGreyScale(bool set) => greyScaleVolume.gameObject.SetActive(set);
}
