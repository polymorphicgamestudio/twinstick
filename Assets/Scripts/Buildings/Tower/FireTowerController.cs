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
		Rigidbody fireball = Instantiate(bombPrefab, barrel.position, barrel.rotation).GetComponent<Rigidbody>();
		fireball.velocity = fireball.transform.forward * bombSpeed;
    }
}
