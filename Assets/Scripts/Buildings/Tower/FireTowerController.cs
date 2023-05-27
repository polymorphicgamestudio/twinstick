using ShepProject;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class FireTowerController : BaseTower
{
    public GameObject bombPrefab;
    public float bombSpeed = 20f;


    public override void ShootTurret()
    {

        Instantiate(bombPrefab, barrel.position, Quaternion.identity)
            .GetComponent<Rigidbody>().velocity = barrel.forward * bombSpeed;
    
    }
}
