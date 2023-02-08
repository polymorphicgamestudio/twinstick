using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotController : MonoBehaviour {



	[SerializeField] Transform target;
	[SerializeField] Transform root;
	[SerializeField] Transform head;

	Vector3 centerOfBalance;
	[SerializeField] float yOffset = 1f;



	public Transform[] footTargets; // L, R




	// We will put all our animation code in LateUpdate.
	// This allows other systems to update the environment first, 
	// allowing the animation system to adapt to it before the frame is drawn.
	void LateUpdate() {
		Vector3 towardObjectFromHead = target.position - head.position;
		Quaternion headRotationGoal = Quaternion.LookRotation(towardObjectFromHead, transform.up);
		head.rotation = Quaternion.Slerp(head.rotation, headRotationGoal, 5f * Time.deltaTime);




		centerOfBalance = (footTargets[0].position + footTargets[1].position) / 2f;
		float breathDisplace = Mathf.Sin(Time.time * 2f) * 0.05f;

		//root.position = Vector3.Lerp(root.position, centerOfBalance + Vector3.up * breathDisplace, 10f * Time.deltaTime);
		root.position = centerOfBalance + Vector3.up * (yOffset + breathDisplace);
	}
}