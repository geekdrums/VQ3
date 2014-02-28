using UnityEngine;
using System.Collections;

public class Encounter : MonoBehaviour {

    [System.Serializable]
    public class StateSet
    {
        public string nameList;
        public string this[int i]
        {
            get{
                return nameList.Split( ' ' )[i];
            }
        }
    }

    public GameObject[] Enemies;
    public StateSet[] StateSets;

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
