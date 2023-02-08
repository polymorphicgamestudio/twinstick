using UnityEngine;

public class TwinstickMovement : MonoBehaviour {

	int currentX = 0;
	int currentZ = 0;
	float currentXFloat = 0f;
	float currentZFloat = 0f;
	int goalX = 0;
	int goalZ = 0;
	float moveTime = 0.05f;
	float timePercentX = 0f;
	float timePercentZ = 0f;

	[SerializeField] GameObject obstacle;

	public LayerMask mask;

	void Start() {
    }

    void Update() {
		SetCurrentXFloat();
		SetCurrentZFloat();
		transform.position = new Vector3(currentXFloat, 0, currentZFloat);
		if (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0)
			RotateBasedOnMovement();

		if (Input.GetKey(KeyCode.Space))
			AttemptPlaceObstacle(currentX, currentZ);
	}
	void SetCurrentXFloat() {
		if (timePercentX == 0f) {
			currentX = Mathf.RoundToInt(transform.position.x);
			if (Input.GetAxisRaw("Horizontal") != 0) {
				if (CheckCollision(currentX + (int)Input.GetAxisRaw("Horizontal"), goalZ))
					return;
				goalX = currentX + (int)Input.GetAxisRaw("Horizontal");
				timePercentX += Time.deltaTime / moveTime;
			}
		}
		else {
			currentXFloat = Mathf.Lerp(currentX, goalX, timePercentX);
			timePercentX = timePercentX > 1 ? 0 : timePercentX + Time.deltaTime / moveTime;
		}
	}
	void SetCurrentZFloat() {
		if (timePercentZ == 0f) {
			currentZ = Mathf.RoundToInt(transform.position.z);
			if (Input.GetAxisRaw("Vertical") != 0) {
				if (CheckCollision(goalX, currentZ + (int)Input.GetAxisRaw("Vertical")))
					return;
				goalZ = currentZ + (int)Input.GetAxisRaw("Vertical");
				timePercentZ += Time.deltaTime / moveTime;
			}
		}
		else {
			currentZFloat = Mathf.Lerp(currentZ, goalZ, timePercentZ);
			timePercentZ = timePercentZ > 1 ? 0 : timePercentZ + Time.deltaTime / moveTime;
		}
	}
	bool CheckCollision(int x, int z) {
		return (Physics.Raycast(new Vector3(x, -1, z), Vector3.up, 2f, mask));
	}
	void AttemptPlaceObstacle(int x, int z) {
		if (Physics.Raycast(new Vector3(x, -1, z), Vector3.up, 2f))
			return;
		Instantiate(obstacle, new Vector3(x, 0.5f, z), Quaternion.identity);
	}



	protected virtual void RotateBasedOnMovement() {
		Vector3 moveDir = new Vector3(goalX - currentX, 0, goalZ - currentZ);
		//Quaternion rot = Quaternion.FromToRotation(Vector3.forward, new Vector3(moveDir.x, 0, moveDir.y));
		transform.rotation = Quaternion.LookRotation(moveDir);
		//transform.rotation = Quaternion.Lerp(transform.rotation, rot, turnSpeed * Time.deltaTime);
	}
}