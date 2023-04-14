using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

namespace ShepProject {


	public struct ChooseTargetJob : IJobParallelFor {

		[NativeDisableContainerSafetyRestriction]
		public NativeList<ushort> choosableTargets;

		[NativeDisableContainerSafetyRestriction]
		public NativeArray<ushort> targetIDs;
		[NativeDisableContainerSafetyRestriction]
		public NativeSlice<float2> positions;

		[NativeDisableContainerSafetyRestriction]
		public NativeArray<ushort> objectIDs;


		public void Execute(int index) {


			//if there is a target, return
			if (targetIDs[objectIDs[index]] != ushort.MaxValue)
				return;

			//else if no target, check to see which is the closest and start attackingS

			float minDistSq = 1000000;
			float current = 0;

			for (ushort i = 0; i < choosableTargets.Length; i++) {

				current = math.distancesq(positions[objectIDs[index]], positions[choosableTargets[i]]);

				if (current < minDistSq) {

					minDistSq = current;
					targetIDs[objectIDs[index]] = choosableTargets[i];

				}


			}

			


		}
	}


	/*
	 * 
	 * want to have an optimal distance for slimes and towers
	 *	both will be different, will have them as genes
	 *	
	 * when less than an optimal distance, will move away
	 * 
	 * when greater than an optimal distance, will move towards
	 * 
	 * 
	 * 
	 */

	public struct AIMovementJob : IJobParallelFor {

		[NativeDisableContainerSafetyRestriction]
		public NativeArray<ushort> objectIDs;
		[NativeDisableContainerSafetyRestriction]
		public NativeArray<ushort> objectQuadIDs;

		[NativeDisableContainerSafetyRestriction]
		public NativeArray<ushort> targetIDs;


		[NativeDisableContainerSafetyRestriction]
		public NativeArray<float2> positions;
		[NativeDisableContainerSafetyRestriction]
		public NativeSlice<Quad> buckets;

        [NativeDisableContainerSafetyRestriction]
        public NativeParallelHashMap<QuadKey, Quad> quads;

        [NativeDisableContainerSafetyRestriction]
        public NativeArray<byte> neighborCounts;
        [NativeDisableContainerSafetyRestriction]
        public NativeArray<QuadKey> objectNeighbors;

		public int maxNeighborCount;

        //initial headings
        [NativeDisableContainerSafetyRestriction]
		public NativeSlice<float> headings;

		[NativeDisableContainerSafetyRestriction]
		public GenesArray genes;


		public float deltaTime;

		public void Execute(int index) {


			//checking for player ID, since player is always the first thing spawned
			if (objectIDs[index] == 0)
				return;

			int objectID = objectIDs[index];

			ObjectType objType = genes.GetObjectType(objectIDs[index]);

			if (objType == ObjectType.Wall)
				return;

			float2 moveTowards = new float2();

			if (objType != ObjectType.Sheep) {

				moveTowards = (positions[targetIDs[objectIDs[index]]] - positions[objectIDs[index]]) // 500f);
					* 10; //instead of hardcoded number will use the sheep attraction variable

			}

			moveTowards += SearchBucket(index, objType, buckets[objectQuadIDs[objectIDs[index]]].key);

			for (int i = 0; i < neighborCounts[objectIDs[index]]; i++) {
				moveTowards += SearchBucket(index, objType, objectNeighbors[objectIDs[index] * maxNeighborCount + i]);

			}


			float headingCalculation = math.atan2(moveTowards.x, moveTowards.y);


			if (headingCalculation < 0) {

				headingCalculation += 2 * math.PI;
			}

			float headingDegrees = math.degrees(headings[objectIDs[index]]);
			float newHeadingDegrees = math.degrees(headingCalculation);

			float local = newHeadingDegrees - headingDegrees;

			if (local < -180) {

				//turn right
				headings[objectIDs[index]] -= 2 * math.PI;
			}
			else if (local > 180) {
				//turn left
				headings[objectIDs[index]] += 2 * math.PI;
			}

			headings[objectIDs[index]] 
				//= headingCalculation;
				= math.lerp(headings[objectIDs[index]], headingCalculation, deltaTime * 6);


		}


		private float2 SearchBucket(int index, ObjectType objType, QuadKey key) {

			float maxDist = 8;
			float maxDistSq = maxDist * maxDist;
			float2 localPosition = new float2();
			float2 moveTowards = new float2();

			Quad current = new Quad();
			current = quads[key];

			if (current.startIndex < 0)
				return moveTowards;

			for (int i = current.startIndex; i <= current.endIndex; i++) {

				//to ignore itself in all calculations
				if (objectIDs[i] == objectIDs[index])
					continue;

				localPosition = (positions[objectIDs[i]] - positions[objectIDs[index]]);


				//for debugging rays only
				Vector3 pos = new Vector3();
				pos.x = positions[objectIDs[index]].x;
				pos.y = 1;
				pos.z = positions[objectIDs[index]].y;

				Vector3 local = new Vector3();


				float sqDist = (math.pow(localPosition.x, 2) + math.pow(localPosition.y, 2));

				//if greater than this distance, ignore and continue on
				if (sqDist > maxDistSq) {

					continue;
				}


				//that divided by maxDist to get the scaledVector
				float angle = math.atan2(localPosition.y, localPosition.x);
					//SignedAngle(positions[objectIDs[index]], positions[objectIDs[i]]));
				float degrees = math.degrees(angle);

				float2 one = new float2(math.cos(angle), math.sin(angle));

				Debug.DrawRay(pos, new float3(one.x, 0, one.y), Color.yellow);

				localPosition /= maxDist;

				//Debug.DrawLine(pos + new Vector3(one.x, 0, one.y),
					//pos + new Vector3(one.x, 0, one.y) - new Vector3(localPosition.x, 0, localPosition.y), Color.magenta);

				one -= localPosition;
				//then 1 - (localPosition percent to max distance) is then the strength of the force that it needs to repel

				//an optimal distance
				if (genes.GetObjectType(objectIDs[i]) == ObjectType.Slime
					&& objType == ObjectType.Slime) {


					float optimalDist = genes.GetSlimeOptimalDistance(objectIDs[index]);
					if (sqDist < (optimalDist * optimalDist)) {


						//slimes are too close, so get further away
						moveTowards -= one * 10;
						// math.normalize(localPosition) * math.lerp(1, 64, 1 - distancePercent);

						//* math.lerp(50, 3, (sqDist / (optimalDist * optimalDist)));


						local.x = one.x;
						local.z = one.y;
						local.y = 0;
						

						//local /= 20f;
						Debug.DrawRay(pos, local, Color.red);
						
					}
					else {

						//slimes are too far, so get closer
						moveTowards += one * 5;

						//moveTowards += math.normalize(localPosition) * math.lerp(1, 64, 1 - distancePercent);

						//localPosition * math.lerp(1, 2f, 1 - ((optimalDist * optimalDist) / sqDist));

						//local = (localPosition * math.lerp(1, 64, 1 - distancePercent)).xxy;
						//local.y = 0;

						//local /= 20f;
						//Debug.DrawRay(pos, local, Color.green);
					}






				}

				//else {
				//moveTowards += (localPosition
				////attraction level for object type, will mirror vector if wants to get away
				//* genes.GetAttraction(objectIDs[index], (int)genes.GetObjectType(objectIDs[i]))
				////distance falloff, further away means it cares less
				//* math.lerp(.5f, 2f, 1 - (sqDist / maxDistSq)));
				//}




			}



			return moveTowards;
		}


	}


}



