using ShepProject;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using static UnityEngine.UI.Image;

public class IceTowerController : BaseTower
{
    public Transform positions;
    public Transform barrel;
    public ParticleSystem shoot;
    public static float iceDuration = 3f;

    // Start is called before the first frame update
    void Start()
    {

    }

    public override void ShootTurret()
    {
        ParticleSystem exp = Instantiate(shoot, barrel.position, positions.rotation);
        Destroy(exp.gameObject, iceDuration);
        StartCoroutine(WaitForThreeSeconds());
    }


    IEnumerator WaitForThreeSeconds()
    {
        yield return new WaitForSeconds(iceDuration);
    }
}
