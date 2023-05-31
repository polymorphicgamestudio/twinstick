using ShepProject;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceSprayController : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        other.GetComponent<EnemyPhysicsMethods>().DealDamage(10, DamageType.Ice);

    }

    private void OnTriggerStay(Collider other)
    {
        other.GetComponent<EnemyPhysicsMethods>().DealDamage(10, DamageType.Ice);

    }



    //private void OnParticleCollision(GameObject other)
    //{




    //}


}
