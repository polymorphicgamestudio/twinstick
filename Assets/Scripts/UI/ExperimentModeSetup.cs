using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ShepProject
{
    public static class ExperimentModeSettings
    {

        public static bool initialized;
        public static int slimeCount;
        public static float standardDeviation;
        public static float mutationChance;
        public static float typeMutationChance;


    }

    public class ExperimentModeSetup : MonoBehaviour
    {

        [SerializeField]
        public Slider slimeCount;
        [SerializeField]
        public Slider standardDeviation;
        [SerializeField]
        public Slider mutationChance;
        [SerializeField]
        public Slider typeMutationChance;

        [SerializeField]
        private Button launchButton;

        private bool reset;

        private void Awake()
        {
            launchButton.onClick.AddListener(IsInitialized);
            ExperimentModeSettings.initialized = false;
        }


        private void Update()
        {

            ExperimentModeSettings.slimeCount = (int)slimeCount.value;
            ExperimentModeSettings.standardDeviation = standardDeviation.value;
            ExperimentModeSettings.mutationChance = mutationChance.value;
            ExperimentModeSettings.typeMutationChance = typeMutationChance.value;


        }

        private void IsInitialized()
        {
            ExperimentModeSettings.initialized = true;

        }


        private void OnDisable()
        {
            
        }

    }


}