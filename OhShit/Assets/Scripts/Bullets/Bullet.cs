using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Bullet : MonoBehaviour
{
    [SerializeField] private float speed = 15f;
    [SerializeField] private float lifetime = 3f;
    [SerializeField] private float damage;
    [SerializeField] private float knockbackForce;
    [SerializeField] private float spriteAngleOffset = 0f;
    [SerializeField] private string targetTag = "Enemy";

    private Rigidbody2D rb;

    public float DamageValue => damage;
    public float KnockbackValue => knockbackForce;

    public static event Action<Vector3, Vector3> onBulletHit;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        rb.linearVelocity = transform.right * speed;
        transform.Rotate(0f, 0f, spriteAngleOffset);

        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(targetTag))
        {
            Vector3 hitDirection = rb.linearVelocity.normalized;

            IDamageable damageable = other.GetComponent<IDamageable>();
            Debug.Log($"IDamageable found: {damageable != null}");

            if (damageable != null)
            {
                Vector2 knockback = (Vector2)hitDirection * knockbackForce;
                damageable.TakeDamage(damage, knockback);
                Debug.Log($"TakeDamage called with damage: {damage}, knockback: {knockback}");
            }

            onBulletHit?.Invoke(transform.position, hitDirection);
        }

        Destroy(gameObject);
    }
}