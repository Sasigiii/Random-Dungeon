using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[DisallowMultipleComponent]
public class EnemyIdle : MonoBehaviour
{
    private Rigidbody2D _rigidbody2D; 
    private EnemyIdleEvent _enemyIdleEvent;

    private void Awake()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
         _enemyIdleEvent = GetComponent<EnemyIdleEvent>();
    }

    private void OnEnable()
    {
        _enemyIdleEvent.OnIdle += OnEnemyIdle;
    }

    private void OnDisable()
    {
        _enemyIdleEvent.OnIdle -= OnEnemyIdle;
    }

    private void OnEnemyIdle(EnemyIdleEvent idleEvent)
    {
        _rigidbody2D.velocity = Vector2.zero;
    }
}
