using UnityEngine;

namespace ShepProject {

	public class PlayerMovementController : MonoBehaviour {

		Rigidbody rb;
		Vector2 speed = new Vector2(5f, 20f); // walk, run
		[SerializeField] CameraController cameraController;
		[HideInInspector] public bool running = false;

		private void Awake() {
			ShepGM.player = transform;
		}

		void Start() {
			rb = GetComponent<Rigidbody>();
		}

		private void FixedUpdate() {
			Vector3 inputForward = cameraController.directionForward * Input.GetAxis("Vertical");
			Vector3 inputRight = cameraController.directionRight * Input.GetAxis("Horizontal");
			running = Input.GetKey(KeyCode.LeftShift);
			rb.velocity = Vector3.ClampMagnitude(inputForward + inputRight, 1f) * (running ? speed.y : speed.x);
		}
	}

}