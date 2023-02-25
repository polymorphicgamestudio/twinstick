using ShepProject;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public struct EnemyMovementJob : IJobParallelFor {

	[NativeDisableContainerSafetyRestriction]
	public NativeArray<ushort> objectIDs;
	public NativeArray<ushort> objectQuadIDs;

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

	//attractions for each type of object
	// slash genes
	[NativeDisableContainerSafetyRestriction]
	public NativeArray<float> genes;


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



		float2 direction = (positions[0] - positions[objectIDs[index]]);
		float headingCalculation = math.atan2(direction.x, direction.y);

		//if you change sides when you're behind enemies, they turn the wrong direction

		//float slimeAttraction =
		//	objectIDs[index] * (int)GeneGroups.TotalGeneCount // gets us to the object's type
		//	+ 1 //to get to start of attractions in array
		//	+ (int)Attraction.Slime; //then to finally get us to slime attraction value

		////have direction to the player, now need to check other objects in the bucket
		//float2 localPosition = new float2();
		//float incrementalHeading = 0;

		//for (int i = buckets[objectQuadIDs[index]].startIndex; i <= buckets[objectQuadIDs[index]].endIndex; i++) {


		//	//for now just worrying about slimes
		//	switch (genes[objectIDs[i] * (int)GeneGroups.TotalGeneCount]) {

		//		case (int)ObjectType.Slime: {

		//				//check its attraction to other slime gene
		//				//then based on that and how close they are, turn towards/away

		//				localPosition = (positions[i] - positions[objectIDs[index]]);
		//				incrementalHeading = math.atan2(localPosition.x, localPosition.y);

		//				headingCalculation += (incrementalHeading * (slimeAttraction / 10f));


		//				break;
		//			}

		//	}

		//}




		newHeadings[objectIDs[index]] = math.lerp(headings[objectIDs[index]], headingCalculation, deltaTime);

		float3 temp = math.forward(quaternion.Euler(0, newHeadings[objectIDs[index]], 0)) * (deltaTime * 5);
		direction.x = temp.x;
		direction.y = temp.z;
		positions[objectIDs[index]] += direction;




	}
}
