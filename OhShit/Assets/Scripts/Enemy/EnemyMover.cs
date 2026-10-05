using System.Runtime.CompilerServices;
using UnityEngine;

public class EnemyMover : MonoBehaviour
{
    [SerializeField]
    private float _speed;

    [SerializeField]
    private float _rotationSpeed;

    [SerializeField]
    private float _health;

    // defaults used if the bullet doesn't provide values
    [SerializeField] private float _defaultBulletDamage = 1f;
    [SerializeField] private float _defaultKnockback = 2f;

    private Rigidbody2D _rigidbody;
    private PlayerAware _playerAware;
    private Vector2 _targetDirection;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _playerAware = GetComponent<PlayerAware>();
    }

    private void FixedUpdate()
    {
        UpdateTargetDirection();
        RotateTowardsTarget();
        SetVelocity();
    }

    private void UpdateTargetDirection()
    {
        if (_playerAware.AwareOfPlayer)
        {
            _targetDirection = _playerAware.DirectionToPayer;
        }
        else
        {
            _targetDirection = Vector2.zero;
        }
    }

    private void RotateTowardsTarget()
    {
        if (_targetDirection == Vector2.zero)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(Vector3.forward, _targetDirection);
        Quaternion rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, _rotationSpeed * Time.fixedDeltaTime);

        _rigidbody.SetRotation(rotation.eulerAngles.z);
    }

    private void SetVelocity()
    {
        if (_targetDirection == Vector2.zero)
        {
            _rigidbody.linearVelocity = Vector2.zero;
        }
        else
        {
            _rigidbody.linearVelocity = transform.up * _speed;
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Bullet"))
            return;

        float damage = _defaultBulletDamage;
        float knockback = _defaultKnockback;

        var bulletScript = collision.GetComponent<Bullet>();
        if (bulletScript != null)
        {

            damage = bulletScript.DamageValue;
            knockback = bulletScript.KnockbackValue;
        }
        else
        {
            var bulletRb = collision.attachedRigidbody;
            if (bulletRb != null)
                knockback = Mathf.Max(_defaultKnockback, bulletRb.linearVelocity.magnitude * 0.5f);
        }

        _health -= damage;

        Vector2 knockDir = ((Vector2)transform.position - (Vector2)collision.transform.position).normalized;
        if (float.IsNaN(knockDir.x) || float.IsNaN(knockDir.y))
            knockDir = Vector2.up; // safe fallback

        if (_rigidbody != null)
            _rigidbody.AddForce(knockDir * knockback, ForceMode2D.Impulse);

        Destroy(collision.gameObject);

        if (_health <= 0f)
            Die();
    }

    private void Die()
    {
        Destroy(gameObject);
    }
}