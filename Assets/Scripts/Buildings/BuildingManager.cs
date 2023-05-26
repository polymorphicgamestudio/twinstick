using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ShepProject
{

    public class BuildingManager : SystemBase
    {

        public List<BaseTower> prefabs;
        public List<BaseTower> towers;

        [SerializeField]
        private BoxCollider playableArea;
        [SerializeField]
        private BoxCollider wallCollider;



        private void Start()
        {

            towers = new List<BaseTower>();

            //generate a wall surrounding the area
            GeneratePlayableAreaWall();
            Inst.actions.Buildings.BuildingOne.performed += BuildingOneCallback;


        }


        private void Update()
        {

            /*
             * if towers don't have a target, need to search for one
             * 
             * 
             */


            for (int i = 0; i < towers.Count; i++)
            {
                if (towers[i].NeedsTarget)
                {

                    //slimeTarget = Inst.EnemyManager.QuadTree
                    //.GetClosestObject(objectID, ShepProject.ObjectType.Slime, minDist, maxDist);

                    towers[i].slimeTarget =
                        Inst.EnemyManager.QuadTree
                        .GetClosestObject(towers[i].objectID, ObjectType.Slime, towers[i].minDist, towers[i].maxDist);
                    
                }


            }


            //now done searching, do the manual update

            for (int i = 0; i < towers.Count; i++)
            {

                towers[i].ManualUpdate();


            }



        }







        #region Callbacks

        private void BuildingOneCallback(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            InstantiateBuilding(0);

        }


        private void InstantiateBuilding(int towerIndex)
        {



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

                Inst.EnemyManager.AddWallToList(inst.transform);

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

                Inst.EnemyManager.AddWallToList(inst.transform);

            }


        }

        #endregion



    }





}