using ShepProject;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class AcidTowerController : BaseTower
{
    public Rigidbody bombPrefab;
    public float bombSpeed = 10f;

    public Transform barrel;


    public override void ShootTurret()
    {
        animator.Play("Base Layer.Shoot", 0, 0);
        Rigidbody bomb = 
            Instantiate(bombPrefab.gameObject, barrel.position, Quaternion.identity)
            .GetComponent<Rigidbody>();

        bomb.velocity = barrel.forward * bombSpeed;
    }
}
