using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestInspector : MonoBehaviour
{
    public GameObject myGO;
    public int myInt;
    [Range(0, 1f)]
    public float myFloatRange;
    public string myString;
}
