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
    //public static Transform slimeTarget;

    public Transform positions;
    public Transform barrel;
    public ParticleSystem shoot;

    public LineRenderer beam;
    public static float timeBetweenShots;

    private Vector3 origin;
    private Vector3 endPoint;

    // Start is called before the first frame update
    void Start()
    {
        BaseTower.timeBetweenShots = 2.0f;
    }

    // Update is called once per frame
    void Update()
    {

    }

    void ShootTurret()
    {
        origin = barrel.position;
        endPoint = BaseTower.slimeTarget.position;

            Vector3 dir = endPoint - origin;
            dir.Normalize();
            RaycastHit hit;

        if (Vector3.Distance(BaseTower.slimeTarget.position, this.transform.position) < 50.0f)
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

    IEnumerator WaitForHalfASecond()
    {
        yield return new WaitForSeconds(0.1f);
        beam.enabled = false;
        beam.gameObject.SetActive(false);
        shoot.gameObject.SetActive(false);
    }

}