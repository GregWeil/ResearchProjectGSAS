﻿using UnityEngine;
using System.Collections;

public class PlayerSpawner : MonoBehaviour {

    public GameObject prefab = null;
    public float spawnTime = 1.0f;

    GameObject player = null;
    float timer = 0.0f;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (player != null) {
            if (!player.GetComponentInChildren<CharacterHealth>().Alive()) {
                player = null;
            }
        }
	    if (player == null) {
            timer -= Time.deltaTime;
            if (timer < 0.0f) {
                player = (GameObject)Instantiate(prefab, transform.position, transform.rotation);
                player.name = prefab.name;
                timer = spawnTime;
            }
        }
	}
}

public class PlayerModule : StateMachineUtilities.Modules.Module {

    [StateMachineUtilities.Modules.Method("Player/is alive")]
    public static bool isAlive() {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return false;
        return player.GetComponentInChildren<CharacterHealth>().Alive();
    }
}
