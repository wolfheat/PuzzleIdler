using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UpgradeButton : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private UpgradeData data;

    [SerializeField] private Image image;

    public UpgradeData Data => data;

    private void Start()
    {
        if (data == null) {
            image.sprite = UpgradeDatas.Instance.MissingSprite;
            return;
        }
        image.sprite = data.Image;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("Mouse Clicks Button");
        Upgrades.Instance.UpdateInfoPanel(data);
    }

}
