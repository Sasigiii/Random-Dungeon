using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[DisallowMultipleComponent]

public class Idle : MonoBehaviour
{
    private Rigidbody2D _rigidbody2D;
    // private IdleEvent _idleEvent;

    private void Awake()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        // _idleEvent = GetComponent<IdleEvent>();
    }

    private void OnEnable()
    {
        // _idleEvent.OnIdle += OnIdle;
        EventMgr.RegisterEvent(EventName.IdleEvent, OnIdle);
    }

    private void OnDisable()
    {
        // _idleEvent.OnIdle -= OnIdle;
        EventMgr.UnRegisterEvent(EventName.IdleEvent, OnIdle);
    }

    // private void OnIdle(IdleEvent idleEvent)
    // {
    //     _rigidbody2D.velocity = Vector2.zero;
    // }
    
    private object OnIdle(object[] eventParams)
    {
        _rigidbody2D.velocity = Vector2.zero;
        
        return null;
    }
}