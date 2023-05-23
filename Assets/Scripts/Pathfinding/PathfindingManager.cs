using Drawing;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UIElements;

namespace ShepProject
{

    public class PathfindingManager : SystemBase
    {

        /*
         * will be square grid
         * 
         * need to set up grid with positions in awake
         *      then set up vector field for all nodes
         *      
         *      
         * need a way to query for the shortest path from 
         *      starting node to destination node using A* algorithm
         *      - one way is using vector field for nodes that already know the shortest path
         *      
         *      - other way will just calculate path from nodes
         *          just in case we need it somewhere
         *      
         * need to queue jobs up in update and then by late update the jobs
         *      will be finished so that they cause any halting
         * 
         * 
         * 
         * setting up vector field
         *      needs to all be within the playable area, otherwise there will be no path
         *      
         *      start with node[0] and then solve all of its paths
         *      then after that is done 
         *      
         *      
         */


        #region Variables

        public Transform gridOrigin;
        public Transform testPosition;

        public Collider playableArea;

        [Space(20)]
        public Transform StartPosition;
        public Transform EndPosition;

        public NativeArray<OverlapBoxCommand> overlapCommands;
        public NativeArray<ColliderHit> overlapResults;
        public NativeArray<SquareNode> nodes;

        public NativeParallelMultiHashMap<int, PathNode> openNodeDifficulties;
        public NativeParallelHashMap<int, PathNode> closedNodes;
        public NativeParallelHashMap<int, int> openNodeKeys;
        public NativeList<int> fCostKeys;

        public NativeList<int> finalPathIndices;

        [Space(20)]
        public SquareGridSetupData setupData;

        private int currentRows;
        private int currentColumns;
        private float currentNodeLength;

        public bool drawLabels;

        #endregion

        #region Gizmos

        private void OnDrawGizmos()
        {

            /*
             * draw the grid
             * nodes when game isn't playing will need to be stored inside a C# array, not a native array
             *      if colors are wanted
             * 
             */

            if (setupData.rows < 1 || setupData.columns < 1)
                return;
            if (!Application.isPlaying)
            {


                setupData.origin = gridOrigin.position;
                DrawGridJobNotPlaying drawGrid = new DrawGridJobNotPlaying();
                drawGrid.setupData = setupData;
                drawGrid.builder = DrawingManager.GetBuilder();
                drawGrid.drawLabels = drawLabels;
                drawGrid.Schedule(setupData.rows, SystemInfo.processorCount).Complete();


                drawGrid.builder.Dispose();

            }
            else
            {

            }




        }

        #endregion

        private void Awake()
        {
            setupData.origin = gridOrigin.position;

            openNodeDifficulties = new NativeParallelMultiHashMap<int, PathNode>(5000, Allocator.Persistent);
            closedNodes = new NativeParallelHashMap<int, PathNode>(5000, Allocator.Persistent);
            openNodeKeys = new NativeParallelHashMap<int, int>(5000, Allocator.Persistent);
            fCostKeys = new NativeList<int>(5000, Allocator.Persistent);
            finalPathIndices = new NativeList<int>(5000, Allocator.Persistent);


            CreateNodes();


        }

        // Update is called once per frame
        void Update()
        {

            setupData.origin = gridOrigin.position;
            if (currentRows != setupData.rows || currentColumns != setupData.columns)
            {
                CreateNodes();

            }

            if (currentNodeLength != setupData.nodeLength)
            {

                UpdateNodeInfo();

            }


            //GetNodeIndexFromPosition(testPosition.position);

            //whenever something is placed on grid or removed, need to update the walkable nodes
            UpdateWalkableNodes();

            Profiler.BeginSample("Find Path");

            FindPath(StartPosition.position, EndPosition.position);

            Profiler.EndSample();

            //Profiler.BeginSample("Setup Vector Field");

            //SetupVectorField();

            //Profiler.EndSample();

            //draw grid every function if gizmos are enabled
            DrawGridJobPlaying drawGrid = new DrawGridJobPlaying();
            drawGrid.setupData = setupData;
            drawGrid.builder = DrawingManager.GetBuilder();
            drawGrid.nodes = nodes;
            drawGrid.drawLabels = drawLabels;
            drawGrid.Schedule(setupData.rows * setupData.columns, 1).Complete();
            //drawGrid.Schedule(setupData.rows, SystemInfo.processorCount).Complete();


            drawGrid.builder.Dispose();

        }

        private void OnDisable()
        {
            if (nodes.IsCreated)
                nodes.Dispose();

            if (overlapCommands.IsCreated)
            {
                overlapCommands.Dispose();
                overlapResults.Dispose();
            }
        }

        #region Setting Up Grid

        private void CreateNodes()
        {

            if (nodes.IsCreated)
                nodes.Dispose();

            if (overlapCommands.IsCreated)
            {
                overlapCommands.Dispose();
                overlapResults.Dispose();
            }

            if (setupData.rows < 1 || setupData.columns < 1)
                return;


            nodes = new NativeArray<SquareNode>(setupData.rows * setupData.columns, Allocator.Persistent);
            overlapCommands = new NativeArray<OverlapBoxCommand>(setupData.rows * setupData.columns, Allocator.Persistent);
            overlapResults = new NativeArray<ColliderHit>(setupData.rows * setupData.columns, Allocator.Persistent);
            currentRows = setupData.rows;
            currentColumns = setupData.columns;
            currentNodeLength = setupData.nodeLength;

            CreateNodesJob cnj = new CreateNodesJob();
            cnj.nodes = nodes;
            cnj.nodeLength = setupData.nodeLength;
            cnj.columns = setupData.columns;
            cnj.rows = setupData.rows;
            cnj.Schedule(setupData.rows, SystemInfo.processorCount).Complete();


        }

        private void UpdateNodeInfo()
        {


            currentNodeLength = setupData.nodeLength;

            UpdateNodeInfoJob uni = new UpdateNodeInfoJob();
            uni.nodes = nodes;
            uni.nodeLength = setupData.nodeLength;
            uni.columns = setupData.columns;
            uni.rows = setupData.rows;
            uni.Schedule(setupData.rows, SystemInfo.processorCount).Complete();



        }

        private void UpdateWalkableNodes()
        {

            CreateOverlapCommandsJob coc = new CreateOverlapCommandsJob();
            coc.mask = LayerMask.GetMask("Wall") | LayerMask.GetMask("Tower");
            coc.commands = overlapCommands;
            coc.nodes = nodes;
            coc.rows = currentRows;
            coc.columns = currentColumns;
            coc.origin = setupData.origin;
            coc.halfNodeLength = setupData.nodeLength / 2f;
            coc.Schedule(currentRows * currentColumns, SystemInfo.processorCount).Complete();

            OverlapBoxCommand.ScheduleBatch
                (overlapCommands, overlapResults, SystemInfo.processorCount, 1).Complete();


            UpdateWalkableNodesJob uwn = new UpdateWalkableNodesJob();
            uwn.nodes = nodes;
            uwn.overlapResults = overlapResults;
            uwn.Schedule(setupData.rows * setupData.columns, SystemInfo.processorCount).Complete();




        }

        #endregion

        #region Path Related


        private void SetupVectorField()
        {

            Profiler.BeginSample("Clear Variables");

            openNodeDifficulties.Clear();
            closedNodes.Clear();
            openNodeKeys.Clear();
            fCostKeys.Clear();
            finalPathIndices.Clear();

            Profiler.EndSample();

            GenerateVectorFieldJob job = new GenerateVectorFieldJob();
            job.nodes = nodes;
            job.openNodeDifficulties = openNodeDifficulties;
            job.closedNodes = closedNodes;
            job.openNodeKeys = openNodeKeys;
            job.fCostKeys = fCostKeys;
            job.finalPathIndices = finalPathIndices;
            job.builder = DrawingManager.GetBuilder();
            job.origin = setupData.origin;
            job.columns = currentColumns;
            job.rows = currentRows;
            job.nodeLength = currentNodeLength;
            job.Schedule().Complete();

            job.builder.Dispose();

        }



        private void FindPath(float3 start, float3 end)
        {


            Profiler.BeginSample("Get Node Index From Position");

            int startNodeIndex = GetNodeIndexFromPosition(start);
            int endNodeIndex = GetNodeIndexFromPosition(end);

            Profiler.EndSample();

            int scalar = 1000;
            PathNode currentNode = new PathNode();
            currentNode.index = startNodeIndex;
            currentNode.hCost = math.distance(nodes[startNodeIndex].position,
                nodes[endNodeIndex].position);

            #region A* Version 2

            Profiler.BeginSample("Allocation");

            NativeParallelMultiHashMap<int, PathNode> openNodeDifficulties 
                = new NativeParallelMultiHashMap<int, PathNode>(500, Allocator.Temp);

            NativeParallelMultiHashMapIterator<int> it;

            NativeParallelHashMap<int, int> openNodeKeys 
                = new NativeParallelHashMap<int, int>(500, Allocator.Temp);

            NativeList<int> fCostKeys = new NativeList<int>(500, Allocator.Temp);

            NativeParallelHashMap<int, PathNode> closedNodes = new NativeParallelHashMap<int, PathNode>(500, Allocator.Temp);

            Profiler.EndSample();

            openNodeDifficulties.Add((int)(currentNode.FCost * scalar), currentNode);
            openNodeKeys.Add(currentNode.index, 0);

            fCostKeys.Add((int)(currentNode.FCost * scalar));


            int outDifficultyKey;
            PathNode checkNode = new PathNode();
            int minKeyCost = int.MaxValue;
            int minKeyCostIndex = 0;
            int searched = 0;

            Profiler.BeginSample("Checking Nodes");

            while (currentNode.index != endNodeIndex)
            {

                minKeyCostIndex = 0;
                minKeyCost = int.MaxValue;


                Profiler.BeginSample("Check FCosts");

                for (int i = 0; i < fCostKeys.Length; i++)
                {
                    if (fCostKeys[i] < minKeyCost)
                    {
                        minKeyCost = fCostKeys[i];
                        minKeyCostIndex = i;
                    }

                }

                Profiler.EndSample();

                if (!openNodeDifficulties.TryGetFirstValue(minKeyCost, out currentNode, out it))
                {


                    fCostKeys.RemoveAt(minKeyCostIndex);
                    

                    continue;
                }


                //has a new currentNode, remove it from open nodes, add it to closed nodes, then check neighbors

                closedNodes.Add(currentNode.index, currentNode);
                openNodeKeys.Remove(currentNode.index);
                openNodeDifficulties.Remove((int)(currentNode.FCost * scalar), currentNode);

                Profiler.BeginSample("Check Neighbors");

                if (currentNode.index % currentColumns > 0)
                {
                    //check left node
                    CheckNeighborNode(currentNode.index - 1);
                }

                if (currentNode.index % (currentColumns) != currentColumns - 1 && currentNode.index != endNodeIndex)
                {    //check right node
                    CheckNeighborNode(currentNode.index + 1);
                }
                if ((currentNode.index + currentColumns) < (currentRows * (currentColumns)) 
                    && currentNode.index != endNodeIndex)
                {
                    //check node above
                    CheckNeighborNode(currentNode.index + currentColumns);
                }

                if ((currentNode.index - currentColumns) > 0 && currentNode.index != endNodeIndex)
                {
                    //check node below
                    CheckNeighborNode(currentNode.index - currentColumns);
                }


                Profiler.EndSample();



                void CheckNeighborNode(int index)
                {

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
                        return;
                    }

                    searched++;
                    Profiler.BeginSample("Set up neighbor node");

                    PathNode neighborNode = new PathNode();
                    neighborNode.parentIndex = currentNode.index;
                    neighborNode.index = index;
                    neighborNode.gCost = math.distance(nodes[index].position, nodes[startNodeIndex].position);
                    neighborNode.hCost = math.distance(nodes[index].position, nodes[endNodeIndex].position);

                    Profiler.EndSample();


                    #region Draw Neighbor Node Info

                    Profiler.BeginSample("Neighbor Node Info");

                    float3 poss = new float3(nodes[index].position.x, 0, nodes[index].position.y);

                    //Draw.Label2D(setupData.origin + poss - new float3(currentNodeLength / 4f, 0, 0),
                    //    "Searched: " + searched + "\nHCost: " + neighborNode.hCost + "\nGCost: " + neighborNode.gCost + "\nFCost: " + neighborNode.FCost,
                    //    10, Color.black);


                    Profiler.EndSample();

                    #endregion



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
                                openNodeKeys[checkNode.index] = (int)(checkNode.FCost * scalar);
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


            }

            Profiler.EndSample();


            Profiler.BeginSample("Trace Path");

            NativeArray<int> indices = new NativeArray<int>(1000, Allocator.Temp);

            int j = 0;
            while (currentNode.index != startNodeIndex)
            {
                indices[j] = currentNode.index;

                closedNodes.TryGetValue(currentNode.parentIndex, out currentNode);
                j++;

            }
            indices[j] = currentNode.index;
            j++;

            Profiler.EndSample();

            //draw final path

            #region Draw Searched Nodes

            Profiler.BeginSample("Draw Closed Nodes");

            NativeArray<PathNode> searchedNodes = closedNodes.GetValueArray(Allocator.Temp);

            float3 position = new float3();
            for (int k = 0; k < searchedNodes.Length; k++)
            {

                if (finalPathIndices.Contains(searchedNodes[k].index))
                    continue;

                position.x = nodes[searchedNodes[k].index].position.x;
                position.z = nodes[searchedNodes[k].index].position.y;
                position += setupData.origin;

                Draw.SolidPlane(position, new float3(0, 1, 0),
                    new float2(currentNodeLength), Color.red);

            }




            searchedNodes.Dispose();

            Profiler.EndSample();

            Profiler.BeginSample("Draw Closed Nodes");

            NativeArray<PathNode> openSearched = openNodeDifficulties.GetValueArray(Allocator.Temp);

            position = new float3();
            for (int k = 0; k < openSearched.Length; k++)
            {

                //if (finalPathIndices.Contains(openSearched[k].index))
                //    continue;

                position.x = nodes[openSearched[k].index].position.x;
                position.z = nodes[openSearched[k].index].position.y;
                position += setupData.origin;

                Draw.SolidPlane(position, new float3(0, 1, 0),
                    new float2(currentNodeLength), Color.green);

            }




            openSearched.Dispose();

            Profiler.EndSample();

            #endregion

            #region Draw Final Path

            Profiler.BeginSample("Draw Final Path");

            float3 pos = new float3();
            for (int k = 0; k < j; k++)
            {
                pos.x = nodes[indices[k]].position.x;
                pos.z = nodes[indices[k]].position.y;
                pos += setupData.origin;

                Drawing.Draw.SolidPlane(pos, new float3(0, 1, 0), new float2(currentNodeLength), Color.cyan);

            }

            Profiler.EndSample();

            #endregion



            indices.Dispose();

            openNodeDifficulties.Dispose();
            openNodeKeys.Dispose();
            fCostKeys.Dispose();

            closedNodes.Dispose();


            #endregion

        }

        private bool ContainsInGrid (float3 position)
        {
            if (position.x > (setupData.origin.x + (setupData.nodeLength * setupData.columns)) ||
                position.x < setupData.origin.x)
            {
                return false;

            }

            if (position.z > (setupData.origin.z + ((setupData.nodeLength) * setupData.rows)) ||
                position.z < setupData.origin.z)
            {
                return false;

            }


            return true;


        }

        private int GetNodeIndexFromPosition(float3 position)
        {

            if (!ContainsInGrid(position))
            {

                Debug.LogError("Position not contained within grid.");

                return -1;

            }

            float3 localPosition = (float3)(position - setupData.origin);

            //int rowStart = (int)(localPosition.x / setupData.nodeLength);
            //int column = (int)(localPosition.z / setupData.nodeLength) * currentColumns;

            //int index = rowStart + column;

            //debugging purposes only
            //float3 pos = setupData.origin + new float3(nodes[index].position.x, 0, nodes[index].position.y);
            //Drawing.Draw.Label2D(position, "I: " + index, Color.black);
            ////"L: " + localPosition);
            //Drawing.Draw.SolidPlane(pos, new float3(0, 1, 0), new float2(setupData.nodeLength), Color.blue);

            return (int)(localPosition.x / setupData.nodeLength)
                + (int)(localPosition.z / setupData.nodeLength) * currentColumns;

        }


        #endregion



    }


}