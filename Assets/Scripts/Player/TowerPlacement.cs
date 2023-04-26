using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerPlacement : MonoBehaviour {

	[SerializeField] GameObject[] holograms;
	[SerializeField] GameObject[] transitions;
	[SerializeField] GameObject[] towers;

	RobotController robotController;

	GameObject currentHologram = null;
	public int index = -1;
	int prevIndex = -1;



	private void Awake() {
		robotController = GetComponent<RobotController>();
	}

	void Update() {
		if (robotController.buildMode == false)
			index= -1;
		if (IndexChanged()) {
			ReplaceHologram();
		}
    }

	void ExitBuildMode() {
		if (currentHologram) {
			Destroy(currentHologram);
			currentHologram = null;
		}
	}
	void ReplaceHologram() {
		if (currentHologram) {
			Destroy(currentHologram);
			currentHologram = null;
		}
		if (index < holograms.Length && index >= 0) {
			currentHologram = Instantiate(holograms[index - 1]);
		}
	}
	bool IndexChanged() {
		bool indexChanged = index != prevIndex;
		prevIndex = index;
		return indexChanged;
	}
}
