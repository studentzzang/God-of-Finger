using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FingerPower : MonoBehaviour
{
    public int _power = 5; //기본값5
    private int _maxPower = 20;
    private int _minPower = 0;
    

    void Update()
    {
        InputPower();
    }
    void InputPower()
    {
        int scroll = Mathf.RoundToInt(Input.mouseScrollDelta.y);

        if (scroll != 0)
        {
            _power = Mathf.Clamp(_power + scroll, _minPower, _maxPower);
        }
    }
}
