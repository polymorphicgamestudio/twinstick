using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;


namespace ShepProject {


	/// <summary>
	/// First 28 bits are for the key's data
	/// Last 4 bits are for storing key level, since max is 14 quad divisions
	/// </summary>
	public struct QuadKeyShort {


		private byte level;
		private int key;
		public int Key => key;

		public void ForkLeft() {


		}

		public void ForkRight() {

		}

		public int GetKeyLevel() {


			return level;
		}

		//public float GetLength(float topLength) {

		//	float level = math.pow(2,GetKeyLevel());

		//	return topLength /= level;

		//}

		//public float2 Center(float2 topOrigin) {

		//	//for (int i = 0; i < level; i++) {

		//	//	byte value = (level << (31 - i))

		//	//	bool temp = level ? 1 : 0;


		//	//}


		//}


	}

}