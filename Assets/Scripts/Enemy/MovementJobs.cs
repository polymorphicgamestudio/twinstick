using ShepProject;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEditor.Experimental.GraphView;
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


	public struct AIMovementJob : IJobParallelFor {

		[NativeDisableContainerSafetyRestriction]
		public NativeArray<ushort> objectIDs;
		[NativeDisableContainerSafetyRestriction]
		public NativeArray<ushort> objectQuadIDs;

		[NativeDisableContainerSafetyRestriction]
		public NativeArray<ushort> targetIDs;

		//initial positions for enemies
		[NativeDisableContainerSafetyRestriction]
		public NativeSlice<float2> positions;
		[NativeDisableContainerSafetyRestriction]
		public NativeSlice<Quad> buckets;

		//initial headings
		[NativeDisableContainerSafetyRestriction]
		public NativeSlice<float> headings;

		[NativeDisableContainerSafetyRestriction]
		public NativeSlice<float> newHeadings;


		[NativeDisableContainerSafetyRestriction]
		public NativeArray<ushort> loopCounts;

		//attractions for each type of object
		// slash genes
		[NativeDisableContainerSafetyRestriction]
		public GenesArray genes;


		public float deltaTime;

		public void Execute(int index) {

			/*
			 * 
			 * for now
			 * - chase player
			 * - avoid towers
			 * 
			 * 
			 */

			//player is positions[0] 

			//if this is the player's object
			if (objectIDs[index] == 0)
				return;


			ObjectType objType = (ObjectType)genes[objectIDs[index] * (int)GeneGroups.TotalGeneCount];

			float2 direction = (positions[targetIDs[objectIDs[index]]] - positions[objectIDs[index]]);

			float headingCalculation = math.atan2(direction.x, direction.y);


			if (math.degrees(headingCalculation) < 0)
				headingCalculation += 2 * math.PI;


			if (objType == ObjectType.Sheep) {

				headingCalculation = headings[objectIDs[index]];

			}


			float headingDegrees = math.degrees(headings[objectIDs[index]]);
			float newHeadingDegrees = 0;// math.degrees(headingCalculation);

			float slimeAttraction = genes.GetAttraction(objectIDs[index], Attraction.Slime);
			//float slimeVewDistance = genes.GetViewRange(objectIDs[index], ViewRange.Slime);

			//have direction to the player, now need to check other objects in the bucket
			float2 localPosition = new float2();

			for (int i = buckets[objectQuadIDs[objectIDs[index]]].startIndex;
				i <= buckets[objectQuadIDs[objectIDs[index]]].endIndex; i++) {


				//for now just worrying about slimes
				switch (genes[objectIDs[i] * (int)GeneGroups.TotalGeneCount]) {

					case (int)ObjectType.Slime: {

							/*
							 * check distance of slime
							 * 
							 * if not within a certain distance (squared to save CPU time),
							 *		continue on to the next slime to check
							 * 
							 * 
							 * check whether slime is behind or in front of current slime with dot product
							 * 
							 *		if slime is in front
							 *		
							 * 
							 * adjust heading to go away/towards slime depending on its genes
							 *		
							 * 
							 */


							localPosition = (positions[i] - positions[objectIDs[index]]);
							float sqDist = (math.pow(localPosition.x, 2) + math.pow(localPosition.y, 2));

							//if greater than this distance, ignore and continue on
							if (sqDist > 16) {

								continue;

							}

							float towardsEnemy = math.atan2(localPosition.x, localPosition.y);

							if (towardsEnemy < 0)
								towardsEnemy += 2 * math.PI;

							//if it's between 270-360 and 0-90 then its in front, otherwise its behind

							//if trying to get away, then it should try to turn towards 180 degrees away.
							//Ex. other enemy is at heading 120 degrees
							//		in order to get away, enemy needs to turn towards 300 degrees since it's facing the opposite direction

							//otherwise if they're trying to head towards it, then turn towards the heading that the enemy is at


							float localHeadingDistance = 0;


							if (slimeAttraction < 0) {
								//turn away from the other slime a small fraction, so increase the localHeadingDistance

								towardsEnemy += math.PI;
								towardsEnemy %= (math.PI * 2);

								localHeadingDistance = towardsEnemy - headings[objectIDs[index]];

								/*
								 * 
								 * if angle is negative
								 *		then need to turn to the right to get away from it unless its greater than 180
								 *		
								 * if angle is positive, need to turn to the left to get away from it
								 *		unless it's greater than 180
								 */

								//if between 0 to 180 turn left
								if (localHeadingDistance < -math.PI || (localHeadingDistance > 0 && localHeadingDistance < math.PI)) {
									//turn to the left to get away from it

									headingCalculation += (math.PI / 12f);// * math.lerp();

								}

								//if between 0 to -180 turn right
								else if (localHeadingDistance > math.PI || (localHeadingDistance < 0 && localHeadingDistance > -math.PI)) {
									//turn to the right to get away
									headingCalculation -= (math.PI / 12f);

								}


							}

							break;
						}

				}


			}

			//before lerping, need to also check if they've crossed the 360 to 0 angle or vice versa 


			headingDegrees = math.degrees(headings[objectIDs[index]]);
			newHeadingDegrees = math.degrees(headingCalculation);


			if (headingDegrees > 180 && newHeadingDegrees < 20) {

				headings[objectIDs[index]] -= 2 * math.PI;


			}
			else if (headingDegrees < 180 && newHeadingDegrees > 340) {

				headings[objectIDs[index]] += 2 * math.PI;

			}



			newHeadings[objectIDs[index]] 
				= math.lerp(headings[objectIDs[index]], headingCalculation, deltaTime);




		}


		private void MoveTowardsTarget(int index) {


		}


	}


}















/*

using ShepProject;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEditor.Experimental.GraphView;
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


	public struct AIMovementJob : IJobParallelFor {

		[NativeDisableContainerSafetyRestriction]
		public NativeArray<ushort> objectIDs;
		[NativeDisableContainerSafetyRestriction]
		public NativeArray<ushort> objectQuadIDs;

		[NativeDisableContainerSafetyRestriction]
		public NativeArray<ushort> targetIDs;

		//initial positions for enemies
		[NativeDisableContainerSafetyRestriction]
		public NativeSlice<float2> positions;
		[NativeDisableContainerSafetyRestriction]
		public NativeSlice<Quad> buckets;

		//initial headings
		[NativeDisableContainerSafetyRestriction]
		public NativeSlice<float> headings;

		[NativeDisableContainerSafetyRestriction]
		public NativeSlice<float> newHeadings;


		[NativeDisableContainerSafetyRestriction]
		public NativeArray<ushort> loopCounts;

		//attractions for each type of object
		// slash genes
		[NativeDisableContainerSafetyRestriction]
		public GenesArray genes;


		public float deltaTime;

		public void Execute(int index) {

			/*
			 * 
			 * for now
			 * - chase player
			 * - avoid towers
			 * 
			 * 
			 

//player is positions[0] 

//if this is the player's object
if (objectIDs[index] == 0)
	return;


ObjectType objType = (ObjectType)genes[objectIDs[index] * (int)GeneGroups.TotalGeneCount];


if (objType == ObjectType.Wall)
	return;
float2 localPosition = (positions[targetIDs[objectIDs[index]]] - positions[objectIDs[index]]);
float newHeadingCalculation = math.atan2(localPosition.x, localPosition.y);


if (objType == ObjectType.Sheep) {

	newHeadingCalculation = headings[objectIDs[index]];
}



float objectAttraction = 0;
//float slimeVewDistance = genes.GetViewRange(objectIDs[index], ViewRange.Slime);

for (int i = buckets[objectQuadIDs[objectIDs[index]]].startIndex;
	i <= buckets[objectQuadIDs[objectIDs[index]]].endIndex; i++) {



	localPosition = (positions[i] - positions[objectIDs[index]]);
	float sqDist = (math.pow(localPosition.x, 2) + math.pow(localPosition.y, 2));

	//if greater than this distance, ignore and continue on
	if (sqDist > 25) {

		continue;

	}

	ObjectType obj = genes.GetObjectType(i);

	//for now just worrying about slimes
	switch (obj) {

		case ObjectType.Slime: {

				objectAttraction = genes.GetAttraction(objectIDs[index], Attraction.Slime);

				if (objectAttraction < 0) {
					//turn away from the other slime a small fraction, so increase the localHeadingDistance

					newHeadingCalculation += TurnAwayFromObject(objectAttraction, localPosition, index);

				}

				break;
			}

		case ObjectType.Wall: {

				//objectAttraction = genes.GetAttraction(objectIDs[index], Attraction.Wall);
				//newHeadingCalculation += TurnAwayFromObject(objectAttraction, localPosition, index);

				break;
			}

	}


}

//this is to weight the target's position a little heavier than localAvoidance
//newHeadingCalculation = math.lerp(newHeadingCalculation, newHeadingCalculation + localAvoidanceCalculation, .3f);

if (math.degrees(newHeadingCalculation) < 0)
	newHeadingCalculation += 2 * math.PI;

float headingDegrees = math.degrees(headings[objectIDs[index]]);
float newHeadingDegrees = math.degrees(newHeadingCalculation);


if (headingDegrees > 180 && newHeadingDegrees < 60) {

	headings[objectIDs[index]] -= 2 * math.PI;


}
else if (headingDegrees < 180 && newHeadingDegrees > 300) {

	headings[objectIDs[index]] += 2 * math.PI;
}
newHeadings[objectIDs[index]] = math.lerp(headings[objectIDs[index]],
newHeadingCalculation,
	deltaTime);


		}


		private float TurnAwayFromObject(float attraction, float2 localPosition, int thisIndex) {

	float angleTowards = math.atan2(localPosition.x, localPosition.y);

	//to make the angle face the opposite direction
	angleTowards += math.PI;
	angleTowards %= (math.PI * 2);
	float changeInAngle = angleTowards - headings[objectIDs[thisIndex]];

	/*
	* 
	* if angle is negative
	*		then need to turn to the right to get away from it unless its greater than 180
	*		
	* if angle is positive, need to turn to the left to get away from it
	*		unless it's greater than 180
	

	//if between 0 to 180 turn left
	if (changeInAngle < -math.PI || (changeInAngle > 0 && changeInAngle <= math.PI)) {
		//turn to the left to get away from it

		return (math.PI / 6f) * math.abs(attraction);

	}

	//if between 0 to -180 turn right
	else //if (changeInAngle > math.PI || (changeInAngle < 0 && changeInAngle > -math.PI))
	{
		//turn to the right to get away

		return -((math.PI / 6f) * math.abs(attraction));
	}

}


	}


}




 */











