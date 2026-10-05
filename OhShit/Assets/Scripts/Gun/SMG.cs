using UnityEngine;

public class SMG : GUN
{
    [SerializeField] private float spreadAngle = 15f;

    protected override void Fire()
    {
        float randomOffset = Random.Range(-spreadAngle / 2f, spreadAngle / 2f);
        Quaternion spreadRotation = firePoint.rotation * Quaternion.Euler(0f, 0f, randomOffset);

        Instantiate(bulletPrefab, firePoint.position, spreadRotation);
    }

}
