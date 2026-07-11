using UnityEngine;
using UnityEngine.InputSystem;

public class LockCursor : MonoBehaviour
{

    private void Start()
    {
        // Hide and lock the mouse cursor when the game starts
        Lock();
    }

    private void Update()
    {
        // Allow the player to unlock the mouse with Escape (for example, for menus)
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (Cursor.lockState == CursorLockMode.Locked)
                UnlockCursor();
            else
                Lock();
        }
    }

    private void Lock()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}