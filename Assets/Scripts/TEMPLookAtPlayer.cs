using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TEMPLookAtPlayer : MonoBehaviour {

	public Transform target;

    void Update() {
		transform.rotation = Quaternion.LookRotation(target.position - transform.position, Vector3.up);
    }
}