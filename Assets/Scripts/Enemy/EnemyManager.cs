using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEditor;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.Profiling;

namespace ShepProject {




    public class EnemyManager : SystemBase {


        [SerializeField]
        private GameObject burrowPrefab;
        public GameObject sheepPrefab;

        public int sheepCount;

        private List<EnemyBurrow> burrows;

        private GenesArray genes;

		//will contain IDs of sheep and player, and towers won't be targeted
		private NativeList<ushort> choosableTargets;
        
        //will store the target id of the slime
        private NativeArray<ushort> targetIDs;

        private NativeArray<float> headings;
        private NativeArray<float> newHeadings;

        private NativeArray<ushort> loopCounts;

        private bool switchedHeadings;

        private QuadTree quadTree;

        [SerializeField]
        private bool spawningEnemies;

        public bool SpawningEnemies => spawningEnemies;



		private void Start() {


            genes = new GenesArray(50000, Allocator.Persistent);

			choosableTargets = new NativeList<ushort>(1000, Allocator.Persistent);
			targetIDs = new NativeArray<ushort>(1000, Allocator.Persistent);

            for (int i = 0; i < targetIDs.Length; i++) {


                targetIDs[i] = ushort.MaxValue;

            }

            headings = new NativeArray<float>(1000, Allocator.Persistent);
            newHeadings = new NativeArray<float>(1000, Allocator.Persistent);
            switchedHeadings = false;
            loopCounts = new NativeArray<ushort>(50000, Allocator.Persistent);

            burrows = new List<EnemyBurrow>();

            quadTree = new QuadTree(1000, 10);

            quadTree.AddTransform(Inst.player.transform);
            genes.SetObjectType(0, ObjectType.Player);

            //uncomment to add player to list of choosableTargets
            choosableTargets.Add(0);

			SpawnSheep();


			AddBurrow();
			AddBurrow();
			AddBurrow();
			AddBurrow();
			AddBurrow();
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

            //get targets before updating movement

            ChooseTargetJob ctj = new ChooseTargetJob();
            ctj.choosableTargets = choosableTargets;
            ctj.positions = quadTree.positions;
            ctj.objectIDs = quadTree.objectIDs;
            ctj.targetIDs = targetIDs;
            ctj.Schedule(quadTree.positionCount + 1, SystemInfo.processorCount).Complete();


            AIMovementJob moveJob = new AIMovementJob();
            moveJob.positions = quadTree.positions.Slice(0, quadTree.positionCount + 1);
            moveJob.buckets = quadTree.quadsList.Slice(0, quadTree.QuadsListLength);
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
            moveJob.genes = genes;
            moveJob.targetIDs = targetIDs;
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

                if (genes[i * (int)GeneGroups.TotalGeneCount] == (int)ObjectType.Sheep) {

                    //if being chased, set velocity, otherwise don't
                    if (headings[i] != newHeadings[i])
					    quadTree.Transforms[i].gameObject.GetComponent<Rigidbody>().velocity = (quadTree.Transforms[i].forward * 3);

					continue;

                }

				quadTree.Transforms[i].gameObject.GetComponent<Rigidbody>().velocity = (quadTree.Transforms[i].forward * 5);



			}

			Profiler.EndSample();

		}


        public void AddEnemyToList(Transform enemy) {

            ushort id =  quadTree.AddTransform(enemy);

            //offset 1 for type, then attractions count
            genes.SetObjectType(id, ObjectType.Slime);
            genes.SetAttraction(id, Attraction.Slime, -2);

            //genes[id * (1 + (int)Attraction.Count)] = (int)ObjectType.Slime;
            //genes[(id * (int)GeneGroups.TotalGeneCount) + 1 + (int)Attraction.Slime] = -2;
        }

        private void AddBurrow() {

            EnemyBurrow burrow = GameObject.Instantiate(burrowPrefab).GetComponent<EnemyBurrow>();

            int min = 8;
            int max = 15;

            //if (Random.value > .5f)
                burrow.gameObject.transform.position = new Vector3(Random.Range(-5, 5) , 0, Random.Range(min, max));
            //else
				//burrow.gameObject.transform.position = new Vector3(Random.Range(-min, -max), 0, Random.Range(-min, -max));

			burrow.Initialize(this, .5f);

            burrows.Add(burrow);
            burrow.gameObject.SetActive(true);
		}



        private void SpawnSheep() {

            for (int i = 0; i < sheepCount; i++) {

                GameObject sheep = Instantiate(sheepPrefab);

                sheep.transform.position = new Vector3(Random.Range(-4, 4), 0, Random.Range(-4, 4));

				ushort id = quadTree.AddTransform(sheep.transform);

				genes.SetObjectType(id, ObjectType.Sheep);
				genes.SetAttraction(id, Attraction.Slime, -5);

				//genes[id * (1 + (int)Attraction.Count)] = (int)ObjectType.Sheep;
				//genes[(id * (int)GeneGroups.TotalGeneCount) + 1 + (int)Attraction.Slime] = -5;
                choosableTargets.Add(id);

                sheep.SetActive(true);
			}



        }

		private void OnDisable() {
            quadTree.Dispose();

            genes.Dispose();
            headings.Dispose();
            newHeadings.Dispose();
            loopCounts.Dispose();
		}



	}

}
