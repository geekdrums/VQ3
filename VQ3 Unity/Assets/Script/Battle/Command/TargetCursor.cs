using UnityEngine;
using System.Collections;

public class TargetCursor : MonoBehaviour {

    Vector3 initialPosition;
    string initialText;

	// Use this for initialization
	void Start () {
        initialPosition = transform.position;
        initialText = GetComponent<TextMesh>().text;
	}
	
	// Update is called once per frame
	void Update () {
        Command NextCommand = GameContext.PlayerConductor.commandGraph.NextCommand;
        if( NextCommand != null )
        {
            GetComponent<TextMesh>().text = NextCommand.GetComponent<TextMesh>().text;
        }
	}

    public void OnBattleStart()
    {
        transform.position = initialPosition;
        GetComponent<TextMesh>().text = initialText;
    }
}
