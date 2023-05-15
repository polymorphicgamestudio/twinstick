using ShepProject;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using static UnityEngine.UI.Image;

public class IceTowerController : MonoBehaviour
{

    List<Transform> slimes;
    Transform nearestslime;

    public Transform positions;
    public Transform barrel;
    public ParticleSystem shoot;
    public static float iceDuration;

    private Boolean playing = false;

    //private Animator anim;
    public static float timeBetweenShots;

    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating("ShootTurret", timeBetweenShots, timeBetweenShots);
    }

    // Update is called once per frame
    void Update()
    {
        slimes = ShepGM.GetList(ShepGM.Thing.Slime);
        if (slimes.Count > 0)
        {
            nearestslime = ShepGM.GetNearestFromList(slimes, this.transform.position);
            if (playing == false)
            {
                Vector3 newDirection = Vector3.RotateTowards(positions.forward, nearestslime.position - this.transform.position, Time.deltaTime * 15, 0.0f);
                positions.rotation = Quaternion.LookRotation(newDirection);
            } 
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

            if (Vector3.Distance(nearestslime.position, this.transform.position) < 10.0f)
            {
                ParticleSystem exp = Instantiate(shoot, barrel.position, positions.rotation);
                playing = true;
                Destroy(exp.gameObject, iceDuration);
                StartCoroutine(WaitForThreeSeconds());
            }
        }
    }


    IEnumerator WaitForThreeSeconds()
    {
        yield return new WaitForSeconds(iceDuration);
        playing = false;
    }
}
