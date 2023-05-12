using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace ShepProject
{

    public struct ObjectForces
    {
        private float2x4 data;


        public ObjectForces(float2x4 data = new float2x4())
        {
            this.data = data;
        }

        public float2 SlimesForce
        {
            get => data.c0;
            set => data.c0 = value;
        }

        public float2 WallsForce
        {
            get => data.c1;
            set => data.c1 = value;
        }

        public float2 TowersForce
        {
            get => data.c2;
            set => data.c2 = value;
        }

        public float2 SheepForce
        {
            get => data.c3;
            set => data.c3 = value;
        }


        public static ObjectForces operator +(ObjectForces a, ObjectForces b)
        {

            return new ObjectForces(a.data + b.data);

        }

        public static ObjectForces operator -(ObjectForces a, ObjectForces b)
        {

            return new ObjectForces(a.data - b.data);

        }

        public static ObjectForces operator *(ObjectForces a, ObjectForces b)
        {

            return new ObjectForces(a.data * b.data);

        }

        public static ObjectForces operator /(ObjectForces a, ObjectForces b)
        {

            return new ObjectForces(a.data / b.data);

        }

    }


}