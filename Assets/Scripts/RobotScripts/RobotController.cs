using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotController : MonoBehaviour {

	[SerializeField] Rigidbody player;
	[SerializeField] Transform root;
	[SerializeField] Transform head;

	Vector3 centerOfBalance;
	[SerializeField] float yOffset = 1f;


	Vector3 mousePos = Vector3.zero;
	Plane floorPlane = new Plane(Vector3.up, 0f);
	float distFromMousePos = 0f;

	bool running;
	Quaternion lookDirection;

	public Transform[] footTargets; // L, R









	void Update() {
		SetMousePos();
		distFromMousePos = Vector3.Distance(root.position, mousePos);

		running = Input.GetKey(KeyCode.LeftShift) && player.velocity.sqrMagnitude > 1f;
	}

	// By Putting our animation code in LateUpdate, we allow other systems to update the environment first 
	// this allows the animation to adapt before the frame is drawn.
	void LateUpdate() {
		SetLookDirection();

		centerOfBalance = (footTargets[0].position + footTargets[1].position) / 2f;
		float breathDisplace = Mathf.Sin(Time.time * 2f) * 0.05f;

		//root.position = Vector3.Lerp(root.position, centerOfBalance + Vector3.up * breathDisplace, 10f * Time.deltaTime);
		root.position = centerOfBalance + Vector3.up * (yOffset + breathDisplace);

		
		player.rotation = Quaternion.RotateTowards(player.rotation, lookDirection, 180f * Time.deltaTime);
		head.rotation = Quaternion.Slerp(head.rotation, lookDirection, 5f * Time.deltaTime);
	}







	void SetMousePos() {
		float distance;
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		if (floorPlane.Raycast(ray, out distance))
			mousePos = ray.GetPoint(distance);
	}
	void SetLookDirection() {
		Vector3 dir = running ? player.velocity : Vector3.ProjectOnPlane(mousePos - player.position, Vector3.up);
		lookDirection = Quaternion.LookRotation(dir, Vector3.up);
	}
}