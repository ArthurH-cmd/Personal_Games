using UnityEngine;

public class GunEffects : MonoBehaviour
{
    private GUN gun;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip shootClip;
    [SerializeField] private AudioClip reloadClip;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    [Header("Recoil")]
    [SerializeField] private float recoilForce = 0.5f;
    [SerializeField] private float maxRecoil = 5f;
    [SerializeField] private float recoilRecoverySpeed = 8f;

    [Header("Shake Settings")]
    public float shakeAmount = 0.01f;
    [SerializeField] private float shakeFrequency = 20f;
    [SerializeField] private float shakeSmoothing = 15f; // higher = snappier, lower = smoother

    private Transform recoilHolder;
    private Vector3 recoilRestPos;
    private float currentRecoil = 0f;
    private bool hasRestPos = false;

    private Vector2 noiseSeed;
    private Vector3 currentShakeOffset;

    private void Awake()
    {
        gun = GetComponent<GUN>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        // random per-instance offset so noise isn't sampled near the axes,
        // and so multiple guns don't shake in perfect sync
        noiseSeed = new Vector2(Random.Range(0f, 1000f), Random.Range(0f, 1000f));
    }

    private void OnEnable()
    {
        if (gun == null) return;
        gun.onShoot += HandleShoot;
        gun.recoilControl += HandleRecoil;
        gun.onReload += HandleReload;
    }

    private void OnDisable()
    {
        if (gun == null) return;
        gun.onShoot -= HandleShoot;
        gun.recoilControl -= HandleRecoil;
        gun.onReload -= HandleReload;
    }

    private void Update()
    {
        if (!hasRestPos || recoilHolder == null) return;

        if (currentRecoil > 0f)
        {
            currentRecoil = Mathf.MoveTowards(currentRecoil, 0f, recoilRecoverySpeed * Time.deltaTime);
        }

        float t = Time.time * shakeFrequency;
        float intensity = shakeAmount * (currentRecoil / maxRecoil);

        Vector3 targetShake = new Vector3(
            (Mathf.PerlinNoise(noiseSeed.x + t, 0f) - 0.5f) * 2f,
            (Mathf.PerlinNoise(noiseSeed.y + t, 100f) - 0.5f) * 2f,
            0f
        ) * intensity;

        // smooth toward the target shake instead of snapping to it
        currentShakeOffset = Vector3.Lerp(currentShakeOffset, targetShake, Time.deltaTime * shakeSmoothing);

        Vector3 basePos = recoilRestPos - new Vector3(currentRecoil, 0, 0);
        recoilHolder.localPosition = basePos + currentShakeOffset;
    }

    private void HandleShoot(Transform firePoint)
    {
        if (audioSource != null && shootClip != null)
            audioSource.PlayOneShot(shootClip);

        if (animator != null)
        {
            animator.SetTrigger("Fire");
        }
    }

    private void HandleReload()
    {
        if (audioSource != null && reloadClip != null)
            audioSource.PlayOneShot(reloadClip);

        if (animator != null)
            animator.SetTrigger("Reload");
    }

    private void HandleRecoil(Transform holder)
    {
        if (holder == null) return;

        if (!hasRestPos || recoilHolder != holder)
        {
            recoilHolder = holder;
            recoilRestPos = holder.localPosition + new Vector3(currentRecoil, 0, 0);
            hasRestPos = true;
        }

        currentRecoil = Mathf.Min(currentRecoil + recoilForce, maxRecoil);
    }
}