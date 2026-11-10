using System;
using UnityEngine;

public class LedgeDetector : MonoBehaviour
{
    public event Action<Vector3, Vector3> OnLedgeDetected;
   private void OnTriggerEnter(Collider other)
   {
      Debug.Log("Ledge Detected");
      OnLedgeDetected?.Invoke(other.ClosestPointOnBounds(transform.position), other.transform.forward);
   }
}
