using UnityEngine;
using System.Collections;

public class SsCommandedComipoChan : MonoBehaviour
{
	SsButtonBehaviour[]	_buttons;
	SsSprite		_sprite;
	GameObject		_cube;
	
	// Use this for initialization
	void Start()
	{
		_sprite = GetComponent<SsSprite>();
		_buttons = FindObjectsOfType(typeof(SsButtonBehaviour)) as SsButtonBehaviour[];
		foreach (var e in _buttons)
			e.OnPushed += OnPushed;
		_cube = GameObject.Find("Cube");
	}
	
	// Update is called once per frame
	void Update()
	{
		if (_cube)
		{
			_cube.transform.Rotate(Vector3.one * Time.deltaTime * 16, Space.World);
		}
	}
	
	public void OnPushed(GameObject go)
	{
		if (go.name == "btnAttack")
			ChangeAnime("attack_ssa", 1);
		if (go.name == "btnJump")
			ChangeAnime("jump_ssa", 1);
		if (go.name == "btnRun")
			ChangeAnime("dash_ssa", 0);
	}
	
	void ChangeAnime(string name, int loop)
	{
		SsAnimation anm = SsAssetDatabase.Instance.GetAnime(name);
		_sprite.PlayCount = loop;
		_sprite.Animation = anm;
		if (loop == 1)
			_sprite.AnimationFinished = AnimeFinished;
		else
			_sprite.AnimationFinished = null;
	}

	public	void AnimeFinished(SsSprite sprite)
	{
		ChangeAnime("stand_ssa", 0);
	}
}
