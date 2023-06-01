using UnityEngine;
using System.Collections.Generic;
using Unity.Mathematics;
using Random = UnityEngine.Random;
using UnityEngine.UIElements;

namespace ShepProject
{

	/**************************************************
	 This holds references to things for easy retrieval
	things add and remove themselves in awake / destroy
	**************************************************/
	public class ShepGM : MonoBehaviour
	{


		public static ShepGM inst;

		public PlayerInputActions actions;

		public Transform player;

		[SerializeField]
		private NPCManager npcManager;

		[SerializeField]
		private BuildingManager buildingManager;

		[SerializeField]
		private PathfindingManager pathfindingManager;

		public NPCManager NPCManager => npcManager;

		public event EventTrigger gameOver;

		private void Awake()
		{
			actions = new PlayerInputActions();
			actions.Player.Enable();
			actions.UI.Enable();
			actions.Buildings.Enable();

			if (npcManager != null)
			{
				npcManager.Initialize(this);

			}

			if (pathfindingManager != null)
			{
				pathfindingManager.Initialize(this);
			}

			if (buildingManager != null)
			{
				buildingManager.Initialize(this);

			}


			if (inst == null)
			{
				inst = this;
			}
			else
			{
				Debug.LogError("ShemGM Instance already exists!");
			}
		}

		public void GameOverEventTrigger()
		{
			gameOver.Invoke();
		}



	}

}