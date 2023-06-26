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

    private Vector3 forward => barrel.position - transform.position;

    public override bool IsShooting => (currentBurstTime > 0);

    protected override void Start()
    {
        base.Start();

        for (int i = 0; i < animator.runtimeAnimatorController.animationClips.Length; i++)
        {
            if (animator.runtimeAnimatorController.animationClips[i].name.ToLower().Contains("shoot"))
                burstTime = animator.runtimeAnimatorController.animationClips[i].length;

        }

    }

    public override void ManualUpdate()
    {

        base.ManualUpdate();

        currentBurstTime -= Time.deltaTime;
        if (beam.enabled)
        {

            beam.SetPosition(0, barrel.position);
            
            if (hit.collider != null)
            beam.SetPosition(1, hit.point);
            else
            {
                beam.SetPosition(1, barrel.position + (rotBoneHoz.forward * maxDist * 1.25f));
            }
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

        if (!Physics.Raycast(new Ray(barrel.position, direction), out hit, maxDist * 1.25f, mask))
        {
            //hit.point = direction * (maxDist * 1.5f);
            //return;
        }
        else
        {
            hit.collider.GetComponent<EnemyPhysicsMethods>().DealDamage(towerDamage, DamageType.Blaster);
        }


        base.ShootTurret();







    }


}