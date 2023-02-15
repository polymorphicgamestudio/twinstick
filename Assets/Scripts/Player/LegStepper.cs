using ShepProject;
using System.Collections;
using UnityEngine;

public class LegStepper : MonoBehaviour {

	PlayerMovementController playerController;

	[SerializeField] Transform controller = null;
	[SerializeField] Vector3 homeOffset = Vector3.zero;
	Vector2 homeRadiusRange = new Vector2(0.1f, 0.546f);
	float homeRadius = 0.1f;
	float overshootFraction = 0.9f;
	Vector3 moveDurationRange = new Vector3(0.8f, 0.2f, 0.05f); //idle, walk, run
	float moveDuration = 0.2f;
	public Transform[] stepTargets; // L, R
	float[] distFromHome;
	bool[] moving;
	Vector3[] home;
	//private Quaternion[] footRotOffset;


	private void Awake() {
		playerController = ShepGM.inst.player.GetComponent<PlayerMovementController>();
	}
	void Start() {
		distFromHome = new float[2];
		moving = new bool[2];
		home = new Vector3[2];
		//SetFootRotOffsets();
	}

	void Update() {
		UpdateHomePositions();
		CheckMove();
		//DrawHomePositions();
	}

	void CheckMove() {
		distFromHome[0] = Vector3.Distance(stepTargets[0].position, home[0]);
		distFromHome[1] = Vector3.Distance(stepTargets[1].position, home[1]);
		if (distFromHome[0] > distFromHome[1]) { 
			if (!moving[1]) TryMove(0);
		}
		else if (!moving[0]) TryMove(1);

		AdjustFeetWhileIdle();
	}
	void AdjustFeetWhileIdle() {
		if (!moving[0] && !moving[1]) {
			homeRadius = Mathf.Lerp(homeRadius, homeRadiusRange.x, 1f * Time.deltaTime);
			overshootFraction = Mathf.Lerp(overshootFraction, 0f, 10f * Time.deltaTime);
			moveDuration = Mathf.Lerp(moveDuration, moveDurationRange.x, 5f * Time.deltaTime);
		}
		else {
			homeRadius = Mathf.Lerp(homeRadius, homeRadiusRange.y, 10f * Time.deltaTime);
			overshootFraction = Mathf.Lerp(overshootFraction, 0.8f, 10f * Time.deltaTime);
			// faster sprinting feet!!!!!!!!!!
			float targetFootSpeed = playerController.running ? moveDurationRange.z : moveDurationRange.y;
			moveDuration = Mathf.Lerp(moveDuration, targetFootSpeed, 5f * Time.deltaTime);
		}
	}
	void TryMove(int index) {
		if (moving[index]) return; // If we are already moving, don't start another move
		if (distFromHome[index] > homeRadius)
			StartCoroutine(MoveToHome(index));
	}

	IEnumerator MoveToHome(int index) {
		moving[index] = true;
		float timeElapsed = 0;
		
		Vector3 startPoint = stepTargets[index].position;
		Quaternion startRot = stepTargets[index].rotation;

		Vector3 towardHome = home[index] - startPoint;
		Vector3 overshootVector = towardHome.normalized * homeRadius * overshootFraction;
		Vector3 endPoint = home[index] + overshootVector;
		//RaycastHit hit;
		//if (Physics.Raycast(endPoint + Vector3.up * stepDistance, Vector3.down, out hit, stepDistance * 4f))
		//	endPoint = hit.point;
		

		Vector3 centerPoint = (startPoint + endPoint) / 2f;
		//lift
		//lift less when sprinting!!!!!!!
		float liftAmount = playerController.running ? 0f : Vector3.Distance(startPoint, endPoint) / 2f;
		centerPoint += Vector3.up * liftAmount;
		Quaternion endRot = controller.rotation;// * footRotOffset[index];

		// Here we use a do-while loop so the normalized time goes past 1.0 on the last iteration,
		// placing us at the end position before ending.
		do {
			timeElapsed += Time.deltaTime;
			float normalizedTime = timeElapsed / moveDuration;
			//Easing.Cubic.InOut(normalizedTime); optionally add easing to the timing
			// Quadratic bezier curve
			stepTargets[index].position = Vector3.Lerp(
				Vector3.Lerp(startPoint, centerPoint, normalizedTime),
				Vector3.Lerp(centerPoint, endPoint, normalizedTime),
				normalizedTime);
			stepTargets[index].rotation = Quaternion.Slerp(startRot, endRot, normalizedTime);

			Debug.DrawRay(stepTargets[index].position, stepTargets[index].forward);

			yield return null;
		} while (timeElapsed < moveDuration);

		moving[index] = false;
	}
	
	void UpdateHomePositions() {
		home[0] = controller.TransformPoint(new Vector3(-homeOffset.x, homeOffset.y, homeOffset.z));
		home[1] = controller.TransformPoint(homeOffset);
	}
	void DrawHomePositions() {
		for (int i = 0; i < home.Length; i++) {
			Debug.DrawRay(home[i], Vector3.up * 0.1f);
			Debug.DrawRay(home[i], controller.right * homeRadius, Color.magenta);
			Debug.DrawRay(home[i], -controller.right * homeRadius, Color.cyan);
		}
	}
}