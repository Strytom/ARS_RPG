using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Link")]
    [SerializeField] private Transform _target; // The object the camera follows (our player)

    [Header("Orbit parameters")]
    [SerializeField] private float _distance = 5.0f;       // Distance from the player
    [SerializeField] private float _sensitivityX = 0.15f;  // Mouse sensitivity on X
    [SerializeField] private float _sensitivityY = 0.1f;   // Mouse sensitivity on Y
    [SerializeField] private float _minPitch = -20f;       // Minimum pitch angle
    [SerializeField] private float _maxPitch = 70f;        // Maximum pitch angle

    private InputSystem_Actions _inputActions;
    private float _yaw;   // Horizontal rotation angle
    private float _pitch; // Vertical pitch angle

    private void Awake()
    {
        _inputActions = new InputSystem_Actions();
        
        // Initialize the camera start angles based on its current scene rotation
        Vector3 currentEuler = transform.eulerAngles;
        _yaw = currentEuler.y;
        _pitch = currentEuler.x;
    }

    private void OnEnable()
    {
        _inputActions.Enable();
    }

    private void OnDisable()
    {
        _inputActions.Disable();
    }

    private void Start()
    {
        // Hide and lock the mouse cursor when the game starts
        LockCursor();
    }

    private void Update()
    {
        // Allow the player to unlock the mouse with Escape (for example, for menus)
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (Cursor.lockState == CursorLockMode.Locked)
                UnlockCursor();
            else
                LockCursor();
        }
    }

    private void LateUpdate()
    {
        if (_target == null) return;

        if (Cursor.lockState == CursorLockMode.Locked)
        {
            // Read mouse delta (mouse change per frame) from the Input Action Asset
            Vector2 lookInput = _inputActions.Player.Look.ReadValue<Vector2>();

            // Accumulate rotation angles
            _yaw += lookInput.x * _sensitivityX;
            _pitch -= lookInput.y * _sensitivityY; // Invert Y so moving the mouse up raises the camera

            // Clamp the vertical angle so the camera doesn't go below the ground
            _pitch = Mathf.Clamp(_pitch, _minPitch, _maxPitch);
        }

        // Calculate the new camera rotation
        Quaternion rotation = Quaternion.Euler(_pitch, _yaw, 0);

        // Find the target camera position (move it back from the player by _distance)
        Vector3 position = _target.position + rotation * (Vector3.back * _distance);

        // Apply the position and rotation
        transform.rotation = rotation;
        transform.position = position;
    }

    private void LockCursor()
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