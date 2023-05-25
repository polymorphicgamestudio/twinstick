using ShepProject;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class bombController : MonoBehaviour
{
    //public ParticleSystem exp;

    Rigidbody projectile;
    public ParticleSystem explosion;

    // Start is called before the first frame update
    void Start()
    {
        projectile = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        projectile.MovePosition(this.transform.position + projectile.velocity * Time.deltaTime);
    }

    void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.CompareTag("Slime"))
        {

            collider.GetComponent<EnemyPhysicsMethods>().DealDamage(100, DamageType.Acid);
            //Destroy(collider.gameObject);
            ParticleSystem exp = Instantiate(explosion, projectile.position, projectile.rotation);
            Destroy(exp.gameObject, 2.0f);
        }
        if (collider.gameObject.CompareTag("Untagged"))
        {
            Destroy(this.gameObject);
            ParticleSystem exp = Instantiate(explosion, projectile.position, projectile.rotation);
            Destroy(exp.gameObject, 2.0f);

        }
    }
}

