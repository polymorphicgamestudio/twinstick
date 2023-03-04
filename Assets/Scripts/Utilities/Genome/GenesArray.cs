using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;


namespace ShepProject {


	public enum GeneGroups {

		Type,
		Attractions,
		ViewRanges,
		Resistances,
		Speed,
		TurnRate,
		TotalGeneCount =
			1 // object type
			+ (int)Attraction.Count //for all the possible attractions an object can have
			+ (int)ViewRange.Count 
			+ (int)Resistance.Count
			+ 1 // for the speed
			+ 1 // for the turn rate

	}

	public enum Attraction {
		Slime = 0,
		Sheep,
		BaseTower,
		BlasterTower,
		FireTower,
		AcidTower,
		LightningTower,
		IceTower,
		LaserTower,
		Count
	}

	public enum ViewRange {

		Slime,
		Tower,
		Player,
		Count
	}

	public enum Resistance {

		Player,
		Blaster,
		Count

	}

	public enum ObjectType {
		Player,
		Slime,
		Sheep,
		BlasterTower,
		FireTower,
		AcidTower,
		LightningTower,
		IceTower,
		LaserTower,
		Wall

	}


	public struct GenesArray {

		[NativeDisableContainerSafetyRestriction]
		private NativeArray<float> genes;

		public float this[int index] { get { return genes[index]; } }


		public GenesArray(int genesCount, Allocator type) {

			genes = new NativeArray<float>(genesCount, type);

		}


		private int IDTypeIndex(int id) {

			return id * (int)GeneGroups.TotalGeneCount;
		}

		public float GetAttraction(int id, Attraction attraction) {

			return genes[IDTypeIndex(id) + 1 + (int)attraction];
		}
		public void SetAttraction(int id, Attraction attraction, float value) {

			genes[IDTypeIndex(id) + 1 + (int)attraction] = value;

		}



		public float GetViewRange(int id, ViewRange range) {

			return genes[IDTypeIndex(id) + 1 + (int)Attraction.Count + (int)range];
		}

		public void SetViewRange(int id, ViewRange range, float value) {

			genes[IDTypeIndex(id) + 1 + (int)Attraction.Count + (int)range] = value;
		}


		public ObjectType GetObjectType(int id) {

			return (ObjectType)IDTypeIndex(id);

		}

		public void SetObjectType(int id, ObjectType type) {

			genes[IDTypeIndex(id)] = (int)type;

		}






		public float GetSpeed(int id) {
			return 0f;
		}

		/// <summary>
		/// NO-OP
		/// </summary>
		/// <returns></returns>
		public float GetResistance() {

			return 0f;	
		}

		/// <summary>
		/// NO-OP
		/// </summary>
		/// <returns></returns>
		public float GetTurnRate() {

			return 0f;
		}

		public void Dispose() {

			genes.Dispose();
		}



	}

}