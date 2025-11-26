using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControls : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Mouse Settings")]
    [SerializeField] private float rotationSpeed = 720f; // Degrees per second - higher = snappier
    [SerializeField] private float mouseDeadzone = 1f;

    [Header("Input")]
    [SerializeField] private InputActionAsset inputActions;

    [Header("Camera")]
    [SerializeField] private Camera mainCamera;

    private Animator animator;
    private InputAction moveAction;
    private Rigidbody rb;
    private Vector2 moveInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // Get main camera if not assigned
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    void OnEnable()
    {
        // Find and enable the move action
        if (inputActions != null)
        {
            var actionMap = inputActions.FindActionMap("Player");
            if (actionMap != null)
            {
                moveAction = actionMap.FindAction("Move");
                moveAction.Enable();
            }
        }
    }

    void OnDisable()
    {
        if (moveAction != null)
        {
            moveAction.Disable();
        }
    }

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // Get movement input
        if (moveAction != null)
        {
            moveInput = moveAction.ReadValue<Vector2>();
        }

        // Rotate to face mouse
        RotateTowardsMouse();

        // Punch controls
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            animator.SetTrigger("Rightpunch");
            Debug.Log("Right Punch Thrown!");
        }
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            animator.SetTrigger("Leftpunch");
            Debug.Log("Left Punch thrown");
        }

        if (Keyboard.current.fKey.isPressed)
        {
            Debug.Log("BLOCKING");
            animator.SetBool("IsBlocking", true);
        }
        else
        {
            Debug.Log("dropped guard");
            animator.SetBool("IsBlocking", false);
        }

       
        if (animator != null)
        {
            animator.SetFloat("Speed", moveInput.magnitude);
        }
    }

    void FixedUpdate()
    {
        // Move the player relative to camera direction
        if (moveInput.magnitude > 0.1f)
        {
            // Get camera forward and right vectors (flattened to XZ plane)
            Vector3 cameraForward = mainCamera.transform.forward;
            Vector3 cameraRight = mainCamera.transform.right;

            cameraForward.y = 0;
            cameraRight.y = 0;

            cameraForward.Normalize();
            cameraRight.Normalize();

            // Calculate movement direction relative to camera
            Vector3 movement = (cameraForward * moveInput.y + cameraRight * moveInput.x).normalized;

            // Move the player
            rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
        }
    }

    void RotateTowardsMouse()
    {
        // Get mouse position in world space
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        Plane groundPlane = new Plane(Vector3.up, transform.position);

        if (groundPlane.Raycast(ray, out float distance))
        {
            Vector3 mouseWorldPosition = ray.GetPoint(distance);

            // Calculate direction to mouse
            Vector3 direction = mouseWorldPosition - transform.position;
            float distanceToMouse = direction.magnitude;
            direction.y = 0; // Keep rotation on horizontal plane

            // Only rotate if mouse is outside the deadzone
            if (distanceToMouse > mouseDeadzone)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);

                // Rotate towards mouse with a max rotation speed
                float maxRotationThisFrame = rotationSpeed * Time.deltaTime;
                rb.rotation = Quaternion.RotateTowards(rb.rotation, targetRotation, maxRotationThisFrame);
            }
        }
    }
}