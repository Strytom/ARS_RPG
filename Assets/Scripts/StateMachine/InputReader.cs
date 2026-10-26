using UnityEngine;
using UnityEngine.InputSystem;

public class InputReader : MonoBehaviour, InputSystem_Actions.IPlayerActions
{
    public Vector2 MovementValue { get; private set; }
    public event System.Action OnAttackEvent, OnTargetEvent;
    public bool IsAttacking { get; private set; }
    public bool IsNextAttackQueued { get; private set; }

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
        if(!context.performed) { return; }
        OnAttackEvent?.Invoke();
    }

    public void OnCrouch(InputAction.CallbackContext context)
    {
    }

    public void OnDodge(InputAction.CallbackContext context)
    {   
    }
    public void OnHeavyAttack(InputAction.CallbackContext context)
    {
        
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
    }

    public void OnJump(InputAction.CallbackContext context)
    {
    
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
