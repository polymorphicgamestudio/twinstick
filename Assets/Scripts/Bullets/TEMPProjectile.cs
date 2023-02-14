using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TEMPProjectile : MonoBehaviour {

	//	distance = Vector3.Distance(Camera.main.transform.position, Camera.main.GetComponent<CameraController>().target.position);
	
	void Update() {
		Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y,60.5f));
		Vector3 flat = Vector3.ProjectOnPlane(worldPosition - transform.position, Vector3.up);

		transform.rotation = Quaternion.LookRotation(flat, Vector3.up);
	}
}