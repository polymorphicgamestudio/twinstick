using ShepProject;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ShepProject
{

    public class ExplosiveProjectile : MonoBehaviour
    {

        public DamageType damageType;
        public ParticleSystem explosion;
        public float explosionRange;

        void OnTriggerEnter(Collider collider)
        {
            Explode();

        }


        protected void Explode()
        {
            ParticleSystem exp = Instantiate(explosion, transform.position, transform.rotation);
            Collider[] overlaps = Physics.OverlapSphere(transform.position, 3, LayerMask.GetMask("Slime"));

            for (int i = 0; i < overlaps.Length; i++)
            {
                overlaps[i].GetComponent<EnemyPhysicsMethods>().DealDamage(100, damageType);


            }

            Destroy(exp.gameObject, 2.0f);
            Destroy(gameObject);

        }


    }


}