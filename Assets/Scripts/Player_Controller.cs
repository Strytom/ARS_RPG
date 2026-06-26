using UnityEngine;

public class Player_Controller : MonoBehaviour
{
    private Rigidbody _rb;
    private InputSystem_Actions _control;

    [Header("Player atributes")]
    public float _speed;

    private void Awake() {
        //Get components
        _rb = GetComponent<Rigidbody>();
        _control = new InputSystem_Actions();
    }
    private void FixedUpdate() {
        
    }
}
