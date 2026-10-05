using System.Collections.Generic;
using UnityEngine;

public class MuzzleFlashParticleSystemHandler : MonoBehaviour
{
    public static MuzzleFlashParticleSystemHandler Instance { get; private set; }

    private MeshParticalSystem meshParticalSystem;
    private List<Flash> flashList;
    private List<Smoke> smokeList;
    private GUN gun;

    [Header("Rendering")]
    [SerializeField] private string sortingLayerName = "Effects";
    [SerializeField] private int sortingOrder = 0;

    private void Awake()
    {
        Instance = this;
        meshParticalSystem = GetComponent<MeshParticalSystem>();

        if (meshParticalSystem == null)
            Debug.LogError($"{name}: MeshParticalSystem component missing on this GameObject.");

        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            meshRenderer.sortingLayerName = sortingLayerName;
            meshRenderer.sortingOrder = sortingOrder;
        }

        flashList = new List<Flash>();
        smokeList = new List<Smoke>();
    }

    private void OnEnable()
    {
        PlayerAim.onGunChanged += HandleGunChanged;
    }

    private void OnDisable()
    {
        PlayerAim.onGunChanged -= HandleGunChanged;

        if (gun != null)
            gun.onShoot -= HandleShoot;
    }

    private void Update()
    {
        for (int i = 0; i < flashList.Count; i++)
        {
            Flash flash = flashList[i];
            flash.Update();

            if (flash.IsComplete())
            {
                flash.ForceHide();
                flashList.RemoveAt(i);
                i--;
            }
        }

        for (int i = 0; i < smokeList.Count; i++)
        {
            Smoke smoke = smokeList[i];
            smoke.Update();

            if (smoke.IsComplete())
            {
                smoke.ForceHide();
                smokeList.RemoveAt(i);
                i--;
            }
        }
    }

    private void HandleGunChanged(GUN newGun)
    {
        if (gun != null)
            gun.onShoot -= HandleShoot;

        gun = newGun;

        if (gun != null)
            gun.onShoot += HandleShoot;
    }

    private void HandleShoot(Transform firePoint)
    {
        SpawnMuzzleFlash(firePoint.position, firePoint.right);
    }

    public void SpawnMuzzleFlash(Vector3 position, Vector3 direction)
    {
        flashList.Add(new Flash(position, direction, meshParticalSystem));

        int smokeCount = Random.Range(1, 3);
        for (int i = 0; i < smokeCount; i++)
        {
            smokeList.Add(new Smoke(position, direction, meshParticalSystem));
        }
    }

    private class Flash
    {
        private MeshParticalSystem meshParticalSystem;
        private Vector3 position;
        private int quadIndex;
        private Vector3 quadSize;
        private float rotation;
        private float lifetime;
        private float timer;
        private Color startColor;
        private Color endColor;

        public Flash(Vector3 position, Vector3 direction, MeshParticalSystem meshParticalSystem)
        {
            this.position = position + direction * 0.25f;
            this.meshParticalSystem = meshParticalSystem;

            rotation = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            quadSize = new Vector3(0.35f, 0.15f); 
            lifetime = 0.06f;
            timer = 0f;

            startColor = new Color(1f, 0.95f, 0.7f);
            endColor = new Color(1f, 0.6f, 0.2f);

            quadIndex = meshParticalSystem.AddQuad(position, rotation, quadSize, startColor);
        }

        public void Update()
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / lifetime);

            float scale = Mathf.Lerp(1f, 0f, t);
            Vector3 currentSize = quadSize * scale;

            Color currentColor = Color.Lerp(startColor, endColor, t);

            meshParticalSystem.UpdateQuad(quadIndex, position, rotation, currentSize, currentColor);
        }

        public bool IsComplete()
        {
            return timer >= lifetime;
        }

        public void ForceHide()
        {
            meshParticalSystem.UpdateQuad(quadIndex, position, rotation, Vector3.zero, endColor);
        }
    }

    private class Smoke
    {
        private MeshParticalSystem meshParticalSystem;
        private Vector3 position;
        private Vector3 direction;
        private int quadIndex;
        private Vector3 startSize;
        private Vector3 maxSize;
        private float rotation;
        private float rotationSpeed;
        private float moveSpeed;
        private float lifetime;
        private float timer;

        public Smoke(Vector3 position, Vector3 direction, MeshParticalSystem meshParticalSystem)
        {
            this.position = position;
            this.direction = (direction + new Vector3(Random.Range(-0.3f, 0.3f), Random.Range(-0.1f, 0.1f))).normalized;
            this.meshParticalSystem = meshParticalSystem;

            startSize = new Vector3(0.05f, 0.05f);
            maxSize = new Vector3(0.25f, 0.25f) * Random.Range(0.8f, 1.2f);
            rotation = Random.Range(0f, 360f);
            rotationSpeed = Random.Range(-90f, 90f);
            moveSpeed = Random.Range(0.5f, 1f);
            lifetime = Random.Range(0.3f, 0.5f);
            timer = 0f;

            quadIndex = meshParticalSystem.AddQuad(position, rotation, startSize);
        }

        public void Update()
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / lifetime);

            position += direction * moveSpeed * Time.deltaTime;
            rotation += rotationSpeed * Time.deltaTime;

            float sizeT = t < 0.5f ? t * 2f : 1f - (t - 0.5f) * 2f;
            sizeT = Mathf.Clamp01(sizeT);

            Vector3 currentSize = Vector3.Lerp(startSize, maxSize, sizeT);

            meshParticalSystem.UpdateQuad(quadIndex, position, rotation, currentSize);
        }

        public bool IsComplete()
        {
            return timer >= lifetime;
        }

        public void ForceHide()
        {
            meshParticalSystem.UpdateQuad(quadIndex, position, rotation, Vector3.zero);
        }
    }
}