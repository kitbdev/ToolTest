using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleSpawn : MonoBehaviour
{
    public GameObject spawnMe;
    // Start is called before the first frame update
    void Start()
    {
        Instantiate(spawnMe);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
