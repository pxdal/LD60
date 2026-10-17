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

    public bool IsHolding(int lane)
    {
        switch (lane)
        {
            case 1:
                return IsHolding1();
            case 2:
                return IsHolding2();
            case 3:
                return IsHolding3();
            default:
                throw new System.Exception("invalid tapping lane " + lane);
        }
    }

    public bool IsTapping(int lane)
    {
        switch (lane)
        {
            case 1:
                return IsTapping1();
            case 2:
                return IsTapping2();
            case 3:
                return IsTapping3();
            default:
                throw new System.Exception("invalid tapping lane " + lane);
        }
    }

    public bool IsHolding1()
    {
        return tap1.IsPressed();
    }

    public bool IsTapping1()
    {
        return tap1.WasPressedThisFrame();
    }

    public bool IsHolding2()
    {
        return tap2.IsPressed();
    }

    public bool IsTapping2()
    {
        return tap2.WasPressedThisFrame();
    }

    public bool IsHolding3()
    {
        return tap3.IsPressed();
    }

    public bool IsTapping3()
    {
        return tap3.WasPressedThisFrame();
    }


    // event listeners //

    void Start()
    {
        tap1 = InputSystem.actions.FindAction("Tap1");
        tap2 = InputSystem.actions.FindAction("Tap2");
        tap3 = InputSystem.actions.FindAction("Tap3");
    }
}
