using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Body : MonoBehaviour, IDamageable
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;

    [Header("Knockback")]
    [SerializeField] private float knockbackMultiplier = 1f;
    [SerializeField] private float maxKnockbackSpeed = 6f;
    [SerializeField] private float knockbackDrag = 8f;

    [Header("Hit Feedback")]
    [SerializeField] private SpriteRenderer bodySprite;
    [SerializeField] private Color hitFlashColor = Color.white;
    [SerializeField] private float hitFlashDuration = 0.08f;

    private Rigidbody2D rb2D;
    private Color originalColor;
    private Coroutine flashRoutine;

    private void Awake()
    {
        rb2D = GetComponent<Rigidbody2D>();

        // High linear damping stops the body from sliding forever after a hit -
        // without this, knockback velocity only decays via Unity's default drag (0),
        // meaning it would slide at a near-constant speed until it hit something.
        rb2D.linearDamping = knockbackDrag;

        // Prevent the body from spinning wildly on knockback if that's not desired.
        rb2D.constraints |= RigidbodyConstraints2D.FreezeRotation;

        if (bodySprite == null)
            bodySprite = GetComponentInChildren<SpriteRenderer>();

        if (bodySprite != null)
            originalColor = bodySprite.color;

        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount, Vector2 knockback)
    {
        if (currentHealth <= 0f) return;

        currentHealth -= amount;

        if (rb2D != null)
        {
            // Set velocity directly instead of AddForce: AddForce accumulates on top
            // of any existing velocity (so rapid hits stack into an ever-growing
            // speed), while setting velocity directly gives a consistent, predictable
            // knockback regardless of what the body was already doing.
            Vector2 scaledKnockback = Vector2.ClampMagnitude(knockback * knockbackMultiplier, maxKnockbackSpeed);
            rb2D.linearVelocity = scaledKnockback;
        }

        if (bodySprite != null)
        {
            if (flashRoutine != null)
                StopCoroutine(flashRoutine);

            flashRoutine = StartCoroutine(HitFlash());
        }

        if (currentHealth <= 0f)
            Die();
    }

    private IEnumerator HitFlash()
    {
        bodySprite.color = hitFlashColor;
        yield return new WaitForSeconds(hitFlashDuration);
        bodySprite.color = originalColor;
    }

    private void Die()
    {
        if (SlowMotionManager.Instance != null)
            SlowMotionManager.Instance.TriggerSlowMotion();

        Destroy(gameObject);
    }
}