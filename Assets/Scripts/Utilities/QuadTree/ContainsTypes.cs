using ShepProject;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public struct ContainsTypes
{
    private bool3 containsBuffer1;
    private bool2 containsBuffer2;

    public bool this[ObjectType index]
    {
        get
        {

            switch (index)
            {

                case ObjectType.Slime:
                {
                    return ContainsSlimes;
                }
                case ObjectType.Tower:
                {
                    return ContainsTowers;
                }
                case ObjectType.Sheep:
                {
                    return ContainsSheep;
                }
                case ObjectType.Player:
                {
                    return ContainsPlayer;
                }
                case ObjectType.Wall:
                {
                    return ContainsWalls;

                }
                default:
                {
                    throw new System.Exception( "Invalid choice " + index + " for ContainsTypes.");
                }
            }

        }


        set
        {

            switch (index)
            {

                case ObjectType.Slime:
                    {
                        ContainsSlimes = value;
                        break;
                    }
                case ObjectType.Tower:
                    {
                        ContainsTowers = value;
                        break;
                    }
                case ObjectType.Sheep:
                    {
                        ContainsSheep = value;
                        break;
                    }
                case ObjectType.Player:
                    {
                        ContainsPlayer = value;
                        break;
                    }
                case ObjectType.Wall:
                    {
                        ContainsWalls = value;
                        break;

                    }
            }

        }
    }

    public bool ContainsSlimes
    {
        get => containsBuffer1[0];
        set => containsBuffer1[0] = value;
    }

    public bool ContainsTowers
    {
        get => containsBuffer1[1];
        set => containsBuffer1[1] = value;
    }
    public bool ContainsSheep
    {
        get => containsBuffer1[2];
        set => containsBuffer1[2] = value;
    }
    public bool ContainsPlayer
    {
        get => containsBuffer2[0];
        set => containsBuffer2[0] = value;
    }

    public bool ContainsWalls
    {
        get => containsBuffer2[1];
        set => containsBuffer2[1] = value;
    }

    public ContainsTypes(bool2 containsBuffer2 = new bool2(), bool3 containsBuffer1 = new bool3())
    {
        this.containsBuffer2 = containsBuffer2;
        this.containsBuffer1 = containsBuffer1;

    }

    public ContainsTypes(bool3 containsBuffer1, bool2 containsBuffer2 = new bool2())
    {
        this.containsBuffer2 = containsBuffer2;
        this.containsBuffer1 = containsBuffer1;

    }

    public static ContainsTypes operator |(ContainsTypes lhs, ContainsTypes rhs)
    {
        ContainsTypes newTypes = new ContainsTypes();

        newTypes.containsBuffer1 = lhs.containsBuffer1 | rhs.containsBuffer1;
        newTypes.containsBuffer2 = lhs.containsBuffer2 | rhs.containsBuffer2;


        return newTypes;

    }



}
