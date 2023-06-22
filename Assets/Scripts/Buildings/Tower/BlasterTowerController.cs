using ShepProject;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class BlasterTowerController : BeamTowerController
{

    private RaycastHit hit;
    [SerializeField]
    private float burstTime;

    [SerializeField]
    private float currentBurstTime;
    [SerializeField]
    private float betweenShotCooldownTime;
    private float currentBetweenShotCooldownTime;

    protected override bool IsShooting => (currentBurstTime > 0);

    public override void ManualUpdate()
    {

        base.ManualUpdate();

        currentBurstTime -= Time.deltaTime;
        if (beam.enabled)
        {

            beam.SetPosition(0, barrel.position);
            beam.SetPosition(1, hit.point);

        }
        else
        {

            currentBetweenShotCooldownTime -= Time.deltaTime;
            if (currentBurstTime > 0 && currentBetweenShotCooldownTime <= 0)
            {
                ShootTurret();
            }



        }


    }

    public override void EndOfWave()
    {
        base.EndOfWave();



    }

    public override void ShootTurret()
    {

        if (currentBurstTime <= 0)
        {
            currentBurstTime = burstTime;

        }
        currentBetweenShotCooldownTime = betweenShotCooldownTime;
        direction = barrel.position - transform.position;
        direction.y = 0;

        if (!Physics.Raycast(new Ray(barrel.position, direction), out hit, maxDist, mask))
        {
            return;
        }

        hit.collider.GetComponent<EnemyPhysicsMethods>().DealDamage(towerDamage, DamageType.Blaster);

        base.ShootTurret();







    }


}