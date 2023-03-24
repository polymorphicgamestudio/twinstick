using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.VisualScripting;


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


			ObjectType objType = genes.GetObjectType(objectIDs[index]);

			if (objType == ObjectType.Wall)
				return;

			float2 moveTowards = new float2();

			if (objType != ObjectType.Sheep)
				moveTowards = (positions[targetIDs[objectIDs[index]]] - positions[objectIDs[index]]) * 10;

			float maxDist = 16;


			moveTowards += SearchBucket(index, buckets[objectQuadIDs[objectIDs[index]]].key, maxDist);

			for (int i = 0; i < neighborCounts[index]; i++)
			{
				moveTowards += SearchBucket(index, objectNeighbors[index * maxNeighborCount + i], maxDist);

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
				= math.lerp(headings[objectIDs[index]], headingCalculation, deltaTime * 3);


		}


		private float2 SearchBucket(int index, QuadKey key, float maxDist)
		{

			

			float2 localPosition = new float2();
			float2 moveTowards = new float2();

			Quad current = new Quad();
			current = quads[key];

			if (current.startIndex < 0)
				return moveTowards;

            for (int i = current.startIndex; i <= current.endIndex; i++)
            {

                //to ignore itself in all calculationsSS
                if (objectIDs[i] == objectIDs[index])
                    continue;

                localPosition = (positions[objectIDs[i]] - positions[objectIDs[index]]);

                float sqDist = (math.pow(localPosition.x, 2) + math.pow(localPosition.y, 2));

                //if greater than this distance, ignore and continue on
                if (sqDist > maxDist)
                {

                    continue;
				}

                moveTowards += (localPosition
                //attraction level for object type, will mirror vector if wants to get away
                * genes.GetAttraction(objectIDs[index], (int)genes.GetObjectType(objectIDs[i]))
                //distance falloff, further away means it cares less
                * math.lerp(.5f, 2f, 1 - (sqDist / maxDist)));


            }



            return moveTowards;
		}





	}


}



