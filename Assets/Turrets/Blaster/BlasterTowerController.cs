using ShepProject;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class BlasterTowerController : MonoBehaviour
{

    List<Transform> slimes;
    Transform nearestslime;

    public Transform positions;
    public Transform barrel;
    public ParticleSystem shoot;

    public LineRenderer beam;
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

            if (Vector3.Distance(nearestslime.position, this.transform.position) < 50.0f)
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

                ParticleSystem exp = Instantiate(shoot, origin, barrel.rotation);
                shoot.gameObject.SetActive(true);
                beam.enabled = true;
                beam.gameObject.SetActive(true);

                StartCoroutine(WaitForHalfASecond());
                Destroy(exp.gameObject, 0.1f);
            }
        }

    }

    IEnumerator WaitForHalfASecond()
    {
        yield return new WaitForSeconds(0.1f);
        beam.enabled = false;
        beam.gameObject.SetActive(false);
        shoot.gameObject.SetActive(false);
    }

}