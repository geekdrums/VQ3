using UnityEngine;
using System.Collections;

public class ChangeAttrByScript : MonoBehaviour {

	GameObject	goAlpha;
	GameObject	goHide;
	SsSprite	sprAlpha;
	SsSprite	sprHide;

	// Use this for initialization
	void Start () {
		goAlpha = GameObject.Find("ChangeAttrByScript_alpha_ssa");
		goHide = GameObject.Find("ChangeAttrByScript_hide_ssa");
		sprAlpha = goAlpha.GetComponent<SsSprite>();
		sprHide = goHide.GetComponent<SsSprite>();
	}
	
	int state = 0;
	
	// Update is called once per frame
	void Update () {
		if (Input.GetButtonDown("Fire1"))
		{
			SsPart partAlphaParent = sprAlpha.GetPart("parent");
			SsPart partHideParent = sprHide.GetPart("parent");
			switch (state)
			{
			case 0:
				// half-transparent and hide
				partAlphaParent.ForceAlpha(0.5f);
				partHideParent.ForceShow(false);
				break;
			case 1:
				// opaque and show
				partAlphaParent.ForceAlpha(1f);
				partHideParent.ForceShow(true);
				break;
			}
			state = (state + 1) % 2;
		}
	}
}
