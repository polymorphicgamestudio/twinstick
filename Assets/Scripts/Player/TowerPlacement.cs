using ShepProject;
using UnityEngine;
using UnityEngine.InputSystem;

public class TowerPlacement : MonoBehaviour {

	[SerializeField] GameObject[] holograms;
	[SerializeField] GameObject[] transitions;
	[SerializeField] GameObject[] towers;

	Vector2 bounds = new Vector2(72, 40);  // (5, 3) * 16 - 8
	Color colorValid = new Color(0.05f, 0.39f, 1f, 0.25f);
	Color colorInvalid = new Color(1f, 0.07f, 0.07f, 0.25f);


	[SerializeField] GameObject hologramWall;
	WallPlacement wallPlacement;
	[SerializeField] GameObject wall;
	[SerializeField] ParticleSystem projectorParticles; // also used as reference for positioning wall hologram;
	GameObject activeWallHologram = null;

	RobotController robotController;
	GameObject currentHologram = null; //tower holograms
	int index = -1;
	int prevIndex = -1;

	bool buildMode = false;
	bool running = false;

	[SerializeField] AudioClip errorSound;

	[HideInInspector] public int actionSelectionNumber = 1; // 1 - 10


	private void Awake() {
		robotController = GetComponent<RobotController>();
	}
	private void Start() {
		ShepGM.inst.actions.Player.Action.performed += Action_performed;
		ShepGM.inst.actions.Player.Run.performed += HideHologramsWhileRunning;
		ShepGM.inst.actions.Player.Run.canceled += ShowHologramsAfterRunning;
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
		if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)) actionSelectionNumber = 1;
		if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2)) actionSelectionNumber = 2;
		if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3)) actionSelectionNumber = 3;
		if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4)) actionSelectionNumber = 4;
		if (Input.GetKeyDown(KeyCode.Alpha5) || Input.GetKeyDown(KeyCode.Keypad5)) actionSelectionNumber = 5;
		if (Input.GetKeyDown(KeyCode.Alpha6) || Input.GetKeyDown(KeyCode.Keypad6)) actionSelectionNumber = 6;
		if (Input.GetKeyDown(KeyCode.Alpha7) || Input.GetKeyDown(KeyCode.Keypad7)) actionSelectionNumber = 7;
		if (Input.GetKeyDown(KeyCode.Alpha8) || Input.GetKeyDown(KeyCode.Keypad8)) actionSelectionNumber = 8;
		if (Input.GetKeyDown(KeyCode.Alpha9) || Input.GetKeyDown(KeyCode.Keypad9)) actionSelectionNumber = 9;
		if (Input.GetKeyDown(KeyCode.Alpha0) || Input.GetKeyDown(KeyCode.Keypad0)) actionSelectionNumber = 0;
		if (Input.GetKeyDown(KeyCode.Q)) actionSelectionNumber--;
		if (Input.GetKeyDown(KeyCode.E)) actionSelectionNumber++;

		if (ShepGM.inst.actions.Player.ActionSelectionDown.triggered) actionSelectionNumber--;
		if (ShepGM.inst.actions.Player.ActionSelectionUp.triggered) actionSelectionNumber++;
		if (actionSelectionNumber > 10) actionSelectionNumber = 1;
		if (actionSelectionNumber < 1) actionSelectionNumber = 10;
	}
	void UpdateBuildModeBasedOnActionSelectionNumber() {
		index = actionSelectionNumber - 4;
		robotController.buildMode = index < holograms.Length && index >= 0;
	}
	void HideHologramsWhileRunning(InputAction.CallbackContext context) {
		running = true;
		if (currentHologram) {
			Destroy(currentHologram);
			currentHologram = null;
		}
		if (activeWallHologram) {
			Destroy(activeWallHologram);
			activeWallHologram = null;
		}
		projectorParticles.Stop();
	}
	void ShowHologramsAfterRunning(InputAction.CallbackContext context) {
		running = false;
		ReplaceHologram();
		if (currentHologram || activeWallHologram) projectorParticles.Play();
	}
	void Action_performed(InputAction.CallbackContext context) {
		if (actionSelectionNumber > 0 && actionSelectionNumber < 4)
			robotController.animController.SetTrigger("Shoot");
		if (currentHologram) {
			PlaceTower();
		}
		if (activeWallHologram) {
			PlaceWall();
		}
	}
	void PlaceTower() {
		if (!ValidTowerLocation(robotController.forwardTilePos)) {
			PlayErrorSound();
			return;
		}
		GameObject tower = Instantiate(transitions[index], robotController.forwardTilePos, Quaternion.identity);
		tower.GetComponent<TowerBuildUpEffect>().Initialize(
			robotController.forwardTilePos, AwayFromPlayer(), 7f, towers[index]);
	}
	void PlaceWall() {
		if (!wallPlacement.validLocation) {
			PlayErrorSound();
			return;
		}
		wallPlacement.PlaceWall();
		activeWallHologram = null;
		ReplaceHologram();
	}
	void PlayErrorSound() {
		ShepGM.inst.player.GetComponent<AudioSource>().PlayOneShot(errorSound);
	}
	bool ValidTowerLocation(Vector3 pos) {
		bool obstructed = Physics.Raycast(pos - Vector3.up, Vector3.up, 3f, LayerMask.GetMask("Tower"));
		bool inBounds = Mathf.Abs(pos.x) <= bounds.x && Mathf.Abs(pos.z) <= bounds.y;
		return !obstructed && inBounds;
	}
	void UpdateHologramColor() {
		foreach (Renderer r in currentHologram.GetComponentsInChildren<Renderer>()) {
			if (ValidTowerLocation(robotController.forwardTilePos))
				r.material.color = colorValid;
			else
				r.material.color = colorInvalid;
		}
	}


	Quaternion AwayFromPlayer() {
		return Quaternion.LookRotation(currentHologram.transform.position - ShepGM.inst.player.position, Vector3.up);
	}
	void UpdateHologramPosition() {
		if (currentHologram) {
			currentHologram.transform.position = robotController.hologramPos;
			UpdateHologramColor();
		}
		if (activeWallHologram) {
			wallPlacement.PositionWall(WallReferencePosition(), WallReferenceRotation());
		}
	}
	void ReplaceHologram() {
		if (currentHologram) {
			Destroy(currentHologram);
			currentHologram = null;
		}
		if (activeWallHologram) {
			Destroy(activeWallHologram);
			activeWallHologram = null;
		}
		if (index < holograms.Length && index >= 0 && !running) {
			robotController.ForceHologramPosUpdate();
			currentHologram = Instantiate(holograms[index], robotController.forwardTilePos, Quaternion.identity);
		}
		if (actionSelectionNumber == 10 && !running) {
			activeWallHologram = Instantiate(hologramWall, WallReferencePosition(), WallReferenceRotation());
			wallPlacement = activeWallHologram.GetComponent<WallPlacement>();
			wallPlacement.InitializeSmoothValues(WallReferencePosition(), WallReferenceRotation());
		}
		BuildModeChange();
	}
	void BuildModeChange() {
		bool newBuildMode = actionSelectionNumber >= 4;
		bool buildModeChanged = newBuildMode != buildMode;
		buildMode = newBuildMode;
		if (!buildModeChanged) return;
		if (buildMode) {
			robotController.animController.SetTrigger("BuildMode");
			projectorParticles.Play();
		}
		if (!buildMode) {
			robotController.animController.SetTrigger("AttackMode");
			projectorParticles.Stop();
		}
	}
	bool IndexChanged() {
		bool indexChanged = index != prevIndex;
		prevIndex = index;
		return indexChanged;
	}
	Vector3 WallReferencePosition() {
		Vector3 wallRefPos = projectorParticles.transform.position + projectorParticles.transform.forward * 5f;
		wallRefPos.y = 0;
		return wallRefPos;
	}
	Quaternion WallReferenceRotation() {
		return Quaternion.Euler(0, projectorParticles.transform.eulerAngles.y, 0);
	}
}