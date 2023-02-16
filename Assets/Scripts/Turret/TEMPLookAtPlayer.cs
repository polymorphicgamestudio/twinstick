using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TEMPLookAtPlayer : MonoBehaviour {

	Transform player;
	public Vector3 rotationOffset = Vector3.zero;

	void Awake() {
		player = ShepProject.ShepGM.inst.player;
	}

	void Update() {
		Quaternion offset = Quaternion.Euler(rotationOffset);
		transform.rotation = offset * Quaternion.LookRotation(player.position - transform.position, Vector3.up);
    }
}