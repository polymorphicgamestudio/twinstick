using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;


namespace ShepProject {
	[RequireComponent(typeof(MatrixBlender))]
	public class CameraController : MonoBehaviour {

		private MatrixBlender blender;
		private Matrix4x4 ortho, perspective;
		float fov = 60f, near = .3f, far = 200f;
		private float aspect;
		private bool orthoOn = false, CamFollowPlayer = true;
		Camera cam;

		[Tooltip("Orthographic Size")] float size = 50f;
		Vector3 orthoCamPos = new Vector3(0, 76, -44);
		float orthoCamAngle = 60f;

		Transform player;

		float zoomInput;
        float zoomPercent = 0.5f;
        Vector2 zoomRange = new Vector2(5f, 60f);
		Vector2 vertAngleRange = new Vector2(10f, 85f);
		float perspAngleVert = 60f;
		float perspAngleHoz = 0f;
		bool rotating = false;

		Vector3 perspCamSmoothPos;

		[HideInInspector]
		public Vector3 directionForward = Vector3.forward, directionRight = Vector3.right;

		void Start() {
			cam = Camera.main;	
			CreateMatrices();
			cam = GetComponent<Camera>();
			cam.projectionMatrix = perspective;
			blender = GetComponent<MatrixBlender>();
			player = ShepGM.inst.player;


			ShepGM.inst.Input.Actions.Player.Zoom.performed += Zoom_performed;
			ShepGM.inst.Input.Actions.Player.Zoom.canceled += Zoom_canceled;
			ShepGM.inst.Input.Actions.Player.ToggleOrth.performed += ToggleOrth_performed;
			ShepGM.inst.Input.Actions.Player.RotateCam.performed += RotateCam_performed;
			ShepGM.inst.Input.Actions.Player.RotateCam.canceled += RotateCam_canceled;
		}

		private void Zoom_performed(InputAction.CallbackContext context) {
			zoomInput = Mathf.Clamp(ShepGM.inst.Input.Actions.Player.Zoom.ReadValue<float>(),-20,20);
		}
		private void Zoom_canceled(InputAction.CallbackContext context) {
			zoomInput = 0f;
		}
		private void ToggleOrth_performed(InputAction.CallbackContext context) {
			ToggleOrtho();
		}
		private void RotateCam_performed(InputAction.CallbackContext context) {
			rotating = true;
		}
		private void RotateCam_canceled(InputAction.CallbackContext context) {
			rotating = false;
		}



		void Update() {
			zoomPercent = Mathf.Clamp01(zoomPercent + zoomInput * (zoomPercent + 0.05f) * 0.01f);
			RotateCamera();
			PerspectiveCameraControls();
		}

		void RotateCamera() {
			if (rotating)
				perspAngleHoz += ShepGM.inst.Input.Actions.Player.MouseDelta.ReadValue<Vector2>().x;
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
            float zoom = Mathf.Lerp(zoomRange.x, zoomRange.y, zoomPercent);
            float progress = 0f;
			while (progress < 1f) {
				progress = (Time.time - startTime) / duration;
				Vector3 direction = Quaternion.AngleAxis(perspAngleVert, Vector3.right) * -Vector3.forward;
				cam.transform.position = Vector3.Slerp(startPos, player.position + Vector3.up + direction * zoom, progress);
				cam.transform.rotation = Quaternion.Slerp(startRot, endRot, progress);
				yield return new WaitForEndOfFrame();
			}
			perspCamSmoothPos = cam.transform.position;
			CamFollowPlayer = true;
		}
		void ToggleOrtho() {
			orthoOn = !orthoOn;
			if (orthoOn) EnterOrtho();
			else ExitOrtho();
		}

		void PerspectiveCameraControls() {
			if (!CamFollowPlayer) return;
            
            float zoom = Mathf.Lerp(zoomRange.x, zoomRange.y, zoomPercent);
			perspAngleVert = Mathf.Lerp(vertAngleRange.x, vertAngleRange.y, Mathf.Pow(zoomPercent,0.5f));

            //forward vector rotation around the up axis
            directionForward = Quaternion.AngleAxis(perspAngleHoz, Vector3.up) * Vector3.forward;
			directionRight = Vector3.Cross(-directionForward, Vector3.up);
			Vector3 camDirection = Quaternion.AngleAxis(perspAngleVert, directionRight) * -directionForward;
			Vector3 followPosition = player.position + Vector3.up + camDirection * zoom;
			perspCamSmoothPos = Vector3.Lerp(perspCamSmoothPos, followPosition, Time.deltaTime * 10f);
			cam.transform.position = followPosition;
			cam.transform.LookAt(player.position + Vector3.up);
			cam.transform.position = perspCamSmoothPos;
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
}