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

	// Use this for initialization
	void Start () {
		animation_ = GetComponent<Animation>();
	}

	// Update is called once per frame
	void Update()
	{
		if( animation_.isPlaying == false && commandIcon_ != null )
		{
			Destroy(commandIcon_.gameObject);
			Destroy(commandName_.gameObject);

			commandIcon_ = null;
			commandName_ = null;
			skillList_ = null;
		}
	}

	public void Set(PlayerCommand command, SkillListUI skillList, GameObject commandName)
	{
		commandIcon_ = (PlayerCommand)Instantiate(command, commandIconParent.transform);
		commandIcon_.transform.localPosition = Vector3.zero;
		commandIcon_.enabled = false;

		commandName_ = (GameObject)Instantiate(commandName, commandNameParent.transform);
		commandName_.transform.position = commandName_.transform.position + Vector3.back * 50;
		if( command.themeColor == EThemeColor.White )
		{
			commandName_.GetComponentsInChildren<TextMesh>()[0].color = ColorManager.Base.Dark;
			commandName_.GetComponentsInChildren<TextMesh>()[1].color = ColorManager.Base.Dark;
			commandName_.GetComponentInChildren<CounterSprite>().CounterColor = ColorManager.Base.Dark;
		}

		skillList_ = (SkillListUI)Instantiate(skillList, transform.parent);
		skillList_.transform.position = skillList_.transform.position + Vector3.back * 50;
		skillList_.Execute(enhIconParent);


		float mtu = (float)Music.MusicalTimeUnit;

		AnimManager.AddAnim(commandIcon_.gameObject, commandIcon_.transform.localScale * 1.3f, ParamType.Scale, AnimType.BounceIn, 3 * mtu, mtu);

		AnimManager.AddAnim(commandName_.gameObject, Vector3.zero, ParamType.Position, AnimType.Linear, 0.2f, mtu);
		AnimManager.AddAnim(commandName_.gameObject, Vector3.left * 200, ParamType.Position, AnimType.Time, nameAnimLength, mtu * 9);

		AnimManager.AddAnim(skillList_.gameObject, skillListParent.transform.position, ParamType.Position, AnimType.Time, skillAnimLength, mtu * 9);

		animation_["SkillCutInAnim"].speed = (float)(Music.CurrentTempo / 60.0);
		animation_.Play();
	}
}
