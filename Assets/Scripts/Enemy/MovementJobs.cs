using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using static System.Collections.Specialized.BitVector32;

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

			//else if no target, check to see which is the closest and start attacking

			float minDistSq = 1000000;
			float current = 0;

			for (ushort i = 0; i < choosableTargets.Length; i++) {

				if (choosableTargets[i] == ushort.MaxValue)
					continue;

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

    public struct DetermineSheepAnimationStateJob : IJobParallelFor
    {

        public void Execute(int index)
        {
            throw new NotImplementedException();
        }

    }


    public struct GatherForcesWithinRangeJob : IJobParallelFor
    {

        /* 
		 * for each agent, search within its given range for a type of object
		 * 
		 */

        [NativeDisableContainerSafetyRestriction]
        public NativeArray<ushort> objectIDs;

        [NativeDisableContainerSafetyRestriction]
        public NativeArray<ushort> targetIDs;

        [NativeDisableContainerSafetyRestriction]
        public NativeArray<float2> positions;

        [NativeDisableContainerSafetyRestriction]
        public GenesArray genes;

        [NativeDisableContainerSafetyRestriction]
        public NativeParallelHashMap<QuadKey, Quad> quads;
         
        [NativeDisableContainerSafetyRestriction]
        public NativeArray<float> sheepDistancesToSlime;

        [NativeDisableContainerSafetyRestriction]
        public NativeArray<float2> objectForces;

		public ObjectType targetType;


        public void Execute(int index)
        {

            int objectID = objectIDs[index];

			//checking for player ID, since player is always the first thing spawned
			if (objectID == 0)
                return;

			ObjectType objType = genes.GetObjectType(objectID);

            if (objType == ObjectType.Wall || objType == ObjectType.Tower)
                return;

            //now at this point, should only be sheep and slimes

            if (objType == ObjectType.Slime && targetType == ObjectType.Sheep)
            {

                objectForces[(objectID * (int)ObjectType.Count) + (int)targetType]
                    = math.normalize(positions[targetIDs[objectID]] - positions[objectID])
                    * genes.GetAttraction(objectID, Attraction.Sheep);


            }
            else if (objType == ObjectType.Sheep && targetType == ObjectType.Slime)
            {
                if (targetType == ObjectType.Slime)
                {
                    sheepDistancesToSlime[(objectID)] =
                        SearchChildrenForClosestObjectDistance(quads[new QuadKey()].key, objectID, ObjectType.Slime, 8);
                }
            }

            //now search all the buckets including neighbors
            //will only be the bucket 

            SearchChildrenForForce(quads[new QuadKey()].key, objectID, objType);


        }

        private void SearchChildrenForForce(QuadKey parentKey, int objectID, ObjectType objType)
        {
            //this will search through all child quads for bottom level quads and then check if those quads 
            //have the required objects inside of them and a search of the bucket will be required

            if (!quads[parentKey].ContainsObjectType(objType))
                return;

            QuadKey checkKey = parentKey;
            float maxDistance = genes.GetViewRange(objectID, ViewRange.Tower);

            //slime max distance
            if (8 > maxDistance) 
            {
                maxDistance = 8;
            }


            if (!quads[parentKey].key.IsDivided)
            {
                //check this quad for the required object

                //need to update return type from float2 to objectForces
                objectForces[(objectID * (int)ObjectType.Count) + (int)targetType] += 
                    //(objectForces[(objectID * (int)ObjectType.Count) + (int)targetType]
                    GatherBucketForces(objectID, objType, parentKey);

                return;

            }
            //else only check children

            //top left quad
			checkKey = parentKey;
			checkKey.LeftBranch();
			checkKey.RightBranch();
			if (quads[checkKey].IsWithinDistance(positions[objectID], maxDistance)) 
            {
				SearchChildrenForForce(checkKey, objectID, objType);
            }

			checkKey = parentKey;
			checkKey.LeftBranch();
            checkKey.LeftBranch();
            if (quads[checkKey].IsWithinDistance(positions[objectID], maxDistance)) 
            {
				SearchChildrenForForce(checkKey, objectID, objType);
			}

			checkKey = parentKey;
            checkKey.RightBranch();
            checkKey.LeftBranch();
            if (quads[checkKey].IsWithinDistance(positions[objectID], maxDistance)) 
            {
				SearchChildrenForForce(checkKey, objectID, objType);
			}

			checkKey = parentKey;
            checkKey.RightBranch();
            checkKey.RightBranch();
            if (quads[checkKey].IsWithinDistance(positions[objectID], maxDistance)) 
            {
				SearchChildrenForForce(checkKey, objectID, objType);
			}

		}

        private float SearchChildrenForClosestObjectDistance(QuadKey parentKey, int objectID, ObjectType objType, float maxDistance) 
        {

            //top down for searching for this object
            QuadKey checkKey = parentKey;
            float minDistance = 10000;
            float tempMin = 0;
			if (!quads[parentKey].key.IsDivided) {
				//check this quad for the required object

				return SearchBucketForClosestObject(parentKey, objectID, objType, maxDistance);

			}

			//top left quad
			checkKey = parentKey;
			checkKey.LeftBranch();
			checkKey.RightBranch();
			if (quads[checkKey].IsWithinDistance(positions[objectID], maxDistance)) 
            {
                minDistance = SearchChildrenForClosestObjectDistance(checkKey, objectID, objType, maxDistance);
			}

			checkKey = parentKey;
			checkKey.LeftBranch();
			checkKey.LeftBranch();
			if (quads[checkKey].IsWithinDistance(positions[objectID], maxDistance)) 
            {
                tempMin = SearchChildrenForClosestObjectDistance(checkKey, objectID, objType, maxDistance);

                if (tempMin < minDistance)
                    minDistance = tempMin;

			}

			checkKey = parentKey;
			checkKey.RightBranch();
			checkKey.LeftBranch();
			if (quads[checkKey].IsWithinDistance(positions[objectID], maxDistance)) 
            {
				tempMin = SearchChildrenForClosestObjectDistance(checkKey, objectID, objType, maxDistance);

				if (tempMin < minDistance)
					minDistance = tempMin;
			}

			checkKey = parentKey;
			checkKey.RightBranch();
			checkKey.RightBranch();
			if (quads[checkKey].IsWithinDistance(positions[objectID], maxDistance)) 
            {
				tempMin = SearchChildrenForClosestObjectDistance(checkKey, objectID, objType, maxDistance);

				if (tempMin < minDistance)
					minDistance = tempMin;
			}

            return minDistance;

		}

        private float SearchBucketForClosestObject(QuadKey key, int objectID, ObjectType objType, float maxDistance) 
        {


			float minDist = 10000;

			if (!quads[key].ContainsObjectType(objType))
                return minDist;

            float tempMin = 0;
            for (int i = quads[key].startIndex; i <= quads[key].endIndex; i++) 
            {
                if (genes.GetObjectType(objectIDs[i]) != objType) 
                    continue;
                tempMin = math.distancesq(positions[objectIDs[i]], positions[objectID]);

				if (tempMin < minDist)
                    minDist = tempMin;

            }


            return minDist;
        }


		private float2 GatherBucketForces(int objectID, ObjectType objType, QuadKey key)
        {

            float maxDist = 8;
            float maxDistSq = maxDist * maxDist;
            float2 localPosition = new float2();

            float2 moveTowards = new float2();

            Quad current = new Quad();
            current = quads[key];

            if (current.startIndex < 0)
                return moveTowards;

            for (int i = current.startIndex; i <= current.endIndex; i++)
            {

                //to ignore itself in all calculations
                if (objectIDs[i] == objectID)
                    continue;

                //ignore it if it is not the type we're looking for
                if (genes.GetObjectType(objectIDs[i]) != targetType)
                    continue;

                localPosition = (positions[objectIDs[i]] - positions[objectID]);


                //for debugging rays only
                Vector3 pos = new Vector3();
                pos.x = positions[objectID].x;
                pos.y = 1;
                pos.z = positions[objectID].y;

                Vector3 local = new Vector3();

                float sqDist = (math.pow(localPosition.x, 2) + math.pow(localPosition.y, 2));

                //if greater than this distance, ignore and continue on
                if (sqDist > maxDistSq)
                {

                    continue;
                }

                //that divided by maxDist to get the scaledVector
                float angle = math.atan2(localPosition.y, localPosition.x);

                float2 one = new float2(math.cos(angle), math.sin(angle));

                localPosition /= maxDist;
                one -= localPosition;



                //now it is same type, so add to the force based on the attraction

                switch (targetType)
                {
                    case ObjectType.Slime:
                    {
                        if (objType == ObjectType.Slime)
                        {

                            float optimalDist = genes.GetOptimalDistance(objectID, OptimalDistance.Slime);
                            if (sqDist < (optimalDist * optimalDist))
                            {
                                //slimes are too close, so get further away
                                moveTowards -= one * (genes.GetAttraction(objectID, Attraction.Slime) * 4);

                                local.x = one.x;
                                local.z = one.y;
                                local.y = 0;
                                local *= -1;

                                Debug.DrawRay(pos, local, Color.red);

                            }
                            else
                            {

                                //slimes are too far, so get closer
                                moveTowards += one * (genes.GetAttraction(objectID, Attraction.Slime));

                                local.x = one.x;
                                local.z = one.y;
                                local.y = 0;

                                Debug.DrawRay(pos, local, Color.green);
                            }


                        }
                        else
                        {
                            moveTowards -= one * genes.GetAttraction(objectID, Attraction.Slime);


                            local.x = one.x;
                            local.z = one.y;
                            local.y = 0;
                            local *= -1;

                            Debug.DrawRay(pos, local, Color.red);

                        }
                        break;

                    }
                    //case ObjectType.Sheep:
                    //{

                    //    break;
                    //}
                    case ObjectType.Wall:
                    {

                        //everything wants to avoid walls
                        moveTowards -= (one
                            * genes.GetAttraction(objectID, (int)genes.GetObjectType(objectIDs[i])));
                        break;
                    }
                    case ObjectType.Tower:
                    {

                        //slimes want to avoid towers
                        if (objType == ObjectType.Slime)
                        {
                            moveTowards -= (one
                                * genes.GetAttraction(objectID, (int)genes.GetObjectType(objectIDs[i])));

                        }
                        //sheep want to get closer to towers
                        else if (objType == ObjectType.Sheep)
                        {
                            moveTowards += (one
                                * genes.GetAttraction(objectID, (int)genes.GetObjectType(objectIDs[i])));
                        }

                        break;
                    }
                    //case ObjectType.Player:
                    //    break;
                    //case ObjectType.Count:
                    //    break;
                    default:
                        break;
                }


            }



            return moveTowards;
        }

    }

    public struct CalculateHeadingJob : IJobParallelFor
    {


        /*
		 * search for objects in each of the four quadrants
		 * 
		 * 
		 * 
		 */

        [NativeDisableContainerSafetyRestriction]
        public NativeArray<float2> objectForces;
        [NativeDisableContainerSafetyRestriction]
        public NativeArray<float> headings;
        public GenesArray genes;

        public float deltaTime;


        public void Execute(int index)
        {

            ObjectType type = genes.GetObjectType(index);

            if ( type == ObjectType.Sheep)
            {
                int test = 0;
            }

            if (type != ObjectType.Sheep && type != ObjectType.Slime)
                return;


            float2 moveTowards = new float2();
            for (int i = 0; i < (int)ObjectType.Count; i++)
            {
                moveTowards += objectForces[(index * (int)ObjectType.Count) + i];

            }

            float headingCalculation = math.atan2(moveTowards.x, moveTowards.y);


            if (headingCalculation < 0)
            {

                headingCalculation += 2 * math.PI;
            }

            //float headingDegrees = math.degrees(headings[objectIDs[index]]);
            //float newHeadingDegrees = math.degrees(headingCalculation);

            //float local = newHeadingDegrees - headingDegrees;

            float local = headingCalculation - headings[index];

            if (local < -math.PI)
            {

                //turn right
                headings[index] -= 2 * math.PI;
            }
            else if (local > math.PI)
            {
                //turn left
                headings[index] += 2 * math.PI;
            }

            headings[index]
                //= headingCalculation;
                = math.lerp(headings[index], headingCalculation, deltaTime * 2);




        }
    }


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

		public NativeArray<float> sheepDistancesToSlime;

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

				moveTowards = math.normalize(positions[targetIDs[objectIDs[index]]] - positions[objectIDs[index]]) 
					* genes.GetAttraction(objectID, Attraction.Sheep);


			}
			else
			{

				sheepDistancesToSlime[objectIDs[index] - 1] = 10000;
			
			}

			ObjectForces forces = SearchBucket(index, objType, buckets[objectQuadIDs[objectIDs[index]]].key);

			for (int i = 0; i < neighborCounts[objectIDs[index]]; i++)
			{
                forces += SearchBucket(index, objType, objectNeighbors[objectIDs[index] * maxNeighborCount + i]);

			}
			
			float sqDist = (math.pow(forces.SlimesForce.x, 2) + math.pow(forces.SlimesForce.y, 2));
            if (sqDist > math.pow(genes.GetAttraction(objectID, Attraction.Slime), 2))
			{
                //normalization
                forces.SlimesForce /= (math.sqrt(sqDist));
                forces.SlimesForce *= genes.GetAttraction(objectID, Attraction.Slime);

            }

            sqDist = (math.pow(forces.WallsForce.x, 2) + math.pow(forces.WallsForce.y, 2));
            if (sqDist > math.pow(genes.GetAttraction(objectID, Attraction.Wall), 2))
            {
                //normalization
                forces.WallsForce /= (math.sqrt(sqDist));
                forces.WallsForce *= genes.GetAttraction(objectID, Attraction.Wall);

            }

            sqDist = (math.pow(forces.TowersForce.x, 2) + math.pow(forces.TowersForce.y, 2));
            if (sqDist > math.pow(genes.GetAttraction(objectID, Attraction.Tower), 2))
            {
                //normalization
                forces.TowersForce /= (math.sqrt(sqDist));
                forces.TowersForce *= genes.GetAttraction(objectID, Attraction.Tower);

            }

            //sqDist = (math.pow(forces.WallsForce.x, 2) + math.pow(forces.WallsForce.y, 2));
            //if (sqDist > math.pow(genes.GetAttraction(objectID, Attraction.Wall), 2))
            //{
            //    //normalization
            //    forces.WallsForce /= (math.sqrt(sqDist));
            //    forces.WallsForce *= genes.GetAttraction(objectID, Attraction.Wall);

            //}


            moveTowards += forces.SlimesForce;
			moveTowards += forces.WallsForce;

            //float3 position = new float3();
            //position.x = positions[objectID].x;
            //position.z = positions[objectID].y;

            //float3 dir = new float3();
            //dir.x = moveTowards.x;
            //dir.z = moveTowards.y;

            //Debug.DrawRay(position, dir, Color.yellow);

            //
            /*
             * 
             * TO DO NEXT
             * 
			 * towers working with avoidance
			 *		job will only run when divided at least once
			 *		
			 *		each quad will be checked recursively for towers
			 *		each quad will also have the types of objects in it
			 *		ex. bool containsTowers
			 *			bool containsSheep
			 *			bool containsSlime
			 *			bool containsWalls
			 * 
			 *		will have to assign these bools in a separate job
			 * 
			 * as well as sheep being attracted
			 * then start working on between rounds for gene evolution
			 * 
			 */

            float headingCalculation = math.atan2(moveTowards.x, moveTowards.y);


			if (headingCalculation < 0) {

				headingCalculation += 2 * math.PI;
			}

            //float headingDegrees = math.degrees(headings[objectIDs[index]]);
            //float newHeadingDegrees = math.degrees(headingCalculation);

            //float local = newHeadingDegrees - headingDegrees;

            float local = headingCalculation - headings[objectIDs[index]];

            if (local < -math.PI) {

				//turn right
				headings[objectIDs[index]] -= 2 * math.PI;
			}
			else if (local > math.PI) {
				//turn left
				headings[objectIDs[index]] += 2 * math.PI;
			}

			headings[objectIDs[index]] 
				//= headingCalculation;
				= math.lerp(headings[objectIDs[index]], headingCalculation, deltaTime * 2);


		}


		private ObjectForces SearchBucket(int index, ObjectType objType, QuadKey key) {



			float maxDist = 8;
			float maxDistSq = maxDist * maxDist;
			float2 localPosition = new float2();
			//float2 moveTowards = new float2();

			ObjectForces moveTowards = new ObjectForces();

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

				if (objType == ObjectType.Sheep && genes.GetObjectType(objectIDs[i]) == ObjectType.Slime)
				{
                    int ID = objectIDs[index] - 1;

                    if (sheepDistancesToSlime[objectIDs[index] - 1] > sqDist)
					{
						sheepDistancesToSlime[objectIDs[index] - 1] = sqDist;
					}
				}

				//if greater than this distance, ignore and continue on
				if (sqDist > maxDistSq) {

					continue;
				}

				//that divided by maxDist to get the scaledVector
				float angle = math.atan2(localPosition.y, localPosition.x);

				float2 one = new float2(math.cos(angle), math.sin(angle));

				localPosition /= maxDist;
				one -= localPosition;

                switch (genes.GetObjectType(objectIDs[i]))
				{
					case ObjectType.Slime:
					{

						if (objType == ObjectType.Slime)
						{
                            
                            float optimalDist = genes.GetOptimalDistance(objectIDs[index], OptimalDistance.Slime);
                            if (sqDist < (optimalDist * optimalDist))
                            {
                                //slimes are too close, so get further away
                                moveTowards.SlimesForce -= one * (genes.GetAttraction(objectIDs[index], Attraction.Slime) * 4);

                                local.x = one.x;
                                local.z = one.y;
                                local.y = 0;
                                local *= -1;

                                Debug.DrawRay(pos, local, Color.red);

                            }
                            else
                            {

								//slimes are too far, so get closer
								moveTowards.SlimesForce += one * (genes.GetAttraction(objectIDs[index], Attraction.Slime));

								local.x = one.x;
								local.z = one.y;
								local.y = 0;

								Debug.DrawRay(pos, local, Color.green);
							}
						}
						else
						{
                            moveTowards.SlimesForce -= one * genes.GetAttraction(objectIDs[index], Attraction.Slime);


                            local.x = one.x;
                            local.z = one.y;
                            local.y = 0;
                            local *= -1;

                            Debug.DrawRay(pos, local, Color.red);

                        }
						break;
					}
					case ObjectType.Wall:
					{
						//everything wants to avoid walls
                        moveTowards.WallsForce -= (one
							* genes.GetAttraction(objectIDs[index], (int)genes.GetObjectType(objectIDs[i])));
                        break;
                    }
					case ObjectType.Tower:
					{

						//sheep want to move closer to towers, slimes want to move away

                        moveTowards.TowersForce += (one
                            * genes.GetAttraction(objectIDs[index], (int)genes.GetObjectType(objectIDs[i])));
                        break;
					}
                    case ObjectType.Sheep:
                    {
						//everything that can move wants to get closer to sheep
                        moveTowards.SheepForce += (one
                            * genes.GetAttraction(objectIDs[index], (int)genes.GetObjectType(objectIDs[i])));
                        break;
                    }

                }


			}



			return moveTowards;
		}


	}


}



