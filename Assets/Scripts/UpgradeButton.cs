using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UpgradeButton : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private UpgradeData data;

    [SerializeField] private Image image;
    [SerializeField] private Material greyscaleMaterial;
    private Material normalMaterial;

    public UpgradeData Data => data;

    private void Awake()
    {
        // Store the default material
        normalMaterial = image.material;
        image.material = greyscaleMaterial;
    }

    private void Start()
    {
        if (data == null) {
            image.sprite = UpgradeDatas.Instance.MissingSprite;
            return;
        }
        image.sprite = data.Image;
        //SetAsOwned(true);
    }

    public void SetAsOwned(bool set) => image.material = set ? normalMaterial : greyscaleMaterial;

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("Mouse Clicks Button");
        Upgrades.Instance.UpdateInfoPanel(data);
    }

}
