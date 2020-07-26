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
    [SerializeField] protected Weapon weapon;
    [SerializeField] protected Rigidbody2D bulletRB;
    [SerializeField] protected Vector2 bulletDir;
    [SerializeField] protected bool rotateBulletWithAim;

    protected float shotSpeed;
    protected float fireDir;
    protected float accuracy;
    protected bool isFired = false;

    public FakeHeightObject FakeHeight { get; set; }
    public Collider2D BulletCollider { get; private set; }
    protected virtual void Awake()
    {
        BulletCollider = GetComponent<Collider2D>();
        bulletRB = GetComponent<Rigidbody2D>();
        bulletRB.useFullKinematicContacts = true;
    }

    // Update is called once per frame
    protected void Update()
    {
        if (isFired) MoveBullet();
    }

    public virtual void OnShoot(float shotSpeed, Vector2 shootDir, float finalAccuracy, Weapon weapon)
    {
        this.weapon = weapon;
        Physics2D.IgnoreCollision(weapon.CurrentPlayer.MainCollider, BulletCollider);

        //Normalize direction vector and get z-axis angle for new bullet in degrees
        this.bulletDir = shootDir;
        shootDir = shootDir.normalized;

        this.shotSpeed = shotSpeed;
        this.accuracy = finalAccuracy;
        //TODO: Modify shoot direction based on final accuracy percentage.

        if(rotateBulletWithAim)
        {
            float bulletAngleZ = Mathf.Atan2(shootDir.y, shootDir.x) * Mathf.Rad2Deg;
            if (bulletAngleZ < 0) bulletAngleZ += 360;
            transform.localEulerAngles = new Vector3(0, 0, bulletAngleZ);
        }
        
        //Flip y-scale of bullet and its shadow when shooting in -x direction, just like guns do when
        //aiming in that direction.
        if (shootDir.x < 0)
        {
            Vector3 newScale = transform.localScale;
            newScale.y = -newScale.y;
            transform.localScale = newScale;
        }

        //bulletRB.AddForce(shootDir * shotSpeed, ForceMode2D.Impulse);
        isFired = true;
    }

    protected virtual void MoveBullet()
    {
        this.transform.position += (Vector3)bulletDir * shotSpeed * Time.deltaTime;
    }

    protected void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.rigidbody.bodyType == RigidbodyType2D.Static)
            gameObject.SetActive(false);
    }
}