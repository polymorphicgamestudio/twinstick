using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel;
using Unity.Jobs;
using UnityEditor;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.Profiling;

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


			genes = new GenesArray(maxEnemies * ((int)GeneGroups.TotalGeneCount + 1), Allocator.Persistent);

			choosableTargets = new NativeList<ushort>(100, Allocator.Persistent);
			targetIDs = new NativeArray<ushort>(maxEnemies, Allocator.Persistent);
			headings = new NativeArray<float>(maxEnemies, Allocator.Persistent);
			burrows = new List<EnemyBurrow>();
			quadTree = new QuadTree(maxEnemies, 25);
			quadTree.enemyManager = this;

			currentCountdownToWave = countdownToWave;

			enemyPhysicsMethods = new Dictionary<int, EnemyPhysicsMethods>(maxEnemies);

			for (int i = 0; i < targetIDs.Length; i++)
				targetIDs[i] = ushort.MaxValue;



		}

		private void Start() {



			quadTree.AddTransform(Inst.player.transform);
			genes.SetObjectType(0, ObjectType.Player);


			//uncomment to add player to list of choosableTargets
			//choosableTargets.Add(0);

			AddSheep();



			//generate a wall surrounding the area
			Inst.GeneratePlayableAreaWall();


			AddBurrow();
			AddBurrow();


			#region Spawn Burrows For Testing


			//         AddBurrow();
			//         AddBurrow();
			//         AddBurrow();
			//         AddBurrow();
			//         AddBurrow();
			//         AddBurrow();
			//         AddBurrow();
			//         AddBurrow();
			//         AddBurrow();
			//         AddBurrow();

			//         AddBurrow();
			//         AddBurrow();
			//         AddBurrow();
			//         AddBurrow();
			//         AddBurrow();
			//         AddBurrow();
			//         AddBurrow();
			//         AddBurrow();
			//         AddBurrow();
			//         AddBurrow();

			//         AddBurrow();
			//         AddBurrow();
			//         AddBurrow();
			//         AddBurrow();
			//         AddBurrow();
			//         AddBurrow();
			//         AddBurrow();
			//         AddBurrow();
			//         AddBurrow();
			//         AddBurrow();

			//AddBurrow();
			//AddBurrow();
			//AddBurrow();
			//AddBurrow();
			//AddBurrow();
			//AddBurrow();
			//AddBurrow();
			//AddBurrow();
			//AddBurrow();
			//AddBurrow();

			//AddBurrow();
			//AddBurrow();
			//AddBurrow();
			//AddBurrow();
			//AddBurrow();
			//AddBurrow();
			//AddBurrow();
			//AddBurrow();
			//AddBurrow();
			//AddBurrow();

			#endregion

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
			moveJob.positions = quadTree.positions;
			moveJob.buckets = quadTree.quadsList.Slice(0, quadTree.QuadsListLength);
			moveJob.headings = headings;
			moveJob.objectIDs = quadTree.objectIDs;
			moveJob.objectQuadIDs = quadTree.objectQuadIDs;
			moveJob.quads = quadTree.quads;
			moveJob.genes = genes;
			moveJob.targetIDs = targetIDs;
			moveJob.deltaTime = Time.deltaTime;
			moveJob.neighborCounts = quadTree.neighborCounts;
			moveJob.objectNeighbors = quadTree.objectNeighbors;
			moveJob.maxNeighborCount = quadTree.maxNeighborQuads;
			moveJob.Run(quadTree.positionCount + 1);
			//moveJob.Schedule(quadTree.positionCount + 1, SystemInfo.processorCount).Complete();


			//after movement, write the information back to the transforms

			WriteTransformsJob wtj = new WriteTransformsJob();
			wtj.positions = quadTree.positions;
			wtj.rotation = headings;
			wtj.Schedule(quadTree.TransformAccess);

			Profiler.BeginSample("Writing Velocities");

			for (int i = 0; i <= quadTree.positionCount; i++) {

				if (genes.GetObjectType(quadTree.objectIDs[i]) == ObjectType.Sheep) {

					//if being chased, set velocity, otherwise don't
					//if (headings[i] != newHeadings[i])
					quadTree.Transforms[quadTree.objectIDs[i]].gameObject.GetComponent<Rigidbody>().velocity
						= (quadTree.Transforms[quadTree.objectIDs[i]].forward * 3);

					continue;

				}
				else if (genes.GetObjectType(quadTree.objectIDs[i]) == ObjectType.Slime) {

					quadTree.Transforms[quadTree.objectIDs[i]].gameObject.GetComponent<Rigidbody>().velocity
						= (quadTree.Transforms[quadTree.objectIDs[i]].forward * 3.25f);

				}


			}

			Profiler.EndSample();




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
			genes.SetAttraction(id, Attraction.Slime, -2);
			genes.SetAttraction(id, Attraction.Wall, -50);
			genes.SetSlimeOptimalDistance(id, 3);

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

		private void AddBurrow() {

			EnemyBurrow burrow = GameObject.Instantiate(burrowPrefab).GetComponent<EnemyBurrow>();

			int min = -15;
			int max = 15;

			//if (Random.value > .5f)
			burrow.gameObject.transform.position = new Vector3(Random.Range(-15, 15), 0, Random.Range(min, max));
			//else
			//burrow.gameObject.transform.position = new Vector3(Random.Range(-min, -max), 0, Random.Range(-min, -max));

			burrow.Initialize(this, .5f);

			burrows.Add(burrow);
			burrow.gameObject.SetActive(true);
		}

		private void AddSheep() {

			for (int i = 0; i < sheepCount; i++) {

				GameObject sheep = Instantiate(sheepPrefab);

				sheep.transform.position = new Vector3(Random.Range(1, 4), 0, Random.Range(-4, -3));

				ushort id = quadTree.AddTransform(sheep.transform);

				genes.SetObjectType(id, ObjectType.Sheep);
				genes.SetAttraction(id, Attraction.Slime, -2);
				genes.SetAttraction(id, Attraction.Wall, -50);
				//genes.SetViewRange(id, ViewRange.Slime, )

				choosableTargets.Add(id);

				sheep.SetActive(true);
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



		private void OnDisable() {
			quadTree.Dispose();

			genes.Dispose();
			headings.Dispose();

			choosableTargets.Dispose();
			targetIDs.Dispose();

		}



	}

}
