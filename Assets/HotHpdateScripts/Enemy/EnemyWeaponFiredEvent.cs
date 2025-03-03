using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class EnemyWeaponFiredEvent : MonoBehaviour
{
    public event Action<EnemyWeaponFiredEvent, EnemyWeaponFiredEventArgs> OnWeaponFired;

    public void CallWeaponFiredEvent(Weapon weapon)
    {
        OnWeaponFired?.Invoke(this, new EnemyWeaponFiredEventArgs() { weapon = weapon });
    }
}

public class EnemyWeaponFiredEventArgs : EventArgs
{
    public Weapon weapon;
}
