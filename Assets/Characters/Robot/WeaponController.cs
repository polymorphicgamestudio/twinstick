using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    public Transform positions;
    public Rigidbody bulletPrefab;
    public float bulletSpeed;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var BulletBody = Instantiate(bulletPrefab, positions.position, Quaternion.identity).GetComponent<Rigidbody>();
            BulletBody.velocity = positions.forward * bulletSpeed;
        }

    }
}
