using UnityEngine;
using WolfheatProductions.SoundMaster;

public class ToggleMenuButton : MonoBehaviour
{
    [SerializeField] private GameObject panel;

    public void ToggleMenu()
    {
        panel.SetActive(!panel.activeSelf);
        if(panel.activeSelf)
            SoundMaster.Instance.PlaySound(SoundName.MenuToggleMiniGame);
        else
            SoundMaster.Instance.PlaySound(SoundName.MenuClosing);
    }
}
