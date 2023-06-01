using Drawing;
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

namespace ShepProject
{




    public class AIManager : SystemBase
    {

        #region Variables

        [SerializeField]
        private GameObject burrowPrefab;
        public GameObject sheepPrefab;
        public GameObject endOfWaveCanvasParent;

        public int burrowCount;
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

        public float CurrentCountdownToWave => currentCountdownToWave;


        [SerializeField]
        private int enemiesCountToSpawn;
        private int enemiesLeftToSpawn;

        private int enemiesLeftToKill;


        public Button startButton;

        [SerializeField]
        private bool spawningEnemies;

        public int[] idChecks;

        private Dictionary<int, EnemyPhysicsMethods> enemyPhysicsMethods;

        public Transform[] burrowLocations;

        private List<EnemyBurrow> burrows;

        private GenesArray genes;
        public GenesArray Genes => genes;

        //will contain IDs of sheep and player, and towers won't be targeted
        private NativeList<ushort> choosableTargets;


        //will store the target id of the slime
        private NativeArray<ushort> targetIDs;
        private NativeArray<float> sheepDistancesToSlimes;
        private NativeArray<float2> objectForces;

        private NativeArray<float> headings;

        private QuadTree quadTree;
        public QuadTree QuadTree => quadTree;

        public InitialSlimeValues slimeValues;


        public bool SpawningEnemies => spawningEnemies;

        #endregion

        #region Debugging

        private bool slimeCustomization;

        private void OnGUI()
        {


            //if (GUI.Button(new Rect(50, 25, 250, 50), "Damage Random Enemies")) {
            //	int enemiesToDamage = 5;


            //	for (int i = quadTree.positionCount; i > 0; i--) {

            //		if (genes.GetObjectType(i) != ObjectType.Slime)
            //			continue;

            //		enemyPhysicsMethods[i].DealDamage(25, DamageType.Blaster);

            //		enemiesToDamage--;

            //		if (enemiesToDamage == 0)
            //			break;

            //	}


            //}



        }

        private void OnDrawGizmos()
        {
            if (quadTree != null)
                quadTree.OnDrawGizmos();


        }

        #endregion


        public void StartRound()
        {
            currentCountdownToWave = 0;


            for (int i = 0; i < choosableTargets.Length; i++)
            {

                quadTree.Transforms[choosableTargets[i]].GetComponent<Rigidbody>().isKinematic = false;


            }

        }

        private void Awake()
        {

            startButton.onClick.AddListener(StartRound);

            int targetCount = 100;
            genes = new GenesArray(maxEnemies, ((int)GeneGroups.TotalGeneCount + 1), Allocator.Persistent);

            choosableTargets = new NativeList<ushort>(targetCount, Allocator.Persistent);
            targetIDs = new NativeArray<ushort>(maxEnemies, Allocator.Persistent);
            sheepDistancesToSlimes = new NativeArray<float>(targetCount * (int)ObjectType.Count, Allocator.Persistent);

            objectForces = new NativeArray<float2>(maxEnemies * (int)ObjectType.Count, Allocator.Persistent);

            headings = new NativeArray<float>(maxEnemies, Allocator.Persistent);

            burrows = new List<EnemyBurrow>();
            quadTree = new QuadTree(maxEnemies, 35);

            quadTree.enemyManager = this;

            currentCountdownToWave = countdownToWave;

            enemyPhysicsMethods = new Dictionary<int, EnemyPhysicsMethods>(maxEnemies);

            for (int i = 0; i < targetIDs.Length; i++)
                targetIDs[i] = ushort.MaxValue;


        }


        private void Start()
        {



            Inst.gameOver += GameOver;
            if (Inst.player != null)
            {
                quadTree.AddTransform(Inst.player.transform);
                genes.SetObjectType(0, ObjectType.Player);
            }
            else
            {
                Debug.LogError("Player is null inside ShepGM! Errors WILL occur!");
            }



            //uncomment to add player to list of choosableTargets
            //choosableTargets.Add(0);

            AddSheepToList();


            AddBurrow(burrowCount);

            //50 burrows for testing
            //AddBurrow(50);


        }

        void Update()
        {
            if (!duringWave)
            {
                NonWaveUpdate();
            }
            else
            {

                DuringWaveUpdate();

            }

        }

        private void NonWaveUpdate()
        {


            if (choosableTargets.Length == 0)
                return;

            if (updateInitialize)
            {
                updateInitialize = false;
                currentCountdownToWave = countdownToWave;

                if (waveNumber > 0)
                {
                    endOfWaveCanvasParent.SetActive(true);
                }



                return;
            }

            //now just countdown timer
            currentCountdownToWave -= Time.deltaTime;


            if (currentCountdownToWave <= 0)
            {
                duringWave = true;
                updateInitialize = true;

            }



        }

        private void DuringWaveUpdate()
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
             */

            if (choosableTargets.Length == 0)
                return;

            if (updateInitialize)
            {
                updateInitialize = false;

                //spawn any additional required burrows
                //then set the amount of enemies for them to each spawn

                enemiesLeftToSpawn = enemiesCountToSpawn;
                int avg = enemiesCountToSpawn / burrows.Count;

                for (int i = 0; i < burrows.Count; i++)
                {

                    burrows[i].AddEnemyCountToSpawn(avg);
                    enemiesLeftToSpawn -= avg;

                }

                while (enemiesLeftToSpawn > 0)
                {

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
                = resetJob.Schedule(objectForces.Length, SystemInfo.processorCount);

            quadTree.NewFrame();

            //spawn enemies

            if (enemiesLeftToSpawn > 0)
            {

                for (int i = 0; i < burrows.Count; i++)
                {
                    burrows[i].ManualUpdate();

                }

            }

            TreeObjectCount = quadTree.positionCount;

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
            NativeArray<GatherForcesWithinRangeJob> gfjs
                = new NativeArray<GatherForcesWithinRangeJob>(handles.Length, Allocator.Temp);

            for (int i = 0; i < handles.Length; i++)
            {

                //tower distance falloff check to make sure
                //they don't end up weaving in and out of tower range
                GatherForcesWithinRangeJob gfj = new GatherForcesWithinRangeJob();
                gfj.positions = quadTree.positions;
                gfj.objectIDs = quadTree.objectIDs;
                gfj.targetIDs = targetIDs;
                gfj.genes = genes;
                gfj.objectForces = objectForces;
                gfj.quads = quadTree.quads;
                gfj.targetType = (ObjectType)i;
                gfj.idsToCheck = new NativeArray<int>(idChecks, Allocator.TempJob);
                gfj.sheepDistancesToSlime = sheepDistancesToSlimes;
                gfj.builder = Drawing.DrawingManager.GetBuilder();

                gfjs[i] = gfj;
                //gfj.Run((quadTree.positionCount + 1));
                handles[i] = gfj.Schedule((quadTree.positionCount + 1), SystemInfo.processorCount);

            }

            for (int i = 0; i < handles.Length; i++)
            {
                handles[i].Complete();
                gfjs[i].idsToCheck.Dispose();
                gfjs[i].builder.Dispose();
            }

            handles.Dispose();
            gfjs.Dispose();

            //now need to add forces and convert them to a heading
            //only does one index per object

            NativeArray<int> idsToCheck = new NativeArray<int>(idChecks, Allocator.TempJob);

            CalculateHeadingJob chj = new CalculateHeadingJob();
            chj.objectIDs = quadTree.objectIDs;
            chj.objectForces = objectForces;
            chj.genes = genes;
            chj.headings = headings;
            chj.deltaTime = Time.deltaTime;
            chj.builder = DrawingManager.GetBuilder();
            chj.positions = quadTree.positions;
            chj.idsToCheck = idsToCheck;
            //chj.Run(QuadTree.positionCount + 1);
            chj.Schedule(QuadTree.positionCount + 1, SystemInfo.processorCount).Complete();

            chj.builder.Dispose();
            idsToCheck.Dispose();

            //after movement, write the information back to the transforms

            WriteTransformsJob wtj = new WriteTransformsJob();
            wtj.positions = quadTree.positions;
            wtj.genes = genes;
            wtj.rotation = headings;
            wtj.Schedule(quadTree.TransformAccess);

            Profiler.BeginSample("Writing Velocities");

            for (int i = 0; i <= quadTree.positionCount; i++)
            {

                if (genes.GetObjectType(quadTree.objectIDs[i]) == ObjectType.Sheep)
                {

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
                            = (quadTree.Transforms[quadTree.objectIDs[i]].forward * (genes.GetSpeed(quadTree.objectIDs[i]) / 2f));
                    }
                    else if (sheepDistancesToSlimes[ID] >= 4)
                    {
                        //run animation
                        quadTree.Transforms[quadTree.objectIDs[i]].gameObject
                            .GetComponent<Animator>().SetBool("Moving", true);
                        quadTree.Transforms[quadTree.objectIDs[i]].gameObject
                            .GetComponent<Animator>().SetBool("Running", true);

                        quadTree.Transforms[quadTree.objectIDs[i]].gameObject.GetComponent<Rigidbody>().velocity
                            = (quadTree.Transforms[quadTree.objectIDs[i]].forward * genes.GetSpeed(quadTree.objectIDs[i]));
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
                else if (genes.GetObjectType(quadTree.objectIDs[i]) == ObjectType.Slime)
                {

                    quadTree.Transforms[quadTree.objectIDs[i]].gameObject.GetComponent<Rigidbody>().velocity
                        = (quadTree.Transforms[quadTree.objectIDs[i]].forward * genes.GetSpeed(quadTree.objectIDs[i]));

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
        private void RemoveObjectFromTree(ushort objectID)
        {

            quadTree.RemoveTransform(objectID);


        }

        #region Adding Object Types To Quad Tree

        public void AddEnemyToList(Transform enemy)
        {


            //this needs to have object pooling attached to it
            //probably will eventually need to also have specific settings 
            //to control exactly what type of enemy is spawned
            //and to control its genes


            enemiesLeftToSpawn--;
            ushort id = quadTree.AddTransform(enemy);
            genes.AddGenesToObject(id);

            //offset 1 for type, then attractions count
            genes.SetObjectType(id, ObjectType.Slime);
            genes.SetAttraction(id, ObjectType.Sheep, slimeValues.sheepAttraction); // 1
            genes.SetAttraction(id, ObjectType.Tower, slimeValues.towerAttraction); // 1
            genes.SetAttraction(id, ObjectType.Slime, slimeValues.slimeAttraction); // .5
            genes.SetAttraction(id, ObjectType.Wall, slimeValues.wallAttraction); // 1

            //setting trait values, not gene values
            genes.SetViewRange(id, ViewRange.Tower, slimeValues.towerViewRange);
            genes.SetViewRange(id, ViewRange.Slime, slimeValues.slimeViewRange);
            genes.SetViewRange(id, ViewRange.Player, slimeValues.playerViewRange);
            genes.SetViewRange(id, ViewRange.Wall, slimeValues.wallViewRange);

            genes.SetOptimalDistance(id, OptimalDistance.Slime, slimeValues.slimeOptimalDistance);
                //-8 / genes.GetViewRange(id, ViewRange.Slime));

            //value within something like 1-20
            genes.SetSpeed(id, slimeValues.slimeSpeed);
            genes.SetTurnRate(id, slimeValues.slimeTurnRate);
            genes.SetHealth(id, slimeValues.slimeHealth);

            EnemyPhysicsMethods methods = enemy.GetComponent<EnemyPhysicsMethods>();

            if (!methods.Initialized())
            {
                methods.SetInitialInfo(id, genes, this);
            }

            enemyPhysicsMethods.Add(id, methods);



        }

        public void AddWallToList(Transform wall)
        {


            //for adding all the child points to make sure enemies can avoid them correctly
            for (int i = 0; i < 5; i++)
            {


                ushort id = quadTree.AddTransform(wall.transform.GetChild(0));
                wall.transform.GetChild(0).parent = null;
                genes.SetObjectType(id, ObjectType.Wall);


            }

        }

        private void AddSheepToList()
        {

            int min = -10;
            int max = 10;

            for (int i = 0; i < sheepCount; i++)
            {

                GameObject sheep = Instantiate(sheepPrefab);

                sheep.transform.position = new Vector3(Random.Range(min, max), 0, Random.Range(min, max));

                ushort id = quadTree.AddTransform(sheep.transform);
                genes.AddGenesToObject(id);

                genes.SetObjectType(id, ObjectType.Sheep);
                genes.SetAttraction(id, ObjectType.Slime, 1);
                genes.SetAttraction(id, ObjectType.Wall, 1);

                genes.SetTurnRate(id, .5f);
                genes.SetViewRange(id, ViewRange.Slime, 8);
                genes.SetSpeed(id, 3);

                choosableTargets.Add(id);

                sheep.SetActive(true);
            }



        }

        public void AddTowerToList(BaseTower tower)
        {
            tower.objectID = QuadTree.AddTransform(tower.transform);
            genes.SetObjectType(tower.objectID, ObjectType.Tower);


        }

        private void AddBurrow(int count = 1)
        {

            for (int i = 0; i < burrowLocations.Length; i++)
            {
                EnemyBurrow burrow = GameObject.Instantiate(burrowPrefab).GetComponent<EnemyBurrow>();

                burrow.transform.position = burrowLocations[i].position;

                burrow.Initialize(this, .2f);

                burrows.Add(burrow);
                burrow.gameObject.SetActive(true);

            }


            //int min = 30;
            //int max = 40;

            //for (int i = 0; i < count; i++)
            //{
            //    EnemyBurrow burrow = GameObject.Instantiate(burrowPrefab).GetComponent<EnemyBurrow>();



            //    //if (Random.value > .5f)
            //    burrow.gameObject.transform.position = new Vector3(Random.Range(min, max), 0, Random.Range(min, max));
            //    //else
            //    //burrow.gameObject.transform.position = new Vector3(Random.Range(-min, -max), 0, Random.Range(-min, -max));

            //    burrow.Initialize(this, .2f);

            //    burrows.Add(burrow);
            //    burrow.gameObject.SetActive(true);

            //}

        }

        #endregion

        public void OnEnemyDeath(ushort id)
        {

            RemoveObjectFromTree(id);

            enemyPhysicsMethods.Remove(id);


            enemiesLeftToKill--;

            if (enemiesLeftToKill <= 0)
            {
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





            //Debug.Log("Enemy Dead!");


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

        private void OnDisable()
        {
            quadTree.Dispose();

            genes.Dispose();
            headings.Dispose();

            choosableTargets.Dispose();
            targetIDs.Dispose();

            objectForces.Dispose();

            sheepDistancesToSlimes.Dispose();

        }



    }

}
