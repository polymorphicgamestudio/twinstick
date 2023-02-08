using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MatrixBlender))]
public class CameraController : MonoBehaviour {
	private Matrix4x4 ortho, perspective;
	float fov = 60f, near = .3f, far = 200f;
	[Tooltip("Orthographic Size")]
	float size = 50f;
	private float aspect;
	private MatrixBlender blender;
	private bool orthoOn = false, CamFollowPlayer = true;
	Camera cam;

	Vector3 orthoCamPos = new Vector3(0, 76, -44);
	float orthoCamAngle = 60f;

	public Transform player;

	Vector2 zoomRange = new Vector2(5f, 40f);
	float zoom = 35f;
	Vector2 vertAngleBounds = new Vector2(0f, 80f);
	float perspAngleVert = 60f;
	float perspAngleHoz = 0f;
	Vector2 lastMousePos = Vector2.zero;
	
	[HideInInspector]
	public Vector3 directionForward = Vector3.forward, directionRight = Vector3.right;


	void Start() {
		CreateMatrices();
		cam = GetComponent<Camera>();
		cam.projectionMatrix = perspective;
		blender = GetComponent<MatrixBlender>();
	}
	void Update() {
		if (Input.GetKeyDown(KeyCode.Space)) {
			ToggleOrtho();
		}
		PerspectiveCameraControls();
		if (Input.mouseScrollDelta.y != 0) Zoom();
		lastMousePos = Input.mousePosition;
	}
	
	private void CreateMatrices() {
		aspect = (float)Screen.width / (float)Screen.height;
		ortho = Matrix4x4.Ortho(-size * aspect, size * aspect, -size, size, near, far);
		perspective = Matrix4x4.Perspective(fov, aspect, near, far);
	}
	
	void EnterOrtho() {
		orthoOn = true;
		CreateMatrices();
		StartCoroutine(CamToOrthoPos(1f));
		blender.BlendToMatrix(ortho, 1f, 8, true);
		AlignPerspCamForward();
	}
	IEnumerator CamToOrthoPos(float duration) {
		CamFollowPlayer = false;
		float startTime = Time.time;
		Vector3 startPos = cam.transform.position;
		Vector3 endPos = orthoCamPos;
		Quaternion startRot = cam.transform.rotation;
		Quaternion endRot = Quaternion.Euler(orthoCamAngle, 0, 0);
		float progress = 0f;
		while (progress < 1f) {
			progress = (Time.time - startTime) / duration;
			cam.transform.position = Vector3.Slerp(startPos, endPos, progress);
			cam.transform.rotation = Quaternion.Slerp(startRot, endRot, progress);
			yield return new WaitForEndOfFrame();
		}
		cam.transform.position = endPos;
		cam.transform.rotation = endRot;
	}
	void ExitOrtho() {
		orthoOn = false;
		CreateMatrices();
		StartCoroutine(CamToPerspPos(1f));
		blender.BlendToMatrix(perspective, 1f, 8, false);
	}
	IEnumerator CamToPerspPos(float duration) {
		float startTime = Time.time;
		Vector3 startPos = cam.transform.position;
		Quaternion startRot = cam.transform.rotation;
		Quaternion endRot = Quaternion.Euler(perspAngleVert, 0, 0);
		float progress = 0f;
		while (progress < 1f) {
			progress = (Time.time - startTime) / duration;
			Vector3 direction = Quaternion.AngleAxis(perspAngleVert, Vector3.right) * -Vector3.forward;
			cam.transform.position = Vector3.Slerp(startPos, player.position + Vector3.up + direction * zoom, progress);
			cam.transform.rotation = Quaternion.Slerp(startRot, endRot, progress);
			yield return new WaitForEndOfFrame();
		}
		CamFollowPlayer = true;
	}
	void ToggleOrtho() {
		orthoOn = !orthoOn;
		if (orthoOn) EnterOrtho();
		else ExitOrtho();
	}
	
	void Zoom() {
		zoom = Mathf.Clamp(zoom + Input.mouseScrollDelta.y, zoomRange.x, zoomRange.y);
		if (zoom == zoomRange.y) {
			if (!orthoOn) EnterOrtho();
		}
		else if (orthoOn) ExitOrtho();
	}
	void PerspectiveCameraControls() {
		if (!CamFollowPlayer) return;
		//rotate mouse by holding center click
		if (Input.GetMouseButton(2)) {
			Vector2 mouseDelta = (Vector2)Input.mousePosition - lastMousePos;
			perspAngleVert = ClampAngle(perspAngleVert - mouseDelta.y, vertAngleBounds.x, vertAngleBounds.y);
			perspAngleHoz += mouseDelta.x;
		}
		//forward vector rotation around the up axis
		directionForward = Quaternion.AngleAxis(perspAngleHoz, Vector3.up) * Vector3.forward;
		//vector orthogonal to directionForward and up axis
		directionRight = Vector3.Cross(-directionForward, Vector3.up);
		//direction vector controlled by directionForward and directionRight
		Vector3 camDirection = Quaternion.AngleAxis(perspAngleVert, directionRight) * -directionForward;
		
		cam.transform.position = player.position + Vector3.up + camDirection * zoom;
		cam.transform.LookAt(player.position + Vector3.up);
	}
	void AlignPerspCamForward() {
		perspAngleHoz = 0f;
		directionForward = Vector3.forward;
		directionRight = Vector3.right;
		//also align vertical
		perspAngleVert = orthoCamAngle;
	}
	
	float ClampAngle(float angle, float min, float max) {
		do {
			if (angle < -360) angle += 360;
			if (angle > 360) angle -= 360;
		} while (angle < -360 || angle > 360);
		return Mathf.Clamp(angle, min, max);
	}
}