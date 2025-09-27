using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Inputs : MonoBehaviour
{

    public PlayerControls PlayerControls;


    public static Inputs Instance { get; private set; }
    public static Action<int> NumberPressed;

    private void Awake()
    {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        PlayerControls = new PlayerControls();
        PlayerControls.Enable();
    }

    private void OnEnable()
    {
        PlayerControls.Player.Zero.performed += Zero;
        PlayerControls.Player.One.performed += One;
        PlayerControls.Player.Two.performed += Two;
        PlayerControls.Player.Three.performed += Three;
        PlayerControls.Player.Four.performed += Four;
        PlayerControls.Player.Five.performed += Five;
        PlayerControls.Player.Six.performed += Six;
        PlayerControls.Player.Seven.performed += Seven;
        PlayerControls.Player.Eight.performed += Eight;
        PlayerControls.Player.Nine.performed += Nine;
        PlayerControls.Player.A.performed += A;
    }
    
    private void OnDisable()
    {
        PlayerControls.Player.Zero.performed -= Zero;
        PlayerControls.Player.One.performed -= One;
        PlayerControls.Player.Two.performed -= Two;
        PlayerControls.Player.Three.performed -= Three;
        PlayerControls.Player.Four.performed -= Four;
        PlayerControls.Player.Five.performed -= Five;
        PlayerControls.Player.Six.performed -= Six;
        PlayerControls.Player.Seven.performed -= Seven;
        PlayerControls.Player.Eight.performed -= Eight;
        PlayerControls.Player.Nine.performed -= Nine;
        PlayerControls.Player.A.performed -= A;
    }   

    private void A(InputAction.CallbackContext context)
    {
        Debug.Log("A");
    }
    private void Zero(InputAction.CallbackContext context)
    {
        Debug.Log("ZERO");
        NumberPressed?.Invoke(0);
    }
    private void One(InputAction.CallbackContext context)
    {
        Debug.Log("ONE");
        NumberPressed?.Invoke(1);
    }
    private void Two(InputAction.CallbackContext context)
    {
        Debug.Log("TWO");
        NumberPressed?.Invoke(2);
    }
    private void Three(InputAction.CallbackContext context)
    {
        Debug.Log("THREE");
        NumberPressed?.Invoke(3);
    }
    private void Four(InputAction.CallbackContext context)
    {
        Debug.Log("FOUR");
        NumberPressed?.Invoke(4);
    }
    private void Five(InputAction.CallbackContext context)
    {
        Debug.Log("FIVE");
        NumberPressed?.Invoke(5);
    }
    private void Six(InputAction.CallbackContext context)
    {
        Debug.Log("SIX");
        NumberPressed?.Invoke(6);
    }
    private void Seven(InputAction.CallbackContext context)
    {
        Debug.Log("SEVEN");
        NumberPressed?.Invoke(7);
    }
    private void Eight(InputAction.CallbackContext context)
    {
        Debug.Log("EIGHT");
        NumberPressed?.Invoke(8);
    }
    private void Nine(InputAction.CallbackContext context)
    {
        Debug.Log("NINE");
        NumberPressed?.Invoke(9);
    }
}
        
