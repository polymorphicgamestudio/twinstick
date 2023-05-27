using ShepProject;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class BlasterTowerController : BeamTowerController
{

    private RaycastHit hit;


    public override void ManualUpdate()
    {
        base.ManualUpdate();

        if (beam.enabled)
        {

            beam.SetPosition(0, barrel.position);
            beam.SetPosition(1, hit.point);

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







    }


}