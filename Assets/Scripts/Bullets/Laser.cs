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
        public LineRenderer beam;


        private void Awake()
        {

            laserEnd.gameObject.SetActive(false);
            muzzleEffect.gameObject.SetActive(false);
        }

        void Start()
        {
            beam = GetComponent<LineRenderer>();


            beam.enabled = false;

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