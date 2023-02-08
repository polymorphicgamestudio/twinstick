using UnityEngine;

public class Animate : MonoBehaviour {

	[SerializeField]
	protected LayerMask rayIgnoreLayer;
	
	protected Animator animator;
	protected Vector3 forwardDirection = Vector3.forward; // used by RotateToGround
	protected Vector3 currentDirection; // used by Animate
	protected Vector3 currentPosition; // used by Animate
	protected float maxSpeed = 1f;
	protected Vector3 realVelocity;

	protected Vector2 backRayOffset = new Vector2(-0.2f, 0.2f); // z, y (from tranform)
	protected Vector2 frontRayOffset = new Vector2(0.2f, 0.2f); // z, y (from tranform)

	protected float smoothSpeed = 0f;
	protected float smoothTurn = 0f;


	protected virtual void Start() {
		animator = GetComponent<Animator>();
		currentDirection = transform.forward;
		currentPosition = transform.position;
		maxSpeed = GetComponent<Move>().moveSpeed.y;
	}
	protected virtual void FixedUpdate() {
		SetRealVelocity();
		RotateBasedOnMovement(4f * Mathf.Clamp01(smoothSpeed*6f));
		//RotateToGround(); disabled for now to improve performance
		PlayAnimation();
	}
	protected virtual void RotateBasedOnMovement(float turnSpeed) {
		Quaternion rotToVel = Quaternion.FromToRotation(-Vector3.forward, Vector3.ProjectOnPlane(realVelocity, Vector3.up));
		transform.rotation = Quaternion.Lerp(transform.rotation, rotToVel, turnSpeed * Time.fixedDeltaTime);
	}
	protected virtual void SetForwardDirection() {
		Vector3 squash = Vector3.ProjectOnPlane(transform.forward, Vector3.up);
		if (squash.sqrMagnitude > 0.1f)
			forwardDirection = squash.normalized;
		else
			Vector3.RotateTowards(transform.up, Vector3.up, Time.fixedDeltaTime, 0f);
	}
	protected virtual void RotateToGround() {
		SetForwardDirection();
		Vector3 backFootPosition = transform.position + forwardDirection * backRayOffset.x;
		Vector3 frontFootPosition = transform.position + forwardDirection * frontRayOffset.x;
		Vector3 backRayStart = backFootPosition + Vector3.up * backRayOffset.y;
		Vector3 frontRayStart = frontFootPosition + Vector3.up * frontRayOffset.y;
		
		if (Physics.Raycast(backRayStart, Vector3.down, out RaycastHit hitBack, 10f, ~rayIgnoreLayer))
			backFootPosition = hitBack.point;
		if (Physics.Raycast(frontRayStart, Vector3.down, out RaycastHit hitFront, 10f, ~rayIgnoreLayer))
			frontFootPosition = hitFront.point;
		//Debug.DrawLine(backFootPosition, frontFootPosition, Color.red);
		
		Vector3 feetVector = frontFootPosition - backFootPosition;
		Vector3 smoothRotation = Vector3.RotateTowards(transform.forward, feetVector, Time.fixedDeltaTime, 0f);
		transform.rotation = Quaternion.LookRotation(smoothRotation, Vector3.up);
	}
	protected virtual void SetRealVelocity() { // useful because rb.velocity doesn't reflect actual motion because it returns whatever it's being set to in order to try move the object.
		realVelocity = (transform.position - currentPosition) / Time.fixedDeltaTime;
		currentPosition = transform.position;
	}
	protected virtual float TurnAngle() { // angle of turn on Y axis since last frame based on tranform
		float turnAngle = Vector3.SignedAngle(currentDirection, transform.forward, transform.up);
		currentDirection = transform.forward;
		return turnAngle;
	}
	protected virtual float PlaneSpeed() { // speed that transform is moving on XZ plane
		return Vector3.ProjectOnPlane(realVelocity, Vector3.up).magnitude;
	}
	protected virtual void PlayAnimation() {
		smoothSpeed = (9f * smoothSpeed + PlaneSpeed() / maxSpeed) / 10f;
		//Debug.Log(smoothSpeed);
		animator.SetFloat("Speed", smoothSpeed);

		//float turn = smoothSpeed < 0.1f ? 0f : TurnAngle();
		/*
		smoothTurn = Mathf.Clamp((24f * smoothTurn + TurnAngle()) / 25f, -1f, 1f);
		animator.SetFloat("Turn", smoothTurn);
		*/
	}
}