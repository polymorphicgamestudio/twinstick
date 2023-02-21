using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : MonoBehaviour {

	public Transform laserEnd;
	public Transform muzzleEffect;
    LineRenderer beam;

    public bool particles = true;

    void Start () {
        beam = GetComponent<LineRenderer>();
    }
    void Update() {
        Vector3 dir = laserEnd.position - transform.position;
        if (particles) {
            Quaternion rotation = Quaternion.LookRotation(dir);
            laserEnd.rotation = rotation;
            muzzleEffect.rotation = rotation;
        }
        beam.SetPosition(0, transform.position);
        beam.SetPosition(1, laserEnd.position);
    }
}