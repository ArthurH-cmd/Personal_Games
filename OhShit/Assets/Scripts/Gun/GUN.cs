using System;
using UnityEngine;

public class GUN : MonoBehaviour
{
    [Header("Sprite")]
    [SerializeField] protected SpriteRenderer GunSprite;

    [Header("Basic Info")]
    [SerializeField] protected string gunName;
    [SerializeField] protected string gunDescription;
    [SerializeField] protected int fireRate;
    [SerializeField] protected int maxAmmo;
    [SerializeField] protected float reloadTime;
    [SerializeField] protected bool isAutomatic;

    [Header("References")]
    [SerializeField] protected Transform firePoint;
    [SerializeField] protected GameObject bulletPrefab;
    [SerializeField] private Transform handPointL;
    [SerializeField] private Transform handPointR;
    [SerializeField] private Transform casingEjectPoint;

    [Header("Throw Settings")]
    [SerializeField] protected float throwForce = 5f;
    [SerializeField] protected float throwTorque = 200f;
    [SerializeField] protected float pickupDelay = 0.3f;

    [Header("Casing")]
    [SerializeField] private Material casingMaterial;
    [SerializeField] private Vector3 casingQuadSize = new Vector3(0.04f, 0.18f);


    // Events
    public static event Action<Transform> onAnyGunShoot;
    public event Action<Transform> onShoot;
    public event Action<Transform> recoilControl;
    public event Action onReload;

    // State
    protected Transform flipPivot;   // the transform SetFlipped() scales — set fresh every Equip()
    protected Transform gunHolder;   // dynamic — wherever the gun is currently parented (equip/drop)
    protected int currentAmmo;
    protected float timeSinceLastShot;
    protected bool isReloading;

    private Vector3 originalLocalScale; // cached once, restored on Equip/Drop so a flip never sticks

    // Components
    private Collider2D pickupCollider;
    private Rigidbody2D rb2D;
    private GunEffects gunEffects;

    // Public accessors
    public bool IsAutomatic => isAutomatic;
    public SpriteRenderer Sprite => GunSprite;
    public Transform HandPointL => handPointL;
    public Transform HandPointR => handPointR;
    public Material CasingMaterial => casingMaterial;
    public Vector3 CasingQuadSize => casingQuadSize;
    public Transform CasingEjectPoint => casingEjectPoint;

    // ---------- Unity Lifecycle ----------

    protected virtual void Awake()
    {
        originalLocalScale = transform.localScale;

        gunHolder = transform.parent; // fallback only — gets overwritten by Equip()
        flipPivot = transform.parent; // fallback only — gets overwritten by Equip()

        if (handPointL == null) handPointL = FindDeepChild(transform, "HandPointL");
        if (handPointR == null) handPointR = FindDeepChild(transform, "HandPointR");

        if (handPointL == null || handPointR == null)
            Debug.LogWarning($"{gunName}: HandPointL/HandPointR not found on gun.");

        if (GunSprite == null)
            GunSprite = GetComponentInChildren<SpriteRenderer>();

        if (casingEjectPoint == null)
            casingEjectPoint = FindDeepChild(transform, "CasingEjectPoint");

        pickupCollider = GetComponent<Collider2D>();
        rb2D = GetComponent<Rigidbody2D>();
        gunEffects = GetComponent<GunEffects>();
    }

    protected virtual void Start()
    {
        currentAmmo = maxAmmo;
    }

    protected virtual void Update()
    {
        timeSinceLastShot += Time.deltaTime;
    }

    // ---------- Flipping ----------

    public void SetFlipped(bool flipped)
    {
        if (flipPivot == null) return;

        // always reapply — never trust a cached "already flipped" bool,
        // since flipPivot is a shared transform written to by whichever gun is equipped
        Vector3 scale = flipPivot.localScale;
        scale.y = Mathf.Abs(scale.y) * (flipped ? -1f : 1f);
        flipPivot.localScale = scale;
    }

    // ---------- Equip / Drop ----------

    public void Equip(Transform holder)
    {
        transform.SetParent(holder, false);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = originalLocalScale; // undo any flip baked in while dropped

        gunHolder = holder; // used by Drop()
        flipPivot = holder; // used by SetFlipped() — refreshed on every equip, always correct

        if (gunEffects != null)
            gunEffects.enabled = true; // only the equipped gun's effects may touch the shared holder

        if (pickupCollider != null)
            pickupCollider.enabled = false;

        rb2D.bodyType = RigidbodyType2D.Kinematic;
        rb2D.linearVelocity = Vector2.zero;
        rb2D.angularVelocity = 0f;
    }

    public void Drop(Vector3 worldPosition, Vector2 throwDirection)
    {
        transform.SetParent(null); // detach into world space — must NOT re-parent onto gunHolder
        transform.localScale = originalLocalScale; // reset immediately — don't let the flip carry over

        if (gunEffects != null)
            gunEffects.enabled = false; // stops it from ever touching GunHolder again while dropped

        transform.position = worldPosition;
        transform.rotation = Quaternion.identity;

        if (pickupCollider != null)
            pickupCollider.enabled = false;

        rb2D.bodyType = RigidbodyType2D.Dynamic;
        rb2D.linearVelocity = Vector2.zero;
        rb2D.angularVelocity = 0f;
        rb2D.AddForce(throwDirection.normalized * throwForce, ForceMode2D.Impulse);
        rb2D.AddTorque(throwTorque, ForceMode2D.Impulse);

        StartCoroutine(ReenablePickupAfterDelay());
    }

    private System.Collections.IEnumerator ReenablePickupAfterDelay()
    {
        yield return new WaitForSeconds(pickupDelay);
        if (pickupCollider != null)
            pickupCollider.enabled = true;
    }

    // ---------- Firing / Reloading ----------

    protected virtual void Fire()
    {
        Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
    }

    public void TryFire()
    {
        if (isReloading) return;

        if (currentAmmo <= 0)
        {
            StartCoroutine(Reload());
            return;
        }

        if (timeSinceLastShot < 1f / fireRate) return;

        Fire();
        currentAmmo--;
        timeSinceLastShot = 0f;

        onShoot?.Invoke(firePoint);
        recoilControl?.Invoke(transform.parent);
        onAnyGunShoot?.Invoke(firePoint);
    }

    public void TryReload()
    {
        if (isReloading) return;
        if (currentAmmo >= maxAmmo) return;

        StartCoroutine(Reload());
    }

    protected System.Collections.IEnumerator Reload()
    {
        isReloading = true;
        onReload?.Invoke();
        yield return new WaitForSeconds(reloadTime);
        currentAmmo = maxAmmo;
        isReloading = false;
    }

    // ---------- Helpers ----------

    private static Transform FindDeepChild(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name) return child;
            Transform result = FindDeepChild(child, name);
            if (result != null) return result;
        }
        return null;
    }
}