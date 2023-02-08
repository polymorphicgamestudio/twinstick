using UnityEngine;

public class GridWanderAI : MonoBehaviour {

	int dirX = 0;
	int dirZ = 0;
	int currentX = 0;
	int currentZ = 0;
	float currentXFloat = 0f;
	float currentZFloat = 0f;
	int goalX = 0;
	int goalZ = 0;
	float moveTime = 0.2f;
	float timePercent = 0f;

	public bool enableObstacles = false;
	//TexCutieAnimate animator;

	void Start() {
		//animator = GetComponent<TexCutieAnimate>();
	}

	void Update() {
		Move();
		transform.position = new Vector3(currentXFloat, 0, currentZFloat);
		RotateBasedOnMovement();
	}
	void Move() {
		if (timePercent == 0f) {
			currentX = Mathf.RoundToInt(transform.position.x);
			currentZ = Mathf.RoundToInt(transform.position.z);
			if (Random.value < 0.4f) {
				dirX = Mathf.Clamp(Random.value < 0.5f ? dirX + 1 : dirX - 1, -1, 1);
				dirZ = Mathf.Clamp(Random.value < 0.5f ? dirZ + 1 : dirZ - 1, -1, 1);
			}
			if (CheckCollision(currentX + dirX, currentZ + dirZ))
				return;
			goalX = currentX + dirX;
			goalZ = currentZ + dirZ;
			//animator.Run((dirX != 0 || dirZ != 0));

			timePercent += Time.deltaTime / moveTime;
		}
		else {
			currentXFloat = Mathf.Lerp(currentX, goalX, timePercent);
			currentZFloat = Mathf.Lerp(currentZ, goalZ, timePercent);
			timePercent = timePercent > 1 ? 0 : timePercent + Time.deltaTime / moveTime;
		}
	}
	bool CheckCollision(int x, int z) {
		if (enableObstacles)
			return (Physics.Raycast(new Vector3(x, -1, z), Vector3.up, 2f));
		else
			return (Mathf.Abs(x) > 31 || Mathf.Abs(z) > 31);
	}
	
	protected virtual void RotateBasedOnMovement() {
		Vector3 moveDir = new Vector3(goalX - currentX, 0, goalZ - currentZ);
		if (moveDir.sqrMagnitude != 0)
			transform.rotation = Quaternion.LookRotation(moveDir);
	}
}