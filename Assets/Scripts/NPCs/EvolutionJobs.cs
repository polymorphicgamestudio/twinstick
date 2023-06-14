using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace ShepProject
{



    public struct ChooseParentSlimes : IJobParallelFor
    {

        [ReadOnly]
        public NativeArray<int> fitnessRanges;

        [ReadOnly]
        public NativeArray<int> slimeFitnesses;


        public NativeArray<ChromosoneParents> parents;


        public void Execute(int index)
        {

            Random rand = Random.CreateFromIndex((uint)index * 14121);

            ChromosoneParents currentParents = new ChromosoneParents();

            currentParents.parentOne = GetSlimeParent(rand.NextInt(0, fitnessRanges[fitnessRanges.Length] + 1));
            currentParents.parentTwo = GetSlimeParent(rand.NextInt(0, fitnessRanges[fitnessRanges.Length] + 1));

            parents[index] = currentParents;

        }


        private ushort GetSlimeParent(int randValue)
        {

            int startIndex = 0;
            int endIndex = 0;

            int fraction = 2;

            while ((endIndex - startIndex) > 8)
            {

                if (randValue < fitnessRanges[startIndex + (fitnessRanges.Length / fraction)])
                {

                    endIndex = startIndex + (fitnessRanges.Length / fraction);

                }
                else
                {

                    startIndex += (fitnessRanges.Length / fraction);

                }

            }


            for (ushort i = (ushort)startIndex; i <= endIndex; i++)
            {

                if (randValue > fitnessRanges[i])
                {
                    i--;
                    return i;

                }


            }


            return ushort.MaxValue;


        }

    }



    public struct CreateNextGeneration : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<ChromosoneParents> parents;
        [ReadOnly]
        public NativeArray<float> parentGenes;


        public NativeArray<float> childGenes;


        public void Execute(int index)
        {
            
            /*
             * 
             * how to create a genome
             * 
             * 
             *      for each gene that isn't one of the two types,
             *          choose randomly between parentOne and ParentTwo
             *          
             *      from there, randomly choose then if it will mutate 
             *      
             *      then assign that gene to the child
             *      
             *      
             *      
             *      if it's a type then randomly choose each one from the two of each parent
             *          then randomly choose if it will mutate from one of those types
             *          
             *          
             *      Question about resistances, 
             *          if the type mutates will it stay at the same strength
             *              or will the resistance reset?
             *          
             *      
             *      
             *      
             *      
             */

            


            

        }


    }








}


