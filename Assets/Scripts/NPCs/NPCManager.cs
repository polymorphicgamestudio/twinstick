using Drawing;
using System;
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




    public class NPCManager : SystemBase
    {

        #region Variables

        [SerializeField]
        private GameObject burrowPrefab;
        public GameObject sheepPrefab;
        public GameObject endOfWaveCanvasParent;

        public EnemyPhysicsMethods slimePrefab;

        public int burrowCount;
        public int sheepCount;
        private int waveNumber;
        private bool duringWave;
        private bool updateInitialize;

        [SerializeField]
        private float countdownToWave;
        [SerializeField]
        private float currentCountdownToWave;

        public float CurrentCountdownToWave => currentCountdownToWave;



        public int MaxTreeObjects => slimeValues.slimeCount + 750;

        private int enemiesLeftToSpawn;
        private int enemiesLeftToKill;
        [SerializeField]
        private short quadTreeBucketSize;

        public Button startButton;

        [SerializeField]
        private bool writeEvolutionDataToFile;

        private Dictionary<int, EnemyPhysicsMethods> enemyPhysicsMethods;
        private Dictionary<int, SheepPhysicsMethods> sheepPhysicsMethods;
        private List<EnemyBurrow> burrows;

        [SerializeField]
        private SigmoidInfo[] sigmoids;

        private EvolutionStructure evolutionStructure;
        public EvolutionStructure EvolutionStructure => evolutionStructure;


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
        public EnemyObjectPool slimePool;

        #endregion

        #region Debugging

        private bool slimeCustomization;

        #endregion


        private void RemoveCountdown()
        {
            currentCountdownToWave = 0;

        }

        private void StartRound()
        {

            startButton.onClick.RemoveListener(StartRound);

            for (int i = 1; i < choosableTargets.Length; i++)
            {

                sheepPhysicsMethods[choosableTargets[i]].UnsetKinematic();


            }


            duringWave = true;
            updateInitialize = true;


        }

        private void Awake()
        {

            startButton.onClick.AddListener(RemoveCountdown);
            startButton.onClick.AddListener(StartRound);

            int targetCount = sheepCount + 1;
            evolutionStructure = 
                new EvolutionStructure(slimeValues.slimeCount, MaxTreeObjects, (int)Genes.TotalGeneCount,
                    sigmoids, 0);


            slimePool = new EnemyObjectPool(slimePrefab, (ushort)MaxTreeObjects, 200);

            choosableTargets = new NativeList<ushort>(targetCount, Allocator.Persistent);
            targetIDs = new NativeArray<ushort>(MaxTreeObjects, Allocator.Persistent);
            sheepDistancesToSlimes = new NativeArray<float>(targetCount * (int)ObjectType.Count, Allocator.Persistent);

            objectForces = new NativeArray<float2>(MaxTreeObjects * (int)ObjectType.Count, Allocator.Persistent);

            headings = new NativeArray<float>(MaxTreeObjects, Allocator.Persistent);

            burrows = new List<EnemyBurrow>();
            quadTree = new QuadTree(MaxTreeObjects, quadTreeBucketSize);

            quadTree.npcManager = this;

            currentCountdownToWave = countdownToWave;

            enemyPhysicsMethods = new Dictionary<int, EnemyPhysicsMethods>(MaxTreeObjects);
            sheepPhysicsMethods = new Dictionary<int, SheepPhysicsMethods>(sheepCount);
            waveNumber = 1;

            for (int i = 0; i < targetIDs.Length; i++)
                targetIDs[i] = ushort.MaxValue;





        }


        private void Start()
        {



            Inst.gameOver += GameOver;
            if (Inst.player != null)
            {
                quadTree.AddTransform(Inst.player.transform, ObjectType.Player);
                //genes.SetObjectType(0, ObjectType.Player);
            }
            else
            {
                Debug.LogError("Player is null inside ShepGM! Errors WILL occur!");
            }



            //uncomment to add player to list of choosableTargets
            choosableTargets.Add(0);

            AddSheepToList();


            AddBurrow(burrowCount);


        }

        public void ManualUpdate()
        {


            //initialSlimeSpawnCount = (ushort)slimeValues.slimeCount;

            if (!duringWave)
            {
                NonWaveUpdate();
            }
            else
            {

                DuringWaveUpdate();
                evolutionStructure.ManualUpdate(this);

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
                    Inst.EndOfWaveEventTrigger(waveNumber);

                    evolutionStructure.GenerateSlimesForNextWave(writeEvolutionDataToFile,
                    new EvolutionDataFileInfo()
                    {
                        info = sigmoids,
                        waveNumber = waveNumber - 2,
                        standardDeviation = slimeValues.standardDeviation,
                        mutationChance = slimeValues.mutationChance,
                        typeMutationChance = slimeValues.typeMutationChance
                    });

                    //endOfWaveCanvasParent.SetActive(true);

                    for (int i = 1; i < choosableTargets.Length; i++)
                    {
                        
                        sheepPhysicsMethods[choosableTargets[i]].WaveEndInfoSet();

                    }

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
                Inst.StartOfWaveEventTrigger(waveNumber);
                //spawn any additional required burrows
                //then set the amount of enemies for them to each spawn

                enemiesLeftToSpawn = slimeValues.slimeCount;
                int avg = slimeValues.slimeCount / burrows.Count;

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
                enemiesLeftToSpawn = slimeValues.slimeCount;
                enemiesLeftToKill = slimeValues.slimeCount;

                //return;

            }

            Profiler.BeginSample("Reset Object Forces");
            ResetNativeArrayJob<float2> resetJob = new ResetNativeArrayJob<float2>();
            resetJob.array = objectForces;
            JobHandle resetJobHandle
                = resetJob.Schedule(objectForces.Length, SystemInfo.processorCount);


            Profiler.EndSample();


            Profiler.BeginSample("QT New Frame");

            quadTree.NewFrame();

            Profiler.EndSample();
            //spawn enemies

            if (enemiesLeftToSpawn > 0)
            {

                for (int i = 0; i < burrows.Count; i++)
                {
                    burrows[i].ManualUpdate();

                }

            }

            //TreeObjectCount = quadTree.positionCount;

            Profiler.BeginSample("QT Update");

            quadTree.Update();

            Profiler.EndSample();

            resetJobHandle.Complete();

            if (quadTree.positionCount <= 0)
                return;

            //get targets before updating movement

            Profiler.BeginSample("Choose Target Job");

            ChooseTargetJob ctj = new ChooseTargetJob();
            ctj.choosableTargets = choosableTargets;
            ctj.positions = quadTree.positions;
            ctj.objectIDs = quadTree.objectIDs;
            ctj.targetIDs = targetIDs;
            ctj.Schedule(quadTree.positionCount + 1, SystemInfo.processorCount).Complete();


            Profiler.EndSample();

            Profiler.BeginSample("Gather Forces Jobs");

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
                gfj.pathQueries = Inst.Pathfinding.GetQueryingStructure();
                gfj.targetIDs = targetIDs;
                gfj.evolutionStructure = evolutionStructure;
                gfj.objTypes = quadTree.objTypes;
                gfj.objectForces = objectForces;
                gfj.quads = quadTree.quads;
                gfj.targetType = (ObjectType)i;
                gfj.sheepDistancesToSlime = sheepDistancesToSlimes;
                //gfj.builder = Drawing.DrawingManager.GetBuilder();

                gfjs[i] = gfj;
                //gfj.Run((quadTree.positionCount + 1));
                handles[i] = gfj.Schedule((quadTree.positionCount + 1), SystemInfo.processorCount);

            }

            for (int i = 0; i < handles.Length; i++)
            {
                handles[i].Complete();
                //gfjs[i].idsToCheck.Dispose();
                //gfjs[i].builder.DiscardAndDispose();
            }

            handles.Dispose();
            gfjs.Dispose();

            Profiler.EndSample();

            //now need to add forces and convert them to a heading
            //only does one index per object

            //NativeArray<int> idsToCheck = new NativeArray<int>(idChecks, Allocator.TempJob);

            Profiler.BeginSample("Calculate Heading Job");

            CalculateHeadingJob chj = new CalculateHeadingJob();
            chj.objectIDs = quadTree.objectIDs;
            chj.objectForces = objectForces;
            chj.evolutionStructure = evolutionStructure;
            chj.headings = headings;
            chj.deltaTime = Time.deltaTime;
            chj.objTypes = quadTree.objTypes;
            //chj.builder = DrawingManager.GetBuilder();
            chj.positions = quadTree.positions;
            //chj.idsToCheck = idsToCheck;
            //chj.Run(QuadTree.positionCount + 1);
            chj.Schedule(QuadTree.positionCount + 1, SystemInfo.processorCount).Complete();

            //chj.builder.Dispose();
            //idsToCheck.Dispose();

            Profiler.EndSample();

            //after movement, write the information back to the transforms

            Profiler.BeginSample("Write Transforms Job");

            WriteTransformsJob wtj = new WriteTransformsJob();
            wtj.positions = quadTree.positions;
            wtj.evolutionStructure = evolutionStructure;
            wtj.objTypes = quadTree.objTypes;
            wtj.rotation = headings;
            JobHandle handle = wtj.Schedule(quadTree.TransformAccess);//.Complete();

            Profiler.EndSample();

            Profiler.BeginSample("Writing Velocities");

            for (int i = 0; i <= quadTree.positionCount; i++)
            {


                /*
                 * 
                 * cache sheep animator and rigidbody and put them inside a dictionary
                 * 
                 */
                if (quadTree.objTypes[quadTree.objectIDs[i]] == ObjectType.Sheep)
                {
                    Profiler.BeginSample("Sheep Velocity");
                    //if being chased, set velocity, otherwise don't

                    sheepPhysicsMethods[
                        quadTree.objectIDs[i]].SetSlimeDistance(sheepDistancesToSlimes[quadTree.objectIDs[i]]);

                    Profiler.EndSample();

                }
                else if (quadTree.objTypes[quadTree.objectIDs[i]] == ObjectType.Slime)
                {

                    Profiler.BeginSample("Enemy Velocity");

                    enemyPhysicsMethods[quadTree.objectIDs[i]].SetVelocity(
                        quadTree.Transforms[quadTree.objectIDs[i]].forward 
                        * evolutionStructure.GetSpeed(quadTree.objectIDs[i]));
                    //quadTree.Transforms[quadTree.objectIDs[i]].gameObject.GetComponent<Rigidbody>().velocity
                    //= (quadTree.Transforms[quadTree.objectIDs[i]].forward * genes.GetSpeed(quadTree.objectIDs[i]));

                    Profiler.EndSample();
                }


            }

            Profiler.EndSample();

            quadTree.ProcessDeletions();


            //making sure that writing transforms finishes before end of frame
            handle.Complete();

        }

        public void UpdateSlimeValues(InitialSlimeValues slimeValues)
        {
            this.slimeValues = slimeValues;

            for (int i = 0; i <= quadTree.positionCount; i++)
            {
                if (quadTree.objTypes[quadTree.objectIDs[i]] == ObjectType.Slime)
                {

                    //UpdateEnemyGenes(quadTree.objectIDs[i]);

                }

            }

        }

        public EnemyPhysicsMethods GetEnemyPhysicsMethodFromInstanceID(int instanceID)
        {
            if (quadTree.objectIDsFromInstanceID.TryGetValue(instanceID, out ushort objectID))
            {
                return enemyPhysicsMethods[objectID];

            }


            return null;
        }

        #region Adding Object Types To Quad Tree

        public void AddEnemyToList(Transform enemy)
        {


            //this needs to have object pooling attached to it
            //probably will eventually need to also have specific settings 
            //to control exactly what type of enemy is spawned
            //and to control its genes


            enemiesLeftToSpawn--;
            ushort id = quadTree.AddTransform(enemy, ObjectType.Slime);
            evolutionStructure.AddGenesToObject(id);

            //UpdateEnemyGenes(id);

            EnemyPhysicsMethods methods = enemy.GetComponent<EnemyPhysicsMethods>();

            if (!methods.Initialized)
            {
                methods.SetInitialInfo(id, this);
            }

            enemyPhysicsMethods.Add(id, methods);



        }

        //private void UpdateEnemyGenes(int id)
        //{

        //    //offset 1 for type, then attractions count
        //    //genes.SetObjectType(id, ObjectType.Slime);
        //    evolutionStructure.SetAttraction(id, ObjectType.Sheep, slimeValues.sheepAttraction); // 1
        //    evolutionStructure.SetAttraction(id, ObjectType.Tower, slimeValues.towerAttraction); // 1
        //    evolutionStructure.SetAttraction(id, ObjectType.Slime, slimeValues.slimeAttraction); // .5
        //    evolutionStructure.SetAttraction(id, ObjectType.Wall, slimeValues.wallAttraction); // 1

        //    //setting trait values, not gene values
        //    evolutionStructure.SetViewRange(id, ObjectType.Tower, slimeValues.towerViewRange);
        //    evolutionStructure.SetViewRange(id, ObjectType.Slime, slimeValues.slimeViewRange);
        //    evolutionStructure.SetViewRange(id, ObjectType.Player, slimeValues.playerViewRange);
        //    evolutionStructure.SetViewRange(id, ObjectType.Wall, slimeValues.wallViewRange);

        //    evolutionStructure.SetSlimeOptimalDistance(id, slimeValues.slimeOptimalDistance);
        //    //-8 / genes.GetViewRange(id, ViewRange.Slime));

        //    //value within something like 1-20
        //    evolutionStructure.SetSpeed(id, slimeValues.slimeSpeed);
        //    evolutionStructure.SetTurnRate(id, slimeValues.slimeTurnRate);
        //    evolutionStructure.SetHealth(id, slimeValues.slimeHealth);

        //}

        public void AddWallToList(Transform wall)
        {

            //for adding all the child points to make sure enemies can avoid them correctly
            for (int i = 0; i < 5; i++)
            {

                ushort id = quadTree.AddTransform(wall.transform.GetChild(0), ObjectType.Wall);
                wall.transform.GetChild(0).parent = null;

            }

        }

        private void AddSheepToList()
        {

            int min = 5;
            int max = 10;

            for (int i = 0; i < sheepCount; i++)
            {

                GameObject sheep = Instantiate(sheepPrefab);


                sheep.transform.position = new Vector3(Random.Range(min, max), 0, Random.Range(min, max));

                ushort id = quadTree.AddTransform(sheep.transform, ObjectType.Sheep);

                SheepPhysicsMethods methods = sheep.GetComponent<SheepPhysicsMethods>();
                methods.SetInitialInfo(id, this);
                sheepPhysicsMethods.Add(id, methods);

                choosableTargets.Add(id);

                sheep.SetActive(true);
            }



        }

        public void AddTowerToList(TowerBaseClass tower)
        {
            tower.objectID = QuadTree.AddTransform(tower.transform, ObjectType.Tower);
            //genes.SetObjectType(tower.objectID, ObjectType.Tower);


        }

        private void AddBurrow(int count = 1)
        {


            int min = 25;
            int max = 50;

            for (int i = 0; i < count; i++)
            {
                EnemyBurrow burrow = Instantiate(burrowPrefab).GetComponent<EnemyBurrow>();



                //if (Random.value > .5f)
                burrow.gameObject.transform.position = new Vector3(Random.Range(min, max), 0, Random.Range(min, max));
                //else
                //burrow.gameObject.transform.position = new Vector3(Random.Range(-min, -max), 0, Random.Range(-min, -max));

                burrow.Initialize(this, .2f);

                burrows.Add(burrow);
                burrow.gameObject.SetActive(true);

            }

        }

        #endregion

        public void OnEnemyDeath(ushort id)
        {

            quadTree.RemoveTransform(id);
            slimePool.ReturnObject(enemyPhysicsMethods[id]);
            enemyPhysicsMethods.Remove(id);


            enemiesLeftToKill--;
            if (enemiesLeftToKill <= 0)
            {

                duringWave = false;
                updateInitialize = true;
                waveNumber++;
            }

        }

        public void OnSheepDeath(ushort id)
        {

            ResetSlimeTargetID resetTargets = new ResetSlimeTargetID();
            resetTargets.targetIDs = targetIDs;
            resetTargets.sheepID = id;
            JobHandle handle = resetTargets.Schedule(targetIDs.Length, SystemInfo.processorCount);

            for (int j = 0; j < choosableTargets.Length; j++)
            {

                if (choosableTargets[j] != id)
                    continue;

                choosableTargets.RemoveAt(j);

                break;

            }

            quadTree.QueueDeletion(id);
            sheepPhysicsMethods.Remove(id);


            //if all sheep are dead, game over.
            if (choosableTargets.Length == 0)
            {
                Inst.GameOverEventTrigger();

            }

            handle.Complete();

        }

        private void GameOver()
        {

            Debug.Log("Game Over");

            for (int i = 0; i <= quadTree.positionCount; i++)
            {

                if (quadTree.objTypes[quadTree.objectIDs[i]] != ObjectType.Slime)
                    continue;

                enemyPhysicsMethods[quadTree.objectIDs[i]].GameOver();

            }


        }

        private void OnDisable()
        {
            quadTree.Dispose();

            headings.Dispose();

            choosableTargets.Dispose();
            targetIDs.Dispose();

            objectForces.Dispose();

            sheepDistancesToSlimes.Dispose();


            evolutionStructure.Dispose();

        }



    }

}
