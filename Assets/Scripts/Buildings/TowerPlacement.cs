using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ShepProject
{

    public class TowerPlacement : BuildingPlacementBase
    {

        protected override void Awake()
        {
            base.Awake();
            mask = LayerMask.GetMask("Tower");

        }


        public override void PlacementUpdate(BuildingManager manager)
        {
            base.PlacementUpdate(manager);
            transform.position = manager.Controller.forwardTilePos;
            transform.rotation = LookAwayFromPlayer(manager);

        }


        public override bool IsValidLocation(BuildingManager manager)
        {

            bool obstructed = Physics.Raycast(manager.Controller.forwardTilePos - Vector3.up,
                Vector3.up, 3f, mask);
            bool inBounds = 
                Mathf.Abs(manager.Controller.forwardTilePos.x) <= manager.bounds.x 
                && Mathf.Abs(manager.Controller.forwardTilePos.z) <= manager.bounds.y;

            return !obstructed && inBounds;

            //return false;

        }

        public override void InitialPlacement(BuildingManager manager)
        {

            //PlacementUpdate(manager);
            transform.position = manager.Controller.forwardTilePos;
            transform.rotation = LookAwayFromPlayer(manager);
        }


    }

}