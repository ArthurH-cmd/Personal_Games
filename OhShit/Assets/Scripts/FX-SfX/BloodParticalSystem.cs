using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BloodParticleSystem: MonoBehaviour
{
    public static BloodParticleSystem Instance { get; private set; }

    [SerializeField] private Vector3 particleSize = new Vector3(0.05f, 0.05f);

    private MeshParticalSystem meshParticalSystem;
    private List<Single> singleList;

    private void Awake()
    {
        Instance = this;
        meshParticalSystem = GetComponent<MeshParticalSystem>();
        singleList = new List<Single>();
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
        float bloodParticleCount = 3;
        for (int i = 0; i < bloodParticleCount; i++)
        {
            singleList.Add(new Single(position, ApplyRotationToVector(direction, Random.Range(-15f, 15f)), meshParticalSystem, particleSize));
        }
    }

    private static Vector3 ApplyRotationToVector(Vector3 vector, float angle)
    {
        return Quaternion.Euler(0, 0, angle) * vector;
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
        private int uvIndex;

        public Single(Vector3 position, Vector3 direction, MeshParticalSystem meshParticalSystem, Vector3 quadSize)
        {
            this.position = position;
            this.direction = direction;
            this.meshParticalSystem = meshParticalSystem;

            this.quadSize = quadSize;
            rotation = Random.Range(0, 360f);
            moveSpeed = Random.Range(3f, 5f);
            uvIndex = Random.Range(0, 8);

            quadIndex = meshParticalSystem.AddQuad(position, rotation, quadSize, Color.white, uvIndex);
        }

        public void Update()
        {
            position += direction * moveSpeed * Time.deltaTime;
            rotation += 360f * (moveSpeed / 10f) * Time.deltaTime;

            meshParticalSystem.UpdateQuad(quadIndex, position, rotation, quadSize, Color.white, uvIndex);

            float slowDownFactor = 3.5f;
            moveSpeed -= moveSpeed * slowDownFactor * Time.deltaTime;
        }

        public bool IsParticleComplete()
        {
            return moveSpeed < .1f;
        }
    }
}