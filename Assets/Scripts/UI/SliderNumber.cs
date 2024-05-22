using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;


public class SliderNumber : MonoBehaviour{
    [SerializeField] private Slider slider;
    [SerializeField] private Data save_value;
    private TextMeshProUGUI slider_number;

    void Start(){
        slider_number = GetComponent<TextMeshProUGUI>();
    }

    void Update(){
        slider_number.text = slider.value.ToString();
        save_value.Value = slider.value;
    }
}
