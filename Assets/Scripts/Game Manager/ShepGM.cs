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

		public Transform player;
        public AudioSource playerAudioSource;


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
        public event EventTrigger<int> startOfWave;
        public event EventTrigger<int> endOfWave;

		private void Awake() 
		{

            if (npcs != null)
            {
                npcs.Initialize(this);
            }

            if (pathfinding != null)
            {
                pathfinding.Initialize(this);
            }

            if (buildings != null)
            {
                buildings.Initialize(this);
            }

            if (input != null)
            {
                input.Initialize(this);
            }


            if (inst == null)
            {
                inst = this;
            }
            else
            {
                Debug.LogError("ShepGM Instance already exists!");
            }

        }



        private void Update()
        {


            input.ManualUpdate();

            if (npcs != null)
            {
                npcs.ManualUpdate();
                buildings.ManualUpdate();
                pathfinding.ManualUpdate();

            }

        }

        public void StartOfWaveEventTrigger(int waveEndedNumber)
        {
            startOfWave?.Invoke(waveEndedNumber);
        }

        public void EndOfWaveEventTrigger(int waveEndedNumber)
        {
            endOfWave?.Invoke(waveEndedNumber);
        }

        public void GameOverEventTrigger() 
		{
			gameOver.Invoke();
		}

	}
}