using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public class WeaponBase
{

    public string name;
    public float timer;
    public float currentTimer;
    public float bulletSpeed;
    public bool shooting;

    public GameObject projectile;
    public Transform bulletSpawn;

    public void Update()
    {
        currentTimer -= Time.deltaTime;

        if (currentTimer > 0)
            return;

        if (!shooting)
            return;

        Shoot();


    }

    public void EquipWeapon(Transform bulletSpawn)
    {
        currentTimer = timer;
        this.bulletSpawn = bulletSpawn;
    }

    public void Shoot()
    {
        GameObject bullet = GameObject.Instantiate(projectile);
        bullet.transform.position = bulletSpawn.position;
        bullet.transform.rotation = bulletSpawn.rotation;
        bullet.GetComponent<Projectile>()
            .Initialize((bulletSpawn.position - bulletSpawn.parent.position).normalized, bulletSpeed);


    }


}
