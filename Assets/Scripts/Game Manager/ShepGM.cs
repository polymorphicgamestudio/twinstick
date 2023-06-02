using UnityEngine;
using System.Collections.Generic;
using Unity.Mathematics;
using Random = UnityEngine.Random;
using UnityEngine.UIElements;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ShepProject {

	/**************************************************
	 This holds references to things for easy retrieval
	things add and remove themselves in awake / destroy
	**************************************************/
	public class ShepGM : MonoBehaviour {


		public static ShepGM inst;

		public PlayerInputActions actions;

		public Transform player;

		[SerializeField]
		private AIManager aiManager;

		[SerializeField]
		private BuildingManager buildingManager;

		[SerializeField]
		private PathfindingManager pathfindingManager;

		public AIManager AIManager => aiManager;

		public event EventTrigger gameOver;

		public Canvas canvas;
		public EventSystem eventSystem;
		GraphicRaycaster graphicRaycaster;

		private void Awake() {
			actions = new PlayerInputActions();
			actions.Player.Enable();
			actions.UI.Enable();
			actions.Buildings.Enable();

			if (aiManager != null) {
				aiManager.Initialize(this);
			}

			if (pathfindingManager != null) {
				pathfindingManager.Initialize(this);
			}

			if (buildingManager != null) {
				buildingManager.Initialize(this);
			}


			if (inst == null) {
				inst = this;
			}
			else {
				Debug.LogError("ShemGM Instance already exists!");
			}
			graphicRaycaster = canvas.GetComponent<GraphicRaycaster>();
		}

		public void GameOverEventTrigger() {
			gameOver.Invoke();
		}
		public bool MouseOverHUD() {
			PointerEventData ped = new PointerEventData(eventSystem);
			ped.position = actions.Player.MousePosition.ReadValue<Vector2>();
			List<RaycastResult> results = new List<RaycastResult>();
			graphicRaycaster.Raycast(ped,results);
			return results.Count > 0;
		}
	}
}