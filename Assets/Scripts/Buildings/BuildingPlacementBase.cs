using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.InputSystem.XR;

namespace ShepProject
{

    public abstract class BuildingPlacementBase : MonoBehaviour
    {

        private Renderer[] renderers;
        protected LayerMask mask;

        public LayerMask Mask => mask;
        protected virtual void Awake()
        {
            renderers = GetComponentsInChildren<Renderer>();

        }

        public abstract void InitialPlacement(BuildingManager manager);

        public virtual void PlacementUpdate(BuildingManager manager)
        {
            if (IsValidLocation(manager))
                UpdateHologramColor(manager.colorValid);
            else
                UpdateHologramColor(manager.colorInvalid);




        }

        private void UpdateHologramColor(Color color)
        {

            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].material.color = color;

            }


        }

        protected Quaternion LookAwayFromPlayer(BuildingManager manager)
        {

            return Quaternion.LookRotation(transform.position - manager.Inst.player.position, Vector3.up);

        }

        public abstract bool IsValidLocation(BuildingManager manager);

    }

}