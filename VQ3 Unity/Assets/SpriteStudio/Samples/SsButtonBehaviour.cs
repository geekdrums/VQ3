/**
	SpriteStudio
	
	Button animation sample
	
	The reason why the almost member fields are public is to show in inspector to observe their state.
	
	Copyright(C) 2003-2011 Web Technology Corp. 
	
*/

using UnityEngine;
using System.Collections;

public class SsButtonBehaviour : MonoBehaviour
{
	static SsAssetDatabase	ssdb;
	SsSprite	_sprite;
	SsPart		_mainPart;
	bool		_flashing;

	public string _defaultAnimeName	= "btn_disable_ssa";
	public string _focusAnimeName	= "btn_ssa";
	public string _pushedAnimeName	= "btn_flash_ssa";

	public	delegate void EventFunc(GameObject go);
	public	event	EventFunc	OnPushed;

	void Awake()
	{
		if (!ssdb)
			ssdb = SsAssetDatabase.Instance;
	}
	
	void Start()
	{
		_sprite = GetComponent<SsSprite>();
		_sprite.Animation = ssdb.GetAnime(_defaultAnimeName);
		_mainPart = _sprite.GetPart("part 1");
	}
	
	void Update()
	{
		if (!ssdb) return;
		if (_flashing) return;
		
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		Vector3 pt = ray.GetPoint(0f);
		if (_sprite.ContainsPoint(pt, true))
		{
			string anime = _focusAnimeName;
			if (Input.GetMouseButtonDown(0))
			{
				anime = _pushedAnimeName;
				_flashing = true;
				if (OnPushed != null)
					OnPushed(gameObject);
			}
			_sprite.Animation = ssdb.GetAnime(anime);
			// wait for flash animation finished
			if (_flashing)
			{
				_sprite.PlayCount = 1;
				_sprite.AnimationFinished = FlashFinished;
				_mainPart.OnUserDataKey += OnUserDataKey;
			}
			else
				_sprite.PlayCount = 0;
		}
		else
			_sprite.Animation = ssdb.GetAnime(_defaultAnimeName);
	}
	
	public void FlashFinished(SsSprite sprite)
	{
		_flashing = false;
		_sprite.AnimationFinished = null;
	}

	public void OnUserDataKey(SsPart part, SsAttrValueInterface val)
	{
		var udk = val as SsUserDataKeyValue;
		Debug.LogWarning(part.Sprite.gameObject.name + ": " + udk.String);
		part.OnUserDataKey -= OnUserDataKey;
	}
}
