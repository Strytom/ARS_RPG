using UnityEngine;
using UnityEngine.InputSystem;

public class InputReader : MonoBehaviour, InputSystem_Actions.IPlayerActions
{
    public Vector2 MovementValue { get; private set; }
    public event System.Action OnJumpEvent, OnDodgeEvent, OnTargetEvent;

    private InputSystem_Actions _inputActions;
    private void Start()
    {
        _inputActions = new InputSystem_Actions();
        _inputActions.Player.SetCallbacks(this);

        _inputActions.Player.Enable();
    }
    private void OnDestroy()
    {
        _inputActions.Player.RemoveCallbacks(this);
        _inputActions.Player.Disable();
    }
    public void OnAttack(InputAction.CallbackContext context)
    {
    }

    public void OnCrouch(InputAction.CallbackContext context)
    {
    }

    public void OnDodge(InputAction.CallbackContext context)
    {
        if (!context.performed){return;}
        OnDodgeEvent?.Invoke();
    }
    public void OnHeavyAttack(InputAction.CallbackContext context)
    {
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (!context.performed){return;}
        OnJumpEvent?.Invoke();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        MovementValue = context.ReadValue<Vector2>();
    }

    public void OnNext(InputAction.CallbackContext context)
    {
    }

    public void OnPrevious(InputAction.CallbackContext context)
    {
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
    }

    public void OnTestDamage(InputAction.CallbackContext context)
    {
    }

    public void OnTarget(InputAction.CallbackContext context)
    {
        if (!context.performed) { return; }
        OnTargetEvent?.Invoke();
    }
}
