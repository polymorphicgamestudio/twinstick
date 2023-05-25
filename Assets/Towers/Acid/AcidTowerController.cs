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

    private Animator anim;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
    }

    public override void ShootTurret()
    {
        anim.Play("Base Layer.Shoot", 0, 0);
        var BulletBody = (Rigidbody)Instantiate(bombPrefab, barrel.position, Quaternion.identity);
        BulletBody.velocity = barrel.forward * bombSpeed;
    }
}
