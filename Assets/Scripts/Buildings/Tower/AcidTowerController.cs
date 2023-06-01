using ShepProject;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class AcidTowerController : BaseTower
{
    public GameObject bombPrefab;
    public float bombSpeed = 10f;

    public override void ShootTurret()
    {
        base.ShootTurret();

        Rigidbody bomb = 
            Instantiate(bombPrefab, barrel.position, Quaternion.identity)
            .GetComponent<Rigidbody>();

        bomb.velocity = barrel.forward * bombSpeed;
    }
}
