using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GunEquip : MonoBehaviour
{
    [SerializeField] private InputActionReference interact;

    private PlayerAim playerAim;
    private readonly List<GUN> gunsInRange = new List<GUN>();

    private void Awake()
    {
        playerAim = GetComponent<PlayerAim>();
    }

    private void OnEnable()
    {
        if (interact != null)
            interact.action.started += OnInteractPressed;
    }

    private void OnDisable()
    {
        if (interact != null)
            interact.action.started -= OnInteractPressed;
    }

    public void GunInRange(Collider2D other)
    {
        if (!other.CompareTag("GUN")) return;

        GUN gun = other.GetComponent<GUN>();
        if (gun == null) return;

        if (!gunsInRange.Contains(gun))
        {
            gunsInRange.Add(gun);
            Debug.Log("Gun in range: " + gun.name);
        }
    }

    public void GunOutOfRange(Collider2D other)
    {
        if (!other.CompareTag("GUN")) return;

        GUN gun = other.GetComponent<GUN>();
        if (gun == null) return;

        if (gunsInRange.Contains(gun))
        {
            gunsInRange.Remove(gun);
            Debug.Log("Gun left range: " + gun.name);
        }
    }

    private void OnInteractPressed(InputAction.CallbackContext ctx)
    {
        gunsInRange.RemoveAll(g => g == null);

        if (gunsInRange.Count == 0) return;

        GUN closest = GetClosestGun();
        if (closest != null)
        {
            playerAim.EquipGun(closest);
            gunsInRange.Remove(closest); // it's equipped now, stop tracking it as pickupable
        }
    }

    private GUN GetClosestGun()
    {
        GUN closest = null;
        float closestDist = float.MaxValue;

        foreach (var gun in gunsInRange)
        {
            float dist = (gun.transform.position - transform.position).sqrMagnitude;
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = gun;
            }
        }

        return closest;
    }
}