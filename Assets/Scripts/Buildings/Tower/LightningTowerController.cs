using ShepProject;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;


namespace ShepProject
{

    internal struct ShockJumpData
    {

        public int jumpedFromInstanceID;
        public byte jumpNumber;


    }

    public class LightningTowerController : TowerBaseClass
    {
        public GameObject boltPrefab;
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
        [SerializeField]
        private float jumpRangeDecrease;

        private RaycastHit hit;

        public override bool IsShooting => false;

        private NativeHashMap<int, ShockJumpData> searchedObjects;


        protected override void Start()
        {
            base.Start();

            searchedObjects = new NativeHashMap<int, ShockJumpData>(100, Allocator.Persistent);

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

            
            hit.collider.gameObject.GetInstanceID();

            //EnemyPhysicsMethods methods = GetComponent<EnemyPhysicsMethods>();
            //methods.DealDamage(towerDamage, DamageType.Blaster);



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

            searchedObjects.Clear();
            DamageCurrentTargets(Physics.OverlapSphere(hit.point, jumpRadius, mask), hit.collider.gameObject.GetInstanceID(), 1);


            /*
             * after all the info has been gathered, get the data into an array
             * and then do all the GetComponents and deal the damage
             * instantiate all the lightning objects will correct amount of thickness as well
             * then keep track of them and disable them when needed
             * 
             */

            NativeKeyValueArrays<int, ShockJumpData> enemiesToDamage = searchedObjects.GetKeyValueArrays(Allocator.Temp);

            for (int i = 0; i < searchedObjects.Count; i++)
            {
                EnemyPhysicsMethods enemy = gameManager.NPCS.GetEnemyPhysicsMethodFromInstanceID(enemiesToDamage.Keys[i]);
                //enemy.DealDamage((1 - (enemiesToDamage.Values[i].jumpNumber * jumpDamageDecrease)) * towerDamage, DamageType.Lightning);

                GameObject boltInst = Instantiate(boltPrefab);
                Transform endPos = boltInst.transform.GetChild(0);
                boltInst.transform.position = enemy.transform.position;
                endPos.position = gameManager.NPCS.GetEnemyPhysicsMethodFromInstanceID(enemiesToDamage.Values[i].jumpedFromInstanceID).transform.position;

                Destroy(boltInst, beamActivationTime);

            }

            for (int i = 0; i < searchedObjects.Count; i++)
            {

                EnemyPhysicsMethods enemy = gameManager.NPCS.GetEnemyPhysicsMethodFromInstanceID(enemiesToDamage.Keys[i]);
                enemy.DealDamage((1 - (enemiesToDamage.Values[i].jumpNumber * jumpDamageDecrease)) * towerDamage, DamageType.Lightning);

            }


            enemiesToDamage.Dispose();

           
        }

        private void DamageCurrentTargets(Collider[] colliders, int jumpedFromInstanceID, byte jumpNumber)
        {

            int instID = 0;
            //float damagePercent = 0;
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

                instID = colliders[i].gameObject.GetInstanceID();
                ShockJumpData data = new ShockJumpData();
                data.jumpNumber = jumpNumber;
                data.jumpedFromInstanceID = jumpedFromInstanceID;

                if (searchedObjects.ContainsKey(instID))
                {
                    if (searchedObjects[instID].jumpNumber > jumpNumber)
                    {
                        searchedObjects[instID] = data;

                    }



                }
                else
                {
                    searchedObjects.Add(instID, data);

                    if (jumpNumber < jumpCount)
                        JumpToNextTargets(colliders[i], (byte)(jumpNumber + 1));

                }


            }


        }

        private void JumpToNextTargets(Collider collider, byte jumpNumber)
        {

            /*
             * for this need to check each collider, 
             * check if it's contained in searched 
             * 
             * if contained, check whether or not the damage dealt in this function is larger than before 
             *      if it's larger, then deal the extra damage
             *      
             */


            Collider[] colliders = Physics.OverlapSphere(collider.transform.position, jumpRadius - (jumpCount * jumpRangeDecrease * jumpRadius), mask);

            if (colliders.Length > 0) 
                DamageCurrentTargets(colliders, collider.gameObject.GetInstanceID(), (byte)(jumpNumber + 1));




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

        private void OnDisable()
        {

            searchedObjects.Dispose();

        }


    }

}