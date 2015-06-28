using UnityEngine;
using System.Collections;

public class CommandExplanation : MonoBehaviour {

	public TextMesh CommandName, CommandNameJP;
	public TextMesh Explanation;
	public GameObject IconParent;
	
	PlayerCommandData commandData_;
	Enemy enemyData_;

	// Use this for initialization
	void Start () {
		Reset();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void Set( PlayerCommandData data )
	{
		if( IconParent.transform.childCount > 0 )
		{
			Destroy(IconParent.transform.GetChild(0).gameObject);
		}
		commandData_ = data;
		CommandName.text = commandData_.OwnerCommand.nameText.ToUpper();
		CommandNameJP.text = commandData_.OwnerCommand.name;
		CommandNameJP.transform.localPosition = CommandName.transform.localPosition + Vector3.right * 1.2f * (CommandName.text.Length + 1) + Vector3.down;
		Explanation.text = commandData_.ExplanationText;
		GameObject iconObj = Instantiate(commandData_.OwnerCommand.gameObject) as GameObject;
		iconObj.transform.parent = IconParent.transform;
		iconObj.transform.localPosition = Vector3.zero;
		iconObj.transform.localScale = Vector3.one;
		iconObj.transform.localRotation = Quaternion.identity;
		iconObj.GetComponent<PlayerCommand>().enabled = false;
		Destroy(iconObj.GetComponentInChildren<CounterSprite>().gameObject);
	}
	public void SetEnemy( Enemy enemy )
	{
		enemyData_ = enemy;
		if( IconParent.transform.childCount > 0 )
		{
			Destroy(IconParent.transform.GetChild(0).gameObject);
		}
		CommandName.text = enemy.name.ToUpper();
		CommandNameJP.text = enemy.DisplayName;
		CommandNameJP.transform.localPosition = CommandName.transform.localPosition + Vector3.right * 1.2f * (CommandName.text.Length + 1) + Vector3.down;
		Explanation.text = enemy.ExplanationText;
		GameObject enemyObj = Instantiate(enemy.gameObject) as GameObject;
		enemyObj.transform.parent = IconParent.transform;
		enemyObj.transform.localPosition = Vector3.zero;
		enemyObj.transform.localScale = enemy.transform.localScale * 2.5f;
		enemyObj.transform.localRotation = Quaternion.identity;
		enemyObj.GetComponent<Enemy>().enabled = false;
		enemyObj.GetComponent<SpriteRenderer>().color = Color.white;
	}
	public void Reset()
	{
		if( IconParent.transform.childCount > 0 )
		{
			Destroy(IconParent.transform.GetChild(0).gameObject);
		}
		CommandName.text = "";
		CommandNameJP.text = "";
		Explanation.text = "";
	}
	public void ResetToEnemyData()
	{
		if( enemyData_ != null )
		{
			SetEnemy(enemyData_);
		}
	}
}
