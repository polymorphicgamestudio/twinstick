using ShepProject;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class BombTowerController : BaseTower
{
    public GameObject bombPrefab;
    public float bombSpeed = 10f;

    public float minBombAirTime;
    public float maxBombAirTime;
    private float currentBombAirTime;

    private Vector3 shootingForce;
    private float vertex;

    public override void ShootTurret()
    {
        base.ShootTurret();

        shootingForce = slimeTarget.position - barrel.position;

        currentBombAirTime
            = math.lerp(minBombAirTime, maxBombAirTime,
            ((shootingForce.magnitude - minDist) / (maxDist - minDist)));

        ExplosiveProjectile bomb = 
            Instantiate(bombPrefab, barrel.position, Quaternion.identity)
            .GetComponent<ExplosiveProjectile>();

        //this is to make it start falling
        //at the correct vertex for the parabola
        shootingForce.y = (9.81f * currentBombAirTime) / 2f;

        bomb.transform.rotation = barrel.rotation;
        bomb.damage = towerDamage;
        bomb.rb.velocity = shootingForce;


    }
}
