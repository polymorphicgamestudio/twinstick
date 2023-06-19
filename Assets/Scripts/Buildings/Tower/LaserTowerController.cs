using ShepProject;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using UnityEngine;
using static UnityEngine.UI.Image;

public class LaserTowerController : BeamTowerController
{

    [SerializeField]
    private Laser laser;

    private RaycastHit[] hits;

    protected override void Start()
    {
        base.Start();
        laser.SetBeamDistances(maxDist);
    }


    public override void ManualUpdate()
    {


        base.ManualUpdate();


        if (beam.enabled)
        {

            laser.laserEnd.gameObject.SetActive(true);
            direction = barrel.position - transform.position;
            direction.Normalize();
            direction.y = 0;

            hits = Physics.RaycastAll(new Ray(barrel.position, direction), maxDist, mask);

            if (hits.Length == 0)
                return;

            for (int i = 0; i < hits.Length; i++)
            {
                hits[i].collider.GetComponent<EnemyPhysicsMethods>()
                    .DealDamage(towerDamage * Time.deltaTime, DamageType.Laser);

            }


        }
        else
        {
            laser.laserEnd.gameObject.SetActive(false);

        }


    }



}