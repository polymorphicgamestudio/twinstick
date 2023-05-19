using Drawing;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

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
                setupData.origin - new float3(
                    0,
                    0,
                    index * setupData.nodeLength);

            float halfNodeLength = setupData.nodeLength / 2f;
            float3 topLeft = new float3(-halfNodeLength, 0 , halfNodeLength);
            float3 topRight = new float3(halfNodeLength, 0, halfNodeLength);


            for (int i = 0; i < setupData.columns; i++)
            {
                if(index == 0)//also have to check if unwalkable
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
                    "(" + position.x + " , " + position.z + ")");

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

            if (index % setupData.columns == 0)
                builder.Line(position + topLeft, position + topRight, setupData.walkableColor);

            //topRight to bottomRight
            builder.Line(position + topRight, position - topLeft, setupData.walkableColor);

            //bottomRight to bottomLeft
            builder.Line(position - topLeft, position - topRight, setupData.walkableColor);

            //bottomLeft to topLeft
            builder.Line(position - topRight, position + topLeft, setupData.walkableColor);
            if (drawLabels)
                builder.Label2D(position - new float3(halfNodeLength / 2f, 0, 0),
                    "(" + position.x + " , " + position.z + ")");


            //index tells which row it is
            //DrawRowOfSquares(index);

        }

        //private void DrawRowOfSquares(int index)
        //{
        //    float3 position =
        //        setupData.origin - new float3(
        //            0,
        //            0,
        //            index * setupData.nodeLength);

        //    float halfNodeLength = setupData.nodeLength / 2f;
        //    float3 topLeft = new float3(-halfNodeLength, 0, halfNodeLength);
        //    float3 topRight = new float3(halfNodeLength, 0, halfNodeLength);
        //    float3 up = new float3(0, 1, 0);
        //    int startIndex = (index * setupData.columns);

        //    for (int i = 0; i < setupData.columns; i++)
        //    {
        //        if (!nodes[startIndex + i].walkable)
        //            builder.SolidPlane(position, up, setupData.nodeLength, setupData.unwalkableColor);

        //        if (index == 0)//also have to check if unwalkable
        //            //topLeft to topRight
        //            builder.Line(position + topLeft, position + topRight, setupData.walkableColor);

        //        //topRight to bottomRight
        //        builder.Line(position + topRight, position - topLeft, setupData.walkableColor);

        //        //bottomRight to bottomLeft
        //        builder.Line(position - topLeft, position - topRight, setupData.walkableColor);

        //        //bottomLeft to topLeft
        //        builder.Line(position - topRight, position + topLeft, setupData.walkableColor);
        //        if (drawLabels)
        //            builder.Label2D(position - new float3(halfNodeLength / 2f, 0, 0),
        //                "(" + position.x + " , " + position.z + ")");

        //        position.x += setupData.nodeLength;
        //    }

        //}



    }

    public struct CreateNodesJob : IJobParallelFor
    {

        [NativeDisableContainerSafetyRestriction]
        public NativeArray<SquareNode> nodes;
        public int columns;
        public int rows;

        public void Execute(int index)
        {

            SquareNode node = new SquareNode();

            for (int i = 0; i < columns; i++)
            {
                node.position.x = index;
                node.position.y = -i;
                nodes[(rows * index) + i] = node;

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




}