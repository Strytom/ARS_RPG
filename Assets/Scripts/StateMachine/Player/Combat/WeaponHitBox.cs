using UnityEngine;

public class WeaponHitBox : MonoBehaviour
{
    public GameObject weapon;

    public void EnableWeaponHitBox()
    {
        weapon.SetActive(true);
    }
    public void DisableWeaponHitBox()
    {
        weapon.SetActive(false);
    }
}
