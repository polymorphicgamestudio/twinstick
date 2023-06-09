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
		private NPCManager npcs;

		[SerializeField]
		private BuildingManager buildings;

		[SerializeField]
		private PathfindingManager pathfinding;


        [SerializeField]
        private InputManager input;


		public NPCManager NPCS => npcs;
		public PathfindingManager Pathfinding => pathfinding;
		public InputManager Input => input;


		public event EventTrigger gameOver;

		public Canvas canvas;
		public EventSystem eventSystem;
		GraphicRaycaster graphicRaycaster;

		private void Awake() {
			actions = new PlayerInputActions();
			actions.Player.Enable();
			actions.UI.Enable();
			actions.Buildings.Enable();

			if (npcs != null) {
				npcs.Initialize(this);
			}

			if (pathfinding != null) {
				pathfinding.Initialize(this);
			}

			if (buildings != null) {
				buildings.Initialize(this);
			}


			if (inst == null) {
				inst = this;
			}
			else {
				Debug.LogError("ShepGM Instance already exists!");
			}
			if (canvas == null)
			{
				Debug.LogError("Canvas not set in ShepGM, needed for towers and shooting!");

			}
			else
			{
				graphicRaycaster = canvas.GetComponent<GraphicRaycaster>();

			}
		}

        private void OnDestroy()
        {

            actions.Dispose();


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