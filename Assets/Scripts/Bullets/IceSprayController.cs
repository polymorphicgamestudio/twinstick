using ShepProject;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceSprayController : MonoBehaviour
{
    public float damage;

    private void OnTriggerEnter(Collider other)
    {
        other.GetComponent<EnemyPhysicsMethods>().DealDamage(damage * Time.deltaTime, DamageType.Ice);

    }

    private void OnTriggerStay(Collider other)
    {
        other.GetComponent<EnemyPhysicsMethods>().DealDamage(damage * Time.deltaTime, DamageType.Ice);

    }



    //private void OnParticleCollision(GameObject other)
    //{




    //}


}
