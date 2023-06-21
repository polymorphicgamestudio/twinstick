using ShepProject;
using TMPro;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class WallPlacement : BuildingPlacementBase
{

	Vector2 bounds = new Vector2(72,40);  // (5, 3) * 16 - 8

	Vector3 targetPosition;
	Quaternion targetRotation;
	Vector3 smoothPosition = Vector3.zero;
	Quaternion smoothRotation = Quaternion.identity;
	float lerpFraction = 0f;

	[SerializeField] GameObject wall;
    //float buildTime = 7f;


    protected override void Awake()
    {
		base.Awake();
        mask = LayerMask.GetMask("Wall");

    }

    public override void PlacementUpdate(BuildingManager manager)
    {
        base.PlacementUpdate(manager);

		Vector3 pos = manager.ModeController.WallReferencePosition();
		Quaternion rot = manager.ModeController.WallReferenceRotation();


		Vector3 hoz = new Vector3(SnapNumber(pos.x, 0), 0, SnapNumber(pos.z, 8));
		Vector3 vert = new Vector3(SnapNumber(pos.x, 8), 0, SnapNumber(pos.z, 0));
		bool hozIsCloser = (pos - hoz).sqrMagnitude < (pos - vert).sqrMagnitude;
		targetPosition = hozIsCloser ? hoz : vert;

		float referenceY = rot.eulerAngles.y;
		float clampHozY = referenceY > 270 || referenceY < 90 ? 0 : 180;
		float clampVertY = referenceY < 180 ? 90 : 270;
		float clampedY = hozIsCloser ? clampHozY : clampVertY;
		targetRotation = Quaternion.Euler(0f, clampedY, 0f);

		smoothPosition = Vector3.Lerp(smoothPosition, targetPosition, 10f * Time.deltaTime);
		smoothRotation = Quaternion.Lerp(smoothRotation, targetRotation, 10f * Time.deltaTime);

		lerpFraction = Mathf.Clamp01((64 - (pos - targetPosition).sqrMagnitude) / 50);
		transform.position = Vector3.Lerp(pos, smoothPosition, lerpFraction);
		transform.rotation = Quaternion.Lerp(rot, smoothRotation, lerpFraction);



	}

	float SnapNumber(float num, float offset) 
	{
		return Mathf.Round((num - offset) / 16.0f) * 16 + offset;
	}

    public override bool IsValidLocation(BuildingManager manager)
    {
        bool obstructed 
			= Physics.Raycast(targetPosition - Vector3.up, Vector3.up, 3f, mask);
        bool onGrid 
			= lerpFraction == 1;
        bool inBounds 
			= Mathf.Abs(targetPosition.x) <= bounds.x && Mathf.Abs(targetPosition.z) <= bounds.y;

		bool isAtTarget = (transform.position - targetPosition).sqrMagnitude < .05f;
		bool isAtRotation = (transform.rotation.eulerAngles - targetRotation.eulerAngles).sqrMagnitude < .01f;


		return !obstructed && onGrid && inBounds && isAtTarget && isAtRotation;
    }

    public override void InitialPlacement(BuildingManager manager)
    {
        transform.position = manager.ModeController.WallReferencePosition();
        transform.rotation = manager.ModeController.WallReferenceRotation();

    }
}

/* backup... old method where rotation was based on center of walled square

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