using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceSprayController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnParticleCollision(GameObject other)
    { 
       if (other.gameObject.CompareTag("Slime"))
        {
            Destroy(other.gameObject);
        }
    }
}
