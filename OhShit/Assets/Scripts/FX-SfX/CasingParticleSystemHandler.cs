using System.Collections.Generic;
using UnityEngine;

public class CasingParticleSystemHandler : MonoBehaviour
{
    public static CasingParticleSystemHandler Instance { get; private set; }

    [Header("Rendering")]
    [SerializeField] private string sortingLayerName = "Effects";
    [SerializeField] private int sortingOrder = 0;

    [Header("Collision")]
    [SerializeField] private LayerMask wallLayerMask;

    private Dictionary<Material, MeshParticalSystem> systemsByMaterial;
    private List<Single> singleList;
    private GUN gun;

    private void Awake()
    {
        Instance = this;
        systemsByMaterial = new Dictionary<Material, MeshParticalSystem>();
        singleList = new List<Single>();
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
        for (int i = 0; i < singleList.Count; i++)
        {
            Single single = singleList[i];
            single.Update();

            if (single.IsMovementComplete())
            {
                singleList.RemoveAt(i);
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
        if (gun == null || gun.CasingMaterial == null) return;
        if (gun.CasingEjectPoint == null) return;

        float horizontalSpread = Random.Range(-0.3f, 0.3f);
        Vector3 casingDir = new Vector3(horizontalSpread, 1f, 0f).normalized;

        MeshParticalSystem meshParticalSystem = GetOrCreateSystem(gun.CasingMaterial);
        singleList.Add(new Single(gun.CasingEjectPoint.position, casingDir, meshParticalSystem, gun.CasingQuadSize, wallLayerMask));
    }

    private MeshParticalSystem GetOrCreateSystem(Material material)
    {
        if (systemsByMaterial.TryGetValue(material, out MeshParticalSystem existing))
            return existing;

        GameObject go = new GameObject($"CasingRenderer_{material.name}");
        go.transform.SetParent(transform, false);

        go.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = go.AddComponent<MeshRenderer>();
        meshRenderer.material = material;
        meshRenderer.sortingLayerName = sortingLayerName;
        meshRenderer.sortingOrder = sortingOrder;

        MeshParticalSystem newSystem = go.AddComponent<MeshParticalSystem>();
        systemsByMaterial.Add(material, newSystem);

        return newSystem;
    }

    private class Single
    {
        private MeshParticalSystem meshParticalSystem;
        private Vector3 position;
        private Vector3 direction;
        private int quadIndex;
        private Vector3 quadSize;
        private float rotation;
        private float moveSpeed;
        private LayerMask wallLayerMask;
        private bool stopped;

        public Single(Vector3 position, Vector3 direction, MeshParticalSystem meshParticalSystem, Vector3 quadSize, LayerMask wallLayerMask)
        {
            this.position = position;
            this.direction = direction;
            this.meshParticalSystem = meshParticalSystem;
            this.quadSize = quadSize;
            this.wallLayerMask = wallLayerMask;

            rotation = Random.Range(0f, 360f);
            moveSpeed = Random.Range(3f, 5f);

            quadIndex = meshParticalSystem.AddQuad(position, rotation, quadSize);
        }

        public void Update()
        {
            if (stopped) return;

            Vector3 nextPosition = position + direction * moveSpeed * Time.deltaTime;

            RaycastHit2D hit = Physics2D.Linecast(position, nextPosition, wallLayerMask);
            if (hit.collider != null)
            {
                position = hit.point;
                moveSpeed = 0f;
                stopped = true;
            }
            else
            {
                position = nextPosition;
            }

            rotation += 360f * (moveSpeed / 2f) * Time.deltaTime;

            meshParticalSystem.UpdateQuad(quadIndex, position, rotation, quadSize);

            float slowDownFactor = 3.5f;
            moveSpeed -= moveSpeed * slowDownFactor * Time.deltaTime;
        }

        public bool IsMovementComplete()
        {
            return stopped || moveSpeed < 0.1f;
        }
    }
}