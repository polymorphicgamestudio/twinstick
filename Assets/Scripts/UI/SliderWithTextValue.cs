using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SliderWithTextValue : MonoBehaviour
{
    public string sliderName;
    public Slider slider;
    public TMP_Text text;
    public int decimals;

    private void Awake()
    {
        if (slider == null)
            slider = GetComponent<Slider>();

    }

    // Update is called once per frame
    void Update()
    {
        text.text = (sliderName == null ? "" : sliderName) + ": " + decimal.Round(new decimal(slider.value), decimals).ToString();
    }
}
