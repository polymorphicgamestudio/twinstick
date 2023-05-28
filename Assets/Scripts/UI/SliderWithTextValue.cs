using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SliderWithTextValue : MonoBehaviour
{
    public string sliderName;
    private Slider slider;
    public TMP_Text text;


    private void Awake()
    {
        slider = GetComponent<Slider>();

    }

    // Update is called once per frame
    void Update()
    {
        text.text = sliderName + ": " + decimal.Round(new decimal(slider.value), 4).ToString();
    }
}
