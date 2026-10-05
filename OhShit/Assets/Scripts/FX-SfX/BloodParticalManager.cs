using System.Collections.Generic;
using UnityEngine;

public class BloodParticleSystemHandler : MonoBehaviour
{
    public static BloodParticleSystemHandler Instance { get; private set; }

    [Header("Materials")]
    [SerializeField] private Material[] bloodMaterials;

    [Header("Particle Size")]
    [SerializeField] private Vector3 particleSize = new Vector3(0.05f, 0.05f);
    [SerializeField] private float minSizeMultiplier = 0.6f;
    [SerializeField] private float maxSizeMultiplier = 1.5f;

    [Header("Spread")]
    [SerializeField] private int minParticles = 5;
    [SerializeField] private int maxParticles = 8;
    [SerializeField] private float spreadAngle = 18f;
    [SerializeField] private float spawnJitter = 0.06f;

    [Header("Trail")]
    [SerializeField] private bool enableTrail = true;
    [SerializeField] private float trailInterval = 0.04f;
    [SerializeField] private float trailSizeMultiplier = 0.35f;
    [SerializeField] private int maxTrailPieces = 6;
    [SerializeField] private float trailPositionJitter = 0.015f;

    [Header("Entry Trail")]
    [SerializeField] private bool enableEntryTrail = true;
    [SerializeField] private int entryTrailParticles = 2;
    [SerializeField] private float entryTrailSpread = 20f;
    [SerializeField] private float entryTrailSizeMultiplier = 0.2f;
    [SerializeField] private float entryTrailSpeedMultiplier = 0.35f;

    [Header("Rendering")]
    [SerializeField] private string sortingLayerName = "Effects";
    [SerializeField] private int sortingOrder = 5;

    [Header("Collision")]
    [SerializeField] private LayerMask wallLayerMask;

    private Dictionary<Material, MeshParticalSystem> systemsByMaterial;
    private List<Single> singleList;

    private void Awake()
    {
        Instance = this;
        systemsByMaterial = new Dictionary<Material, MeshParticalSystem>();
        singleList = new List<Single>();

        if (bloodMaterials == null || bloodMaterials.Length == 0)
            Debug.LogWarning($"{name}: no bloodMaterials assigned.");
    }

    private void OnEnable()
    {
        Bullet.onBulletHit += HandleBulletHit;
    }

    private void OnDisable()
    {
        Bullet.onBulletHit -= HandleBulletHit;
    }

    private void Update()
    {
        for (int i = 0; i < singleList.Count; i++)
        {
            Single single = singleList[i];
            single.Update();

            if (single.IsParticleComplete())
            {
                singleList.RemoveAt(i);
                i--;
            }
        }
    }

    private void HandleBulletHit(Vector3 position, Vector3 direction)
    {
        SpawnBlood(position, direction);
    }

    public void SetParticleSize(Vector3 newSize)
    {
        particleSize = newSize;
    }

    public void SpawnBlood(Vector3 position, Vector3 direction)
    {
        if (bloodMaterials == null || bloodMaterials.Length == 0) return;

        SpawnMainBlood(position, direction);

        if (enableEntryTrail)
        {
            SpawnEntryTrail(position, direction);
        }
    }

    private void SpawnMainBlood(Vector3 position, Vector3 direction)
    {
        int count = Random.Range(minParticles, maxParticles + 1);

        for (int i = 0; i < count; i++)
        {
            Material chosenMaterial = bloodMaterials[Random.Range(0, bloodMaterials.Length)];
            MeshParticalSystem meshParticalSystem = GetOrCreateSystem(chosenMaterial);

            Vector3 jitter = new Vector3(
                Random.Range(-spawnJitter, spawnJitter),
                Random.Range(-spawnJitter, spawnJitter),
                0f
            );

            float sizeVariance = Random.Range(minSizeMultiplier, maxSizeMultiplier);
            Vector3 sizedParticle = particleSize * sizeVariance;

            Vector3 particleDirection = ApplyRotationToVector(
                direction,
                Random.Range(-spreadAngle, spreadAngle)
            );

            singleList.Add(new Single(
                position + jitter,
                particleDirection,
                meshParticalSystem,
                sizedParticle,
                wallLayerMask,
                enableTrail,
                trailInterval,
                trailSizeMultiplier,
                maxTrailPieces,
                trailPositionJitter,
                1f
            ));
        }
    }

    private void SpawnEntryTrail(Vector3 position, Vector3 direction)
    {
        Vector3 backwardsDirection = -direction;

        for (int i = 0; i < entryTrailParticles; i++)
        {
            Material chosenMaterial = bloodMaterials[Random.Range(0, bloodMaterials.Length)];
            MeshParticalSystem meshParticalSystem = GetOrCreateSystem(chosenMaterial);

            float sizeVariance = Random.Range(0.7f, 1.1f);
            Vector3 sizedParticle = particleSize * entryTrailSizeMultiplier * sizeVariance;

            Vector3 particleDirection = ApplyRotationToVector(
                backwardsDirection,
                Random.Range(-entryTrailSpread, entryTrailSpread)
            );

            singleList.Add(new Single(
                position,
                particleDirection,
                meshParticalSystem,
                sizedParticle,
                wallLayerMask,
                false,
                0f,
                0f,
                0,
                0f,
                entryTrailSpeedMultiplier
            ));
        }
    }

    private static Vector3 ApplyRotationToVector(Vector3 vector, float angle)
    {
        return Quaternion.Euler(0, 0, angle) * vector;
    }

    private MeshParticalSystem GetOrCreateSystem(Material material)
    {
        if (systemsByMaterial.TryGetValue(material, out MeshParticalSystem existing))
            return existing;

        GameObject go = new GameObject($"BloodRenderer_{material.name}");
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

    /*
     * Represents a single Blood Particle
     * */
    private class Single
    {
        private MeshParticalSystem meshParticalSystem;
        private Vector3 position;
        private Vector3 direction;
        private int quadIndex;
        private Vector3 quadSize;
        private float moveSpeed;
        private float rotation;
        private Color color;
        private LayerMask wallLayerMask;
        private bool stopped;

        private bool enableTrail;
        private float trailInterval;
        private float trailTimer;
        private float trailSizeMultiplier;
        private int maxTrailPieces;
        private int trailPieces;
        private float trailPositionJitter;

        public Single(
            Vector3 position,
            Vector3 direction,
            MeshParticalSystem meshParticalSystem,
            Vector3 quadSize,
            LayerMask wallLayerMask,
            bool enableTrail,
            float trailInterval,
            float trailSizeMultiplier,
            int maxTrailPieces,
            float trailPositionJitter,
            float speedMultiplier)
        {
            this.position = position;
            this.direction = direction;
            this.meshParticalSystem = meshParticalSystem;
            this.quadSize = quadSize;
            this.wallLayerMask = wallLayerMask;

            this.enableTrail = enableTrail;
            this.trailInterval = trailInterval;
            this.trailTimer = trailInterval;
            this.trailSizeMultiplier = trailSizeMultiplier;
            this.maxTrailPieces = maxTrailPieces;
            this.trailPositionJitter = trailPositionJitter;

            float travelAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            rotation = travelAngle + Random.Range(-20f, 20f);

            moveSpeed = Random.Range(16f, 22f) * speedMultiplier;

            float shade = Random.Range(0.75f, 1.1f);
            color = new Color(shade, shade, shade);

            quadIndex = meshParticalSystem.AddQuad(
                position,
                rotation,
                quadSize,
                color
            );
        }

        public void Update()
        {
            if (stopped) return;

            Vector3 nextPosition = position + direction * moveSpeed * Time.deltaTime;

            RaycastHit2D hit = Physics2D.Linecast(
                position,
                nextPosition,
                wallLayerMask
            );

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

            rotation += moveSpeed * 12f * Time.deltaTime;

            meshParticalSystem.UpdateQuad(
                quadIndex,
                position,
                rotation,
                quadSize,
                color
            );

            if (enableTrail)
            {
                UpdateTrail();
            }

            float slowDownFactor = 10f;
            moveSpeed -= moveSpeed * slowDownFactor * Time.deltaTime;
        }

        private void UpdateTrail()
        {
            trailTimer -= Time.deltaTime;

            if (trailTimer <= 0f && trailPieces < maxTrailPieces && moveSpeed > 0.1f)
            {
                SpawnTrailPiece();
                trailTimer = trailInterval;
            }
        }

        private void SpawnTrailPiece()
        {
            Vector3 trailSize = quadSize * trailSizeMultiplier;

            float randomScale = Random.Range(0.7f, 1.2f);
            trailSize *= randomScale;

            float trailRotation = rotation + Random.Range(-30f, 30f);

            Vector2 jitter = Random.insideUnitCircle * trailPositionJitter;

            Vector3 trailPosition = position + new Vector3(
                jitter.x,
                jitter.y,
                0f
            );

            Color trailColor = color;
            trailColor *= Random.Range(0.6f, 0.9f);

            meshParticalSystem.AddQuad(
                trailPosition,
                trailRotation,
                trailSize,
                trailColor
            );

            trailPieces++;
        }

        public bool IsParticleComplete()
        {
            return stopped || moveSpeed < .1f;
        }
    }
}
