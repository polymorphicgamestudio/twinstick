using UnityEngine;
using System.Collections.Generic;

namespace ShepProject {

	/**************************************************
	 This holds references to things for easy retrieval
	things add and remove themselves in awake / destroy
	**************************************************/
	public class ShepGM : MonoBehaviour {


		public static ShepGM inst;

		public PlayerInputActions actions;

		public static Transform player;
		static List<Transform>[] things = new List<Transform>[(int)Thing.Count];

		[SerializeField]
		private EnemyManager enemyManager;

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

			enemyManager.Initialize(this);

			if (inst == null) {
				inst = this;
			}
			else {
				Debug.LogError("ShemGM Instance already exists!");
			}


		}


		static ShepGM() {
			for (int q = 0; q < things.Length; q++) {
				things[q] = new List<Transform>();
			}
		}

		public static List<Transform> GetList(Thing thing) {
			return things[(int)thing];
		}

		public static int GetCount(Thing thing) {
			return GetList(thing).Count;
		}

		public static Transform GetRandom(Thing thing) {
			return GetList(thing)[Random.Range(0, GetCount(thing))];
		}

		public static Transform GetNearest(Thing thing, Vector3 point) {
			if (GetCount(thing) > 0) return GetNearestFromList(GetList(thing), point);
			return null;
		}

		public static Transform GetNearestFromList(List<Transform> list, Vector3 point) {
			float nearestDistance = float.MaxValue;
			Transform nearestObject = null;
			for (int i = 0; i < list.Count; i++) {
				float distance = Vector3.Distance(list[i].transform.position, point);
				if (distance < nearestDistance) {
					nearestDistance = distance;
					nearestObject = list[i];
				}
			}
			return nearestObject;
		}
	}

}