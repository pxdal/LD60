using UnityEngine;
using UnityEngine.InputSystem;

public class TapperBehaviour : MonoBehaviour
{
    // serialized //

    // private //

    InputAction tap1;
    InputAction tap2;
    InputAction tap3;


    // getters + setters //

    public bool IsTapping1()
    {
        return tap1.IsPressed();
    }

    public bool IsTapping2()
    {
        return tap2.IsPressed();
    }

    public bool IsTapping3()
    {
        return tap3.IsPressed();
    }


    // event listeners //

    void Start()
    {
        tap1 = InputSystem.actions.FindAction("Tap1");
        tap2 = InputSystem.actions.FindAction("Tap2");
        tap3 = InputSystem.actions.FindAction("Tap3");
    }
}
