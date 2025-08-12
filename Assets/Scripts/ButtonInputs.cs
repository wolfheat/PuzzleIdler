using BreakInfinity;
using UnityEngine;

public class ButtonInputs : MonoBehaviour
{

    public void ButtonClicked()
    {
        Debug.Log("Clicked Button!");
    }
    public void Add(int amt = 1)
    {
        BigDouble toAdd = amt;
        // If shift is held mult with 1000000000
        if (Inputs.Instance.PlayerControls.Player.Shift.IsPressed()) {
            Debug.Log("Holding shift multiply by 1G");
            toAdd *= 1000000000;
        }
        if (Inputs.Instance.PlayerControls.Player.Ctrl.IsPressed()) {
            Debug.Log("Holding ctrl multiply by 1G");
            toAdd *= 1000000000;
        }
        if (Inputs.Instance.PlayerControls.Player.Alt.IsPressed()) {
            Debug.Log("Holding alt multiply by 1G");
            toAdd *= 1000000000;
        }
        Debug.Log("Add "+ toAdd);
        Stats.AddCoins(toAdd);
    }
}
