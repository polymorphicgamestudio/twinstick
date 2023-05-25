using ShepProject;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class BlasterTowerController : BaseTower {
    public Transform barrel;
    public ParticleSystem shoot;

    public LineRenderer beam;

    private Vector3 origin;
    private Vector3 endPoint;

    public override void Update()
    {
        base.Update();

        //transform.rotation = Quaternion.LookRotation(rotateTarget.position - transform.position);

            //Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(slimeTarget.position), Time.deltaTime * 5);



    }

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
                    hit.collider.GetComponent<EnemyPhysicsMethods>().DealDamage(100, DamageType.Blaster);
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

    IEnumerator WaitForATenthSecond()
    {
        yield return new WaitForSeconds(0.1f);
        beam.enabled = false;
        beam.gameObject.SetActive(false);
        shoot.gameObject.SetActive(false);
    }

}