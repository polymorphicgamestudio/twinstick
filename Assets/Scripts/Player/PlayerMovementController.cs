using UnityEngine;
using UnityEngine.InputSystem;

namespace ShepProject {

	public class PlayerMovementController : MonoBehaviour {

		Rigidbody rb;
		Vector3 moveInput;
		Vector2 speed = new Vector2(5f, 30f); // walk, run
		CameraController cameraController;
		[HideInInspector] public bool running = false;

		[SerializeField] ParticleSystem runParticles;

		private void Awake() {

		}

		void Start() {
			cameraController = Camera.main.GetComponent<CameraController>();
			ShepGM.inst.player = transform;
			rb = GetComponent<Rigidbody>();
			ShepGM.inst.Input.Actions.Player.Move.performed += Move_performed;
			ShepGM.inst.Input.Actions.Player.Move.canceled += Move_canceled;
			ShepGM.inst.Input.Actions.Player.Run.performed += Run_performed;
			ShepGM.inst.Input.Actions.Player.Run.canceled += Run_canceled;
		}

		private void FixedUpdate() {
			rb.velocity = moveInput * (running ? speed.y : speed.x);
		}


		void Move_performed(InputAction.CallbackContext context) {
			Vector2 move = ShepGM.inst.Input.Actions.Player.Move.ReadValue<Vector2>();
			Vector3 forward = cameraController.directionForward * move.y;
			Vector3 right = cameraController.directionRight * move.x;
			moveInput = Vector3.ClampMagnitude(forward + right, 1f);
		}
		void Move_canceled(InputAction.CallbackContext context) {
			moveInput = Vector2.zero;
		}
		void Run_performed(InputAction.CallbackContext context) {
			running = true;
			runParticles.Play();
		}
		void Run_canceled(InputAction.CallbackContext context) {
			running = false;
			runParticles.Stop();
		}
	}
}