using ShepProject;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using UnityEngine;
using static UnityEngine.UI.Image;

public class LaserTowerController : BaseTower { 
    public Transform barrel;
    public ParticleSystem endOfBeam;

    public LineRenderer beam;
    public float beamDuration = 0.5f;

    private Vector3 origin;
    private Vector3 endPoint;

    // Start is called before the first frame update
    void Start() {
        beam.enabled = false;
    }

    public override void ShootTurret() {
            origin = barrel.position;
            endPoint = slimeTarget.position;

            Vector3 dir = endPoint - origin;
            dir.Normalize();
            RaycastHit hit;

            if (Physics.Raycast(origin, dir, out hit)) {
                endPoint = hit.point;
                if (hit.collider.gameObject.CompareTag("Slime")) {
                    hit.collider.GetComponent<EnemyPhysicsMethods>().DealDamage(100, DamageType.Laser);
                }
            }
            beam.SetPosition(0, origin);
            beam.SetPosition(1, endPoint);

            beam.enabled = true;
            beam.gameObject.SetActive(true);
            ParticleSystem end = Instantiate(endOfBeam, endPoint, barrel.rotation);

            StartCoroutine(WaitForHalfASecond());
            Destroy(end.gameObject, beamDuration);
    }

    IEnumerator WaitForHalfASecond() {
        yield return new WaitForSeconds(beamDuration);
        beam.enabled = false;
        beam.gameObject.SetActive(false);
    }

}