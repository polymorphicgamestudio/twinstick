using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Diagnostics;

namespace ShepProject {




    [System.Serializable]
    public struct BitwiseInt : IEquatable<BitwiseInt>
    {

        public float FloatScalar => 1000f;
        public uint Value { get => value; }

        public uint value;

        public byte Length => 32;


        #region Bitwise Number Methods


        public uint GetUnaligned(int startBit, int endBit)
        {
            int length = (endBit - startBit);// > 0 ? (endBit - startBit) : 1;
            length++;

            return ((value << ((Length - 1) - endBit)) >> (Length - length)) << (startBit);
        }

        public uint GetAligned(uint startBit, uint endBit)
        {
            //shifts everything to the left, towards 31 by (31 - endBit) amount 
            //afterwards, to align it's value to 0,
            //it's right shifted up until the starting Bit

            return GetAligned((int)startBit, (int)endBit);


        }

        public uint GetAligned(int startBit, int endBit)
        {
            //shifts everything to the left, towards 31 by (31 - endBit) amount 
            //afterwards, to align it's value to 0,
            //it's right shifted up until the starting Bit, so it starts at 0
            int length = (endBit - startBit);
            length++;
            //if (length == 0) length = 1;

            return (value << ((Length - 1) - endBit)) >> (Length - length);


        }


        public void SetBitsZero(int startBit, int endBit)
        {

            uint v = 0;


            //backup other bits and add them together
            //need left side of requested value
            if (startBit > 0)
                v = GetUnaligned(0, (byte)(startBit - 1));
            if (endBit < (Length - 1))
                v |= GetUnaligned((byte)(endBit + 1), (Length - 1));

            this.value = v;

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="startBit">length away from right side towards 31.</param>
        /// <param name="value">value which will be placed at that starting bit</param>
        public void Set(int startBit, int endBit, uint val)
        {

            SetBitsZero(startBit, endBit);

            this.value |= ((uint)val << startBit);


        }


        #endregion

        public float ToFloat()
        {
            return ((int)value) / FloatScalar;
        }

        public override string ToString()
        {
            return "[ " + ToBinaryString((int)value, Length) + " ]";

        }


        public string ToBinaryString(int value, byte length)
        {

            string s = Convert.ToString(value, 2);
            for (int i = s.Length; i < length; i++)
            {
                s = "0" + s;

            }
            StringBuilder sb = new StringBuilder(s);

            for (int i = 8; i < sb.Length; i += 9)
            {
                sb.Insert(i, "-");

            }

            return sb.ToString();




        }




        public bool Equals(BitwiseInt other)
        {
            return (value == other.Value);
        }


        #region Constructors


        public BitwiseInt(int value = 0)
        {
            this.value = (uint)((int)(value << 0));
        }



        public BitwiseInt(uint value = 0)
        {
            this.value = value;
        }

        public BitwiseInt(float value = 0)
            : this((int)(value * 1000))
        {

        }



        #endregion


        #region Object Overrides



        //public override int GetHashCode()
        //{
        //    return value.GetHashCode();
        //}

        #endregion

        /*
         *  BIT SHIFTING RULES / INFO
         *  
         *  bits read from right to left
         *  when using >> it gets rid of bits to the right
         *  Ex. 
         *  int val = 0000_0000_0000_0000_0000_0000_0011_0100
         *  
         *  val = val >> 2 
         *  val now equals  0000_0000_0000_0000_0000_0000_0000_1101
         * 
         */

    }





    [System.Serializable]
    public struct QuadKey : IEquatable<QuadKey>
    {
        public BitwiseInt key;


        public uint EndIndex => (GetCount() - 1);
        public bool IsDivided => GetDivided();

        #region Constructors

        public QuadKey(int value = 0)
        {
            key = new BitwiseInt(value);
        }

        public QuadKey(uint value = 0)
        {
            key = new BitwiseInt(value);
        }


        #endregion

        #region Heirarchy Methods

        public uint GetQuadHeirarchy()
        {
            if (GetCount() <= 0)
                return 0;

            return key.GetAligned(0, GetCount() - 1);
        }

        public string GetHeirarchyBinaryString()
        {

            string s = "";
            for (byte i = 0; i < GetCount(); i++)
            {
                //writes right to left
                if (key.GetAligned(i, i) > 0)
                {
                    s += "1";

                }
                else
                {
                    s += "0";
                }

            }


            return s;// Convert.ToString(GetQuadHeirarchy(), 2);
        }

        public bool GetHeirarchyBit(int index)
        {
            return (key.GetAligned(index, index) > 0);
        }

        /// <summary>
        /// adds a 0 bit to the next bit in line
        /// </summary>
        public void LeftBranch()
        {

            IncreaseCount(1);
        }


        /// <summary>
        /// adds a 1 bit to the next bit in line
        /// </summary>
        public void RightBranch()
        {
            key.Set((int)GetCount(), (int)GetCount(), 1);
            IncreaseCount(1);

        }


        public void Branch(bool value)
        {

            if (value)
                RightBranch();
            else
                LeftBranch();

        }

        public bool2 PositionToBools(byte position)
        {

            switch (position)
            {
                case 0:
                {
                    //BL = 0
                    return new bool2(false, false);
                }
                case 1:
                {
                    //BR = 1
                    return new bool2(true, false);

                }
                case 2:
                {
                    //TL = 2
                    return new bool2(false, true);
                }
                case 3:
                {
                    //TR = 3
                    return new bool2(true, true);
                }

                default:
                    throw new ArgumentOutOfRangeException("Out of Quad Position Range. " +
                        position + " > " + 3);
            }
        }



        public void SetDivided(bool value = true)
        {
            //sets it to the left of the last count index
            //due to how bitshifting works
            int count = (int)GetCount();
            key.Set(23, 23, (uint)(value? 1 : 0));

        }


        private bool GetDivided()
        {
            int count = (int)GetCount();
            uint val = key.GetAligned(23, 23);

            return val > 0;
        }


        //method that gets range of divisions
        //Ex. division lvl 1-4, with custom square position able to be set at last level
        //or also possible to just read range of heirarchy elements,
        //correct lvl count and isDivided bit required

        public QuadKey GetParentKey()
        {
            if (GetCount() - 2 == 0)
                return new QuadKey();

            return GetLevelPositionRange((int)GetCount() - 2);

        }

        public QuadKey GetLevelPositionRange(int endLevel)
        {
            QuadKey k = new QuadKey(key.GetUnaligned(0, (byte)endLevel));
            k.IncreaseCount((byte)endLevel);

            if (endLevel < GetCount())
            {
                k.SetDivided();
            }
            else if (endLevel == GetCount())
            {
                if (GetDivided())
                    k.SetDivided();
            }

            return k;
        }

        public void SetNextLevelPosition(bool2 position)
        {

            //getCount for current level, then match bools
            Branch(position.x);
            Branch(position.y);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="level"></param>
        /// <param name="value">Range of 0-3, BL = 0 BR = 1 TL = 2 TR = 3</param>
        public void SetLevelPosition(byte level, byte value)
        {
            key.Set(level - 1, level, value);
            if (GetCount() < level)
                IncreaseCount((byte)(level - GetCount()));

        }

        public void SetLevelPosition(byte level, bool2 value)
        {
            byte val = 0;

            if (value.x)
                val += 1;
            if (value.y)
                val += 2;

            SetLevelPosition(level, val);
        }



        #endregion

        #region Equatable


        public bool Equals(QuadKey other)
        {
            return
                (GetCount() == other.GetCount()) &&
                (GetQuadHeirarchy() == other.GetQuadHeirarchy());
        }


        public override bool Equals(object obj)
        {
            if (!(obj is QuadKey key))
                return false;

            return Equals(key);

        }

        public override int GetHashCode()
        {
            return
                GetCount().GetHashCode() *
                GetQuadHeirarchy().GetHashCode();
        }


        #endregion


        #region Debug Related


        public string ToBinaryString()
        {
            return key.ToBinaryString((int)key.value, key.Length);
        }

        public override string ToString()
        {
            int len = GetHeirarchyBinaryString().Length - 1;
            string s = GetHeirarchyBinaryString().PadLeft((int)(GetCount() - len), '+');

            return "IsDiv: " + IsDivided + " Lvl: " + GetCount() + /*" " + Convert.ToString(GetCount(), 2) +*/
                " Branch: " + s;
        }

        public string CountBinaryString()
        {
            return "Count Bin: " + Convert.ToString(GetCount(), 2);
        }



        #endregion




        /// <summary>
        /// If this QuadKey is being used, count will always be at least 1.
        /// </summary>
        /// <returns></returns>
        public uint GetCount()
        {
            return key.GetAligned(24, 31);
        }



        public void IncreaseCount(byte amount, bool log = true)
        {
            uint val = GetCount() + amount;


            key.Set(24, 31, val);

        }


    }


    public struct Quad {

		public QuadKey key;

		public float2 position;
		public float halfLength;

		//bucket
		public short startIndex;
		public short endIndex;

		public short BucketSize => (short)((endIndex - startIndex) + 1);

		public Quad(short startIndex, short endIndex, QuadKey key = new QuadKey()) {

			this.startIndex = startIndex;
			this.endIndex = endIndex;
			position= new float2(0, 0);
			halfLength = 0;
            this.key = key;
		}

		public float Middle(bool zsort) {
			if (zsort) {
				return position.y;
			}
			else {
				return position.x;
			}
		}


		public override string ToString() {


			return "Start: " + startIndex + " End: " + endIndex;
		}

	}

}