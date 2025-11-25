using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Video;

public class PlayerControls : MonoBehaviour
{
    private Animator animator;
    private PlayerInput playerInput = null;


    private void Awake()
    {
        playerInput = new PlayerInput();
    }

    void OnEnable()
    {
  
    }

    void Start()
    {
        animator = GetComponent<Animator>();

    }


    void Update()
    {
        if (Keyboard.current.qKey.wasPressedThisFrame)
        {
            animator.SetBool("RightPunch", true);
            Debug.Log("Right Punch Thrown!");
        }

        // This runs EVERY frame to check if animation is done
        if (animator.GetBool("RightPunch")) // Only check if we're supposed to be punching
        {
            if (animator.GetCurrentAnimatorStateInfo(0).IsName("RightPunch"))
            {
                if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 2.0f)
                {
                    Debug.Log("Animation Finished!");
                    animator.SetBool("RightPunch", false);
                }
            }
        }
    }



}
