using ShepProject;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class FireTowerController : BaseTower {
    public Rigidbody bombPrefab;
    public float bombSpeed = 20f;

    public Transform barrel;


    public override void ShootTurret() {
            var BulletBody = (Rigidbody)Instantiate(bombPrefab, barrel.position, Quaternion.identity);
            BulletBody.velocity = barrel.forward * bombSpeed;
    }
}
