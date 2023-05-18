using ShepProject;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using UnityEngine;
using static UnityEngine.UI.Image;

public class LaserTowerController : MonoBehaviour
{ 
    public Transform positions;
    public Transform barrel;
    public ParticleSystem shoot;
    public ParticleSystem endOfBeam;

    public LineRenderer beam;
    public static float timeBetweenShots = 2f;
    public static float beamDuration = 0.5f;

    private Vector3 origin;
    private Vector3 endPoint;

    private Boolean playing = false;

    // Start is called before the first frame update
    void Start()
    {
        beam.enabled = false;
        InvokeRepeating("ShootTurret", timeBetweenShots, timeBetweenShots);
    }

    void ShootTurret()
    {
        if (BaseTower.slimebool == true)
        {
            origin = barrel.position;
            endPoint = BaseTower.slimeTarget.position;

            Vector3 dir = endPoint - origin;
            dir.Normalize();
            RaycastHit hit;

            if (Physics.Raycast(origin, dir, out hit))
            {
                endPoint = hit.point;
                if (hit.collider.gameObject.CompareTag("Slime"))
                {
                    Destroy(hit.collider.gameObject);
                }
            }
            beam.SetPosition(0, origin);
            beam.SetPosition(1, endPoint);


            ParticleSystem exp = Instantiate(shoot, origin, barrel.rotation);
            playing = true;
            beam.enabled = true;
            beam.gameObject.SetActive(true);
            ParticleSystem end = Instantiate(endOfBeam, endPoint, barrel.rotation);

            StartCoroutine(WaitForHalfASecond());
            Destroy(exp.gameObject, beamDuration);
            Destroy(end.gameObject, beamDuration);
        }
    }

    IEnumerator WaitForHalfASecond()
    {
        yield return new WaitForSeconds(beamDuration);
        beam.enabled = false;
        playing = false;
        beam.gameObject.SetActive(false);
    }

}


