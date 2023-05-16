using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Collections.LowLevel;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.Profiling;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace ShepProject {




	public class EnemyManager : SystemBase {

		#region Variables

		[SerializeField]
		private GameObject burrowPrefab;
		public GameObject sheepPrefab;
		public GameObject endOfWaveCanvasParent;

		public int sheepCount;
		public int maxEnemies;

		[ReadOnly]
		public int TreeObjectCount;

		[SerializeField]
		private int waveNumber;
		private bool duringWave;
		private bool updateInitialize;

		[SerializeField]
		private float countdownToWave;
		[SerializeField]
		private float currentCountdownToWave;

		[SerializeField]
		private int enemiesCountToSpawn;
		private int enemiesLeftToSpawn;

		private int enemiesLeftToKill;

		[SerializeField]
		private bool spawningEnemies;


		private Dictionary<int, EnemyPhysicsMethods> enemyPhysicsMethods;



		private List<EnemyBurrow> burrows;

		private GenesArray genes;
		public GenesArray Genes => genes;

		//will contain IDs of sheep and player, and towers won't be targeted
		private NativeList<ushort> choosableTargets;


		//will store the target id of the slime
		private NativeArray<ushort> targetIDs;
		private NativeArray<float> sheepDistancesToSlimes;
		private NativeArray<float2> objectForces;
		private NativeArray<QuadKey> divisionOneKeys;

		private NativeArray<float> headings;

		private QuadTree quadTree;
		public QuadTree QuadTree => quadTree;


		public bool SpawningEnemies => spawningEnemies;

		#endregion

		#region Debugging

		private void OnGUI() {


			if (GUI.Button(new Rect(50, 25, 250, 50), "Damage Random Enemies")) {
				int enemiesToDamage = 5;


				for (int i = quadTree.positionCount; i > 0; i--) {

					if (genes.GetObjectType(i) != ObjectType.Slime)
						continue;

					enemyPhysicsMethods[i].DealDamage(25, DamageType.Blaster);

					enemiesToDamage--;

					if (enemiesToDamage == 0)
						break;

				}


			}



		}

		private void OnDrawGizmos() {
			if (quadTree != null)
				quadTree.OnDrawGizmos();


		}

		#endregion


		private void Awake() {


			int targetCount = 100;
			genes = new GenesArray(maxEnemies * ((int)GeneGroups.TotalGeneCount + 1), Allocator.Persistent);

			choosableTargets = new NativeList<ushort>(targetCount, Allocator.Persistent);
			targetIDs = new NativeArray<ushort>(maxEnemies, Allocator.Persistent);
			sheepDistancesToSlimes = new NativeArray<float>(targetCount * (int)ObjectType.Count, Allocator.Persistent);

			objectForces = new NativeArray<float2>(maxEnemies * (int)ObjectType.Count, Allocator.Persistent);
			divisionOneKeys = new NativeArray<QuadKey>(4, Allocator.Persistent);

			QuadKey key = new QuadKey();
			key.RightBranch();
			key.LeftBranch();

			//top left
			divisionOneKeys[0] = key;

            key = new QuadKey();
            key.RightBranch();
            key.RightBranch();

            //top topRight
            divisionOneKeys[0] = key;

            key = new QuadKey();
            key.LeftBranch();
            key.RightBranch();

            //bottom right
            divisionOneKeys[0] = key;


            key = new QuadKey();
            key.LeftBranch();
            key.LeftBranch();

            //bottom left
            divisionOneKeys[0] = key;

            headings = new NativeArray<float>(maxEnemies, Allocator.Persistent);

			burrows = new List<EnemyBurrow>();
			quadTree = new QuadTree(maxEnemies, 35);

			quadTree.enemyManager = this;

			currentCountdownToWave = countdownToWave;

			enemyPhysicsMethods = new Dictionary<int, EnemyPhysicsMethods>(maxEnemies);

			for (int i = 0; i < targetIDs.Length; i++)
				targetIDs[i] = ushort.MaxValue;


		}

		private void Start() {



            Inst.gameOver += GameOver;
            quadTree.AddTransform(Inst.player.transform);
			genes.SetObjectType(0, ObjectType.Player);


			//uncomment to add player to list of choosableTargets
			//choosableTargets.Add(0);

			AddSheepToList();



			//generate a wall surrounding the area
			Inst.GeneratePlayableAreaWall();


			AddBurrow(2);

			//50 burrows for testing
			//AddBurrow(50);


		}

		void Update() {
			if (!duringWave) {
				NonWaveUpdate();
			}
			else {

				DuringWaveUpdate();

			}

		}

		private void NonWaveUpdate() {


            if (choosableTargets.Length == 0)
                return;

            if (updateInitialize) {
				updateInitialize = false;
				currentCountdownToWave = countdownToWave;

				if (waveNumber > 0) {
					endOfWaveCanvasParent.SetActive(true);
				}



				return;
			}

			//now just countdown timer
			currentCountdownToWave -= Time.deltaTime;


			if (currentCountdownToWave <= 0) {
				duringWave = true;
				updateInitialize = true;

			}



		}

		private void DuringWaveUpdate() {
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
             */

            if (choosableTargets.Length == 0)
             return;

            if (updateInitialize) {
				updateInitialize = false;

				//spawn any additional required burrows
				//then set the amount of enemies for them to each spawn

				enemiesLeftToSpawn = enemiesCountToSpawn;
				int avg = enemiesCountToSpawn / burrows.Count;

				for (int i = 0; i < burrows.Count; i++) {

					burrows[i].AddEnemyCountToSpawn(avg);
					enemiesLeftToSpawn -= avg;

				}

				while (enemiesLeftToSpawn > 0) {

					burrows[Random.Range(0, burrows.Count)].AddEnemyCountToSpawn(1);


					enemiesLeftToSpawn--;

				}


				//all enemies have been assigned
				//so now set the enemies left to spawn to the enemiesCountToSpawn
				//in order to create a signal when all required enemies have been spawned
				enemiesLeftToSpawn = enemiesCountToSpawn;
				enemiesLeftToKill = enemiesCountToSpawn;

				return;

			}

            ResetNativeArrayJob<float2> resetJob = new ResetNativeArrayJob<float2>();
            resetJob.array = objectForces;
            JobHandle resetJobHandle
                = resetJob.Schedule(headings.Length, SystemInfo.processorCount);


            quadTree.NewFrame();

			//spawn enemies

			if (enemiesLeftToSpawn > 0) {

				for (int i = 0; i < burrows.Count; i++) {
					burrows[i].ManualUpdate();

				}

			}

			TreeObjectCount = quadTree.positionCount;

			//if (quadTree.positionCount >= 1000)
			//spawningEnemies = false;
			quadTree.Update();


            resetJobHandle.Complete();

            if (quadTree.positionCount <= 0)
				return;

            //get targets before updating movement

            ChooseTargetJob ctj = new ChooseTargetJob();
			ctj.choosableTargets = choosableTargets;
			ctj.positions = quadTree.positions;
			ctj.objectIDs = quadTree.objectIDs;
			ctj.targetIDs = targetIDs;
			ctj.Schedule(quadTree.positionCount + 1, SystemInfo.processorCount).Complete();


			NativeArray<JobHandle> handles = new NativeArray<JobHandle>((int)ObjectType.Count, Allocator.TempJob);

			for (int i = 0; i < handles.Length; i++)
			{
                GatherForcesWithinRangeJob gfj = new GatherForcesWithinRangeJob();
                gfj.positions = quadTree.positions;
                gfj.objectIDs = quadTree.objectIDs;
                gfj.targetIDs = targetIDs;
                gfj.genes = genes;
                gfj.objectForces = objectForces;
                gfj.quads = quadTree.quads;
                //gfj.startingKeys = divisionOneKeys;
				gfj.targetType = (ObjectType)i;
				gfj.sheepDistancesToSlime = sheepDistancesToSlimes;
				//gfj.Run((quadTree.positionCount + 1));
				handles[i] = gfj.Schedule((quadTree.positionCount + 1), SystemInfo.processorCount);

            }

			for (int i = 0; i < handles.Length; i++)
			{
				handles[i].Complete();

			}

			//now need to add forces and convert them to a heading
			//only does one index per object
			CalculateHeadingJob chj = new CalculateHeadingJob();
			chj.objectForces = objectForces;
			chj.genes = genes;
			chj.headings = headings;
			chj.deltaTime = Time.deltaTime;
			//chj.Run(QuadTree.positionCount + 1);
			chj.Schedule(QuadTree.positionCount + 1, SystemInfo.processorCount).Complete();

			//after movement, write the information back to the transforms

			WriteTransformsJob wtj = new WriteTransformsJob();
			wtj.positions = quadTree.positions;
			wtj.rotation = headings;
			wtj.Schedule(quadTree.TransformAccess);

			Profiler.BeginSample("Writing Velocities");

			for (int i = 0; i <= quadTree.positionCount; i++) {

				if (genes.GetObjectType(quadTree.objectIDs[i]) == ObjectType.Sheep) {

					//if being chased, set velocity, otherwise don't
					int ID = quadTree.objectIDs[i];

                    if (sheepDistancesToSlimes[ID] > 64)
					{
                        //idle animation
                        quadTree.Transforms[quadTree.objectIDs[i]].gameObject
							.GetComponent<Animator>().SetBool("Moving", false);
                        quadTree.Transforms[quadTree.objectIDs[i]].gameObject
							.GetComponent<Animator>().SetBool("Running", false);

                        quadTree.Transforms[quadTree.objectIDs[i]].gameObject
							.GetComponent<Rigidbody>().velocity = Vector3.zero;
                    }
                    else if (sheepDistancesToSlimes[ID] > 36)
                    {
						//walk animation
						quadTree.Transforms[quadTree.objectIDs[i]].gameObject
							.GetComponent<Animator>().SetBool("Moving", true);
                        quadTree.Transforms[quadTree.objectIDs[i]].gameObject
							.GetComponent<Animator>().SetBool("Running", false);

                        quadTree.Transforms[quadTree.objectIDs[i]].gameObject.GetComponent<Rigidbody>().velocity
                            = (quadTree.Transforms[quadTree.objectIDs[i]].forward * 2);
                    }
                    else if (sheepDistancesToSlimes[ID] >= 0)
                    {
                        //run animation
                        quadTree.Transforms[quadTree.objectIDs[i]].gameObject
							.GetComponent<Animator>().SetBool("Moving", true);
                        quadTree.Transforms[quadTree.objectIDs[i]].gameObject
							.GetComponent<Animator>().SetBool("Running", true);

                        quadTree.Transforms[quadTree.objectIDs[i]].gameObject.GetComponent<Rigidbody>().velocity
                            = (quadTree.Transforms[quadTree.objectIDs[i]].forward * 3);
                    }
					else
					{

						for (int j = 0; j < choosableTargets.Length; j++)
						{

							if (choosableTargets[j] != quadTree.objectIDs[i])
								continue;

								choosableTargets.RemoveAt(j);

							break;

						}
                        //choosableTargets[quadTree.objectIDs[i] - 1] = ushort.MaxValue;

						//die
						genes.ResetIDGenes(quadTree.objectIDs[i]);
						quadTree.QueueDeletion(quadTree.objectIDs[i]);

						for (int j = 0; j < targetIDs.Length; j++)
						{

							if (targetIDs[j] == quadTree.objectIDs[i])
							{
								targetIDs[j] = ushort.MaxValue;
							}

						}

						//if all sheep are dead, game over.

						if (choosableTargets.Length == 0)
						{
							Inst.GameOverEventTrigger();
                            //GameOver();
							break;
						}

					}
				}
				else if (genes.GetObjectType(quadTree.objectIDs[i]) == ObjectType.Slime) {

					quadTree.Transforms[quadTree.objectIDs[i]].gameObject.GetComponent<Rigidbody>().velocity
						= (quadTree.Transforms[quadTree.objectIDs[i]].forward * 3.05f);

				}


			}

			Profiler.EndSample();

			quadTree.ProcessDeletions();


		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="objectID"></param>
		/// <param name="replacementOldID"></param>
		private void RemoveObjectFromTree(ushort objectID) {

			quadTree.RemoveTransform(objectID);


		}

		#region Adding Object Types To Quad Tree

		public void AddEnemyToList(Transform enemy) {




			enemiesLeftToSpawn--;
			ushort id = quadTree.AddTransform(enemy);

			//offset 1 for type, then attractions count
			genes.SetObjectType(id, ObjectType.Slime);
			genes.SetAttraction(id, Attraction.Sheep, 10);
			genes.SetAttraction(id, Attraction.Tower, 10);
			genes.SetAttraction(id, Attraction.Slime, 3);
			genes.SetAttraction(id, Attraction.Wall, 20);
			genes.SetOptimalDistance(id, OptimalDistance.Slime, 3);

			genes.SetHealth(id, 50);

			EnemyPhysicsMethods methods = enemy.GetComponent<EnemyPhysicsMethods>();

			if (!methods.Initialized()) {
				methods.SetInitialInfo(id, genes, this);
			}

			enemyPhysicsMethods.Add(id, methods);



		}

		public void AddWallToList(Transform wall) {


			//for adding all the child points to make sure enemies can avoid them correctly
			for (int i = 0; i < wall.childCount;) {


				ushort id = quadTree.AddTransform(wall.transform.GetChild(i));
				wall.transform.GetChild(i).parent = null;
				genes.SetObjectType(id, ObjectType.Wall);


			}

		}

        private void AddSheepToList()
        {

			int min = -15;
			int max = 15;

            for (int i = 0; i < sheepCount; i++)
            {

                GameObject sheep = Instantiate(sheepPrefab);

                sheep.transform.position = new Vector3(Random.Range(min, max), 0, Random.Range(min, max));

                ushort id = quadTree.AddTransform(sheep.transform);

                genes.SetObjectType(id, ObjectType.Sheep);
                genes.SetAttraction(id, Attraction.Slime, 10);
                genes.SetAttraction(id, Attraction.Wall, 20);

                choosableTargets.Add(id);

                sheep.SetActive(true);
            }



        }

        private void AddBurrow(int count = 1) {

			for (int i = 0; i < count; i++)
			{
				EnemyBurrow burrow = GameObject.Instantiate(burrowPrefab).GetComponent<EnemyBurrow>();

				int min = -20;
				int max = 20;

				//if (Random.value > .5f)
				burrow.gameObject.transform.position = new Vector3(Random.Range(min, max), 0, Random.Range(min, max));
				//else
				//burrow.gameObject.transform.position = new Vector3(Random.Range(-min, -max), 0, Random.Range(-min, -max));

				burrow.Initialize(this, .5f);

				burrows.Add(burrow);
				burrow.gameObject.SetActive(true);

			}

		}



		#endregion

		public void OnEnemyDeath(ushort id) {

			RemoveObjectFromTree(id);

			enemyPhysicsMethods.Remove(id);


			enemiesLeftToKill--;

			if (enemiesLeftToKill <= 0) {
				duringWave = false;
				updateInitialize = true;
			}


			//EnemyPhysicsMethods methods;

			//if (!enemyPhysicsMethods.TryGetValue(replacementOldID, out methods))
			//    return;

			//methods.UpdateID(id);
			//enemyPhysicsMethods.Remove(replacementOldID);
			//enemyPhysicsMethods.Add(id, methods);

			//genes.TransferGenes(replacementOldID, id);

			//update targetID and heading
			//update genes array as well





			Debug.Log("Enemy Dead!");


		}

		private void GameOver()
		{

			Debug.Log("Game Over");

			for (int i = 0; i <= quadTree.positionCount; i++)
			{

				if (genes.GetObjectType(quadTree.objectIDs[i]) != ObjectType.Slime)
					continue;
				Rigidbody rb = quadTree.Transforms[quadTree.objectIDs[i]].gameObject.GetComponent<Rigidbody>();

				rb.velocity = Vector3.zero;
				rb.angularVelocity = Vector3.zero;

            }


		}

		private void OnDisable() {
			quadTree.Dispose();

			genes.Dispose();
			headings.Dispose();

			choosableTargets.Dispose();
			targetIDs.Dispose();

			objectForces.Dispose();
			divisionOneKeys.Dispose();


			sheepDistancesToSlimes.Dispose();

		}



	}

}
