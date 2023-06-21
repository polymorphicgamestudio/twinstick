using ShepProject;
using TMPro;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class WallPlacement : BuildingPlacementBase
{

    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private Vector3 smoothPosition;
    private Quaternion smoothRotation;
	private float lerpFraction = 0f;
	[HideInInspector]
	private new BoxCollider collider;
	public BoxCollider Collider => collider;

    protected override void Awake()
    {
		base.Awake();
        mask = LayerMask.GetMask("Wall");
		collider = GetComponent<BoxCollider>();
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
		float clampHozY = 0;
		float clampVertY = 90;
		float clampedY = hozIsCloser ? clampHozY : clampVertY;
		targetRotation = Quaternion.Euler(0f, clampedY, 0f);

		smoothPosition = Vector3.Lerp(smoothPosition, targetPosition, 10f * Time.deltaTime);
		smoothRotation = Quaternion.Lerp(smoothRotation, targetRotation, 20f * Time.deltaTime);

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
        bool inBounds = Mathf.Abs(targetPosition.x) <= manager.bounds.x 
			&& Mathf.Abs(targetPosition.z) <= manager.bounds.y;

		bool isAtTarget = (transform.position - targetPosition)
			.sqrMagnitude < .05f;

		bool isAtRotation = (transform.rotation.eulerAngles - targetRotation.eulerAngles)
			.sqrMagnitude < .01f;


		return !obstructed && inBounds && isAtTarget && isAtRotation;
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