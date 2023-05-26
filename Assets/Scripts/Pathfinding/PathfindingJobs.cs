using Drawing;
using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;

namespace ShepProject
{

    public struct DrawGridJobNotPlaying : IJobParallelFor
    {

        public SquareGridSetupData setupData;
        public CommandBuilder builder;
        public bool drawLabels;

        public void Execute(int index)
        {
            //index tells which row it is
            DrawRowOfSquares(index);

        }

        private void DrawRowOfSquares(int index)
        {
            float3 position =
                setupData.origin + new float3(
                    setupData.nodeLength / 2f,
                    0,
                    (index * setupData.nodeLength) + setupData.nodeLength / 2f);

            float halfNodeLength = setupData.nodeLength / 2f;
            float3 topLeft = new float3(-halfNodeLength, 0 , halfNodeLength);
            float3 topRight = new float3(halfNodeLength, 0, halfNodeLength);


            for (int i = 0; i < setupData.columns; i++)
            {
                if(index == setupData.rows - 1)//also have to check if unwalkable
                //topLeft to topRight
                builder.Line(position + topLeft, position + topRight, setupData.walkableColor);

                //topRight to bottomRight
                builder.Line(position + topRight, position - topLeft, setupData.walkableColor);

                //bottomRight to bottomLeft
                builder.Line(position - topLeft, position - topRight, setupData.walkableColor);

                //bottomLeft to topLeft
                builder.Line(position - topRight, position + topLeft, setupData.walkableColor);

                if (drawLabels)
                    builder.Label2D(position - new float3(halfNodeLength / 2f, 0 ,0),
                        ((index * setupData.columns) + i).ToString());
                    //"(" + position.x + " , " + position.z + ")");

                position.x += setupData.nodeLength;
            }

        }



    }

    public struct DrawGridJobPlaying : IJobParallelFor
    {
        [NativeDisableContainerSafetyRestriction]
        public NativeArray<SquareNode> nodes;
        public SquareGridSetupData setupData;
        public CommandBuilder builder;
        public bool drawLabels;

        public void Execute(int index)
        {

            float3 position =
            setupData.origin + new float3(nodes[index].position.x, 0, nodes[index].position.y);
            float3 up = new float3(0, 1, 0);

            if (!nodes[index].walkable)
            {
                builder.SolidPlane(position, up, setupData.nodeLength, setupData.unwalkableColor);
                return;
            }


            float halfNodeLength = setupData.nodeLength / 2f;
            float3 topLeft = new float3(-halfNodeLength, 0, halfNodeLength);
            float3 topRight = new float3(halfNodeLength, 0, halfNodeLength);

            if (index > setupData.rows * (setupData.columns - 2))
                builder.Line(position + topLeft, position + topRight, setupData.walkableColor);

            //topRight to bottomRight
            builder.Line(position + topRight, position - topLeft, setupData.walkableColor);

            //bottomRight to bottomLeft
            builder.Line(position - topLeft, position - topRight, setupData.walkableColor);

            //bottomLeft to topLeft
            builder.Line(position - topRight, position + topLeft, setupData.walkableColor);
            if (drawLabels)
                builder.Label2D(position - new float3(halfNodeLength / 2f, 0, 0),
                    index.ToString());

                    //"(" + position.x + " , " + position.z + ")");

        }

    }

    public struct CreateNodesJob : IJobParallelFor
    {

        [NativeDisableContainerSafetyRestriction]
        public NativeArray<SquareNode> nodes;
        public float nodeLength;
        public int columns;
        public int rows;

        public void Execute(int index)
        {

            SquareNode node = new SquareNode();

            for (int i = 0; i < columns; i++)
            {
                node.position.x = (nodeLength * i) + nodeLength / 2f;
                node.position.y = (nodeLength * index) + nodeLength / 2f; 
                nodes[(columns * index) + i] = node;

            }


        }
    }

    public struct UpdateNodeInfoJob : IJobParallelFor
    {
        [NativeDisableContainerSafetyRestriction]
        public NativeArray<SquareNode> nodes;
        public float nodeLength;
        public int columns;
        public int rows;

        public void Execute(int index)
        {
            SquareNode node = new SquareNode();

            for (int i = 0; i < columns; i++)
            {
                node.position.x = (nodeLength * i) + nodeLength / 2f;
                node.position.y = (nodeLength * index) + nodeLength / 2f;
                nodes[(columns * index) + i] = node;

            }
        }
    }


    public struct CreateOverlapCommandsJob : IJobParallelFor
    {
        public NativeArray<OverlapBoxCommand> commands;
        public NativeArray<SquareNode> nodes;
        public Vector3 origin;


        public int mask;
        public int rows;
        public int columns;
        public float halfNodeLength;

        public void Execute(int index)
        {

            OverlapBoxCommand command = new OverlapBoxCommand();
            command.queryParameters.layerMask = mask;
            command.queryParameters.hitTriggers = QueryTriggerInteraction.Collide;
            command.halfExtents = new Vector3(halfNodeLength, halfNodeLength, halfNodeLength);
            command.center = origin + new Vector3(nodes[index].position.x, 0, nodes[index].position.y);

            commands[index] = command;

           

        }

    }


    public struct UpdateWalkableNodesJob : IJobParallelFor
    {

        public NativeArray<SquareNode> nodes;
        public NativeArray<ColliderHit> overlapResults;

        public void Execute(int index)
        {
            SquareNode node = nodes[index];

            if (overlapResults[index].instanceID != 0)
            {
                //if collider is to the right, then set right connection
                //if collider continue top right, 

                node.walkable = false;
            }
            else
            {
                node.walkable = true;
            }

            nodes[index] = node;

        }
    }

    public struct FindPathsJob : IJobParallelFor
    {

        public NativeArray<SquareNode> nodes;


        public void Execute(int index)
        {

        }

    }

    public struct GenerateVectorFieldJob : IJob
    {

        public NativeArray<SquareNode> nodes;

        public NativeParallelMultiHashMap<int, PathNode> openNodeDifficulties;
        public NativeParallelHashMap<int, PathNode> closedNodes;
        public NativeParallelHashMap<int, int> openNodeKeys;

        public NativeList<int> fCostKeys;
        public NativeArray<PathNode> finalPathIndices;
        public NativeArray<byte> vectorField;
        public NativeArray<bool> vectorPathsFilled;

        NativeParallelMultiHashMapIterator<int> it;

        public CommandBuilder builder;

        public float3 origin;
        public int columns;
        public int rows;
        public float nodeLength;
        public int scalar;
        public bool drawNodeInfo;

        public int startNodeIndex;
        public int endNodeIndex;

        int searched;
        int pathLength;

        public void Execute()
        {

            //int startNodeIndex = 0;
            //int endNodeIndex = nodes.Length - 1;

            //FindPath(startNodeIndex, endNodeIndex);


            #region Vector Field Setup

            bool allFilledIn = true;
            do
            {
                pathLength = 0;
                FindPath(startNodeIndex, endNodeIndex);

                //read from finalPathIndices and update data


                for (int i = 0; i < pathLength; i++)
                {
                    //starts with end node, so for both directions on the path
                    //need to set every node in the chain of that path
                    //ex node 5 of path needs to have all nodes, 1,2,3,4,6,7,8,9 all set

                    //ends up being n^2 operations :(
                    for (int j = 0; j < pathLength; j++)
                    {
                        //if the node that is being set is the same node
                        //that we're setting for
                        if (i == j)
                        {

                            //set it to a value that signifies that there is no direction
                            vectorField[i * (columns * rows) + finalPathIndices[j].index] = (int)NodeDirection.NoMovement;
                            continue;


                        }



                        //vectorField[i * (columns * rows) + finalPathIndices[j].index] = finalPathIndices[i + 1].direction;

                        if (j < i)
                        {

                            //if its before it in the path, it needs the direction reversed
                            //which just needs to be direction - 4 to get opposite for enum

                            vectorField[i * (columns * rows) + finalPathIndices[j].index]
                                = (byte)(finalPathIndices[i - 1].direction - 4);

                        }
                        else
                        {

                            //this is for after in the path
                            vectorField[i * (columns * rows) + finalPathIndices[j].index] = finalPathIndices[i + 1].direction;
                        }
                    }



                }



                //after this need to check which ones don't have paths and then queue up a new path
                //try and get furthest ones away first

                allFilledIn = true;
                for (int i = 0; i < nodes.Length; i++)
                {
                    //first one found that doesn't have everything filled in

                    // check its "full" variable which is after every single actual field,
                    // will be 255 to show that it has no empty spots
                    if (AllPathsFilledIn(i))
                    {

                        continue;

                    }

                    startNodeIndex = i;
                    allFilledIn = false;

                    for (int j = nodes.Length - 1; j >= 0; j--)
                    {
                        //most closest one to the end that doesn't have this current node filled in

                        //then set start and end node paths and rerun the loop

                        //then this will continue until all nodes have all paths

                        //
                        if (CurrentNodeFilledIn(i, j))
                        {
                            continue;
                        }

                        endNodeIndex = j;

                        if (startNodeIndex == endNodeIndex)
                        {
                            Debug.LogError("StartNodeIndex and EndNodeIndex are the same.");
                        }



                        break;

                    }

                    break;


                }


                openNodeDifficulties.Clear();
                closedNodes.Clear();
                openNodeKeys.Clear();
                fCostKeys.Clear();

                //trace that back
            } while (!allFilledIn);

            #endregion


        }


        private void FindPath(int startNodeIndex, int endNodeIndex)
        {
            searched = 0;
            PathNode currentNode = new PathNode();
            currentNode.index = startNodeIndex;
            currentNode.gCost = 0;
            currentNode.hCost = math.distance(nodes[startNodeIndex].position, nodes[endNodeIndex].position);

            float orthagonalCost = nodeLength;
            float diagonalCost = math.sqrt(8);

            //need to change this to just choose a correct node in a corner
            openNodeDifficulties.Add((int)(currentNode.FCost * scalar), currentNode);
            openNodeKeys.Add(currentNode.index, 0);

            fCostKeys.Add((int)(currentNode.FCost * scalar));

            int minKeyCost = int.MaxValue;
            int minKeyCostIndex = 0;

            //Profiler.BeginSample("Checking Nodes");

            while (currentNode.index != endNodeIndex)
            {

                minKeyCostIndex = 0;
                minKeyCost = int.MaxValue;


                //Profiler.BeginSample("Check FCosts");
                for (int i = 0; i < fCostKeys.Length; i++)
                {
                    if (fCostKeys[i] < minKeyCost)
                    {
                        minKeyCost = fCostKeys[i];
                        minKeyCostIndex = i;



                    }

                }

                //Profiler.EndSample();
                if (!openNodeDifficulties.TryGetFirstValue(minKeyCost, out currentNode, out it))
                {
                    fCostKeys.RemoveAt(minKeyCostIndex);

                    continue;
                }


                //has a new currentNode, remove it from open nodes, add it to closed nodes, then check neighbors



                closedNodes.Add(currentNode.index, currentNode);
                openNodeKeys.Remove(currentNode.index);
                openNodeDifficulties.Remove((int)(currentNode.FCost * scalar), currentNode);

                if (drawNodeInfo)
                {

                    Profiler.BeginSample("Neighbor Node Info");
                    float3 poss = new float3(nodes[currentNode.index].position.x, 0, nodes[currentNode.index].position.y);


                    builder.Label2D(origin + poss - new float3(nodeLength / 4f, 0, 0),
                        //"Searched: " + searched +

                        "Index: " + currentNode.index +
                        "\nParent: " + currentNode.parentIndex +
                        "\nDir: " + currentNode.direction +

                        "\nHCost: " + currentNode.hCost +
                        "\nGCost: " + currentNode.gCost +
                        "\nFCost: " + currentNode.FCost,

                        12, Color.black);



                    Profiler.EndSample();

                }



                //Profiler.BeginSample("Check Neighbors");

                if (currentNode.index % columns > 0)
                {
                    //check left node
                    CheckNeighborNode(currentNode.index - 1, 
                        ref currentNode, orthagonalCost, (byte)NodeDirection.Left);

                    if (currentNode.index + columns - 1 < nodes.Length)
                    {
                        //top left
                        CheckNeighborNode((currentNode.index + columns - 1),
                            ref currentNode, diagonalCost, (byte)NodeDirection.TopLeft);
                    }

                    if ((currentNode.index - columns) - 1 >= 0)
                    {
                        //bottom left
                        CheckNeighborNode(((currentNode.index - columns) - 1), 
                            ref currentNode, diagonalCost, (byte)NodeDirection.BottomLeft);

                    }

                }
                if (currentNode.index % (columns) != columns - 1 && currentNode.index != endNodeIndex)
                {
                    //check right node
                    CheckNeighborNode(currentNode.index + 1, 
                        ref currentNode, orthagonalCost, (byte)NodeDirection.Right);

                    if (currentNode.index + columns + 1 < nodes.Length)
                    {
                        //top right
                        CheckNeighborNode((currentNode.index + columns + 1), 
                            ref currentNode, diagonalCost, (byte)NodeDirection.TopRight);

                    }

                    if ((currentNode.index - columns) + 1 >= 0)
                    {
                        //bottom right
                        CheckNeighborNode(((currentNode.index - columns) + 1),
                            ref currentNode, diagonalCost, (byte)NodeDirection.BottomRight);

                    }



                }
                if ((currentNode.index + columns) < (rows * (columns)) && currentNode.index != endNodeIndex)
                {
                    //check node above
                    CheckNeighborNode(currentNode.index + columns, 
                        ref currentNode, orthagonalCost, (byte)NodeDirection.Top);
                }

                if ((currentNode.index - columns) >= 0 && currentNode.index != endNodeIndex)
                {
                    //check node below
                    CheckNeighborNode(currentNode.index - columns, 
                        ref currentNode, orthagonalCost, (byte)NodeDirection.Bottom);
                }


                //Profiler.EndSample();

            }

            //Profiler.EndSample();


            //Profiler.BeginSample("Trace Path");

            //int pathLength = 0;
            while (currentNode.index != startNodeIndex)
            {
                if (pathLength == finalPathIndices.Length - 1)
                {
                    break;
                }

                finalPathIndices[pathLength] = currentNode;

                closedNodes.TryGetValue(currentNode.parentIndex, out currentNode);
                pathLength++;

            }
            finalPathIndices[pathLength] = currentNode;
            pathLength++;

            //Profiler.EndSample();



            Profiler.BeginSample("Draw Closed Nodes");

            NativeArray<PathNode> searchedNodes = closedNodes.GetValueArray(Allocator.Temp);

            float3 position = new float3();
            for (int k = 0; k < searchedNodes.Length; k++)
            {

                if (finalPathIndices.Contains(searchedNodes[k]))
                    continue;

                position.x = nodes[searchedNodes[k].index].position.x;
                position.z = nodes[searchedNodes[k].index].position.y;
                position += origin;

                builder.SolidPlane(position, new float3(0, 1, 0),
                    new float2(nodeLength), Color.red);

            }




            searchedNodes.Dispose();

            Profiler.EndSample();

            Profiler.BeginSample("Draw Open Nodes");

            NativeArray<PathNode> openSearched = openNodeDifficulties.GetValueArray(Allocator.Temp);

            position = new float3();
            for (int k = 0; k < openSearched.Length; k++)
            {

                //if (finalPathIndices.Contains(openSearched[k].index))
                //    continue;

                position.x = nodes[openSearched[k].index].position.x;
                position.z = nodes[openSearched[k].index].position.y;
                position += origin;

                builder.SolidPlane(position, new float3(0, 1, 0),
                    new float2(nodeLength), Color.green);

            }




            openSearched.Dispose();

            Profiler.EndSample();

            //draw final path

            #region Draw Final Path

            Profiler.BeginSample("Draw Final Path");

            float3 pos = new float3();
            for (int k = 0; k < pathLength; k++)
            {
                pos.x = nodes[finalPathIndices[k].index].position.x;
                pos.z = nodes[finalPathIndices[k].index].position.y;
                pos += origin;

                builder.SolidPlane(pos, new float3(0, 1, 0), new float2(nodeLength), Color.cyan);

            }

            Profiler.EndSample();

            #endregion







        }


        void CheckNeighborNode(int index, ref PathNode currentNode, float gCostIncrease, byte direction)
        {

            int outDifficultyKey;
            PathNode checkNode = new PathNode();

            if (index < 0 || index >= nodes.Length)
            {
                Debug.LogError("Index out of range");
                return;
            }

            if (closedNodes.ContainsKey(index))
            {
                return;
            }

            if (!nodes[index].walkable)
            {

                closedNodes.Add(index, new PathNode() { index = index });
                //closedNodes.Add(new PathNode() { index = index });
                return;
            }


            Profiler.BeginSample("Set up neighbor node");

            PathNode neighborNode = new PathNode();
            neighborNode.parentIndex = currentNode.index;
            neighborNode.index = index;
            neighborNode.gCost = currentNode.gCost + gCostIncrease;// math.distance(nodes[index].position, nodes[startNodeIndex].position);
            neighborNode.hCost = math.distance(nodes[index].position, nodes[endNodeIndex].position);
            neighborNode.direction = direction;

            Profiler.EndSample();

            searched++;


            if (neighborNode.index == endNodeIndex)
            {
                currentNode = neighborNode;
            }

            if (openNodeKeys.TryGetValue(index, out outDifficultyKey))
            {
                if (openNodeDifficulties.TryGetFirstValue(outDifficultyKey, out checkNode, out it))
                {
                    while (!checkNode.Equals(neighborNode))
                    {
                        openNodeDifficulties.TryGetNextValue(out checkNode, ref it);

                    }

                    if (neighborNode.gCost < checkNode.gCost)
                    {

                        openNodeDifficulties.Remove(outDifficultyKey, checkNode);

                        openNodeKeys[neighborNode.index] = (int)(neighborNode.FCost * scalar);
                        openNodeDifficulties.Add(openNodeKeys[neighborNode.index], neighborNode);

                        //expensive, need to get rid of this.
                        //change this out for acheck with the openNodeDifficulties
                        if (!fCostKeys.Contains(openNodeKeys[neighborNode.index]))
                        {
                            fCostKeys.Add(openNodeKeys[neighborNode.index]);
                        }

                    }


                }

            }
            else
            {

                openNodeDifficulties.Add((int)(neighborNode.FCost * scalar), neighborNode);
                openNodeKeys.Add(neighborNode.index, (int)(neighborNode.FCost * scalar));
                if (!fCostKeys.Contains((int)(neighborNode.FCost * scalar)))
                {
                    fCostKeys.Add((int)(neighborNode.FCost * scalar));
                }

            }



        }

        bool AllPathsFilledIn(int nodeIndex)
        {

            if (vectorPathsFilled[nodeIndex])
            {
                return true;
            }

            for (int i = 0; i < (columns * rows); i++)
            {

                if (vectorField[(nodeIndex * (columns * rows)) + i] == 0)
                {
                    return false;
                }


            }

            vectorPathsFilled[nodeIndex] = true;

            return true;

        }

        bool CurrentNodeFilledIn(int nodeIndex, int nodeCheckIndex)
        {
            if (vectorPathsFilled[nodeIndex])
            {
                return true;
            }

            if (vectorField[nodeIndex * (columns * rows) + nodeCheckIndex] != 0)
                return true;

            return false;



        }


    }



}