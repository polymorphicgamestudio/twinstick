using ShepProject;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ShepProject
{

    public class LightningTowerController : BaseTower
    {
        public GameObject lightningBolt;
        public Transform end;

        protected Vector3 direction;
        public float beamActivationTime;
        private float currentBeamActivationTime;


        private RaycastHit hit;


        protected override void Start()
        {
            base.Start();

            lightningBolt.transform.SetParent(null);
            lightningBolt.SetActive(false);
        }

        public override void ManualUpdate()
        {
            base.ManualUpdate();

            if (lightningBolt.activeInHierarchy)
            {
                currentBeamActivationTime -= Time.deltaTime;

                if (currentBeamActivationTime < 0)
                    lightningBolt.SetActive(false);


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

            hit.collider.GetComponent<EnemyPhysicsMethods>().DealDamage(towerDamage, DamageType.Blaster);



            base.ShootTurret();

            currentBeamActivationTime = beamActivationTime;
            lightningBolt.SetActive(true);

            end.position = hit.point;

        }

        public override void EndOfWave()
        {
            base.EndOfWave();

            Invoke(nameof(TurnOffBolt), currentBeamActivationTime);

        }

        protected void TurnOffBolt()
        {
            currentBeamActivationTime = 0;
            lightningBolt.SetActive(false);
            
        }


    }

}