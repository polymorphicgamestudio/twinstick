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

        //private EvolutionStructure evolutionStructure;

        [SerializeField]
        private Rigidbody rb;
        private NPCManager manager;


        public bool Initialized => enemyID != -1;

        public void UpdateID(ushort enemyID)
        {
            this.enemyID = enemyID;

        }


        public void SetInitialInfo(ushort enemyID, NPCManager manager)
        {

            this.manager = manager;
            this.enemyID = enemyID;
            //this.evolutionStructure = evolutionStructure;

        }

        public void DealDamage(float amount, DamageType damageType)
        {
            //get the resistance for this type of damage to check for any decreases in damage
            //then deal the damage
            float scaledDamage = amount;
            SlimeType type = (SlimeType)damageType;

            if (manager.EvolutionStructure.GetMainType(EnemyID) == type 
                && manager.EvolutionStructure.GetMainType(EnemyID) == type)
            {
                scaledDamage *= (manager.EvolutionStructure.GetMainResistance(EnemyID)
                    > manager.EvolutionStructure.GetSecondaryResistance(EnemyID))
                    ? manager.EvolutionStructure.GetMainResistance(EnemyID)
                    : manager.EvolutionStructure.GetSecondaryResistance(EnemyID);

            }
            else if (manager.EvolutionStructure.GetMainType(EnemyID) == type)
            {
                scaledDamage *= manager.EvolutionStructure.GetMainResistance(EnemyID);

            }

            else if (manager.EvolutionStructure.GetMainType(EnemyID) == type)
            {
                scaledDamage *= manager.EvolutionStructure.GetSecondaryResistance(EnemyID);

            }

            float health = manager.EvolutionStructure.GetHealth(enemyID);
            health -= scaledDamage;

            manager.EvolutionStructure.SetHealth(enemyID, health);

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

        public void GameOver()
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }



    }

}
