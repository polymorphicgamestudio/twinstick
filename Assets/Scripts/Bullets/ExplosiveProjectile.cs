using ShepProject;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


namespace ShepProject
{

    public class ExplosiveProjectile : MonoBehaviour
    {

        public DamageType damageType;
        public ParticleSystem explosion;
        public Rigidbody rb;
        //public float explosionRange;
        [HideInInspector]
        public float damage;
        public float projectileSpeed;

        private void Awake()
        {
            
            rb = GetComponent<Rigidbody>();

        }

        private void Update()
        {
            rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y - (9.81f * Time.deltaTime), rb.velocity.z); 
            
            //want to use this in order to decrease total time it takes in order to reach target
            //haven't done that quite yet
            //* projectileSpeed;
            rb.transform.forward += rb.velocity * Time.deltaTime;

        }



        void OnTriggerEnter(Collider collider)
        {
            Explode();

        }


        protected void Explode()
        {
            ParticleSystem exp = Instantiate(explosion, transform.position, transform.rotation);
            Collider[] overlaps = Physics.OverlapSphere(transform.position, explosionRange, LayerMask.GetMask("Slime"));

            for (int i = 0; i < overlaps.Length; i++)
            {
                overlaps[i].GetComponent<EnemyPhysicsMethods>().DealDamage(damage, damageType);


            }

            Destroy(exp.gameObject, 2.0f);
            Destroy(gameObject);

        }


    }


}