using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildingsListItem : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI itemName;
    [SerializeField] private TextMeshProUGUI itemValue;
    [SerializeField] private Image image;

    public void SetName(string name) => itemName.text = name;
    public void SetValue(string name) => itemValue.text = name;
    public void SetImage(int index) => image.sprite = ImagesIcons.Instance.GetIcon(index);
}
