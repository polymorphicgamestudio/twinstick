using ShepProject;
using UnityEngine;
using UnityEngine.InputSystem;

public class RobotController : MonoBehaviour {

	Rigidbody player;
	[SerializeField] Transform root;
	[SerializeField] Transform head;

	Vector3 centerOfBalance;
	[SerializeField] float yOffset = 1f;


	Vector3 mousePos = Vector3.zero;
	Plane floorPlane = new Plane(Vector3.up, 0f);
	//float distFromMousePos = 0f;

	public Transform[] footTargets; // L, R


	bool running = false;
	Quaternion lookDirection;
	bool usingController = false;
	Vector3 joystickDir;
	Vector3 mouseDir;





	private void Awake() {
		player = ShepGM.inst.player.GetComponent<Rigidbody>();
		ShepGM.inst.actions.Player.Look.performed += Look_performed;
		ShepGM.inst.actions.Player.MouseDelta.performed += MouseDelta_performed;
	}

	private void Look_performed(InputAction.CallbackContext context) {
		usingController = true;
		Vector2 joystickInput = ShepGM.inst.actions.Player.Look.ReadValue<Vector2>();
		if (running) joystickInput = new Vector2(player.velocity.x, player.velocity.z);
		joystickDir = new Vector3(joystickInput.x, 0f, joystickInput.y);
		}
	private void MouseDelta_performed(InputAction.CallbackContext context) {
		usingController = false;
		SetMousePos();
		mouseDir = Vector3.ProjectOnPlane(mousePos - player.position, Vector3.up);
	}


	void Update() {
		SetMousePos();
		//distFromMousePos = Vector3.Distance(root.position, mousePos);

		if (running != player.GetComponent<PlayerMovementController>().running) { 
			running = player.GetComponent<PlayerMovementController>().running;
			//if running changes set joystickDir to velocity;
			joystickDir = player.velocity;
		}
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
		Ray ray = Camera.main.ScreenPointToRay(ShepGM.inst.actions.Player.MousePosition.ReadValue<Vector2>());
		if (floorPlane.Raycast(ray, out distance))
			mousePos = ray.GetPoint(distance);
	}
	void SetLookDirection() {
		Vector3 dir = running ? player.velocity : usingController? joystickDir : mouseDir;
		if (dir == Vector3.zero) return;
		lookDirection = Quaternion.LookRotation(dir, Vector3.up);
	}
}