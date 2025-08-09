using UnityEngine;

public class ButtonInputs : MonoBehaviour
{

    public void ButtonClicked()
    {
        Debug.Log("Clicked Button!");
    }
    public void Add(int amt = 1)
    {
        Debug.Log("Add "+amt);
        Stats.AddCoins(amt);
    }
}
