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
        private int enemyID;

        public int EnemyID => enemyID;

        private GenesArray genes;

        [SerializeField]
        private Rigidbody rb;
        private AIManager manager;


        public bool Initialized()
        {
            return enemyID != -1;

        }

        public void UpdateID(ushort enemyID)
        {
            this.enemyID = enemyID;

        }


        public void SetInitialInfo(ushort enemyID, GenesArray genes, AIManager manager)
        {

            this.manager = manager;
            this.enemyID = enemyID;
            this.genes = genes;

        }

        public void DealDamage(float amount, DamageType damageType)
        {
            //get the resistance for this type of damage to check for any decreases in damage
            //then deal the damage
                                            //using this part when resistances work
            float scaledDamage = amount;// * genes.GetResistance(enemyID, damageType);

            


            float health = genes.GetHealth(enemyID);


            health -= scaledDamage;

            genes.SetHealth(enemyID, health);

            if (health < 0) 
            {
                //enemy is dead, let enemyManager know
                manager.OnEnemyDeath((ushort)enemyID);
                enemyID = -1;
            }


        }

        public void SetVelocity(Vector3 velocity)
        {
            rb.velocity = velocity;

        }




    }

}
