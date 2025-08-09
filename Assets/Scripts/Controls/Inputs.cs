using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Inputs : MonoBehaviour
{

    public PlayerControls PlayerControls;

    void Awake()
    {
        PlayerControls = new PlayerControls();
        PlayerControls.Enable();
    }

    private void OnEnable()
    {
        PlayerControls.Player.One.performed += One;
        PlayerControls.Player.Two.performed += Two;
        PlayerControls.Player.Three.performed += Three;
        PlayerControls.Player.Four.performed += Four;
        PlayerControls.Player.Five.performed += Five;
        PlayerControls.Player.Six.performed += Six;
        PlayerControls.Player.A.performed += A;
    }
    
    private void OnDisable()
    {
        PlayerControls.Player.One.performed -= One;
        PlayerControls.Player.Two.performed -= Two;
        PlayerControls.Player.Three.performed -= Three;
        PlayerControls.Player.Four.performed -= Four;
        PlayerControls.Player.Five.performed -= Five;
        PlayerControls.Player.Six.performed -= Six;
        PlayerControls.Player.A.performed -= A;
    }   

    private void A(InputAction.CallbackContext context)
    {
        Debug.Log("A");
    }
    private void One(InputAction.CallbackContext context)
    {
        Debug.Log("ONE");
    }
    private void Two(InputAction.CallbackContext context)
    {
        Debug.Log("TWO");
    }
    private void Three(InputAction.CallbackContext context)
    {
        Debug.Log("THREE");
    }
    private void Four(InputAction.CallbackContext context)
    {
        Debug.Log("FOUR");
    }
    private void Five(InputAction.CallbackContext context)
    {
        Debug.Log("FIVE");
    }
    private void Six(InputAction.CallbackContext context)
    {
        Debug.Log("SIX");
    }
}
