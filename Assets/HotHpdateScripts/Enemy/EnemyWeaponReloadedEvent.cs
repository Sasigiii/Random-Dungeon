using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class EnemyWeaponReloadedEvent : MonoBehaviour
{
    public event Action<EnemyWeaponReloadedEvent, EnemyWeaponReloadedEventArgs> OnWeaponReloaded;

    public void CallWeaponReloadedEvent(Weapon weapon)
    {
        OnWeaponReloaded?.Invoke(this, new EnemyWeaponReloadedEventArgs() { weapon = weapon });
    }
}

public class EnemyWeaponReloadedEventArgs : EventArgs
{
    public Weapon weapon;
}
