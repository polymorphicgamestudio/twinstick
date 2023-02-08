using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Move : MonoBehaviour {
	protected Rigidbody rb;
	
	public Vector2 moveSpeed = new Vector2(2f,6f); // walk, run
	[SerializeField] protected bool move = true;
	[SerializeField] protected bool run = false;
	[SerializeField] [Range(0f,20f)] protected float chaos = 1f;
	[SerializeField] protected Vector2 attentionSpanRange = new Vector2(2f, 20f);
	protected float currentAttentionSpan = 0f;
	protected Vector3 moveDirection;
	protected Vector3 currentVelocity = Vector3.zero;
	
	protected Vector2 TEMPRandomBehaviorTimeRange = new Vector2(4f, 12f);
	protected float TEMPCurrentRandomBehaviorTime = 0f;
	
	[SerializeField] protected Colors.ColorName[] walkableColors;
	

	protected virtual void Start() {
		rb = GetComponent<Rigidbody>();
		moveDirection = transform.forward;
	}
	protected virtual void FixedUpdate() {
		ApplyAttentionSpan();
		Movement();
	}


	protected virtual void Movement() {
		float speed = move ? (run ? moveSpeed.y : moveSpeed.x) : 0f;
		Vector3 targetVelocity = FindAllowedDirection(2f) * speed;
		rb.velocity = new Vector3(targetVelocity.x, rb.velocity.y, targetVelocity.z);
	}
	protected virtual Vector3 FindAllowedDirection(float checkDistance) {
		if (AdjustMoveDirection(checkDistance)) {
			//Debug.DrawRay(transform.position, moveDirection * checkDistance);
			return moveDirection;
		}
		else return Vector3.zero;
	}
	protected virtual bool AdjustMoveDirection(float checkDistance) {
		float randomDirection = Random.value > 0.5f ? 1 : -1;
		//first, try a random rotation based on chaos
		moveDirection = RotateVector(moveDirection, chaos * randomDirection);
		if (CheckPoint(transform.position + moveDirection * checkDistance)) return true;
		//Second, try a medium rotation to either side
		moveDirection = RotateVector(moveDirection, -20 * randomDirection);
		if (CheckPoint(transform.position + moveDirection * checkDistance)) return true;
		else {
			moveDirection = RotateVector(moveDirection, 40 * randomDirection);
			if (CheckPoint(transform.position + moveDirection * checkDistance)) return true;
		}
		//Last, try medium rotations all the way around the circle until one works
		for (int i = 15; i > 0; i--) {
			moveDirection = RotateVector(moveDirection, 20 * randomDirection);
			if (CheckPoint(transform.position + moveDirection * checkDistance)) return true;
		}
		//If you get to here and nothing has worked, just give up for now
		Debug.Log(name + " couldn't find a valid move.");
		return false;
	}
	protected virtual void ApplyAttentionSpan() {
		if (currentAttentionSpan <= 0f) {
			currentAttentionSpan = Random.Range(attentionSpanRange.x, attentionSpanRange.y);
			moveDirection = RotateVector(moveDirection, Random.Range(-180f,180f));
			
			move = Random.value < 0.7f;
			run  = Random.value < 0.5f;
		}
		currentAttentionSpan -= Time.fixedDeltaTime;
	}
	protected virtual bool CheckPoint(Vector3 point) {
		return GM.cm.IsAllowed(walkableColors, GM.cm.ColorAtPoint(point));
	}
	protected virtual Vector3 RotateVector(Vector3 vector, float angle) {
		return Quaternion.AngleAxis(angle, Vector3.up) * vector;
	}
	//public ColorCheck.ColorName[] GrabWalkable() {
	//	return walkableColors;
	// }
}