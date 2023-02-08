using UnityEngine;

public class LizardController : MonoBehaviour {

	[SerializeField] Transform lookObject = null;
	[SerializeField] Transform rootMotionObject = null;

	//Note: if updating multiple parts in hierarchy, the order of updates is important.
	//Any rotations of a parent will in turn affect the children, so we want to make sure the parents are updated first.
	[SerializeField] Transform[] bones =  null;
	public Vector3 angleAdjust = Vector3.zero;
	
	private float turnSpeed = 100f;
	private float moveSpeed = 0.5f;
	private float turnAcceleration = 5f;
	private float moveAcceleration = 2f;
	private float angToTarget = 0f;
	private float maxAngToTarget = 20f;
	private Vector2 distToTargetRange;
	private Vector3 towardTargetProjected;



	/*what I want to add is:
	force based movement (PID)
	Feet raycast to ground
	body center rotates based on feet position (with smothing)
	body center height determind by feet position (just smoothed average?)
	tail rests on ground
	Stretch goal idea: (Wall climbing)
	*/

	//forForce-based movement
	private Vector2 speed = Vector2.one;
	private Vector2 targetMove = Vector2.zero;



	Vector3 currentVelocity;
	float currentAngularVelocity;

	void Start() {
		distToTargetRange = new Vector2(0.5f, 1f);
	}

	void LateUpdate() {
		RootRotationUpdate();
		rootPositionUpdate();

		LookTracking(bones[0], 3f, 10f, Quaternion.Euler(angleAdjust)); //torso
		LookTracking(bones[1], 3f, 30f, Quaternion.Euler(angleAdjust)); //neck
		LookTracking(bones[2], 4f, 20f, Quaternion.Euler(angleAdjust)); //head
	}


	/*
	void PIDMove() {
		if (Mathf.Abs(angToTarget) > 90f)
			return;

		float lizardHeight = rootMotionObject.position.y;

		Vector3 whereTo = lookObject.position;
		whereTo.y = lizardHeight;



		//Velocity
		Vector2 travelVector = whereTo - (Vector2)transform.position;

		float targetX = speed.x * Mathf.Clamp(travelVector.x, -1f, 1f);
		float targetY = speed.y * Mathf.Clamp(travelVector.y, -1f, 1f);
		Vector2 velocityError = new Vector2(targetX, targetY) - rb.velocity;

		//Calculate force
		//gain 10, max 50 values play pretty well up to a max speed of 10
		float gain = 10f;
		float maxForce = 50f;
		Vector2 force = Vector2.ClampMagnitude(velocityError * gain, maxForce);
		rb.AddForce(force);
	}
	*/

	void RootRotationUpdate() {

		Vector3 towardTarget;

		if (lookObject != null)
		{
			towardTarget = lookObject.position - rootMotionObject.position;
		}
		else
		{
			towardTarget = rootMotionObject.position;
		}
		
		// Vector toward target on the XZ plane
		towardTargetProjected = Vector3.ProjectOnPlane(towardTarget, rootMotionObject.up);
		// Get the angle from the gecko's forward direction to the direction toward toward our target
		angToTarget = Vector3.SignedAngle(rootMotionObject.forward, towardTargetProjected, Vector3.up);

		float targetAngularVelocity = 0;
		if (Mathf.Abs(angToTarget) > maxAngToTarget) {
			targetAngularVelocity = angToTarget > 0 ? turnSpeed : -turnSpeed;
		}
		// Use our smoothing function to gradually change the velocity
		currentAngularVelocity = Mathf.Lerp(currentAngularVelocity, targetAngularVelocity, 1 - Mathf.Exp(-turnAcceleration * Time.deltaTime));
		// Rotate the transform around the Y axis in world space
		rootMotionObject.Rotate(0, Time.deltaTime * currentAngularVelocity, 0, Space.World);
	}

	void rootPositionUpdate() {
		Vector3 targetVelocity = Vector3.zero;

		// Don't move if we're facing away from the target, just rotate in place
		if (Mathf.Abs(angToTarget) < 90f) {
			float distToTarget;
			if (lookObject != null)
			{
				distToTarget = Vector3.Distance(rootMotionObject.position, lookObject.position);
			}
			else
			{
				distToTarget = 0;
			}
			

			// If we're too far away, approach the target
			if (distToTarget > distToTargetRange.y) {
				targetVelocity = moveSpeed * towardTargetProjected.normalized;
			}
			// If we're too close, reverse the direction and move away
			else if (distToTarget < distToTargetRange.x) {
				targetVelocity = moveSpeed * -towardTargetProjected.normalized;
			}
		}
		currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity,1 - Mathf.Exp(-moveAcceleration * Time.deltaTime));

		rootMotionObject.position += currentVelocity * Time.deltaTime;
	}



	void LookTracking(Transform bone, float lookSpeed, float maxTurnAngle, Quaternion angleAdjust) {
		// Store the current bone rotation then reset it so our world to local space transformation will use the head's zero rotation.
		Quaternion currentLocalRotation = bone.localRotation;
		bone.localRotation = Quaternion.identity;

		Vector3 towardObjectFromBone;

		if (lookObject != null)
		{
			towardObjectFromBone = lookObject.position - bone.position;
		}
		else
		{
			towardObjectFromBone = bone.position;
		}
		
		Vector3 targetLocalLookDir = bone.InverseTransformDirection(towardObjectFromBone);

		//apply angle limit
		targetLocalLookDir = Vector3.RotateTowards(Vector3.forward, angleAdjust * targetLocalLookDir, Mathf.Deg2Rad * maxTurnAngle, 0);
		Quaternion targetLocalRotation = Quaternion.LookRotation(targetLocalLookDir, Vector3.up);
		//apply smoothing
		bone.localRotation = Quaternion.Slerp(currentLocalRotation, targetLocalRotation, 1 - Mathf.Exp(-lookSpeed * Time.deltaTime));
	}
}
















//works like a charm (assuming z is forward)
/*
public class LizardController : MonoBehaviour {

	[SerializeField] Transform lookObject;
	
	//Note: if updating multiple parts in hierarchy, the order of updates is important.
	//Any rotations of a parent will in turn affect the children, so we want to make sure the parents are updated first.
	[SerializeField] Transform[] bones;
	
	void LateUpdate() {
		//torso
		LookTracking(bones[0], 3f, 10f, Quaternion.Euler(-45, 0, 0));
		//neck
		LookTracking(bones[1], 3f, 20f, Quaternion.Euler(-45, 0, 0));
	}

	void LookTracking(Transform bone, float lookSpeed, float maxTurnAngle, Quaternion angleAdjust) {
		// Store the current bone rotation then reset it so our world to local space transformation will use the head's zero rotation.
		Quaternion currentLocalRotation = bone.localRotation;
		bone.localRotation = Quaternion.identity;

		Vector3 towardObjectFromBone = lookObject.position - bone.position;
		Vector3 targetLocalLookDir = bone.InverseTransformDirection(towardObjectFromBone);

		//apply angle limit
		targetLocalLookDir = Vector3.RotateTowards(Vector3.forward, angleAdjust * targetLocalLookDir, Mathf.Deg2Rad * maxTurnAngle, 0);
		Quaternion targetLocalRotation = Quaternion.LookRotation(targetLocalLookDir, Vector3.up);
		//apply smoothing
		bone.localRotation = Quaternion.Slerp(currentLocalRotation, targetLocalRotation, 1 - Mathf.Exp(-lookSpeed * Time.deltaTime));
	}
}
*/

/*
void lookTrackingY(Transform bone) {
Quaternion targetRotation = Quaternion.LookRotation(lookObject.position - bone.position, Vector3.up);

bone.rotation = Quaternion.Slerp(bone.rotation, targetRotation, 1 - Mathf.Exp(-2f * Time.deltaTime));

float boneCurrentYRotation = bone.localEulerAngles.y;
if (boneCurrentYRotation > 180)	boneCurrentYRotation -= 360;

float boneClampedYRotation = Mathf.Clamp(boneCurrentYRotation, -20f, 20f);

bone.localEulerAngles = new Vector3(bone.localEulerAngles.x, boneClampedYRotation, bone.localEulerAngles.z);
}
*/
