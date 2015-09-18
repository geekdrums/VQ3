using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyExplanation : MonoBehaviour {

	public GameObject IconParent;
	public TextMesh Explanation;
	public TextMesh NameText;
	public GameObject[] Commands;

	Enemy enemyData_;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}


	public void SetEnemy(Enemy enemy)
	{
		gameObject.SetActive(true);
		enemyData_ = enemy;
		if( IconParent.transform.childCount > 0 )
		{
			Destroy(IconParent.transform.GetChild(0).gameObject);
		}
		Explanation.text = enemy.ExplanationText.Replace("<br/>", System.Environment.NewLine); ;
		GameObject enemyObj = Instantiate(enemy.gameObject) as GameObject;
		enemyObj.transform.parent = IconParent.transform;
		enemyObj.transform.localPosition = Vector3.zero;
		enemyObj.transform.localScale = enemy.transform.localScale;
		enemyObj.transform.localRotation = Quaternion.identity;
		enemyObj.GetComponent<Enemy>().enabled = false;
		enemyObj.GetComponent<SpriteRenderer>().color = Color.white;
		NameText.text = enemyData_.name.ToUpper();

		List<EnemyCommand> enemyCommands = new List<EnemyCommand>();
		foreach( EnemyCommand command in enemyObj.GetComponentsInChildren<EnemyCommand>() )
		{
			if( command.ExplanationText != "" )
			{
				enemyCommands.Add(command);
			}
		}

		for( int i=0; i<Commands.Length; ++i )
		{
			if( i < enemyCommands.Count )
			{
				Commands[i].SetActive(true);
				Commands[i].GetComponentInChildren<TextMesh>().text = enemyCommands[i].ExplanationText;
			}
			else
			{
				Commands[i].SetActive(false);
			}
		}
	}

	public void Hide()
	{
		gameObject.SetActive(false);
	}
}
