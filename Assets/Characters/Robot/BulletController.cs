using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletController : MonoBehaviour
{
    Rigidbody rb;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        rb.MovePosition(this.transform.position + rb.velocity * Time.deltaTime);
    }

    void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.CompareTag("Slime"))
        {
           Destroy(collider.gameObject);
        }
        if (collider.gameObject.CompareTag("Untagged"))
        {
            Destroy(this.gameObject);
        }
     }
}
