using ShepProject;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExperimentModeSettings
{
    /*
     * 
     * NEEDS
     * 
     * - write evolution to file bool
     * 
     * - standard deviation for mutations
     * - mean for mutations (offset)
     * 
     * - slime count per wave
     * - slime speed 
     * - slime turn rate 
     * - slime health
     * 
     * - view range for 
     *      - slimes
     *      - towers
     *      - player
     *      - walls
     *      
     * - initial attractions for
     *      - slimes
     *      - towers
     *      - player
     *      - walls
     * 
     * sheep speed
     * 
     * 
     * 
     * 
     */







}

public class ExperimentModeConfigurationData
{
    public class ObjectInstantiationInfo
    {
        //public int waveNumber;
        public int prefabIndex;
        public Vector3 position;
        public Vector3 rotation;


    }


    public List<ObjectInstantiationInfo> instantiationInfo;


}


/* 
 * Experiment Mode Load Configuration
 * 
 * 
 * list of positions for towers and walls
 *      needs to be updatable by wave number, will need to ask if this is neccesary
 *  
 *  Ex. 1 tower and 4 walls for round one
 *      3 towers and 12 walls for round 3
 *  
 *  How to save?
 *      - button at the end of each wave which can save the configuration to a file
 *      - each configuration will need to have a specific name
 *      
 *  How to load?
 *      - will check the place on the disk where these are saved and can choose one of them
 *      
 *      
 *  
 * 
 * will need a manager in order to make sure that everything is placed exactly how it should be
 *      has an update at the end of each wave
 * 
 * 
 * 
 * 
 */



