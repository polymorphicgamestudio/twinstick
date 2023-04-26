using ShepProject;
using Unity.Mathematics;
using UnityEditor;
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

    public bool BuildMode = false;
    public Transform hologram;
	public Transform TowerToPlace;
	[HideInInspector] public Vector3 forwardTilePos;




	private void Start() {
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
		if (running != player.GetComponent<PlayerMovementController>().running) { 
			running = player.GetComponent<PlayerMovementController>().running;
			//if running changes set joystickDir to velocity;
			joystickDir = player.velocity;
		}
		SetForwardTilePos();
	}

	// By Putting our animation code in LateUpdate, we allow other systems to update the environment first 
	// this allows the animation to adapt before the frame is drawn.
	void LateUpdate() {
		SetNeckAngle(1f, 20f, -20f, 35f);
		SetLookDirection();

		centerOfBalance = (footTargets[0].position + footTargets[1].position) / 2f;
		float breathDisplace = Mathf.Sin(Time.time * 2f) * 0.05f;

		//root.position = Vector3.Lerp(root.position, centerOfBalance + Vector3.up * breathDisplace, 10f * Time.deltaTime);
		root.position = centerOfBalance + Vector3.up * (yOffset + breathDisplace);

		player.rotation = Quaternion.RotateTowards(player.rotation, lookDirection, 180f * Time.deltaTime);

		if (BuildMode) {
			Vector3 forwardDir = lookDirection * Vector3.forward;
			Vector3 inFrontOfRobot = root.position + forwardDir * 4f;
			forwardTilePos = VectorToNearestTilePos(inFrontOfRobot);
			//Vector3 vectorToSnappePos = Vector3.ProjectOnPlane(forwardTilePos - head.position,Vector3.up);
			//Quaternion snappedLookDir = Quaternion.LookRotation(vectorToSnappePos, Vector3.up);
			//head.rotation = Quaternion.Slerp(head.rotation, snappedLookDir, 10f * Time.deltaTime);
			Vector3 vectorToHologramPos = Vector3.ProjectOnPlane(hologram.position - root.position,Vector3.up);
			head.rotation = Quaternion.LookRotation(vectorToHologramPos, Vector3.up);
            if (Input.GetMouseButtonDown(0))
            {
				Instantiate(this.TowerToPlace, forwardTilePos, Quaternion.identity);
				BuildMode = false;
				Destroy(this.hologram.gameObject);
            }
        }
		else
			head.rotation = Quaternion.Slerp(head.rotation, lookDirection, 5f * Time.deltaTime);
	}


	void SetForwardTilePos() {
		if (BuildMode) {
			Vector3 inFrontOfRobotHead = head.position + head.forward * 4f;
			hologram.position = Vector3.Lerp(hologram.position, forwardTilePos, 20f * Time.deltaTime);
			//Debug.DrawRay(forwardTilePos, Vector3.up * 5f, Color.yellow);
		}
		else {
			forwardTilePos = Vector3.zero;
		}
	}
	Vector3 VectorToNearestTilePos(Vector3 inputVector) {
		return new Vector3(SnapNumber(inputVector.x), 0, SnapNumber(inputVector.z));
	}
	float SnapNumber(float num) {
		return Mathf.Round((num + 2) / 4.0f) * 4 - 2;
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
	void SetNeckAngle(float minDist, float maxDist, float minAngle, float maxAngle) {
		distFromMousePos = Vector3.Distance(root.position, mousePos);
		float distPercent = (distFromMousePos - minDist) / (maxDist - minDist);
		float neckAngle = Mathf.Lerp(minAngle, maxAngle, distPercent);
		neck.localEulerAngles = new Vector3(neckAngle, 0f, 0f);
	}
}