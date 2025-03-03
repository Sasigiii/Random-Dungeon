using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[DisallowMultipleComponent]
public class MovementByVelocity : MonoBehaviour
{
    private Rigidbody2D _rigidBody2D;

    private void Awake()
    {
        _rigidBody2D = GetComponent<Rigidbody2D>();
    }
    
    private void OnEnable()
    {
        EventMgr.RegisterEvent(EventName.MovementEvent, MovementEvent_OnMovementByVelocity);
    }
    
    
    private void OnDisable()
    {
        EventMgr.UnRegisterEvent(EventName.MovementEvent, MovementEvent_OnMovementByVelocity);
    }
    
    private object MovementEvent_OnMovementByVelocity(object[] eventParams)
    {
        MoveRigidBody((Vector2)eventParams[0], (float)eventParams[1]);
        
        return null;
    }

    private void MoveRigidBody(Vector2 moveDirection, float moveSpeed)
    {
        _rigidBody2D.velocity = moveDirection * moveSpeed;
    }
}