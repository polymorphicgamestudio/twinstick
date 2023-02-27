using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEditor;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.Profiling;

namespace ShepProject {


    internal enum GeneGroups {

        Type,
        Attractions,
        TotalGeneCount = 
            1 // object type
            + (int)Attraction.Count //for all the possible attractions an object can have
    }

    internal enum Attraction {
        Slime = 0,
        BaseTower,
        BlasterTower,
		FireTower,
		AcidTower,
		LightningTower,
		IceTower,
		LaserTower,
        Count


    }


    internal enum ObjectType {
        Player,
        Slime,
		BlasterTower,
		FireTower,
		AcidTower,
		LightningTower,
		IceTower,
		LaserTower,


	}



    public class EnemyManager : SystemBase {


        [SerializeField]
        private GameObject burrowPrefab;
        private List<EnemyBurrow> burrows;

        private NativeArray<float> traits;

        private NativeArray<float> headings;
        private NativeArray<float> newHeadings;

        private NativeArray<ushort> loopCounts;

        private bool switchedHeadings;

        private QuadTree quadTree;

        [SerializeField]
        private bool spawningEnemies;

        public bool SpawningEnemies => spawningEnemies;



		private void Start() {


            traits = new NativeArray<float>(50000, Allocator.Persistent);
            headings = new NativeArray<float>(1000, Allocator.Persistent);
            newHeadings = new NativeArray<float>(1000, Allocator.Persistent);
            switchedHeadings = false;
            loopCounts = new NativeArray<ushort>(50000, Allocator.Persistent);

            burrows = new List<EnemyBurrow>();

            quadTree = new QuadTree(1000, 7);

            quadTree.AddTransform(Inst.player.transform);
            traits[(int)GeneGroups.Type] = (int)(ObjectType.Player);
			AddBurrow();
			AddBurrow();
			AddBurrow();
			AddBurrow();
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

            //spawningEnemies = false;
            quadTree.Update();

            /*
             * - [0] type
             * - [1] attraction to slimes
             * - [2] base attaction to towers
             * - [3 - 8] for each tower multiplier
             * 
             * 
             * 
             */

            if (quadTree.positionCount <= 0)
                return;

            EnemyMovementJob moveJob = new EnemyMovementJob();
            moveJob.positions = quadTree.positions.Slice(0, quadTree.positionCount + 1);
            moveJob.buckets = quadTree.quadsList.Slice(0, quadTree.QuadsListLength + 1);
            moveJob.loopCounts = loopCounts;

            if (switchedHeadings) {
				moveJob.headings = newHeadings;
				moveJob.newHeadings = headings;
			}
            else {
				moveJob.headings = headings;
				moveJob.newHeadings = newHeadings;
			}

            moveJob.objectIDs = quadTree.objectIDs;
            moveJob.objectQuadIDs = quadTree.objectQuadIDs;
            moveJob.genes = traits;
            moveJob.deltaTime = Time.deltaTime;
            //moveJob.Run(quadTree.positionCount + 1);
            moveJob.Schedule(quadTree.positionCount + 1, SystemInfo.processorCount - 1).Complete();


            //after movement, write the information back to the transforms

            WriteTransformsJob wtj = new WriteTransformsJob();
            wtj.positions = quadTree.positions;

			if (switchedHeadings) {
				wtj.rotation = headings;
			}
			else {
				wtj.rotation = newHeadings;
			}

			switchedHeadings = !switchedHeadings;
			wtj.Schedule(quadTree.TransformAccess);

            Profiler.BeginSample("Writing Velocities");

            for (int i = 0; i <= quadTree.positionCount; i++) {

                quadTree.Transforms[i].gameObject.GetComponent<Rigidbody>().velocity = (quadTree.Transforms[i].forward * 10);



			}

            Profiler.EndSample();

		}


        public void AddEnemyToList(Transform enemy) {

            ushort id =  quadTree.AddTransform(enemy);

            //offset 1 for type, then attractions count
            traits[id * (1 + (int)Attraction.Count)] = (int)ObjectType.Slime;
            traits[(id * (int)GeneGroups.TotalGeneCount) + 1 + (int)Attraction.Slime] = -1;
        }

        private void AddBurrow() {

            EnemyBurrow burrow = GameObject.Instantiate(burrowPrefab).GetComponent<EnemyBurrow>();
            burrow.gameObject.transform.position = new Vector3((Random.value * 30) - 15, 0, (Random.value * 30) - 15);
            burrow.Initialize(this, .5f);

            burrows.Add(burrow);
            burrow.gameObject.SetActive(true);
		}


		private void OnDisable() {
            quadTree.Dispose();

            traits.Dispose();
            headings.Dispose();
            newHeadings.Dispose();
            loopCounts.Dispose();
		}



	}

}
