using ShepProject;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningTowerController : MonoBehaviour
{

    public Transform positions;
    public Transform barrel;
    public ParticleSystem shoot;

    public LineRenderer beam;
    public Transform end;
    public float timeBetweenShots = 3f;

    private Vector3 origin;
    private Vector3 endPoint;

    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating("ShootTurret", timeBetweenShots, timeBetweenShots);
    }

    void ShootTurret()
    {
        //if (BaseTower.slimebool == true)
        //{
        //    origin = barrel.position;
        //    endPoint = BaseTower.slimeTarget.position;

        //    Vector3 dir = endPoint - origin;
        //    dir.Normalize();
        //    RaycastHit hit;

        //    if (Physics.Raycast(origin, dir, out hit))
        //    {
        //        endPoint = hit.point;
        //        if (hit.collider.gameObject.CompareTag("Slime"))
        //        {
        //            Destroy(hit.collider.gameObject);
        //        }
        //    }
        //    beam.SetPosition(0, origin);
        //    beam.SetPosition(1, endPoint);

        //    ParticleSystem exp = Instantiate(shoot, endPoint, barrel.rotation);
        //    beam.enabled = true;
        //    beam.gameObject.SetActive(true);
        //    end.position = endPoint;

        //    StartCoroutine(WaitForHalfASecond());
        //    Destroy(exp.gameObject, 1f);
        //}
    }

    IEnumerator WaitForHalfASecond()
    {
        yield return new WaitForSeconds(1f);
        beam.enabled = false;
        beam.gameObject.SetActive(false);
    }
}
