using UnityEngine;
using System.Collections;

public class SkillCutIn : MonoBehaviour {

	public GameObject skillListParent;
	public GameObject commandIconParent;
	public GameObject commandNameParent;
	public GameObject enhIconParent;

	Animation animation_;

	SkillListUI skillList_;
	PlayerCommand commandIcon_;
	GameObject commandName_;

	public float skillAnimLength = 0.2f;
	public float nameAnimLength = 0.2f;
	public float iconAnimDisntace = 10.0f;

	// Use this for initialization
	void Start () {
		animation_ = GetComponent<Animation>();
	}

	// Update is called once per frame
	void Update()
	{
		if( animation_.isPlaying == false && commandIcon_ != null && AnimManager.IsAnimating(commandIcon_.gameObject) == false )
		{
			DestroyIconAndName();
		}
	}

	void DestroyIconAndName()
	{
		Destroy(commandIcon_.gameObject);
		Destroy(commandName_.gameObject);

		commandIcon_ = null;
		commandName_ = null;
		skillList_ = null;
	}

	public void Set(PlayerCommand command, SkillListUI skillList, GameObject commandName)
	{
		if( commandIcon_ != null )
		{
			DestroyIconAndName();
		}

		commandIcon_ = (PlayerCommand)Instantiate(command, commandIconParent.transform);
		Vector3 initialScale = commandIcon_.transform.localScale;
		commandIcon_.transform.localPosition = Vector3.zero;
		commandIcon_.transform.localScale = Vector3.zero;
		commandIcon_.enabled = false;
		Transform nextRect = commandIcon_.transform.Find("nextRect");
		if( nextRect != null )
		{
			Destroy(nextRect.gameObject);
		}

		commandName_ = (GameObject)Instantiate(commandName, commandNameParent.transform);
		commandName_.transform.position = commandName_.transform.position + Vector3.back * 50;
		if( command.themeColor == EThemeColor.White )
		{
			commandName_.GetComponentsInChildren<TextMesh>()[0].color = ColorManagerObsolete.Base.Dark;
			commandName_.GetComponentsInChildren<TextMesh>()[1].color = ColorManagerObsolete.Base.Dark;
			commandName_.GetComponentInChildren<CounterSprite>().CounterColor = ColorManagerObsolete.Base.Dark;
		}

		skillList_ = (SkillListUI)Instantiate(skillList, transform);
		skillList_.transform.position = skillList_.transform.position + Vector3.back * 50;

		float mtu = (float)Music.Meter.SecPerUnit;

		AnimManager.AddAnim(commandIcon_.gameObject, initialScale.x, AnimParamType.Scale, InterpType.BackOut, time: 3 * mtu, delay: mtu);

		AnimManager.AddAnim(commandName_.gameObject, Vector3.zero, AnimParamType.Position, InterpType.Linear, time: 0.2f, delay:mtu);
		AnimManager.AddAnim(commandName_.gameObject, Vector3.left * 200, AnimParamType.Position, InterpType.Linear, time: nameAnimLength, delay: mtu * 9);

		AnimManager.AddAnim(commandIcon_.gameObject, Vector3.up * iconAnimDisntace, AnimParamType.Position, InterpType.BackOut, time: skillAnimLength, delay: mtu * 5);
		AnimManager.AddAnim(commandIcon_.gameObject, Vector3.zero, AnimParamType.Position, InterpType.BackIn, time: skillAnimLength, delay: (float)Music.Meter.SecPerUnit * 13);
		AnimManager.AddAnim(skillList_.gameObject, skillListParent.transform.localPosition, AnimParamType.Position, InterpType.BackOut, time: skillAnimLength, delay: mtu * 5);

		animation_["SkillCutInAnim"].speed = (float)(Music.CurrentTempo / 60.0);
		animation_.Play();
	}
}
