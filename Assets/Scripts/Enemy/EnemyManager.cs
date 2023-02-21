using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ShepProject {

    public class EnemyManager : SystemBase {


        [SerializeField]
        private GameObject burrowPrefab;

		private List<Transform> enemyList;

        private List<EnemyBurrow> burrows;

        private QuadTree quadTree;

        private bool spawningEnemies;

        public bool SpawningEnemies => spawningEnemies;



		private void Start() {

			enemyList = new List<Transform>();

			burrows = new List<EnemyBurrow>();

            quadTree = new QuadTree(1000, 10);

			AddBurrow();

		}

		void Update() 
        {
            /*
             * each frame, spawn any new required enemies
             * 
             * copy the enemy positions to a float2 buffer for jobs since its a 2d representation
             * 
             * copy the headings also into the job
             * 
             * the types of buildings 
             *      - for each type of slime, have it be able to avoid different types of towers at different levels
             *      
             * 
             * 
             * 
             * run the jobs
             * 
             * after headings have been determined, move the rigidbody to stop collisions
             * 
             * 
             * 
             */

            //doesn't matter if bullets do damage before QT update
            //just need to make sure that they all happen before or after



            quadTree.NewFrame();

            //spawn enemies
            for (int i = 0; i < burrows.Count; i++) {
                burrows[i].ManualUpdate();

            }

            quadTree.Update();

		}


        public void AddEnemyToList(Transform enemy) {

            enemyList.Add(enemy);

        }

        private void AddBurrow() {

            EnemyBurrow burrow = GameObject.Instantiate(burrowPrefab).GetComponent<EnemyBurrow>();
            burrow.Initialize(this, 1f);

            burrows.Add(burrow);

		}


		private void OnDisable() {
            quadTree.Dispose();


		}



	}

}
