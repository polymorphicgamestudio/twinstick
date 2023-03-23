using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ShepProject {

    public class EnemyBurrow : MonoBehaviour {


        private EnemyManager manager;
        [SerializeField]
        private GameObject enemyPrefab;
        private float spawnTime;
        private float currentSpawnTime;

        private int enemiesToSpawn;

        public void Initialize(EnemyManager manager, float spawnTime) {

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
            GameObject enemy = GameObject.Instantiate(enemyPrefab);
            enemy.transform.position = transform.position;
            enemy.SetActive(true);

			manager.AddEnemyToList(enemy.transform);



        }


        public void AddEnemyCountToSpawn(int count)
        {
            enemiesToSpawn += count;

        }



    }

}