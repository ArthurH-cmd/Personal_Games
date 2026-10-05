using UnityEngine;

public class ShotGun : GUN
{
    [SerializeField] private int pelletCount = 6;
    [SerializeField] private float spreadAngle = 15f;

    protected override void Fire()
    {
        for (int i = 0; i < pelletCount; i++)
        {
            float randomOffset = Random.Range(-spreadAngle / 2f, spreadAngle / 2f);
            Quaternion spreadRotation = firePoint.rotation * Quaternion.Euler(0f, 0f, randomOffset);

            Instantiate(bulletPrefab, firePoint.position, spreadRotation);
        }
    }
}
