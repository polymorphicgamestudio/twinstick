using ShepProject;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class BlasterTowerController : BaseTower 
{
 
    public Transform barrel;
    public ParticleSystem blasterParticles;

    public LineRenderer beam;
    private Vector3 direction;


    public float laserActivationTime;
    private float currentlaserActivationTime;


    public override void ManualUpdate()
    {
        base.ManualUpdate();

        if (beam.enabled)
        {
            currentlaserActivationTime -= Time.deltaTime;

            if (currentlaserActivationTime < 0)
                beam.enabled = false;


        }


    }


    public override void ShootTurret()
    {
        direction = barrel.position - transform.position;
        direction.y = 0;

        if (!Physics.Raycast(new Ray(barrel.position, direction), out RaycastHit hit, maxDist, mask))
        {
            return;
        }

        hit.collider.GetComponent<EnemyPhysicsMethods>().DealDamage(100, DamageType.Blaster);

        beam.enabled = true;
        beam.SetPosition(0, barrel.position);
        beam.SetPosition(1, hit.point);

        blasterParticles.Play();


        currentlaserActivationTime = laserActivationTime;
            
    }

}