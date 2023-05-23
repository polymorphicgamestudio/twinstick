using ShepProject;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Collections;

public class RobotControllerTitleScreen : MonoBehaviour {

	Rigidbody player;
	[SerializeField] Transform root;
	[SerializeField] Transform head;
	[SerializeField] Transform neck;

	Vector3 centerOfBalance;
	[SerializeField] float yOffset = 1f;

	[SerializeField] Transform startingLookPosObject;

	Vector3 mouseDirHoz;
	Quaternion bodyLookDir;
	Quaternion headLookDir;

	float neckAngle = 0f;

	Vector3 mousePos = Vector3.zero;
	Plane lookPlane;

	public Transform[] footTargets; // L, R

	bool usingController = true;

	[HideInInspector] public bool buildMode = false;
	[HideInInspector] public Vector3 forwardTilePos;
	[HideInInspector] public Vector3 hologramPos;


	private void Start() {
		player = ShepGM.inst.player.GetComponent<Rigidbody>();
		Transform camTransform = Camera.main.transform;
		lookPlane = new Plane(camTransform.forward, camTransform.position + camTransform.forward * 8f);
		StartCoroutine(nameof(_InitializeNextFrame));
	}
	void Update() {
		SetMousePos();
		SetLookDirection();
		centerOfBalance = (footTargets[0].position + footTargets[1].position) / 2f;
		float breathDisplace = Mathf.Sin(Time.time * 2f) * 0.05f;
		root.position = centerOfBalance + Vector3.up * (yOffset + breathDisplace);
		SetBodyRotation();
		SetNeckAngle(9f, 42f, -20f, 50f);
		SetHeadRotation();
	}

	private void Navigate_performed(InputAction.CallbackContext context) {
		usingController = true;
	}
	private void MouseDelta_performed(InputAction.CallbackContext context) {
		usingController = false;
	}
	void SetMousePos() {
		float distance;
		Ray ray;
		if (usingController) {
			Vector3 camPos = Camera.main.transform.position;
			Vector3 lookPos = EventSystem.current.currentSelectedGameObject ?
				EventSystem.current.currentSelectedGameObject.transform.position : startingLookPosObject.position;
			ray = new Ray(camPos, lookPos - camPos);
		}
		else ray = Camera.main.ScreenPointToRay(ShepGM.inst.actions.Player.MousePosition.ReadValue<Vector2>());

		if (lookPlane.Raycast(ray, out distance))
			mousePos = ray.GetPoint(distance);
		mouseDirHoz = Vector3.ProjectOnPlane(mousePos - player.position, Vector3.up);
	}
	void SetLookDirection() {
		Vector3 bodyLook = mousePos - (player.position - mouseDirHoz + Vector3.up); // starts from behind player
		Vector3 headLook = mousePos - head.position;
		if (bodyLook == Vector3.zero || headLook == Vector3.zero) return;
		bodyLookDir = Quaternion.LookRotation(bodyLook, Vector3.up);
		headLookDir = Quaternion.LookRotation(headLook, Vector3.up);
	}

	void SetBodyRotation() {
		player.rotation = Quaternion.RotateTowards(player.rotation, bodyLookDir, 90f * Time.deltaTime);
	}
	void SetHeadRotation() {
		head.rotation = Quaternion.Slerp(head.rotation, headLookDir, 5f * Time.deltaTime);
	}
	void SetNeckAngle(float minDist, float maxDist, float minAngle, float maxAngle) {
		float dist = Vector3.SqrMagnitude(mousePos - head.position);
		float distPercent = (dist - minDist) / (maxDist - minDist);
		float newNeckAngle = Mathf.Lerp(minAngle, maxAngle, distPercent);
		neckAngle = Mathf.Lerp(neckAngle, newNeckAngle, 5f * Time.deltaTime);
		neck.localEulerAngles = new Vector3(neckAngle, 0f, 0f);
	}
	private IEnumerator _InitializeNextFrame() {
		yield return null;
		ShepGM.inst.actions.UI.Navigate.performed += Navigate_performed;
		ShepGM.inst.actions.Player.MouseDelta.performed += MouseDelta_performed;
	}
}