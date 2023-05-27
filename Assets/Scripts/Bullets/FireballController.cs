using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ShepProject
{

    public class FireballController : ExplosiveProjectile
    {

        public float aliveTime;

        // Update is called once per frame
        void Update()
        {
            aliveTime -= Time.deltaTime;

            if (aliveTime <= 0)
                Explode();


        }
    }

}