using ShepProject;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TowerPlacement : MonoBehaviour {

	[SerializeField] GameObject[] holograms;
	[SerializeField] GameObject[] transitions;
	[SerializeField] GameObject[] towers;

	RobotController robotController;

	GameObject currentHologram = null;
	int index = -1;
	int prevIndex = -1;

	[HideInInspector] public int actionSelectionNumber = 1; // 1 - 10


	private void Awake() {
		robotController = GetComponent<RobotController>();
	}
	private void Start() {
		ShepGM.inst.actions.Player.Action.performed += Action_performed;
	}
	void Update() {
		UpdateActionSelectionNumber();
		UpdateBuildModeBasedOnActionSelectionNumber();
		if (IndexChanged()) {
			ReplaceHologram();
		}
		UpdateHologramPosition();
	}

	//! this should be updated to use the new input system!
	void UpdateActionSelectionNumber() {
		//Temporary bad way of doing things
		if (Input.GetKeyDown(KeyCode.Alpha1)) actionSelectionNumber = 1;
		if (Input.GetKeyDown(KeyCode.Alpha2)) actionSelectionNumber = 2;
		if (Input.GetKeyDown(KeyCode.Alpha3)) actionSelectionNumber = 3;
		if (Input.GetKeyDown(KeyCode.Alpha4)) actionSelectionNumber = 4;
		if (Input.GetKeyDown(KeyCode.Alpha5)) actionSelectionNumber = 5;
		if (Input.GetKeyDown(KeyCode.Alpha6)) actionSelectionNumber = 6;
		if (Input.GetKeyDown(KeyCode.Alpha7)) actionSelectionNumber = 7;
		if (Input.GetKeyDown(KeyCode.Alpha8)) actionSelectionNumber = 8;
		if (Input.GetKeyDown(KeyCode.Alpha9)) actionSelectionNumber = 9;
		if (Input.GetKeyDown(KeyCode.Alpha0)) actionSelectionNumber = 0;

		if (ShepGM.inst.actions.Player.ActionSelectionDown.triggered) actionSelectionNumber--;
		if (ShepGM.inst.actions.Player.ActionSelectionUp.triggered) actionSelectionNumber++;
		if (actionSelectionNumber > 10) actionSelectionNumber = 1;
		if (actionSelectionNumber < 1) actionSelectionNumber = 10;
	}
	void UpdateBuildModeBasedOnActionSelectionNumber() {
		index = actionSelectionNumber - 4;
		robotController.buildMode = index < holograms.Length && index >= 0;
	}

	void Action_performed(InputAction.CallbackContext context) {
		if (actionSelectionNumber > 0 && actionSelectionNumber < 4)
			robotController.animController.SetTrigger("Shoot");
		if (currentHologram) {
			PlaceTower();
		}
	}
	void PlaceTower() {
		GameObject tower = Instantiate(transitions[index], robotController.forwardTilePos, Quaternion.identity);
		tower.GetComponent<TowerBuildUpEffect>().Initialize(
			robotController.forwardTilePos, AwayFromPlayer(), 7f, towers[index]);
	}
	Quaternion AwayFromPlayer() {
		return Quaternion.LookRotation(currentHologram.transform.position - ShepGM.inst.player.position, Vector3.up);
	}
	void UpdateHologramPosition() {
		if (currentHologram) {
			currentHologram.transform.position = robotController.hologramPos;
		}
	}
	void ReplaceHologram() {
		if (currentHologram) {
			Destroy(currentHologram);
			currentHologram = null;
		}
		if (index < holograms.Length && index >= 0) {
			robotController.ForceHologramPosUpdate();
			currentHologram = Instantiate(holograms[index], robotController.forwardTilePos, Quaternion.identity);
		}
	}
	bool IndexChanged() {
		bool indexChanged = index != prevIndex;
		prevIndex = index;
		return indexChanged;
	}
}