using TMPro;
using UnityEngine;

public class BuildingsListItem : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI itemName;
    [SerializeField] private TextMeshProUGUI itemValue;

    public void SetName(string name) => itemName.text = name;
    public void SetValue(string name) => itemValue.text = name;


}
