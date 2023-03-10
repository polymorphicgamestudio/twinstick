using ShepProject;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class AcidTowerController : MonoBehaviour
{

    List<Transform> slimes;
    Transform nearestslime;

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
            nearestslime = ShepGM.GetNearestFromList(slimes, this.transform.position);
            Vector3 newDirection = Vector3.RotateTowards(positions.forward, nearestslime.position - this.transform.position, Time.deltaTime * 15, 0.0f);
            positions.rotation = Quaternion.LookRotation(newDirection);
        }
        else
        {
            positions.eulerAngles = new Vector3(0, 0, 0);
        }
    }

    void ShootTurret()
    {
        if (slimes.Count > 0)
        {
            if (Vector3.Distance(nearestslime.position, this.transform.position) < 20.0f)
            {
                anim.Play("Base Layer.Shoot", 0, 0);
                shoot.Play();
                var BulletBody = (Rigidbody)Instantiate(bombPrefab, barrel.position, Quaternion.identity);
                BulletBody.velocity = barrel.forward * bombSpeed;
            }
        }
    }
}
