using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class PlayerMove : MonoBehaviour
{
    public Rigidbody2D rb;
    public float moveSpeed;
    private Vector2 moveDirection;
    public InputActionReference move;
    private CapsuleCollider2D CapsuleCollider2D;
    private GunEquip gunEquip;

    private void Awake()
    {
        CapsuleCollider2D = GetComponent<CapsuleCollider2D>();
        gunEquip = GetComponent<GunEquip>();
        if (gunEquip == null)
        {
            Debug.LogError("GunEquip component not found on the player.");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        gunEquip.GunInRange(other);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        gunEquip.GunOutOfRange(other);
    }

    private void Update()
    {
        moveDirection = move.action.ReadValue<Vector2>();
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(moveDirection.x * moveSpeed, moveDirection.y * moveSpeed);
    }
}