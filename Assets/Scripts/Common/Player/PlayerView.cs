using JKFrame;
using System;
using UnityEngine;

public class PlayerView : CharacterViewBase
{
    [SerializeField] private Transform weaponRoot;
    private GameObject currentWeapon;
    private void OnDisable()
    {
        SetWeapon(null);
    }
    public void SetWeapon(GameObject weapon)
    {
        if (currentWeapon != null) currentWeapon.GameObjectPushPool();
        if (weapon != null)
        {
            weapon.transform.parent = weaponRoot;
            weapon.transform.localPosition = Vector3.zero;
            weapon.transform.localEulerAngles = Vector3.zero;
        }
        currentWeapon = weapon;
    }

  
}
