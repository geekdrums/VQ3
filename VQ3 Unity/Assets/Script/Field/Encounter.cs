using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Encounter : MonoBehaviour {

    public int Level;
    public GameObject[] Enemies;
    public EnemyConductor.StateSet[] StateSets;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    // TEMP!!! need some args? ex. Player
    public bool IsEncountered()
    {
		return true;//Music.Just.bar >= 2;
    }

}
