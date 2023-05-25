using ShepProject;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceSprayController : MonoBehaviour
{

    void OnParticleCollision(GameObject other)
    { 
       if (other.gameObject.CompareTag("Slime"))
        {
            other.GetComponent<EnemyPhysicsMethods>().DealDamage(100, DamageType.Ice);
        }
    }
}
