using UnityEngine;

public class PlayerMovementController : MonoBehaviour {

	Rigidbody rb;
	Vector2 speed = new Vector2(5f, 20f);
	public CameraController cameraController;
	
	void Start() {
		rb = GetComponent<Rigidbody>();
	}

	private void FixedUpdate() {
		Vector3 inputForward = cameraController.directionForward * Input.GetAxis("Vertical");
		Vector3 inputRight = cameraController.directionRight * Input.GetAxis("Horizontal");
		rb.velocity = Vector3.ClampMagnitude(inputForward + inputRight, 1f) * (Input.GetKey(KeyCode.LeftShift) ? speed.y : speed.x);
	}
}