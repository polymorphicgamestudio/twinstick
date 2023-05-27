using ShepProject;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningTowerController : BeamTowerController
{

    public GameObject lightningBolt;
    public Transform end;

    private RaycastHit hit;


    protected override void Start()
    {
        base.Start();

        lightningBolt.transform.SetParent(null);

    }

    public override void ManualUpdate()
    {
        base.ManualUpdate();

        if (beam.enabled)
        {

            end.position = hit.point;

        }

    }


    public override void ShootTurret()
    {
        direction = barrel.position - transform.position;
        direction.y = 0;

        if (!Physics.Raycast(new Ray(barrel.position, direction), out hit, maxDist, mask))
        {
            return;
        }

        hit.collider.GetComponent<EnemyPhysicsMethods>().DealDamage(100, DamageType.Blaster);



        base.ShootTurret();

        end.position = hit.point;

    }



}
