using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UpgradeButton : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private UpgradeData data;

    [SerializeField] private Image image;
    [SerializeField] private GameObject ownedBackgroundImage;
    [SerializeField] private Material greyscaleMaterial;

    private bool owned = false;
    public UpgradeData Data => data;

    private void Start()
    {
        if (data == null) {
            image.sprite = UpgradeDatas.Instance.MissingSprite;
            return;
        }
        image.sprite = data.Image;
        //SetAsOwned(true);

    }

    public void SetAsOwned(bool set)
    {
        owned = true;
        if(gameObject.activeInHierarchy)
            ApplyActiveMaterial();

    }
    private void OnEnable() => ApplyActiveMaterial();

    private void ApplyActiveMaterial()
    {
        Debug.Log("Applying material for "+name+" owned = "+owned);
        image.material = owned ? null : greyscaleMaterial;
        //ownedBackgroundImage.SetActive(owned);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("Mouse Clicks Button");
        Upgrades.Instance.UpdateInfoPanel(this);
    }

}
