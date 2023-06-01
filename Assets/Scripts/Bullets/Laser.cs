using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ShepProject
{

    public class Laser : MonoBehaviour
    {

        [SerializeField]
        public Transform laserEnd;
        [SerializeField]
        private Transform muzzleEffect;

        [SerializeField]
        private LineRenderer beam;

        void Start()
        {
            beam = GetComponent<LineRenderer>();
        }

        void Update()
        {

            beam.SetPosition(0, transform.position);
            beam.SetPosition(1, laserEnd.position);
        
        }


        public void SetBeamDistances(float maxDistance)
        {
            laserEnd.localPosition = new Vector3(0, .15f, maxDistance);
            beam.SetPosition(1, new Vector3(0, .15f, maxDistance));

        }


    }


}