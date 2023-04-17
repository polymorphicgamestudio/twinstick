using ShepProject;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningTowerController : MonoBehaviour
{

    List<Transform> slimes;
    Transform nearestslime;

    public Transform positions;
    public Transform barrel;
    public ParticleSystem shoot;

    public LineRenderer beam;
    public Transform end;
    public float timeBetweenShots;

    private Vector3 origin;
    private Vector3 endPoint;

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
            Vector3 newDirection = Vector3.RotateTowards(positions.forward, nearestslime.position - this.transform.position, Time.deltaTime, 0.0f);
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
            origin = barrel.position;
            endPoint = nearestslime.position;

            Vector3 dir = endPoint - origin;
            dir.Normalize();
            RaycastHit hit;

            if (Vector3.Distance(nearestslime.position, this.transform.position) < 10.0f)
            {
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

                ParticleSystem exp = Instantiate(shoot, endPoint, barrel.rotation);
                beam.enabled = true;
                beam.gameObject.SetActive(true);
                end.position = endPoint;

                StartCoroutine(WaitForHalfASecond());
                Destroy(exp.gameObject, 1f);
            }
        }

    }

    IEnumerator WaitForHalfASecond()
    {
        yield return new WaitForSeconds(1f);
        beam.enabled = false;
        beam.gameObject.SetActive(false);
    }
}
