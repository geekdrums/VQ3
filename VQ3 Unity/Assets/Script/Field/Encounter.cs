using UnityEngine;
using System.Collections;

public class Encounter : MonoBehaviour {

    public GameObject[] Enemies;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    // TEMP!!! need some args? ex. PLayer
    public bool IsEncountered()
    {
        return true;//Music.Just.bar >= 2;
    }
}
