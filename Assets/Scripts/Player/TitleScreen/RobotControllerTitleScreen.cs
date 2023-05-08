using ShepProject;
using UnityEngine;
using UnityEngine.InputSystem;

public class RobotControllerTitleScreen : MonoBehaviour {

	Rigidbody player;
	[SerializeField] Transform root;
	[SerializeField] Transform head;
	[SerializeField] Transform neck;

	Vector3 centerOfBalance;
	[SerializeField] float yOffset = 1f;


	Vector3 mouseDirHoz;
	Vector3 mouseDirVert;
	Quaternion bodyLookDir;
	Quaternion headLookDir;

	float neckAngle = 0f;

	Vector3 mousePos = Vector3.zero;
	Plane lookPlane;

	public Transform[] footTargets; // L, R

	bool running = false;
	//Quaternion lookDirection;
	bool usingController = false;
	Vector3 joystickDir;

	[HideInInspector] public bool buildMode = false;
	[HideInInspector] public Vector3 forwardTilePos;
	[HideInInspector] public Vector3 hologramPos;


	private void Start() {
		player = ShepGM.inst.player.GetComponent<Rigidbody>();
		ShepGM.inst.actions.Player.Look.performed += Look_performed;
		ShepGM.inst.actions.Player.MouseDelta.performed += MouseDelta_performed;
		Transform camTransform = Camera.main.transform;
		lookPlane = new Plane(camTransform.forward, camTransform.position + camTransform.forward * 8f);
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
		mouseDirHoz = Vector3.ProjectOnPlane(mousePos - player.position, Vector3.up);
		mouseDirVert = Vector3.ProjectOnPlane(mousePos - head.position, head.right);
	}

	// By Putting our animation code in LateUpdate, we allow other systems to update the environment first 
	// this allows the animation to adapt before the frame is drawn.
	void Update() {
		Debug.DrawRay(head.position,mouseDirVert, Color.yellow);

		SetLookDirection();

		centerOfBalance = (footTargets[0].position + footTargets[1].position) / 2f;
		float breathDisplace = Mathf.Sin(Time.time * 2f) * 0.05f;

		//root.position = Vector3.Lerp(root.position, centerOfBalance + Vector3.up * breathDisplace, 10f * Time.deltaTime);
		root.position = centerOfBalance + Vector3.up * (yOffset + breathDisplace);
		SetBodyRotation();
		SetNeckAngle(9f, 42f, -20f, 50f);
		SetHeadRotation();
	}

	void SetMousePos() {
		float distance;
		Ray ray = Camera.main.ScreenPointToRay(ShepGM.inst.actions.Player.MousePosition.ReadValue<Vector2>());
		if (lookPlane.Raycast(ray, out distance))
			mousePos = ray.GetPoint(distance);
	}
	void SetLookDirection() {
		//Vector3 dir = usingController? joystickDir : mouseDirHoz;
		Vector3 bodyLook = mousePos - (player.position - mouseDirHoz + Vector3.up);
		Vector3 headLook = mousePos - head.position;
		if (bodyLook == Vector3.zero || headLook == Vector3.zero) return;
		bodyLookDir = Quaternion.LookRotation(bodyLook, Vector3.up);
		headLookDir = Quaternion.LookRotation(headLook, Vector3.up);
	}

	void SetBodyRotation() {
		Vector3 bodyLook = mousePos - (player.position - mouseDirHoz + Vector3.up);
		if (bodyLook == Vector3.zero) return;
		bodyLookDir = Quaternion.LookRotation(bodyLook, Vector3.up);
		player.rotation = Quaternion.RotateTowards(player.rotation, bodyLookDir, 90f * Time.deltaTime);
	}
	void SetHeadRotation() {
		Vector3 headLook = mousePos - head.position;
		if (headLook == Vector3.zero) return;
		headLookDir = Quaternion.LookRotation(headLook, Vector3.up);
		head.rotation = Quaternion.Slerp(head.rotation, headLookDir, 5f * Time.deltaTime);
	}
	void SetNeckAngle(float minDist, float maxDist, float minAngle, float maxAngle) {
		float dist = Vector3.SqrMagnitude(mousePos - head.position);
		float distPercent = (dist - minDist) / (maxDist - minDist);
		float newNeckAngle = Mathf.Lerp(minAngle, maxAngle, distPercent);
		neckAngle = Mathf.Lerp(neckAngle, newNeckAngle, 5f * Time.deltaTime);
		neck.localEulerAngles = new Vector3(neckAngle, 0f, 0f);
	}
}