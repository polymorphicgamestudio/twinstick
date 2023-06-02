using UnityEngine;

namespace ShepProject {

	public class HUDActionSelectionBar : MonoBehaviour {
		[SerializeField] BuildingManager bM;
		[SerializeField] RectTransform selectionIndicator;
		Vector2 positionRange = new Vector2(-720f, 548.6f);
		float positionGoal, positionCurrent = -720f;
		float lerpFraction = 0f;

		//!!!!!!this needs optimizing!
		void Update() {
			lerpFraction = (bM.actionSelectionNumber - 1) / 9f;
			positionGoal = Mathf.Lerp(positionRange.x, positionRange.y, lerpFraction);
			positionCurrent = (9 * positionCurrent + positionGoal) / 10f;
			selectionIndicator.anchoredPosition = new Vector2(positionCurrent, 9);
		}
		public void SetSelection(int selectionNumber) {
			bM.SetStateFromActionSelectionNumber(selectionNumber);
		}
	}
}