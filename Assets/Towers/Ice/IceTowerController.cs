using ShepProject;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using static UnityEngine.UI.Image;

public class IceTowerController : MonoBehaviour
{
    public Transform positions;
    public Transform barrel;
    public ParticleSystem shoot;
    public static float iceDuration = 3f;

    private Boolean playing = false;

    //private Animator anim;
    public static float timeBetweenShots = 4f;

    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating("ShootTurret", timeBetweenShots, timeBetweenShots);
    }

    void ShootTurret()
    {
        //if (BaseTower.slimebool == true)
        //{
        //        ParticleSystem exp = Instantiate(shoot, barrel.position, positions.rotation);
        //        playing = true;
        //        Destroy(exp.gameObject, iceDuration);
        //        StartCoroutine(WaitForThreeSeconds());
        //}
    }


    IEnumerator WaitForThreeSeconds()
    {
        yield return new WaitForSeconds(iceDuration);
        playing = false;
    }
}
