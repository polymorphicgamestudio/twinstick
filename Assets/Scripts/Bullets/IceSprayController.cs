using ShepProject;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceSprayController : MonoBehaviour
{

    private void OnParticleCollision(GameObject other)
    {

        other.GetComponent<EnemyPhysicsMethods>().DealDamage(10, DamageType.Ice);


    }


}
