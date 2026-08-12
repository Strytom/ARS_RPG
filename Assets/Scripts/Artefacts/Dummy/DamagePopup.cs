using System;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;
public class DamagePopup : MonoBehaviour
{
    private TextMesh _textMesh;
    private Color _textColor;
    private float _disappearTimer = 1.0f; // Lifetime of the text
    private float _moveYSpeed = 2.0f;     // Upward movement speed
    private float _fadeSpeed = 3.0f;       // Fade speed

    public static DamagePopup Create(Vector3 position, int damageAmount, Color color)
    {
        // Create an empty game object in the world
        GameObject popupTransform = new GameObject("DamagePopup");
        popupTransform.transform.position = position + new Vector3(Random.Range(-0.5f, 0.5f), 1.5f, Random.Range(-0.5f, 0.5f));
        
        // Rotate the text to face the camera
        if (Camera.main != null)
        {
            popupTransform.transform.rotation = Quaternion.LookRotation(popupTransform.transform.position - Camera.main.transform.position);
        }

        DamagePopup damagePopup = popupTransform.AddComponent<DamagePopup>();
        damagePopup.Setup(damageAmount, color);

        return damagePopup;
    }

    private void Awake()
    {
        _textMesh = gameObject.AddComponent<TextMesh>();
        _textMesh.anchor = TextAnchor.MiddleCenter;
        _textMesh.alignment = TextAlignment.Center;
        _textMesh.fontSize = 24;
        _textMesh.characterSize = 0.1f; // Adjust the font size for 3D
    }

    public void Setup(int damageAmount, Color color)
    {
        _textMesh.text = damageAmount.ToString();
        _textColor = color;
        _textMesh.color = _textColor;
    }

    private void Update()
    {
        // Smoothly move the text upward
        transform.position += new Vector3(0, _moveYSpeed * Time.deltaTime, 0);

        _disappearTimer -= Time.deltaTime;
        if (_disappearTimer < 0)
        {
            // Smoothly fade the text out via alpha channel
            _textColor.a -= _fadeSpeed * Time.deltaTime;
            _textMesh.color = _textColor;

            if (_textColor.a <= 0)
            {
                Destroy(gameObject);
            }
        }
    }
}
