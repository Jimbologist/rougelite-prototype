using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * This class handles collsion, aniamtion, and extra behavior for a weapon's bullet.
 * No states are applied. Only damage or other effects are carried over from
 * the specific weapon script of the weapon this bullet is associated with.
 */
public class Bullet : MonoBehaviour
{
    [SerializeField] private Rigidbody2D bulletRB;
    [SerializeField] private Vector2 bulletDir;

    void Awake()
    {
        bulletRB = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public virtual void OnShoot(float shotSpeed, Vector2 shootDir, float finalAccuracy)
    {
        //Normalize direction vector and get z-axis angle for new bullet in degrees
        this.bulletDir = shootDir;
        shootDir = shootDir.normalized;

        //TODO: Modify shoot direction based on final accuracy percentage.


        float bulletAngleZ = Mathf.Atan2(shootDir.y, shootDir.x) * Mathf.Rad2Deg;
        if (bulletAngleZ < 0) bulletAngleZ += 360;
        transform.eulerAngles = new Vector3(0, 0, bulletAngleZ);

        bulletRB.AddForce(shootDir * shotSpeed, ForceMode2D.Impulse);
    }
}