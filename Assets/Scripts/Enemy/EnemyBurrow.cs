using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ShepProject {

    public class EnemyBurrow : MonoBehaviour {


        private NPCManager manager;
        private float spawnTime;
        private float currentSpawnTime;

        private int enemiesToSpawn;

        public void Initialize(NPCManager manager, float spawnTime) {

			this.manager = manager;
			this.spawnTime = spawnTime;

		}

        public void ManualUpdate() {

            if (enemiesToSpawn <= 0)
                return;

            currentSpawnTime -= Time.deltaTime;

            if (currentSpawnTime > 0)
                return;

            currentSpawnTime = spawnTime;

            SpawnEnemy();

        }


        public void SpawnEnemy() {

            //spawn base enemy prefab which has all base behaviours
            //then add it to the list



            EnemyPhysicsMethods enemy = manager.GetPooledSlime();
            enemy.transform.position = transform.position;
            enemy.gameObject.SetActive(true);

			manager.AddEnemyToList(enemy);



        }


        public void AddEnemyCountToSpawn(int count)
        {
            enemiesToSpawn += count;

        }



    }

}