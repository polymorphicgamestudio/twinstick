using UnityEngine;
using System.Collections.Generic;
using Unity.Mathematics;
using Random = UnityEngine.Random;
using UnityEngine.UIElements;

namespace ShepProject {

	/**************************************************
	 This holds references to things for easy retrieval
	things add and remove themselves in awake / destroy
	**************************************************/
	public class ShepGM : MonoBehaviour {


		public static ShepGM inst;

		public PlayerInputActions actions;

		public Transform player;
		static List<Transform>[] things = new List<Transform>[(int)Thing.Count];

		[SerializeField]
		private BoxCollider playableArea;
		[SerializeField]
		private BoxCollider wallCollider;

		[SerializeField]
		private EnemyManager enemyManager;

		private PathfindingManager pathfindingManager;

		public EnemyManager EnemyManager => enemyManager;

		public event EventTrigger gameOver;

		public enum Thing {
			Slime,
			Turret,
			SlimeProjectile,
			TurretProjectile,
			PlayerProjectile,
			Count
		}

		
		private void Awake() {
			actions = new PlayerInputActions();
			actions.Player.Enable();
			actions.UI.Enable();

            if (enemyManager != null) {
				enemyManager.Initialize(this);

			}

			if (pathfindingManager != null) {
                pathfindingManager.Initialize(this);
            }

			if (inst == null) {
				inst = this;
			}
			else {
				Debug.LogError("ShemGM Instance already exists!");
			}
		}



        public void GameOverEventTrigger() {
            gameOver.Invoke();
        }

        static ShepGM() {
            for (int q = 0; q < things.Length; q++) {
                things[q] = new List<Transform>();
            }
        }


        private void Start() {
            //generate a wall surrounding the area
            GeneratePlayableAreaWall();

        }

        #region Wall Generation

        public void GeneratePlayableAreaWall() {



			if (playableArea == null) {

				Debug.LogError("Playable Area not set for level.");
				return;
			}

			if (wallCollider == null)
			{
				Debug.LogError("Wall Collider is not assigned from wall prefab.");
				return;
			}

			int xWalls = (int)(playableArea.size.x / wallCollider.size.x);
			int zWalls = (int)(playableArea.size.z / wallCollider.size.x) + 1;


			float3 position = new float3(playableArea.center - (playableArea.size / 2f));
			position.x += wallCollider.size.x;

			GenerateHorizontalWall(position, xWalls);

			position.z += (playableArea.size.z);
			GenerateHorizontalWall(position, xWalls);

			position.x -= wallCollider.size.x / 2f;
			position.z -= wallCollider.size.x / 2f;

			GenerateVerticalWall(position, zWalls);


			position.x += (playableArea.size.x);
			position.x -= wallCollider.size.x / 2f;
			GenerateVerticalWall(position, zWalls);

		}

		private void GenerateHorizontalWall(float3 startPosition, int wallCount) {

			//generating bottom wall
			for (int i = 0; i < wallCount; i++) {

				GameObject inst = Instantiate(wallCollider.gameObject);
				inst.transform.position = startPosition;
				startPosition.x += wallCollider.size.x;

				enemyManager.AddWallToList(inst.transform);

            }

		}

		private void GenerateVerticalWall(float3 startPosition, int wallCount) {

			for (int i = 0; i < wallCount; i++) {

				GameObject inst = Instantiate(wallCollider.gameObject);
                inst.transform.position = startPosition;
				startPosition.z -= wallCollider.size.x;

				Quaternion q = inst.transform.rotation;
				q.eulerAngles += new Vector3(0, 90, 0);
				inst.transform.rotation = q;

				enemyManager.AddWallToList(inst.transform);

            }


		}

        #endregion


	}

}