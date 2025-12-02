using UnityEngine;
using UnityEngine.InputSystem;

public class KeyAnimationController : MonoBehaviour
{
    private Animator anim;

    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        if (Keyboard.current.qKey.wasPressedThisFrame)
        {
            anim.SetTrigger("R-Jab");
        }

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            anim.SetTrigger("L-Jab");
        }

    }
}