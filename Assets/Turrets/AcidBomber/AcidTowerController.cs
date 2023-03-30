using ShepProject;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class AcidTowerController : MonoBehaviour
{

    List<Transform> slimes;
    Transform slimeInRange;
    bool slimeInRangebool = false;

    public Transform positions;
    public Rigidbody bombPrefab;
    public float bombSpeed;
    public Transform barrel;
    public ParticleSystem shoot;

    private Animator anim;
    public float timeBetweenShots;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        InvokeRepeating("ShootTurret", timeBetweenShots, timeBetweenShots);
    }

    // Update is called once per frame
    void Update()
    {
        slimes = ShepGM.GetList(ShepGM.Thing.Slime);
        if (slimes.Count > 0)
        {
            for (int i = 0; i < slimes.Count; i++)
            {
                if (Vector3.Distance(slimes[i].transform.position, this.transform.position) > 15f && Vector3.Distance(slimes[i].transform.position, this.transform.position) < 22f)
                {
                    slimeInRange = slimes[i];
                    slimeInRangebool = true;
                }
                else
                {
                    slimeInRangebool = false;
                }
            }
            if (slimeInRangebool == true)
            {
                Vector3 newDirection = Vector3.RotateTowards(positions.forward, slimeInRange.position - this.transform.position, Time.deltaTime * 15, 0.0f);
                positions.rotation = Quaternion.LookRotation(newDirection);
            }
        }
        else
        {
            positions.eulerAngles = new Vector3(0, 0, 0);
            slimeInRangebool = false;
        }
    }

    void ShootTurret()
    {
        if (slimeInRangebool == true)
        {
                anim.Play("Base Layer.Shoot", 0, 0);
                shoot.Play();
                var BulletBody = (Rigidbody)Instantiate(bombPrefab, barrel.position, Quaternion.identity);
                BulletBody.velocity = barrel.forward * bombSpeed;
        }
    }
}
