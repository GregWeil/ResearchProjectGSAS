﻿using UnityEngine;
using System.Collections;

public class scroll : MonoBehaviour
{

    public float speed = 0.5f;
    public float spud = 0.5f;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Vector2 offset = new Vector2(Time.time * speed, Time.time * spud);
        GetComponent<Renderer>().materials[1].mainTextureOffset = offset;
    }
}
