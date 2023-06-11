using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ShepProject
{

    public class BuildingManager : SystemBase
    {

        public GameObject[] hologramPrefabs;
        public GameObject[] buildUpHolograms;
        public BaseTower[] prefabs;
        public List<BaseTower> towers;

        [SerializeField]
        private BoxCollider playableArea;
        [SerializeField]
        private BoxCollider wallCollider;

        [SerializeField]
        private RobotController controller;
        [SerializeField]
        private RobotModeController modeController;

        private int currentBuildingIndex;
        public int actionSelectionNumber;
        private int previousActionSelection;

        private GameObject currentHologram;
        private WallPlacement wallPlacement;

        private List<GameObject> currentBuildUps;
        private List<int> buildIndices;
        private List<float> buildUpTimers;

        [SerializeField]
        private AudioClip errorSound;

        private bool running;

        Vector2 bounds = new Vector2(72, 40);  // (5, 3) * 16 - 8
        Color colorValid = new Color(0.05f, 0.39f, 1f, 0.25f);
        Color colorInvalid = new Color(1f, 0.07f, 0.07f, 0.25f);



        private void Start()
        {

            actionSelectionNumber = 1;

            currentBuildUps = new List<GameObject>();
            buildUpTimers = new List<float>();
            towers = new List<BaseTower>();
            buildIndices = new List<int>();


            currentBuildingIndex = -1;

            //generate a wall surrounding the area
            GeneratePlayableAreaWall();

            #region Setup Input Callbacks


            Inst.Input.Actions.Player.Run.performed += HideHologramsWhileRunning;
            Inst.Input.Actions.Player.Run.canceled += ShowHologramsAfterRunning;

            #endregion
        }


        private void Update()
        {

            #region Building Towers

            if (currentHologram != null)
            {
                currentHologram.transform.position = controller.hologramPos;
                UpdateTowerHologramColor();
            }

            if (wallPlacement != null)
            {

                wallPlacement.PositionWall(modeController.WallReferencePosition(), modeController.WallReferenceRotation());
                //UpdateWallHologramColor();
            }

            if (Inst.Input.CurrentActionSelection >= 3)
            {

                if (previousActionSelection != Inst.Input.CurrentActionSelection)
                {

                    previousActionSelection = Inst.Input.CurrentActionSelection;
                    InstantiateHologram(Inst.Input.CurrentActionSelection - 3);
                }

            }
            else
            {
                CancelBuildMode();
                previousActionSelection = Inst.Input.CurrentActionSelection;

            }

            if (Inst.Input.ActionSelected)
            {
                ConfirmBuilding();
            }

            #region Old


            for (int i = 0; i < buildUpTimers.Count; i++)
            {
                buildUpTimers[i] -= Time.deltaTime;

                if (buildUpTimers[i] <= 0)
                {

                    GameObject tower = Instantiate(prefabs[buildIndices[i]].gameObject);
                    tower.transform.position = currentBuildUps[i].transform.position;
                    tower.transform.rotation = currentBuildUps[i].transform.rotation;

                    Destroy(currentBuildUps[i]);

                    towers.Add(tower.GetComponent<BaseTower>());

                    Inst.NPCS.AddTowerToList(towers[towers.Count - 1]);

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

            for (int i = 0; i < towers.Count; i++)
            {

                /*
                 * if towers don't have a target, need to search for one
                 * 
                 */

                if (towers[i].NeedsTarget)
                {

                    //slimeTarget = Inst.EnemyManager.QuadTree
                    //.GetClosestObject(objectID, ShepProject.ObjectType.Slime, minDist, maxDist);

                    towers[i].slimeTarget =
                        Inst.NPCS.QuadTree
                        .GetClosestObject(towers[i].objectID, ObjectType.Slime, towers[i].minDist, towers[i].maxDist);

                }


            }


            //now done searching, do the manual update

            for (int i = 0; i < towers.Count; i++)
            {

                towers[i].ManualUpdate();

            }

            #endregion

        }


        ////I really hate how this works, but I'm setting this up for now because selecting towers from the HUD
        ////using the mouse doesn't work otherwise.  Still not a fan of building manager for the record  - landon 
        //public void SetStateFromActionSelectionNumber(int actionNumber)
        //{

        //    actionSelectionNumber = actionNumber;// actionNumber > 10 ? 10 : actionNumber < 1 ? 1 : actionNumber;
        //    if (actionSelectionNumber > 3)
        //    {
        //        modeController.TurnOnBuildMode();

        //        InstantiateHologram(actionSelectionNumber - 3);

        //    }
        //    else
        //    {
        //        CancelBuildMode();
        //    }
        //}

        #region Building Related

        public void CancelBuildMode()
        {
            modeController.TurnOffBuildMode();

            if (currentHologram != null)
                Destroy(currentHologram);

            if (wallPlacement != null)
                Destroy(wallPlacement.gameObject);

            currentBuildingIndex = -1;


        }

        private void ConfirmBuilding()
        {
            if (Inst.Input.MouseOverHUD()) return;
            InstantiateBuildup();
        }

        void HideHologramsWhileRunning(InputAction.CallbackContext context)
        {

            running = true;

            if (modeController.BuildMode)
            {
                modeController.TurnOffProjectorParticles();
            }

            if (currentHologram != null)
            {
                Destroy(currentHologram);
                //currentHologram = null;
            }
            else if (wallPlacement != null)
            {
                Destroy(wallPlacement.gameObject);
                //wallPlacement = null;
            }



        }
        void ShowHologramsAfterRunning(InputAction.CallbackContext context)
        {
            running = false;

            if (currentBuildingIndex == -1)
                return;

            int previous = currentBuildingIndex;
            currentBuildingIndex = -1;
            InstantiateHologram(previous);

            if (currentHologram != null || wallPlacement != null)
                modeController.TurnOnProjectorParticles();

        }

        private void InstantiateHologram(int towerIndex)
        {

            actionSelectionNumber = towerIndex + 4;

            if (running)
                return;

            if (currentBuildingIndex == -1 && towerIndex != 6)
            {
                modeController.TurnOnBuildMode();

            }

            if (currentBuildingIndex != towerIndex)
            {
                if (currentHologram != null)
                {
                    Destroy(currentHologram);
                }
                else if (wallPlacement != null)
                {
                    Destroy(wallPlacement.gameObject);
                }
            }
            else
                return;


            //special exception for wall
            if (towerIndex == 6 && currentBuildingIndex != 6)
            {
                modeController.TurnOnProjectorParticles();
                wallPlacement = Instantiate(hologramPrefabs[towerIndex]).GetComponent<WallPlacement>();
                wallPlacement.PositionWall(modeController.WallReferencePosition(), modeController.WallReferenceRotation());


                currentBuildingIndex = towerIndex;
                return;
            }



            //instantiate hologram prefab and place it in front of robot
            currentHologram = Instantiate(hologramPrefabs[towerIndex].gameObject);
            currentHologram.transform.position = controller.hologramPos;

            currentHologram.SetActive(true);

            currentBuildingIndex = towerIndex;

        }

        private void InstantiateBuildup()
        {
            if (currentHologram == null && wallPlacement == null)
                return;


            if (currentHologram != null)
            {

                if (!ValidTowerLocation(controller.forwardTilePos))
                {
                    PlayErrorSound();
                    return;
                }

                GameObject buildUp = Instantiate(buildUpHolograms[currentBuildingIndex]);
                buildUp.transform.position = currentHologram.transform.position;
                buildUp.transform.rotation =
                    Quaternion.LookRotation(currentHologram.transform.position - ShepGM.inst.player.position, Vector3.up);

                //replace with the actual turret here.
                buildUp.GetComponent<Animator>().SetTrigger("Build");

                currentBuildUps.Add(buildUp);
                buildUpTimers.Add(7f);
                buildIndices.Add(currentBuildingIndex);

                buildUp.SetActive(true);



                Destroy(currentHologram);
                currentHologram = null;
            }
            else
            {

                if (wallPlacement.validLocation)
                {
                    Inst.NPCS.AddWallToList(wallPlacement.transform);

                    wallPlacement.PlaceWall();
                    wallPlacement = null;

                }
                else
                {
                    PlayErrorSound();
                    return;
                }

            }

            Inst.Pathfinding.QueueVectorFieldUpdate();

            int previous = currentBuildingIndex;
            currentBuildingIndex = -1;

            InstantiateHologram(previous);


        }


        void UpdateTowerHologramColor()
        {
            foreach (Renderer r in currentHologram.GetComponentsInChildren<Renderer>())
            {
                if (ValidTowerLocation(controller.forwardTilePos))
                    r.material.color = colorValid;
                else
                    r.material.color = colorInvalid;
            }
        }


        bool ValidTowerLocation(Vector3 pos)
        {

            bool obstructed = Physics.Raycast(pos - Vector3.up, Vector3.up, 3f, LayerMask.GetMask("Tower"));
            bool inBounds = Mathf.Abs(pos.x) <= bounds.x && Mathf.Abs(pos.z) <= bounds.y;
            return !obstructed && inBounds;


        }

        void PlayErrorSound()
        {
            Inst.player.GetComponent<AudioSource>().PlayOneShot(errorSound);
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