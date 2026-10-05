using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAim : MonoBehaviour
{
    private Transform aimTransform;
    private Transform gunHolder;
    private GUN currentGun;

    private Transform handHolder;
    private Transform handL;
    private Transform handR;

    // actions
    public InputActionReference fire;
    public InputActionReference reload;

    public static event Action<GUN> onGunChanged;

    private void Awake()
    {
        aimTransform = transform.Find("Aim");
        gunHolder = aimTransform.Find("GunHolder");
        handHolder = transform.Find("Hands");

        handL = handHolder.Find("HandL");
        handR = handHolder.Find("HandR");

        if (gunHolder == null)
        {
            Debug.LogError("GunHolder not assigned in Inspector on PlayerAim.");
        }

        currentGun = aimTransform.GetComponentInChildren<GUN>();

        if (currentGun != null && gunHolder != null)
        {
            currentGun.Equip(gunHolder);
        }

        onGunChanged?.Invoke(currentGun);

        Debug.Log("Current Gun: " + (currentGun != null ? currentGun.name : "None"));
    }

    private void OnEnable()
    {
        fire.action.started += Fire;
        reload.action.started += Reload;
    }

    private void OnDisable()
    {
        fire.action.started -= Fire;
        reload.action.started -= Reload;
    }

    private void Update()
    {
        HandleAiming();
        HandleAutoFire();
    }

    private void LateUpdate()
    {
        UpdateHandPositions();
    }

    private void HandleAutoFire()
    {
        if (currentGun == null) return;
        if (currentGun.IsAutomatic && fire.action.IsPressed())
        {
            currentGun.TryFire();
        }
    }

    private void HandleAiming()
    {
        Vector3 mousePos = GetWorldMousePos();
        Vector3 aimDirection = (mousePos - aimTransform.position).normalized;

        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
        aimTransform.eulerAngles = new Vector3(0f, 0f, angle);

        bool flipped = angle > 90f || angle < -90f;

        if (currentGun != null && currentGun.Sprite != null) // ignor until we get better modle
        {
        /*
            if (angle < 160f && angle > 20f)
            {
                currentGun.Sprite.sortingOrder = 0; // Behind the player
            }
            else
            {
                currentGun.Sprite.sortingOrder = 1; // In front of the player
            }
        */
        }

        if (currentGun != null)
        {
            currentGun.SetFlipped(flipped);
        }
    }

    public void EquipGun(GUN newGun)
    {
        if (newGun == null || newGun == currentGun) return;

        if (currentGun != null)
        {
            Vector2 throwDirection = aimTransform.right;
            currentGun.Drop(aimTransform.position, throwDirection);
        }

        newGun.Equip(gunHolder);
        UpdateHandPositions();

        currentGun = newGun;

        onGunChanged?.Invoke(currentGun);
    }

    static Vector3 GetWorldMousePos()
    {
        Vector3 screenPos = Mouse.current.position.ReadValue();
        Vector3 vec = GetWorldPosWithZ(screenPos, Camera.main);
        vec.z = 0.0f;
        return vec;
    }

    public static Vector3 GetWorldPosWithZ(Vector3 screenPos, Camera worldCamera)
    {
        return worldCamera.ScreenToWorldPoint(screenPos);
    }

    private void Fire(InputAction.CallbackContext obj)
    {
        if (currentGun != null && !currentGun.IsAutomatic)
        {
            currentGun.TryFire();
        }
    }

    private void Reload(InputAction.CallbackContext obj)
    {
        if (currentGun != null)
        {
            currentGun.TryReload();
        }
    }

    private void UpdateHandPositions()
    {
        if (currentGun == null) return;

        if (currentGun.HandPointL != null && handL != null)
        {
            handL.position = currentGun.HandPointL.position;
            handL.rotation = currentGun.HandPointL.rotation;
        }

        if (currentGun.HandPointR != null && handR != null)
        {
            handR.position = currentGun.HandPointR.position;
            handR.rotation = currentGun.HandPointR.rotation;
        }
    }
}