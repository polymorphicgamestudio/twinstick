using UnityEngine;
using UnityEngine.UIElements;

public class HUDActionSelectionBar : MonoBehaviour {

	[SerializeField] TowerPlacement placement; // on playermodel
	[SerializeField] RectTransform selectionIndicator;
	Vector2 positionRange = new Vector2(-652.5f, 652.5f);
	float positionGoal, positionCurrent = -652.5f;
	float lerpFraction = 0f;






	//!!!!!!this needs optimizing!
	void Update() {
		lerpFraction = (placement.actionSelectionNumber - 1) / 9f;
		positionGoal = Mathf.Lerp(positionRange.x, positionRange.y, lerpFraction);
		positionCurrent = (9 * positionCurrent + positionGoal) / 10f;
		selectionIndicator.anchoredPosition = new Vector2(positionCurrent, 0);
	}
}