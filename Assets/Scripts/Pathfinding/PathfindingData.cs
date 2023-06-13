using Drawing;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;

namespace ShepProject
{

    [System.Serializable]
    public struct SquareNode
    {

        
        public float2 position;
        public bool walkable;

        private bool4x2 obstructions;

        public void ResetObstructions()
        {

            obstructions = default;

        }

        public bool LeftObstructed
        {
            get => obstructions.c0.w;
            set => obstructions.c0.w = value;
        }

        public bool TopLeftObstructed
        {
            get => obstructions.c0.x;
            set => obstructions.c0.x = value;
        }

        public bool TopObstructed
        {
            get => obstructions.c0.y;
            set => obstructions.c0.y = value;
        }

        public bool TopRightObstructed
        {
            get => obstructions.c0.z;
            set => obstructions.c0.z = value;
        }

        public bool RightObstructed
        {
            get => obstructions.c1.w;
            set => obstructions.c1.w = value;
        }

        public bool BottomRightObstructed
        {
            get => obstructions.c1.x;
            set => obstructions.c1.x = value;
        }

        public bool BottomObstructed
        {
            get => obstructions.c1.y;
            set => obstructions.c1.y = value;
        }

        public bool BottomLeftObstructed
        {
            get => obstructions.c1.z;
            set => obstructions.c1.z = value;
        }


    }

    public enum NodeDirection
    {
        Left = 1, TopLeft, Top, TopRight, Right, BottomRight, Bottom, BottomLeft, NoMovement

    }



    [System.Serializable]
    public struct PathNode : IComparable<PathNode>, IEquatable<PathNode>
    {
        public int parentIndex;
        public int index;

        public float gCost;
        public float hCost;
        public float FCost => gCost + hCost;

        public byte direction;

        public int CompareTo(PathNode other)
        {
            if (FCost < other.FCost)
            {
                return -1;
            }
            else if (FCost > other.FCost)
            {
                return 1;
            }

            return 0;

        }

        public bool Equals(PathNode other)
        {
            return index == other.index;
        }


        public override string ToString()
        {
            return "I: " + index + "G: " + gCost + " H: " + hCost + "F: " + FCost;
        }

    }

    public struct PathQueryStructure
    {

        [ReadOnly]
        private NativeArray<byte> vectorField;
        [ReadOnly]
        private NativeArray<float2> directions;
        
        public SquareGridSetupData setupData;


        public PathQueryStructure(NativeArray<byte> vectorField, NativeArray<float2> directions, SquareGridSetupData setupData)
        {

            this.vectorField = vectorField;
            this.directions = directions;

            this.setupData = setupData;



        }

        public bool InsideSameNode(float2 position, float2 destination)
        {
            return GetNodeIndexFromPosition(position) == GetNodeIndexFromPosition(destination);

        }

        public float2 GetHeadingToDestination(float2 position, float2 destination)
        {

            int posNode = GetNodeIndexFromPosition(position);
            int destNode = GetNodeIndexFromPosition(destination);
			
            return directions[vectorField[(posNode * (setupData.columns * setupData.rows)) + destNode]];

        }

        private bool ContainsInGrid(float2 position)
        {
            if (position.x > (setupData.origin.x + (setupData.nodeLength * setupData.columns)) ||
                position.x < setupData.origin.x)
            {
                return false;

            }

            if (position.y > (setupData.origin.z + ((setupData.nodeLength) * setupData.rows)) ||
                position.y < setupData.origin.z)
            {
                return false;

            }


            return true;


        }

        private int GetNodeIndexFromPosition(float2 position)
        {

            if (!ContainsInGrid(position))
            {

                Debug.LogError("Position not contained within grid.");

                return -1;

            }

            float2 localPosition = (position - setupData.originfloat2);

            return (int)(localPosition.x / setupData.nodeLength)
                + (int)(localPosition.y / setupData.nodeLength) * setupData.columns;

        }

    }


    [System.Serializable]
    public struct SquareGridSetupData
    {

        public bool fitPlayableArea;
        public int rows;
        public int columns;
        public float nodeLength;
        [HideInInspector]
        public float3 origin;
        public float2 originfloat2;
        public Color walkableColor;
        public Color unwalkableColor;

    }

}
