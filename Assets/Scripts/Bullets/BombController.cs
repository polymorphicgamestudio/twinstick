using ShepProject;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombController : MonoBehaviour
{

    public ParticleSystem explosion;

    void OnTriggerEnter(Collider collider)
    {

        ParticleSystem exp = Instantiate(explosion, transform.position, transform.rotation);
        Collider[] overlaps = Physics.OverlapSphere(transform.position, 3, LayerMask.GetMask("Slime"));

        for (int i = 0; i < overlaps.Length; i++)
        {
            overlaps[i].GetComponent<EnemyPhysicsMethods>().DealDamage(100, DamageType.Acid);


        }

        Destroy(exp.gameObject, 2.0f);
        Destroy(gameObject);

    }
}

