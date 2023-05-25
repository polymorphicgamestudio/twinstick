using ShepProject;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningTowerController : BaseTower
{

    public Transform positions;
    public Transform barrel;
    public ParticleSystem shoot;

    public LineRenderer beam;
    public Transform end;

    private Vector3 origin;
    private Vector3 endPoint;

    public override void ShootTurret()
    {
            origin = barrel.position;
            endPoint = slimeTarget.position;

            Vector3 dir = endPoint - origin;
            dir.Normalize();
            RaycastHit hit;

            if (Physics.Raycast(origin, dir, out hit))
            {
                endPoint = hit.point;
                if (hit.collider.gameObject.CompareTag("Slime"))
                {
                    hit.collider.GetComponent<EnemyPhysicsMethods>().DealDamage(100, DamageType.Lightning);
                    //Destroy(hit.collider.gameObject);
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

    IEnumerator WaitForHalfASecond()
    {
        yield return new WaitForSeconds(1f);
        beam.enabled = false;
        beam.gameObject.SetActive(false);
    }
}
