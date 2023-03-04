using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;


namespace ShepProject {

	public struct Quad {

		//public QuadKeyShort key;

		public float2 position;
		public float halfLength;

		//bucket
		public short startIndex;
		public short endIndex;

		public short BucketSize => (short)((endIndex - startIndex) + 1);

		public Quad(short startIndex, short endIndex) {

			this.startIndex = startIndex;
			this.endIndex = endIndex;
			position= new float2(0, 0);
			halfLength = 0;

		}

		public float Middle(bool zsort) {
			if (zsort) {
				return position.y;
			}
			else {
				return position.x;
			}
		}


		public override string ToString() {


			return "Start: " + startIndex + " End: " + endIndex;
		}

	}

}