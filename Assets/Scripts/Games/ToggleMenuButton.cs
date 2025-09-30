using UnityEngine;

public class ToggleMenuButton : MonoBehaviour
{
    [SerializeField] private GameObject panel;

    public void ToggleMenu() => panel.SetActive(!panel.activeSelf);

}
