using ShepProject;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField]
    private Rigidbody rb;

    public float aliveTime;

    private void Update()
    {
        aliveTime -= Time.deltaTime;

        if (aliveTime < 0)
            Destroy(gameObject);
    }

    public void Initialize(Vector3 position, float speed)
    {
        rb.velocity = position * speed;

    }


    private void OnTriggerEnter(Collider other)
    {

        if (1 << other.gameObject.layer == LayerMask.GetMask("Slime"))
            other.GetComponent<EnemyPhysicsMethods>().DealDamage(100, DamageType.Player);

        Destroy(gameObject);

    }


}
