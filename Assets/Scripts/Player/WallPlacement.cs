using UnityEngine;

public class WallPlacement : MonoBehaviour {

	//Vector2 bounds = new Vector2(72,40);  // (5, 3) * 16 - 8

	public Transform wall;

	[SerializeField] MeshRenderer mesh;
	Color colorValid = new Color(0.05f, 0.39f, 1f, 0.25f);
	Color colorInvalid = new Color(1f, 0.07f, 0.07f, 0.25f);

	Vector3 center;
	Vector3 targetPosition;
	Quaternion targetRotation;

	Vector3 smoothPosition = Vector3.zero;
	Quaternion smoothRotation = Quaternion.identity;
	float lerpFraction = 0f;


	void Update() {
		PositionWall(transform);
	}
	void PositionWall(Transform reference) {
		Vector3 hoz = new Vector3(SnapNumber(reference.position.x, 0), 0, SnapNumber(reference.position.z, 8));
		Vector3 vert = new Vector3(SnapNumber(reference.position.x, 8), 0, SnapNumber(reference.position.z, 0));
		bool hozIsCloser = (reference.position - hoz).sqrMagnitude < (reference.position - vert).sqrMagnitude;
		targetPosition = hozIsCloser ? hoz : vert;

		float referenceY = reference.eulerAngles.y;
		float clampHozY = referenceY > 270 || referenceY < 90 ? 0 : 180;
		float clampVertY = referenceY < 180 ? 90 : 270;
		float clampedY = hozIsCloser? clampHozY: clampVertY;
		targetRotation = Quaternion.Euler(0f, clampedY, 0f);

		smoothPosition = Vector3.Lerp(smoothPosition, targetPosition, 10f * Time.deltaTime);
		smoothRotation = Quaternion.Lerp(smoothRotation, targetRotation, 10f * Time.deltaTime);

		lerpFraction = Mathf.Clamp01((64 - (reference.position - targetPosition).sqrMagnitude) / 50);
		wall.position = Vector3.Lerp(reference.position, smoothPosition, lerpFraction);
		wall.rotation = Quaternion.Lerp(reference.rotation, smoothRotation, lerpFraction);

		Color holorgramColor = lerpFraction == 1 ? colorValid : colorInvalid;
		mesh.material.SetColor("_ColorA", holorgramColor);
	}
	float SnapNumber(float num, float offset) {
		return Mathf.Round((num - offset) / 16.0f) * 16 + offset;
	}
}




/* backup...   working great but I want to play with some things

	void LerpToTarget(Vector3 inputVector) {
		lerpFraction = Mathf.Clamp01((64 - (inputVector - targetPosition).sqrMagnitude) / 50);
		Quaternion outRotation = Quaternion.LookRotation(center - inputVector);
		smoothPosition = Vector3.Lerp(smoothPosition, targetPosition, 10f * Time.deltaTime);
		smoothRotation = Quaternion.Lerp(smoothRotation, targetRotation, 10f * Time.deltaTime);
		float smoothConditionLerp = Mathf.Clamp01(lerpFraction * 2f - 1f);
		Vector3 smoothPositionConditional = Vector3.Lerp(smoothPosition, targetPosition, smoothConditionLerp);
		Quaternion smoothRotationConditional = Quaternion.Lerp(smoothRotation, targetRotation, smoothConditionLerp);

		wall.position = Vector3.Lerp(inputVector, smoothPositionConditional, lerpFraction);
		wall.rotation = Quaternion.Lerp(outRotation, smoothRotationConditional, lerpFraction);

		Color holorgramColor = lerpFraction == 1? colorValid : colorInvalid;
		mesh.material.SetColor("_ColorA", holorgramColor);
	}
	void SetTargetPositionAndRotation(Vector3 inputVector) {
		center = new Vector3(SnapNumber(inputVector.x, 0), 0, SnapNumber(inputVector.z, 0));
		Vector3 hoz = new Vector3(SnapNumber(inputVector.x, 0), 0, SnapNumber(inputVector.z, 8));
		Vector3 vert = new Vector3(SnapNumber(inputVector.x, 8), 0, SnapNumber(inputVector.z, 0));
		bool hozIsCloser = (inputVector - hoz).sqrMagnitude < (inputVector - vert).sqrMagnitude;
		targetPosition = hozIsCloser ? hoz : vert;
		targetRotation = Quaternion.LookRotation(center - targetPosition);
	}
*/