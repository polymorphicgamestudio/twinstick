using ShepProject;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class AcidTowerController : MonoBehaviour
{

    public Transform positions;

    public Rigidbody bombPrefab;
    public static float bombSpeed = 10f;

    public Transform barrel;
    public ParticleSystem shoot;

    private Animator anim;
    public static float timeBetweenShots = 3.0f;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        InvokeRepeating("ShootTurret", timeBetweenShots, timeBetweenShots);
    }

    void ShootTurret()
    {
        if (BaseTower.slimebool == true)
        {
                anim.Play("Base Layer.Shoot", 0, 0);
                shoot.Play();
                var BulletBody = (Rigidbody)Instantiate(bombPrefab, barrel.position, Quaternion.identity);
                BulletBody.velocity = barrel.forward * bombSpeed;
        }
    }
}
