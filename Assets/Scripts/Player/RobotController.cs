using ShepProject;
using UnityEngine;
using UnityEngine.InputSystem;

public class RobotController : MonoBehaviour {

	Rigidbody player;
	[SerializeField] Transform root;
	[SerializeField] Transform head;
	[SerializeField] Transform neck;

	Vector3 centerOfBalance;
	[SerializeField] float yOffset = 1f;


	Vector3 mousePos = Vector3.zero;
	Plane floorPlane = new Plane(Vector3.up, 0f);
	float distFromMousePos = 0f;

	public Transform[] footTargets; // L, R

	bool running = false;
	Quaternion lookDirection;
	bool usingController = false;
	Vector3 joystickDir;
	Vector3 mouseDir;

	float neckAngle = 0f;

	[HideInInspector] public bool buildMode = false;
	[HideInInspector] public Vector3 forwardTilePos;
	[HideInInspector] public Vector3 hologramPos;

	public Animator animController;


	private void Start() {
		player = ShepGM.inst.player.GetComponent<Rigidbody>();
		ShepGM.inst.Input.Actions.Player.Look.performed += Look_performed;
		ShepGM.inst.Input.Actions.Player.MouseDelta.performed += MouseDelta_performed;
	}

	private void Look_performed(InputAction.CallbackContext context) {
		usingController = true;
		Vector2 joystickInput = ShepGM.inst.Input.Actions.Player.Look.ReadValue<Vector2>();
		if (running) joystickInput = new Vector2(player.velocity.x, player.velocity.z);
		joystickDir = new Vector3(joystickInput.x, 0f, joystickInput.y);
	}
	private void MouseDelta_performed(InputAction.CallbackContext context) {
		usingController = false;
		SetMousePos();
		mouseDir = Vector3.ProjectOnPlane(mousePos - player.position, Vector3.up);
	}

	//animation code put in update so that we can insure IK animation in late update can happen after
	void Update() {
		SetMousePos();
		UpdateJoystickDirOnRunningChange(player.GetComponent<PlayerMovementController>().running);
		SetNeckAngle(1f, 20f, -20f, 35f);
		SetLookDirection();

		centerOfBalance = (footTargets[0].position + footTargets[1].position) / 2f;
		float breathDisplace = Mathf.Sin(Time.time * 2f) * 0.05f;
		root.position = centerOfBalance + Vector3.up * (yOffset + breathDisplace);
		player.rotation = Quaternion.RotateTowards(player.rotation, lookDirection, 180f * Time.deltaTime);

		SetHeadRotation();
	}

	void UpdateJoystickDirOnRunningChange(bool newRunning) {
		if (running == newRunning) return; // no change
		running = newRunning;
		joystickDir = player.velocity;
	}
	Vector3 VectorToNearestTilePos(Vector3 inputVector) {
		return new Vector3(SnapNumber(inputVector.x), 0, SnapNumber(inputVector.z));
	}
	float SnapNumber(float num) {
		return Mathf.Round((num + 2) / 4.0f) * 4 - 2;
	}
	void SetMousePos() {
		float distance;
		Ray ray = Camera.main.ScreenPointToRay(ShepGM.inst.Input.Actions.Player.MousePosition.ReadValue<Vector2>());
		if (floorPlane.Raycast(ray, out distance))
			mousePos = ray.GetPoint(distance);
	}
	void SetLookDirection() {
		Vector3 dir = running ? player.velocity : usingController? joystickDir : mouseDir;
		if (dir == Vector3.zero) return;
		lookDirection = Quaternion.LookRotation(dir, Vector3.up);
	}
	void SetHeadRotation() {
		if (buildMode) {
			Vector3 forwardFromRobot = root.position + lookDirection * Vector3.forward * 4f;
			forwardTilePos = VectorToNearestTilePos(forwardFromRobot);
			hologramPos = Vector3.Lerp(hologramPos, forwardTilePos, 20f * Time.deltaTime);
			Vector3 vectorToHologramPos = Vector3.ProjectOnPlane(hologramPos - root.position, Vector3.up);
			head.rotation = Quaternion.LookRotation(vectorToHologramPos, Vector3.up);
		}
		else head.rotation = Quaternion.Slerp(head.rotation, lookDirection, 5f * Time.deltaTime);
	}
	public void ForceHologramPosUpdate() {
		forwardTilePos = VectorToNearestTilePos(root.position + lookDirection * Vector3.forward * 4f);
		hologramPos = forwardTilePos;
	}
	void SetNeckAngle(float minDist, float maxDist, float minAngle, float maxAngle) {
		distFromMousePos = Vector3.Distance(root.position, mousePos);
		float distPercent = (distFromMousePos - minDist) / (maxDist - minDist);
		float newNeckAngle = Mathf.Lerp(minAngle, maxAngle, distPercent);
		Mathf.Lerp(neckAngle, newNeckAngle, 5f * Time.deltaTime);
		neck.localEulerAngles = new Vector3(neckAngle, 0f, 0f);
	}
}