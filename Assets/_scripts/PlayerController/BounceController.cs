﻿using System;
using System.Collections;
using System.Numerics;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

public delegate void VoidEvent(float time);

[RequireComponent(typeof(Rigidbody2D))]
public class BounceController : MonoBehaviour
{
    #region Variables

    public static Action OnJump;
    public static event VoidEvent OnPlayerDamage;
    private Rigidbody2D _rigidbody2D;
    [SerializeField] private float velocity;
    [SerializeField] private float jumpForce;
    [SerializeField] private float maxDistanceGround;
    [SerializeField] private float immuneTime;
    private float _horizontal, _vertical, _lastHorizontal = 1;
    private bool _jumping;
    public LayerMask groundLayer;
    private bool _canGetDamage = true;
    private bool _canMove = true;

    #endregion


    private void Awake()
    {
        gameObject.SetActive(true);

        _rigidbody2D = GetComponent<Rigidbody2D>();
    }

    private void OnDisable()
    {
        // OnPlayerDamage = null;
        GameManager.OnGameOver -= OnGameOverAction;
    }

    private void OnEnable()
    {
        GameManager.OnGameOver += OnGameOverAction;
    }

    private void OnGameOverAction()
    {
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (Time.timeScale == 0)
        {
            return;
        }


        _horizontal = Input.GetAxisRaw("Horizontal");

        if (_horizontal != 0)
        {
            _lastHorizontal = _horizontal;
        }


        if (Input.GetKeyDown(KeyCode.Space) && IsOnGround())
        {
            Jump();
        }
    }

    private bool _affectedByJumper;

    private void FixedUpdate()
    {
        if (!_canMove) return;


        if (!_affectedByJumper || _horizontal != 0 && Mathf.Abs(_rigidbody2D.velocity.x) < velocity + 1.0f)
        {
            _rigidbody2D.velocity = new Vector2(_horizontal * velocity, _rigidbody2D.velocity.y);
        }
    }

    private void Jump()
    {
        OnJump?.Invoke();

        _rigidbody2D.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }


    private bool IsOnGround()
    {
        var hit = Physics2D.Raycast(transform.position, Vector2.down, maxDistanceGround,
            groundLayer);

        return hit;
    }

    private void OnCollisionStay2D(Collision2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Damage") && _canGetDamage)
        {
            GetDamage(other.gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        _affectedByJumper = false;
        if (other.gameObject.layer == LayerMask.NameToLayer("Jumpers") && _sleepCoroutine == null)
        {
            var direction = other.gameObject.GetComponent<JumperImpulse>().GetDirection();

            if (direction == DirectionVector.right || direction == DirectionVector.left)
            {
                _affectedByJumper = true;
            }

            _rigidbody2D.velocity = Vector2.zero;
            other.gameObject.GetComponent<JumperImpulse>().ApplyImpulse(_rigidbody2D);
            SfxManager.SingleInstance.PlayBounceSound();
        }

        if (other.gameObject.layer == LayerMask.NameToLayer("Damage") && _canGetDamage)
        {
            GetDamage(other.gameObject);
        }
    }


    private void GetDamage(GameObject other)
    {
        _rigidbody2D.velocity = Vector2.zero;
        _canMove = false;
        _canGetDamage = false;
        if (other.CompareTag("DamageLeft"))
        {
            _rigidbody2D.AddForce(Vector2.left * velocity + Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
        else if (other.CompareTag("DamageRight"))
        {
            _rigidbody2D.AddForce(Vector2.left * -velocity + Vector2.up * jumpForce, ForceMode2D.Impulse);
        }

        StopAllCoroutines();
        StartCoroutine(RestartNoImmune());
        OnPlayerDamage?.Invoke(immuneTime);
    }


    private IEnumerator SleepMove(float time)
    {
        _canMove = false;
        yield return new WaitForSeconds(time);
        _canMove = true;
        _sleepCoroutine = null;
    }

    private Coroutine _sleepCoroutine = null;

    private IEnumerator RestartNoImmune(float time = 0.5f)
    {
        _sleepCoroutine = StartCoroutine(SleepMove(time));
        yield return new WaitUntil(() => _sleepCoroutine == null);
        yield return new WaitForSeconds(immuneTime - time);
        _canGetDamage = true;
    }

    public void GetPlayerState(out bool isOnGround, out float horizontalDirection, out bool moving,
        out float verticalForce)
    {
        isOnGround = IsOnGround();
        horizontalDirection = _lastHorizontal;
        verticalForce = _rigidbody2D.velocity.y;
        moving = _horizontal != 0;
    }
}