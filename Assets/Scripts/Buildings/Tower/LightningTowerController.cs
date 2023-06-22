using ShepProject;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;


namespace ShepProject
{

    public class LightningTowerController : TowerBaseClass
    {
        public GameObject lightningBolt;
        public Transform end;

        protected Vector3 direction;
        public float beamActivationTime;
        private float currentBeamActivationTime;

        [SerializeField]
        private float jumpRadius;
        [SerializeField]
        private int jumpCount;
        [SerializeField]
        private float jumpDamageDecrease;


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

            EnemyPhysicsMethods methods = hit.collider.GetComponent<EnemyPhysicsMethods>();
            methods.DealDamage(towerDamage, DamageType.Blaster);



            base.ShootTurret();

            currentBeamActivationTime = beamActivationTime;
            lightningBolt.SetActive(true);

            end.position = hit.point;

            /*
             * overlap sphere and jump to new targets with lower damage after each jump
             * 
             * hash map structure needs to contain
             *      - instance ID or collider
             *          end position will be gotten from this collider's position
             *      - start position
             *          will start from the previous collider's position
             * 
             */

            NativeHashMap<int, int> searched = new NativeHashMap<int, int>(200, Allocator.Temp);
            Collider[] colliders = Physics.OverlapSphere(hit.point, jumpRadius, mask);

            DamageCurrentTargets(colliders, hit.point, ref searched);

            for (int i = 0; i < colliders.Length; i++)
            {

                JumpToNextTargets(colliders[i], ref searched);

            }

            /*
             * after all the info has been gathered, get the data into an array
             * and then do all the GetComponents and deal the damage
             * instantiate all the lightning objects will correct amount of thickness as well
             * then keep track of them and disable them when needed
             * 
             */



            searched.Dispose();

        }

        private void DamageCurrentTargets(Collider[] colliders, Vector3 startPoint, ref NativeHashMap<int, int> searched)
        {

            for (int i = 0; i < colliders.Length; i++)
            {


                /*
                 * check if the collider is already being shocked
                 *      if its already being shocked, check if this shock will do more damage than currently being done
                 *      if more damage is done then update its information in the searched NativeHashMap
                 * 
                 *
                 * 
                 * 
                 */


            }



        }

        private void JumpToNextTargets(Collider collider, ref NativeHashMap<int, int> searched)
        {



            /*
             * for this need to check each collider, 
             * check if it's contained in searched 
             * 
             * if contained, check whether or not the damage dealt in this function is larger than before 
             *      if it's larger, then deal the extra damage
             *      
             *      
             * 
             * 
             */


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