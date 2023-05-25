using ShepProject;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class BlasterTowerController : BaseTower
{
    public Transform positions;
    public Transform barrel;
    public ParticleSystem shoot;

    public LineRenderer beam;

    private Vector3 origin;
    private Vector3 endPoint;

    public static float timeBetweenShots = 2.0f;

    private void Start()
    {
        InvokeRepeating("ShootTurret", timeBetweenShots, timeBetweenShots);
    }

    public void ShootTurret()
    {
        if (BaseTower.slimeTarget != null)
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
            shoot.gameObject.SetActive(true);
            beam.enabled = true;
            beam.gameObject.SetActive(true);

            StartCoroutine(WaitForATenthSecond());
            Destroy(exp.gameObject, 0.1f);
        }

    }

    IEnumerator WaitForATenthSecond()
    {
        yield return new WaitForSeconds(0.1f);
        beam.enabled = false;
        beam.gameObject.SetActive(false);
        shoot.gameObject.SetActive(false);
    }

}