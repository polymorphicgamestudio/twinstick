using System.Collections;
using System.Collections.Generic;
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


            //currentBuildingIndex = -1;

            //generate a wall surrounding the area
            GeneratePlayableAreaWall();

            #region Setup Input Callbacks


            //Inst.Input.Actions.Player.Run.performed += HideHologramsWhileRunning;
            //Inst.Input.Actions.Player.Run.canceled += ShowHologramsAfterRunning;
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

                    towers[i].slimeTarget =
                        Inst.NPCS.QuadTree
                        .GetClosestObject(towers[i].objectID, ObjectType.Slime, towers[i].minDist, towers[i].maxDist);

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

        //void HideHologramsWhileRunning(InputAction.CallbackContext context)
        //{

        //    running = true;

        //    if (modeController.BuildMode)
        //    {
        //        modeController.TurnOffProjectorParticles();
        //    }

        //    if (currentHologram != null)
        //    {
        //        Destroy(currentHologram);
        //        //currentHologram = null;
        //    }
        //    else if (wallPlacement != null)
        //    {
        //        Destroy(wallPlacement.gameObject);
        //        //wallPlacement = null;
        //    }



        //}
        //void ShowHologramsAfterRunning(InputAction.CallbackContext context)
        //{
        //    running = false;

        //    if (currentBuildingIndex == -1)
        //        return;

        //    int previous = currentBuildingIndex;
        //    currentBuildingIndex = -1;
        //    InstantiateHologram(previous);

        //    if (currentHologram != null || wallPlacement != null)
        //        modeController.TurnOnProjectorParticles();

        //}

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