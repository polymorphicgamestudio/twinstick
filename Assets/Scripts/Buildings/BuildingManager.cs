using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ShepProject
{

    public class BuildingManager : SystemBase
    {

        public BuildingPlacementBase[] hologramPrefabs;
        public GameObject[] buildUpPrefabs;
        public GameObject[] prefabs;
        public List<TowerBaseClass> towers;

        [SerializeField]
        private BoxCollider playableArea;
        [SerializeField]
        private BoxCollider wallCollider;

        [SerializeField]
        private RobotController controller;

        public RobotController Controller => controller;

        [SerializeField]
        private RobotModeController modeController;

        public RobotModeController ModeController => modeController;

        private BuildingPlacementBase currentBuilding;

        private List<GameObject> currentBuildUps;
        private List<int> buildIndices;
        private List<float> buildUpTimers;

        [SerializeField]
        private AudioClip errorSound;

        private bool running;

        public readonly Vector2 bounds = new Vector2(72, 40);  // (5, 3) * 16 - 8
        public readonly Color colorValid = new Color(0.05f, 0.39f, 1f, 0.25f);
        public readonly Color colorInvalid = new Color(1f, 0.07f, 0.07f, 0.25f);
        public readonly float buildTime = 7f;

        private void Start()
        {

            currentBuildUps = new List<GameObject>(10);
            buildUpTimers = new List<float>(10);
            towers = new List<TowerBaseClass>(10);
            buildIndices = new List<int>(10);

            //generate a wall surrounding the area
            GeneratePlayableAreaWall();

            #region Setup Input Callbacks


            Inst.Input.Actions.Player.Run.performed += HideHologramsWhileRunning;
            Inst.Input.Actions.Player.Run.canceled += ShowHologramsAfterRunning;
            Inst.Input.actionSelectionChanged += ActionSelectionChanged;
            Inst.endOfWave += EndOfWave;

            #endregion

        }

        public void ManualUpdate()
        {

            #region Building Towers


            if (currentBuilding != null)
            {
                currentBuilding.PlacementUpdate(this);

                if (Inst.Input.ActionSelected)
                {
                    ConfirmBuilding();
                }

            }


            #region Convert To Building


            for (int i = 0; i < buildUpTimers.Count; i++)
            {
                buildUpTimers[i] -= Time.deltaTime;

                if (buildUpTimers[i] <= 0)
                {

                    GameObject current = Instantiate(prefabs[buildIndices[i]]);
                    current.transform.position = currentBuildUps[i].transform.position;
                    current.transform.rotation = currentBuildUps[i].transform.rotation;

                    if (buildIndices[i] < 6)
                    {
                        //no need to set wake trigger, automatically set
                        towers.Add(current.GetComponent<TowerBaseClass>());
                        towers[towers.Count - 1].gameManager = Inst;
                        Inst.NPCS.AddTowerToList(towers[towers.Count - 1]);

                    }

                    Destroy(currentBuildUps[i]);

                    currentBuildUps.RemoveAt(i);
                    buildUpTimers.RemoveAt(i);
                    buildIndices.RemoveAt(i);
                    i--;

                }


            }

            #endregion

            #endregion

            #region Tower Functionality

            if (Inst.NPCS.CurrentCountdownToWave > 0)
                return;

            TowersSearchForTargets();
            TowersUpdate();

            #endregion

        }

        private void TowersSearchForTargets()
        {
            for (int i = 0; i < towers.Count; i++)
            {


                if (towers[i].NeedsTarget)
                {
                    if (towers[i] is BombTowerController)
                    {
                        towers[i].slimeTarget =
                            Inst.NPCS.QuadTree
                            .GetClosestObject(towers[i].objectID, ObjectType.Slime, towers[i].minDist, towers[i].maxDist);
                    }

                    towers[i].slimeTarget =
                        Inst.NPCS.QuadTree
                        .GetClosestVisibleObject(towers[i].objectID, ObjectType.Slime, towers[i].minDist, towers[i].maxDist);

                }


            }
        }

        private void TowersUpdate()
        {
            for (int i = 0; i < towers.Count; i++)
            {

                towers[i].ManualUpdate();

            }

        }

        private void EndOfWave(int data)
        {
            for (int i = 0; i < towers.Count; i++)
            {

                towers[i].EndOfWave();

            }

        }

        private void ActionSelectionChanged(int previousAction, int currentAction)
        {

            if (currentAction >= 3)
            {

                if (currentAction != previousAction)
                {

                    //destroy old if needed, then instantiate new
                    if (currentBuilding != null)
                        Destroy(currentBuilding.gameObject);

                    InstantiateHologram(currentAction - 3);

                }

            }
            else
            {
                if (currentBuilding != null)
                {
                    modeController.TurnOffBuildMode();
                    Destroy(currentBuilding.gameObject);

                }
            }

        }


        #region Building Related


        private void ConfirmBuilding()
        {
            if (Inst.Input.MouseOverHUD()) return;

            if (!currentBuilding.IsValidLocation(this))
            {
                PlayErrorSound();
                return;
            }

            if (currentBuilding is WallPlacement)
            {

                //check to make sure that the wall doesn't enclose an entire area
                if (CheckIfWallEncloses())
                {
                    PlayErrorSound();
                    return;
                }

            }

            GameObject replacement = Instantiate(buildUpPrefabs[Inst.Input.CurrentActionSelection - 3]);
            currentBuildUps.Add(replacement);
            replacement.transform.position = currentBuilding.transform.position;
            replacement.transform.rotation = currentBuilding.transform.rotation;

            if (currentBuilding is WallPlacement)
            {


                replacement.GetComponent<Collider>().enabled = true;
                Inst.NPCS.AddWallToList(replacement.transform);
                Inst.Pathfinding.QueueVectorFieldUpdate();

            }

            replacement.SetActive(true);
            replacement.GetComponent<Animator>().SetTrigger("Build");


            buildUpTimers.Add(buildTime);
            buildIndices.Add(Inst.Input.CurrentActionSelection - 3);



        }

        private bool CheckIfWallEncloses()
        {
            //this is only called when trying to build a wall
            WallPlacement wall = currentBuilding as WallPlacement;
            int instanceID = wall.gameObject.GetInstanceID();


            Collider[] colliders = Physics.OverlapBox(wall.transform.position, wall.Collider.size / 2f, wall.transform.rotation, wall.Mask);
            Drawing.Draw.SolidBox(wall.transform.position, wall.transform.rotation, wall.Collider.size / 2f, Color.green);

            currentBuilding.GetComponent<Collider>().enabled = true;

            /*
             * for the first check, get the first up to four walls if it's in a t configuration
             * then follow each configuration to its conclusion and if the wall that is trying to be placed now is found, 
             * then it can't be palced
             * 
             * shrink any subsequent overlap to keep the previous wall from being overlapped
             * 
             * 
             * if 0 or 180, the wall is running horizontally
             *      no need to adjust size
             * 
             * if 90 or 270 then wall is running vertically
             *      
             * 
             * 
             */

            NativeHashMap<int, int> searched = new NativeHashMap<int, int>(100, Allocator.Temp);
            bool returnValue = false;
            for (int i = 0; i < colliders.Length; i++)
            {

                if (colliders[i].gameObject.GetInstanceID() == wall.gameObject.GetInstanceID())
                    continue;

                searched.Clear();
                if (CheckNextWall(colliders[i], wall.transform.position, instanceID, ref searched))
                {
                    returnValue = true;
                    break;
                }

            }

            searched.Dispose();
            currentBuilding.GetComponent<Collider>().enabled = false;
            return returnValue;

        }

        private bool CheckNextWall(Collider toCheck, Vector3 previous, int originalWallInstanceID,
            ref NativeHashMap<int, int> searched)
        {


            /*
             * depending on the previousPosition, will calculate which side is closer
             * then it will move 1 or 2 away from the closer side to make sure it doesn't
             * overlap the previous wall
             * 
             * 
             */


            Vector3 center = toCheck.transform.position;
            Vector3 halfExtents = new Vector3();
            halfExtents.y = 1;
            halfExtents.x = 7.5f;
            halfExtents.z = 1f;

            //running horizontally
            if (math.abs(toCheck.transform.rotation.eulerAngles.y) < .1f)
            {
                if ((toCheck.transform.position - previous).x < 0)
                {
                    //left side is closer
                    center -= Vector3.right;
                }
                else
                {
                    //right side is closer
                    center += Vector3.right;
                }
            }
            else if (math.abs(toCheck.transform.rotation.eulerAngles.y - 90) < .1f)
            {
                if ((toCheck.transform.position - previous).z < 0)
                {
                    //bottom side is closer
                    center -= Vector3.forward;
                }
                else
                {
                    //top side is closer
                    center += Vector3.forward;
                }
            }
            else
            {

                Debug.Log("Angle Not Accounted For");

            }


            Collider[] colliders = Physics.OverlapBox(center, halfExtents, toCheck.transform.rotation, LayerMask.GetMask("Wall"));
            Drawing.Draw.SolidBox(center, toCheck.transform.rotation, halfExtents * 2, Color.green);

            searched.Add(toCheck.gameObject.GetInstanceID(), 0);

            int instanceID = 0;
            for (int i = 0; i < colliders.Length; i++)
            {

                instanceID = colliders[i].gameObject.GetInstanceID();
                if (instanceID == toCheck.gameObject.GetInstanceID())
                    continue;

                if (instanceID == originalWallInstanceID)
                {
                    //true signifies that it completely encloses an area
                    return true;

                }

                if (searched.ContainsKey(instanceID))
                    continue;

                if (CheckNextWall(colliders[i], toCheck.transform.position, originalWallInstanceID, ref searched))
                    return true;

            }

            return false;

        }


        void HideHologramsWhileRunning(InputAction.CallbackContext context)
        {

            running = true;

            if (modeController.BuildMode)
            {
                modeController.TurnOffProjectorParticles();
            }


            if (currentBuilding != null)
            {
                Destroy(currentBuilding.gameObject);
            }




        }
        void ShowHologramsAfterRunning(InputAction.CallbackContext context)
        {
            running = false;

            if (Inst.Input.CurrentActionSelection >= 3)
            {
                InstantiateHologram(Inst.Input.CurrentActionSelection - 3);
                modeController.TurnOnProjectorParticles();
            }


        }

        private void InstantiateHologram(int towerIndex)
        {

            if (running)
                return;

            ModeController.TurnOnBuildMode();

            if (currentBuilding != null)
            {
                Destroy(currentBuilding.gameObject);
            }

            currentBuilding = Instantiate(hologramPrefabs[towerIndex].gameObject).GetComponent<BuildingPlacementBase>();
            currentBuilding.InitialPlacement(this);


            currentBuilding.gameObject.SetActive(true);

        }

        void PlayErrorSound()
        {
            Inst.playerAudioSource.PlayOneShot(errorSound);
        }

        #endregion

        #region Wall Generation

        public void GeneratePlayableAreaWall()
        {


            if (playableArea == null)
            {

                Debug.LogError("Playable Area not set for level.");
                return;
            }

            if (wallCollider == null)
            {
                Debug.LogError("Wall Collider is not assigned from wall prefab.");
                return;
            }

            int xWalls = (int)(playableArea.size.x / wallCollider.size.x);
            int zWalls = (int)(playableArea.size.z / wallCollider.size.x) + 1;


            float3 position = new float3(playableArea.center - (playableArea.size / 2f));
            position.x += wallCollider.size.x;

            GenerateHorizontalWall(position, xWalls);

            position.z += (playableArea.size.z);
            GenerateHorizontalWall(position, xWalls);

            position.x -= wallCollider.size.x / 2f;
            position.z -= wallCollider.size.x / 2f;

            GenerateVerticalWall(position, zWalls);


            position.x += (playableArea.size.x);
            position.x -= wallCollider.size.x / 2f;
            GenerateVerticalWall(position, zWalls);

        }

        private void GenerateHorizontalWall(float3 startPosition, int wallCount)
        {

            //generating bottom wall
            for (int i = 0; i < wallCount; i++)
            {

                GameObject inst = Instantiate(wallCollider.gameObject);
                inst.transform.position = startPosition;
                startPosition.x += wallCollider.size.x;

                Inst.NPCS.AddWallToList(inst.transform);

            }

        }

        private void GenerateVerticalWall(float3 startPosition, int wallCount)
        {

            for (int i = 0; i < wallCount; i++)
            {

                GameObject inst = Instantiate(wallCollider.gameObject);
                inst.transform.position = startPosition;
                startPosition.z -= wallCollider.size.x;

                Quaternion q = inst.transform.rotation;
                q.eulerAngles += new Vector3(0, 90, 0);
                inst.transform.rotation = q;

                Inst.NPCS.AddWallToList(inst.transform);

            }


        }

        #endregion

    }
}