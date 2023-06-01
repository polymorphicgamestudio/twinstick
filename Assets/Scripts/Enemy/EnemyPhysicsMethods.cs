using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace ShepProject
{

    [RequireComponent(typeof(Rigidbody))]
    public class EnemyPhysicsMethods : MonoBehaviour
    {

        [SerializeField]
        private ushort enemyID;

        public int EnemyID => enemyID;

        private GenesArray genes;

        [SerializeField]
        private Rigidbody rb;
        private NPCManager manager;
        private float currentHealth;

        public bool Initialized()
        {
            return enemyID != ushort.MaxValue;

        }

        public void UpdateID(ushort enemyID)
        {
            this.enemyID = enemyID;

        }


        public void SetInitialInfo(ushort enemyID, GenesArray genes, NPCManager manager)
        {

            this.manager = manager;
            this.enemyID = enemyID;
            this.genes = genes;

        }

        public void DealDamage(float amount, DamageType damageType)
        {
            //get the resistance for this type of damage to check for any decreases in damage
            //then deal the damage

            //Debug.Log("Deal Damage: " + enemyID);

                                            //using this part when resistances work
            float scaledDamage = amount;// * genes.GetResistance(enemyID, damageType);

            currentHealth = genes.GetHealth(enemyID);
            currentHealth -= scaledDamage;

            genes.SetHealth(enemyID, currentHealth);

            if (currentHealth <= 0) 
            {
                //enemy is dead, let enemyManager know
                manager.OnEnemyDeath(enemyID);
                enemyID = ushort.MaxValue;
            }


        }

        public void SetVelocity(Vector3 velocity)
        {
            rb.velocity = velocity;

        }




    }

}
