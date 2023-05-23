using Drawing;
using System;
using System.Collections.Generic;
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
                    setupData.nodeLength,
                    0,
                    index * setupData.nodeLength);

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

        public NativeList<int> finalPathIndices;

        NativeParallelMultiHashMapIterator<int> it;

        public CommandBuilder builder;

        public float3 origin;
        public int columns;
        public int rows;
        public float nodeLength;

        public void Execute()
        {

            int startNodeIndex = 0;
            int endNodeIndex = nodes.Length - 1;

            FindPath(startNodeIndex, endNodeIndex);

            //trace that back


        }


        private void FindPath(int startNodeIndex, int endNodeIndex)
        {

            PathNode currentNode = new PathNode();

            //need to change this to just choose a correct node in a corner
            openNodeDifficulties.Add((int)(currentNode.FCost * 100), currentNode);
            openNodeKeys.Add(currentNode.index, 0);

            fCostKeys.Add((int)(currentNode.FCost * 100));

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
                openNodeDifficulties.Remove((int)(currentNode.FCost * 100), currentNode);

                //Profiler.BeginSample("Check Neighbors");

                if (currentNode.index % columns > 0)
                    //check left node
                    CheckNeighborNode(currentNode.index - 1, ref currentNode, startNodeIndex, endNodeIndex);

                if (currentNode.index % (columns) != columns - 1 && currentNode.index != endNodeIndex)
                    //check right node
                    CheckNeighborNode(currentNode.index + 1, ref currentNode, startNodeIndex, endNodeIndex);

                if ((currentNode.index + columns) < (rows * (columns)) && currentNode.index != endNodeIndex)
                {
                    //check node above
                    CheckNeighborNode(currentNode.index + columns, ref currentNode, startNodeIndex, endNodeIndex);
                }

                if ((currentNode.index - columns) > 0 && currentNode.index != endNodeIndex)
                {
                    //check node below
                    CheckNeighborNode(currentNode.index - columns, ref currentNode, startNodeIndex, endNodeIndex);
                }


                //Profiler.EndSample();

            }

            //Profiler.EndSample();


            //Profiler.BeginSample("Trace Path");

            //int j = 0;
            while (currentNode.index != startNodeIndex)
            {
                finalPathIndices.AddNoResize(currentNode.index);

                closedNodes.TryGetValue(currentNode.parentIndex, out currentNode);
                //j++;

            }
            finalPathIndices.AddNoResize(currentNode.index);
            //j++;

            //Profiler.EndSample();

            //draw final path

            #region Draw Final Path

            //Profiler.BeginSample("Draw Final Path");

            //float3 pos = new float3();
            //for (int k = 0; k < finalPathIndices.Length; k++)
            //{
            //    pos.x = nodes[finalPathIndices[k]].position.x;
            //    pos.z = nodes[finalPathIndices[k]].position.y;
            //    pos += origin;

            //    builder.SolidPlane(pos, new float3(0, 1, 0), new float2(nodeLength), Color.cyan);

            //}

            //Profiler.EndSample();

            #endregion







        }


        void CheckNeighborNode(int index, ref PathNode currentNode, int startNodeIndex, int endNodeIndex)
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
            neighborNode.gCost = math.distance(nodes[index].position, nodes[startNodeIndex].position);
            neighborNode.hCost = math.distance(nodes[index].position, nodes[endNodeIndex].position);

            Profiler.EndSample();

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

                    if (checkNode.hCost < neighborNode.hCost)
                    {

                        openNodeDifficulties.SetValue(checkNode, it);
                        openNodeKeys[checkNode.index] = (int)(checkNode.FCost * 100);
                    }

                }

            }
            else
            {

                openNodeDifficulties.Add((int)(neighborNode.FCost * 100), neighborNode);
                openNodeKeys.Add(neighborNode.index, (int)(neighborNode.FCost * 100));
                if (!fCostKeys.Contains((int)(neighborNode.FCost * 100)))
                {
                    fCostKeys.Add((int)(neighborNode.FCost * 100));
                }

            }



        }


        //private bool ContainsInGrid(float3 position)
        //{
        //    if (position.x > (origin.x + (nodeLength * columns)) ||
        //        position.x < origin.x)
        //    {
        //        return false;

        //    }

        //    if (position.z > (origin.z + ((nodeLength) * rows)) ||
        //        position.z < origin.z)
        //    {
        //        return false;

        //    }


        //    return true;


        //}

        //private int GetNodeIndexFromPosition(float3 position)
        //{

        //    if (!ContainsInGrid(position))
        //    {

        //        Debug.LogError("Position not contained within grid.");

        //        return -1;

        //    }

        //    float3 localPosition = (float3)(position - origin);

        //    //int rowStart = (int)(localPosition.x / setupData.nodeLength);
        //    //int column = (int)(localPosition.z / setupData.nodeLength) * currentColumns;

        //    //int index = rowStart + column;

        //    //debugging purposes only
        //    //float3 pos = setupData.origin + new float3(nodes[index].position.x, 0, nodes[index].position.y);
        //    //Drawing.Draw.Label2D(position, "I: " + index, Color.black);
        //    ////"L: " + localPosition);
        //    //Drawing.Draw.SolidPlane(pos, new float3(0, 1, 0), new float2(setupData.nodeLength), Color.blue);

        //    return (int)(localPosition.x / nodeLength)
        //        + (int)(localPosition.z / nodeLength) * columns;

        //}




    }



}