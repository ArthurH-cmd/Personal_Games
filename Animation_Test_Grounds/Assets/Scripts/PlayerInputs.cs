using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputs : MonoBehaviour
{
    private Animator animator;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            animator.SetTrigger("R-Jab");
            Debug.Log("Right Punch Thrown!");
        }
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            
            Debug.Log("Left Punch thrown");
        }
    }
}
