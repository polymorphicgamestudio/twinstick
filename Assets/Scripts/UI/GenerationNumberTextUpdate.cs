using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace ShepProject
{

    public class GenerationNumberTextUpdate : MonoBehaviour
    {

        public TMP_Text numberText;

        private void Start()
        {
            ShepGM.inst.startOfWave += StartOfWave;
        }

        private void StartOfWave(int waveStarted)
        {
            numberText.text = (waveStarted < 10 ? "0" : "") + (waveStarted).ToString();

        }


    }

}