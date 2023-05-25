using ShepProject;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class FireTowerController : MonoBehaviour
{
    public Transform positions;

    public Rigidbody bombPrefab;
    public static float bombSpeed = 20f;

    public Transform barrel;
    public ParticleSystem shoot;

    public static float timeBetweenShots = 2f;

    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating("ShootTurret", timeBetweenShots, timeBetweenShots);
    }

    void ShootTurret()
    {
        //if (BaseTower.slimebool == true) {
        //    shoot.Play();
        //    var BulletBody = (Rigidbody)Instantiate(bombPrefab, barrel.position, Quaternion.identity);
        //    BulletBody.velocity = barrel.forward * bombSpeed;
        //}
    }
}
