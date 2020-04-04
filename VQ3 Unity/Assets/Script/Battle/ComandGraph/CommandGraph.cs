using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

public enum VoxButton
{
    None,
    Ball,
	OK,
    Count
}

[ExecuteInEditMode]
public class CommandGraph : MonoBehaviour
{
	public static Timing AllowInputEnd = new Timing(3, 3, 0);
	public static Timing SkillCutInTiming = new Timing(3, 3, 2);
	public static Timing WaitInputEnd = new Timing(0, 3, 0);

	#region editor params

	public GameObject CommandIconPrefab;
	public GameObject IntroCommandIconPrefab;
	public GameObject RevertCommandIconPrefab;
	public GameObject InvertCommandIconPrefab;
	public GameObject EdgePrefab;
	public GameObject CommandDataPrefab;
	public List<GameObject> SkillPrefabs;
	public List<BGAnimBase> BGAnims;

	public GameObject CommandSphere;
	public GameObject EdgeSphere;
	public GameObject SelectSpot;
	public GameObject AreaRect;

	public MidairPrimitive CurrentRect;
	public MidairPrimitive NextRect;
	
	public TextMesh CurrentCommandName;
	public GameObject NextRectEffect;

	public CommandExplanation CommandExp;
	public CommandListUI CommandList;
	public SkillCutIn SkillCutIn;

	public Vector3 MaxScale = new Vector3(0.24f, 0.24f, 0.24f);
	public float ScaleCoeff = 0.0f;
	public float MaskColorCoeff = 0.06f;
	public float MaskStartPos = 3.0f;
	public float SphereRadius = 6.5f;
	public float MAX_LATITUDE;
	public float ROTATE_COEFF;
	public float BUTTON_RADIUS;
	public bool UPDATE_BUTTON;

	public PlayerCommand IntroCommand;

	#endregion


	#region property

	public List<PlayerCommand> CommandNodes { get; private set; }
	public PlayerCommand NextCommand { get; private set; }
	public PlayerCommand CurrentCommand { get; private set; }
	public PlayerCommand OldCommand { get; private set; }
	public PlayerCommand PreviewCommand { get; private set; }
	public VoxButton CurrentButton { get; private set; }

	#endregion


	#region private params

	bool IsInvert { get { return CurrentCommand is InvertCommand; } }
	bool IsLastInvert { get { return IsInvert && (GameContext.LuxSystem.BreakTime == 1 || (CurrentCommand as InvertCommand).IsLast); } }
	int CommandLoopCount;
	float SphereLatitude
	{
		get
		{
			Quaternion up = Quaternion.LookRotation(Vector3.up, Vector3.up);
			Quaternion down = Quaternion.LookRotation(Vector3.down, Vector3.up);
			Quaternion rotUp = Quaternion.LookRotation(transform.up, Vector3.up);
			Quaternion rotDown = Quaternion.LookRotation(-transform.up, Vector3.up);
			return Mathf.Min(Quaternion.Angle(rotUp, up), Quaternion.Angle(rotDown, down), Quaternion.Angle(rotUp, down), Quaternion.Angle(rotDown, up));
		}
	}

	Vector3 oldMousePosition;
	Quaternion targetRotation;
	Quaternion offsetRotation;

	#endregion


	void Awake()
	{
		CommandNodes = new List<PlayerCommand>();
		CommandNodes.AddRange(GetComponentsInChildren<PlayerCommand>());
	}

	// Use this for initialization
	void Start()
	{
		offsetRotation = Quaternion.LookRotation(transform.position - SelectSpot.transform.position);
		CurrentButton = VoxButton.None;
		CurrentCommand = IntroCommand;

		CurrentRect.transform.parent = this.transform;
		CurrentRect.transform.localPosition = Vector3.zero;
		CurrentRect.transform.localScale = Vector3.zero;
		CurrentRect.transform.localRotation = Quaternion.identity;
		NextRect.transform.localScale = Vector3.zero;

		ColorManager.OnBaseColorChanged += this.OnBaseColorChanged;
	}


	#region initialize line & command params

	void InitializeLinks()
	{
		List<Pair<PlayerCommand, PlayerCommand>> commandPairs = new List<Pair<PlayerCommand, PlayerCommand>>();
		foreach( PlayerCommand command in CommandNodes )
		{
			command.SetLink(false);
			foreach( PlayerCommand link in command.links )
			{
				if( null == commandPairs.Find((Pair<PlayerCommand, PlayerCommand> pair) =>
						(pair.first == command && pair.second == link) || (pair.first == command && pair.second == link)) )
				{
					commandPairs.Add(new Pair<PlayerCommand, PlayerCommand>(link, command));
				}
			}
		}
		foreach( Pair<PlayerCommand, PlayerCommand> pair in commandPairs )
		{
			InstantiateLine(pair.first, pair.second);
		}
	}

	void InstantiateLine(PlayerCommand from, PlayerCommand to)
	{
		LineRenderer edge = (Instantiate(EdgePrefab) as GameObject).GetComponent<LineRenderer>();
		edge.transform.position = EdgeSphere.transform.position;
		edge.transform.parent = EdgeSphere.transform;
		edge.SetVertexCount(2);
		edge.SetPosition(0, from.transform.localPosition);
		edge.SetPosition(1, to.transform.localPosition);
		from.OnEdgeCreated(edge.GetComponent<CommandEdge>());
		to.OnEdgeCreated(edge.GetComponent<CommandEdge>());
	}

#if UNITY_EDITOR

	enum CommandListProperty
	{
		__LineStart,
		AcquireLevel,
		Name,
		EnglishName,
		Icon,
		Category,
		Longitude,
		Latitude,
		CommandLevel,
		RequireStar,
		InitialStar,
		Defend,
		Heal,
		Music,
		BGAnim,
		Skill1,
		Skill2,
		Skill3,
		Skill4,
		Link1,
		Link2,
		Link3,
		Link4,
		Link5,
		DescribeText,
		ExplanationText,
	}

	void UpdateCommandList()
	{
		CurrentRect.transform.parent = transform;
		NextRect.transform.parent = transform;
		foreach( PlayerCommand command in GetComponentsInChildren<PlayerCommand>() )
		{
			DestroyImmediate(command.gameObject);
		}
		foreach( LineRenderer commandEdge in EdgeSphere.GetComponentsInChildren<LineRenderer>() )
		{
			DestroyImmediate(commandEdge.gameObject);
		}
		CommandNodes = new List<PlayerCommand>();

		string path = Application.streamingAssetsPath + "/VQ3List - Command.csv";
		StreamReader reader = File.OpenText(path);
		if( reader != null )
		{
			Dictionary<PlayerCommand, string[]> LinkDictionary = new Dictionary<PlayerCommand, string[]>();
			PlayerCommand playerCommand = null;
			reader.ReadLine();
			string lines = reader.ReadToEnd();
			string[] lineSeparator = new string[] { "__" };
			string[] lineTexts = lines.Split(lineSeparator, System.StringSplitOptions.RemoveEmptyEntries);
			char[] separator = new char[] { ',' };
			foreach( string line in lineTexts )
			{
				string[] propertyTexts = line.Split(separator, System.StringSplitOptions.None);

				GameObject commandDataObj = Instantiate(CommandDataPrefab) as GameObject;
				PlayerCommandData commandData = commandDataObj.GetComponent<PlayerCommandData>();

				commandData.MusicBlockName = propertyTexts[(int)CommandListProperty.Music];
				commandData.DefendPercent = int.Parse(propertyTexts[(int)CommandListProperty.Defend]);
				commandData.HealPercent = int.Parse(propertyTexts[(int)CommandListProperty.Heal]);
				commandData.CommandLevel = int.Parse(propertyTexts[(int)CommandListProperty.CommandLevel]);
				commandData.RequireSP = int.Parse(propertyTexts[(int)CommandListProperty.RequireStar]);

				string[] skillTexts = new string[4];
				skillTexts[0] = propertyTexts[(int)CommandListProperty.Skill1];
				skillTexts[1] = propertyTexts[(int)CommandListProperty.Skill2];
				skillTexts[2] = propertyTexts[(int)CommandListProperty.Skill3];
				skillTexts[3] = propertyTexts[(int)CommandListProperty.Skill4];
				commandData._timingStr = "";
				commandData._skillList = new List<Skill>();
				foreach( string skillText in skillTexts )
				{
					if( skillText == "" ) break;
					string skillName = skillText.Substring(0, skillText.IndexOf(" "));
					Skill skill = SkillPrefabs.Find((GameObject obj) => obj.name == skillName).GetComponent<Skill>();
					if( skill != null )
					{
						commandData._skillList.Add(skill);
						commandData._timingStr += skillText.Substring(skillText.IndexOf(" ")) + ",";
					}
					else
					{
						Debug.LogError("Can't find " + skillText);
					}
				}
				commandData.DescribeText = propertyTexts[(int)CommandListProperty.DescribeText].Replace("\"", "");
				commandData.ExplanationText = propertyTexts[(int)CommandListProperty.ExplanationText].Replace("\"", "");

				string commandName = propertyTexts[(int)CommandListProperty.Name];
				if( commandName == "" && playerCommand  != null )
				{
					commandData.OwnerCommand = playerCommand;
					commandData.transform.parent = playerCommand.transform;
					commandData.transform.localPosition = Vector3.zero;
					commandData.DescribeText = playerCommand.commandData[0].DescribeText;
					commandData.ExplanationText = playerCommand.commandData[0].ExplanationText;
					commandData.name = playerCommand.name + commandData.CommandLevel.ToString();
					playerCommand.commandData.Add(commandData);
				}
				else
				{
					GameObject commandObj = null;
					string categoryName = propertyTexts[(int)CommandListProperty.Category];
					if( categoryName == "V" )
					{
						commandObj = Instantiate(InvertCommandIconPrefab) as GameObject;
					}
					else if( categoryName == "R" )
					{
						commandObj = Instantiate(RevertCommandIconPrefab) as GameObject;
					}
					else if( categoryName == "I" )
					{
						commandObj = Instantiate(IntroCommandIconPrefab) as GameObject;
						IntroCommand = commandObj.GetComponent<PlayerCommand>();
					}
					else
					{
						commandObj = Instantiate(CommandIconPrefab) as GameObject;
					}
					commandObj.name = commandName;
					commandObj.transform.parent = this.transform;
					playerCommand = commandObj.GetComponent<PlayerCommand>();
					CommandNodes.Add(playerCommand);
					LinkDictionary.Add(playerCommand, new string[] { 
                    propertyTexts[(int)CommandListProperty.Link1],propertyTexts[(int)CommandListProperty.Link2],propertyTexts[(int)CommandListProperty.Link3],
                    propertyTexts[(int)CommandListProperty.Link4],propertyTexts[(int)CommandListProperty.Link5] });
					playerCommand.acquireLevel = int.Parse(propertyTexts[(int)CommandListProperty.AcquireLevel]);
					playerCommand.longitude = int.Parse(propertyTexts[(int)CommandListProperty.Longitude]);
					playerCommand.latitude = int.Parse(propertyTexts[(int)CommandListProperty.Latitude]);
					playerCommand.nameText = propertyTexts[(int)CommandListProperty.EnglishName];
					playerCommand.iconStr = propertyTexts[(int)CommandListProperty.Icon];
					playerCommand.numSP = int.Parse(propertyTexts[(int)CommandListProperty.InitialStar]);
					if( propertyTexts[(int)CommandListProperty.BGAnim] != "" )
					{
						playerCommand.BGAnim = BGAnims.Find((BGAnimBase anim) => anim.name.StartsWith(propertyTexts[(int)CommandListProperty.BGAnim]));
					}

					playerCommand.themeColor = EThemeColor.White;
					if( playerCommand.iconStr.Contains('A') )
					{
						playerCommand.themeColor = EThemeColor.Blue;
					}
					else if( playerCommand.iconStr.Contains('W') )
					{
						playerCommand.themeColor = EThemeColor.Green;
					}
					else if( playerCommand.iconStr.Contains('F') )
					{
						playerCommand.themeColor = EThemeColor.Red;
					}
					else if( playerCommand.iconStr.Contains('L') )
					{
						playerCommand.themeColor = EThemeColor.Yellow;
					}


					playerCommand.ValidatePosition();

					commandData.OwnerCommand = playerCommand;
					commandData.transform.parent = playerCommand.transform;
					commandData.transform.localPosition = Vector3.zero;
					commandData.name = playerCommand.name + commandData.CommandLevel.ToString();
					playerCommand.commandData = new List<PlayerCommandData>();
					playerCommand.commandData.Add(commandData);
				}
			}

			foreach( PlayerCommand commandNode in CommandNodes )
			{
				if( !LinkDictionary.ContainsKey(commandNode) )
				{
					continue;
				}
				commandNode.links = new List<PlayerCommand>();
				foreach( string linkStr in LinkDictionary[commandNode] )
				{
					if( linkStr != "" )
					{
						PlayerCommand linkNode = CommandNodes.Find((PlayerCommand command) => command.name == linkStr);
						if( linkNode != null )
						{
							commandNode.links.Add(linkNode);
						}
					}
				}
			}

			InitializeLinks();
		}
		else
		{
			Debug.LogError(path + " not found!");
		}
	}

#endif

	#endregion


	// Update is called once per frame
	void Update()
	{
#if UNITY_EDITOR
		if( !UnityEditor.EditorApplication.isPlaying )
		{
			if( UPDATE_BUTTON )
			{
				UPDATE_BUTTON = false;
				UpdateCommandList();
			}
			return;
		}
#endif
		UpdateInput();

		switch( GameContext.BattleState )
		{
		case BattleState.Continue:
			break;
		case BattleState.Wait:
			if( Music.IsJustChangedWhen((Timing t) => t.GetTotalUnits(Music.Meter) % 16 == WaitInputEnd.GetTotalUnits(Music.Meter)) )
			{
				SetNextBlock();
			}
			if( NextCommand != null && Music.NextBlockName == NextCommand.GetBlockName() && Music.IsJustChangedWhen((Timing t) => t.GetTotalUnits(Music.Meter) % 16 == SkillCutInTiming.GetTotalUnits(Music.Meter) % 16) )
			{
				OnSkillCutIn();
			}
			CurrentRect.SetArc(0);
			CurrentRect.SetArc(0);
			break;
		case BattleState.Intro:
			if( Music.IsJustChangedAt(AllowInputEnd) /*|| (NextCommand != null && Music.IsJustChangedWhen((Timing t) => t.MusicalTime % 16 == WaitInputEnd.MusicalTime))*/ )
			{
				SetNextBlock();
			}
			if( NextCommand != null && Music.IsJustChangedAt(SkillCutInTiming) )
			{
				OnSkillCutIn();
			}
			CurrentRect.SetArc((float)(1.0f - Music.MusicalTime / LuxSystem.TurnMusicalUnits));
			break;
		case BattleState.Battle:
		case BattleState.Eclipse:
			if( Music.IsJustChangedAt(AllowInputEnd) )
			{
				SetNextBlock();
			}
			if( NextCommand != null && Music.IsJustChangedAt(SkillCutInTiming) )
			{
				OnSkillCutIn();
			}
			CurrentRect.SetArc((float)(1.0f - Music.MusicalTime / LuxSystem.TurnMusicalUnits));
			break;
		case BattleState.Endro:
			break;
		}
		CurrentRect.SetColor(ColorManager.Base.Front);
	}

	void OnSkillCutIn()
	{
		NextRect.SetArc(1);
		NextRect.SetSize(7.0f);
		NextRect.GetComponentsInChildren<MidairPrimitive>()[1].SetSize(7.0f);
		CurrentRect.SetArc(0);
		SkillCutIn.Set(NextCommand, CommandExp.SkillListUI, CommandExp.CommandName.gameObject);
	}


	#region update command

	void UpdateInput()
	{
		Ray ray = GameContext.MainCamera.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		Physics.Raycast(ray.origin, ray.direction, out hit, Mathf.Infinity);

		if( Input.GetMouseButtonDown(0) )
		{
			oldMousePosition = Input.mousePosition;
			if( hit.collider == CommandSphere.GetComponent<Collider>() )
			{
				if( GameContext.State != GameState.Battle || ( GameContext.BattleState == BattleState.Wait && NextCommand == null ) || Music.Just < AllowInputEnd )
				{
					CurrentButton = VoxButton.Ball;
					PushCommandButton(hit.point);
				}
			}
		}
		else if( Input.GetMouseButtonUp(0) )
		{
		}
		else if( Input.GetMouseButton(0) )
		{
			if( CurrentButton == VoxButton.Ball )
			{
				Quaternion oldRotation = transform.rotation;

				Vector3 deltaV = Input.mousePosition - oldMousePosition;
				transform.rotation *= (Quaternion.Inverse(transform.rotation)
                    * Quaternion.AngleAxis(deltaV.y * ROTATE_COEFF, Vector3.right)
                    * Quaternion.AngleAxis(deltaV.x * ROTATE_COEFF, Vector3.down) * transform.rotation);
				oldMousePosition = Input.mousePosition;

				SelectSpot.transform.parent = transform;
				transform.rotation = Quaternion.Inverse(Quaternion.LookRotation(-SelectSpot.transform.localPosition)) * offsetRotation;
				SelectSpot.transform.parent = transform.parent;
				if( SphereLatitude > MAX_LATITUDE )
				{
					transform.rotation = oldRotation;
				}
			}
		}
		else
		{
			CurrentButton = VoxButton.None;
			if( NextCommand != null || GameContext.State == GameState.Result )
			{
				transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, 0.1f);
				if( NextCommand == IntroCommand && Quaternion.Angle(transform.rotation, targetRotation) < 0.1f )
				{
					NextCommand = null;
					transform.rotation = targetRotation;
				}
			}
		}

		EdgeSphere.transform.rotation = transform.rotation;

		if( NextCommand != null )
		{
			if( GameContext.BattleState == BattleState.Wait || GameContext.BattleState == BattleState.Endro || GameContext.BattleState == BattleState.Continue )
			{
				NextRect.SetArc(-1);
			}
			else
			{
				NextRect.SetArc(-Music.MusicalTime / LuxSystem.TurnMusicalUnits);
			}
		}
		else if( hit.collider == CommandSphere.GetComponent<Collider>() )
		{
			PlayerCommand command = FindCommand(hit.point);
			if( command != null )
			{
				float commandCoeff = command.transform.localScale.x / 0.24f;
				if( command == CurrentCommand ) commandCoeff += 0.2f;
				NextRect.transform.position = Vector3.Lerp(NextRect.transform.position, command.transform.position + Vector3.back * 3, 0.2f);
				NextRect.transform.localScale = Vector3.Lerp(NextRect.transform.localScale, Vector3.one * commandCoeff, 0.2f);
			}
			else
			{
				NextRect.transform.position = Vector3.Lerp(NextRect.transform.position, hit.point + Vector3.back * 3, 0.2f);
				NextRect.transform.localScale = Vector3.Lerp(NextRect.transform.localScale, Vector3.one * 0.1f, 0.2f);
			}
			NextRect.transform.rotation = Quaternion.identity;
			NextRect.SetColor(ColorManager.Base.Front);
			NextRect.SetSize(1.5f);
			NextRect.SetWidth(0.2f);
			NextRect.GetComponentsInChildren<MidairPrimitive>()[1].SetSize(1.5f);
			NextRect.GetComponentsInChildren<MidairPrimitive>()[1].SetWidth(0.2f);
			NextRect.SetArc(GameContext.BattleState == BattleState.Wait ? -1.0f : -Music.MusicalTime / LuxSystem.TurnMusicalUnits);

			if( command != PreviewCommand )
			{
				PreviewCommand = command;
				if( command != null )
				{
					SEPlayer.Play("tick");
					CommandExp.Set(PreviewCommand, isPreview: true);
				}
				else
				{
					CommandExp.Reset();
				}
			}
		}
		else
		{
			NextRect.transform.localScale = Vector3.Lerp(NextRect.transform.localScale, Vector3.zero, 0.2f);
		}

		if( Music.IsJustChangedBar() )
		{
			AnimManager.AddAnim(CurrentRect.gameObject, 9.0f, ParamType.PrimitiveRadius, AnimType.Time, 0.05f);
			AnimManager.AddAnim(CurrentRect.gameObject, 7.0f, ParamType.PrimitiveRadius, AnimType.Time, 0.1f, 0.05f);
			AnimManager.AddAnim(CurrentRect.gameObject, 5.0f, ParamType.PrimitiveWidth, AnimType.Time, 0.05f);
			AnimManager.AddAnim(CurrentRect.gameObject, 3.0f, ParamType.PrimitiveWidth, AnimType.Time, 0.1f, 0.05f);
		}
	}

	PlayerCommand FindCommand(Vector3 position)
	{
		PlayerCommand ret = null;
		float minDistance = 99999;
		foreach( PlayerCommand command in GetLinkedCommands() )
		{
			if( command is IntroCommand ) continue;

			float d = (position - command.transform.position).magnitude;
			if( d < minDistance )
			{
				minDistance = d;
				ret = command;
			}
		}
		return (minDistance <= BUTTON_RADIUS ? ret : null);
	}

	void PushCommandButton(Vector3 position)
	{
		PlayerCommand selectedCommand = FindCommand(position);
		if( selectedCommand != null && selectedCommand != NextCommand )
		{
			Select(selectedCommand);
		} 
		else if( GameContext.State == GameState.Setting )
		{
			GameContext.PlayerConductor.OnDeselectedCommand();
		}
	}

	void SetNextBlock()
	{
		if( GameContext.BattleState == BattleState.Win )
		{
			return;
		}

		if( NextCommand == null || NextCommand == IntroCommand )
		{
			if( GameContext.PlayerConductor.IsEclipse && GameContext.LuxSystem.IsOverFlow )
			{
				foreach( PlayerCommand c in CurrentCommand.LinkedCommands )
				{
					if( c is InvertCommand )
					{
						Select(c);
						break;
					}
				}
			}
			else if( IsInvert && !IsLastInvert )
			{
				NextCommand = CurrentCommand;
				CommandExp.Set(NextCommand);
			}
			else
			{
				if( IsInvert ) CurrentCommand = IntroCommand;
				NextCommand = null;
				OldCommand = null;
				Music.SetNextBlock("wait", new System.EventHandler((object sender, System.EventArgs e) => { GameContext.BattleConductor.SetState(BattleState.Wait); }));
			}
		}

		if( NextCommand != null )
		{
			OldCommand = CurrentCommand;
			if( OldCommand == NextCommand )
			{
				CommandLoopCount++;
			}
			else
			{
				CommandLoopCount = 0;
			}
			//TODO: Add variation logic here( former block, parameter, etc... )

			Music.SetNextBlock(NextCommand.GetBlockName(),
				new System.EventHandler((object sender, System.EventArgs e) =>
				{
					if( NextCommand is RevertCommand )
					{
						GameContext.BattleConductor.SetState(BattleState.Eclipse);
					}
					else if( NextCommand is InvertCommand )
					{
						GameContext.LuxSystem.SetState(LuxState.Overload);
						GameContext.BattleConductor.SetState(BattleState.Battle);
					}
					else
					{
						GameContext.BattleConductor.SetState(BattleState.Battle);
					}
				}
				));
		}

	}

	IEnumerable<PlayerCommand> GetLinkedCommands()
	{
		if( GameContext.State == GameState.Setting )
		{
			foreach( PlayerCommand c in CommandNodes )
			{
				if( c.state != CommandState.DontKnow )
				{
					yield return c;
				}
			}
		}
		else if( GameContext.LuxSystem.State == LuxState.Overload && GameContext.LuxSystem.BreakTime == 1 )
		{
			foreach( PlayerCommand c in IntroCommand.LinkedCommands )
			{
				if( c != null && c.IsUsable() )
				{
					yield return c;
				}
			}
		}
		else
		{
			if( CurrentCommand.IsUsable() ) yield return CurrentCommand;
			foreach( PlayerCommand c in CurrentCommand.LinkedCommands )
			{
				if( c != null && c.IsUsable() )
				{
					yield return c;
				}
			}
		}
	}

	#endregion


	#region public functions

	public void OnExecCommand()
	{
		if( OldCommand != null )
		{
			OldCommand.SetLink(false);
			foreach( PlayerCommand c in OldCommand.LinkedCommands )
			{
				foreach( CommandEdge edge in c.linkLines )
				{
					if( edge.State < EdgeState.Unacquired )
					{
						edge.State = EdgeState.Unlinked;
					}
				}
				c.SetLink(false);
			}
		}
		if( NextCommand != null )
		{
			CurrentCommand = NextCommand;
			CurrentRect.transform.parent = CurrentCommand.transform;
			CurrentRect.transform.localPosition = Vector3.forward;
			CurrentRect.transform.localScale = Vector3.one;
			CurrentRect.transform.localRotation = Quaternion.identity;
			NextRect.transform.parent = transform;
			PreviewCommand = null;
			foreach( PlayerCommand c in GetLinkedCommands() )
			{
				c.SetLink(true);
			}
			NextCommand = null;

			if( IsLastInvert )
			{
				Select(IntroCommand);
				IntroCommand.SetCurrent();
				IntroCommand.SetLink(true);
				foreach( PlayerCommand c in IntroCommand.LinkedCommands )
				{
					c.SetLink(true);
				}
				CurrentRect.transform.parent = IntroCommand.transform;
				CurrentRect.transform.localPosition = Vector3.forward;
				CurrentRect.transform.localScale = Vector3.one;
				CurrentRect.transform.localRotation = Quaternion.identity;
			}

			Color themeColor = ColorManager.GetThemeColor(CurrentCommand.themeColor).Bright;
			CurrentCommandName.text = CurrentCommand.nameText.ToUpper();
			CurrentCommandName.color = themeColor;
			CurrentCommandName.transform.parent.GetComponent<Animation>().Play("CommandBarAnim");
			CommandList.OnExecCommand();
			CommandExp.Reset();
		}
		else
		{
			CurrentCommandName.text = "";
		}
	}

	public void ShowAcquireCommand(PlayerCommand command)
	{
		SEPlayer.Play("newCommand");
		targetRotation = Quaternion.Inverse(Quaternion.LookRotation(-command.transform.localPosition)) * offsetRotation;
		Select(command);
	}

	public PlayerCommand CheckAcquireCommand(int Level)
	{
		foreach( PlayerCommand command in CommandNodes )
		{
			if( command.acquireLevel <= Level && command.state == CommandState.DontKnow )
			{
				return command;
			}
		}
		return null;
	}

	public PlayerCommand CheckForgetCommand(int Level)
	{
		foreach( PlayerCommand command in CommandNodes )
		{
			if( command.acquireLevel > Level && command.state > CommandState.DontKnow )
			{
				return command;
			}
		}
		return null;
	}

	public void OnBattleStart()
	{
		foreach( PlayerCommand command in GetComponentsInChildren<PlayerCommand>() )
		{
			command.SetLink(false);
		}
		foreach( CommandEdge edge in EdgeSphere.GetComponentsInChildren<CommandEdge>() )
		{
			if( edge.IsUsable )
			{
				edge.State = EdgeState.Unlinked;
			}
			else
			{
				edge.State = EdgeState.DontKnow;
			}
		}
		OldCommand = null;
		NextCommand = null;
		CommandLoopCount = 0;
		Select(IntroCommand);
		OnExecCommand();
		transform.rotation = Quaternion.Inverse(Quaternion.LookRotation(-IntroCommand.transform.localPosition)) * offsetRotation;
		CurrentRect.transform.parent = IntroCommand.transform;
		CurrentRect.transform.localPosition = Vector3.forward;
		CurrentRect.transform.localScale = Vector3.one;
		CurrentRect.transform.localRotation = Quaternion.identity;

		float delay = 0;
		foreach( PlayerCommand command in GetComponentsInChildren<PlayerCommand>() )
		{
			Vector3 commandPosition = command.transform.localPosition;
			command.transform.localPosition = Vector3.zero;
			AnimManager.AddAnim(command.gameObject, commandPosition, ParamType.Position, AnimType.BounceIn, 0.5f, delay);
			delay += 0.07f;
		}

		CommandExp.OnBattleStart();
	}

	public void OnShieldBreak()
	{
		foreach( PlayerCommand linkedCommand in GetLinkedCommands() )
		{
			if( linkedCommand is RevertCommand )
			{
				linkedCommand.SetLink(true);
				foreach( CommandEdge edge in linkedCommand.linkLines )
				{
					if( edge.GetOtherSide(linkedCommand) == CurrentCommand )
					{
						edge.State = EdgeState.Linked;
						break;
					}
				}
				break;
			}
		}
	}

	public void OnShieldRecover()
	{
		foreach( PlayerCommand linkedCommand in CurrentCommand.LinkedCommands )
		{
			if( linkedCommand is RevertCommand )
			{
				linkedCommand.SetLink(false);
				foreach( CommandEdge edge in linkedCommand.linkLines )
				{
					if( edge.GetOtherSide(linkedCommand) == CurrentCommand )
					{
						edge.State = EdgeState.Unlinked;
						break;
					}
				}
				break;
			}
		}
	}

	public void OnEndro()
	{
	}

	public void OnEnterContinue()
	{
	}

	public void OnEnterSetting()
	{
		CurrentRect.transform.localScale = Vector3.zero;
		CheckLinkedFromIntro();
		Deselect();
	}

	public void OnBaseColorChanged(BaseColor Base)
	{
		if( NextCommand == null )
		{
		}
	}

	public void Deselect()
	{
		if( NextCommand != null )
		{
			NextCommand.Deselect();
			foreach( PlayerCommand linkedCommand in GetLinkedCommands() )
			{
				linkedCommand.SetLink(true);
			}
			NextCommand = null;
			NextRect.transform.localScale = Vector3.zero;
			CommandList.DeleteCommand();
		}
	}

	public void Select(PlayerCommand command)
	{
		if( NextCommand != null )
		{
			CommandList.DeleteCommand();
		}
		CommandList.AddCommand(command);
		NextCommand = command;
		foreach(PlayerCommand notSelectedCommand in GetLinkedCommands())
		{
			if( notSelectedCommand != NextCommand )
				notSelectedCommand.Deselect();
		}
		NextCommand.Select();
		CommandExp.Set(NextCommand);
		NextRect.transform.parent = NextCommand.transform;
		NextRect.transform.localPosition = Vector3.forward;
		NextRect.transform.localScale = Vector3.one;
		NextRect.transform.localRotation = Quaternion.identity;
		if( NextCommand == CurrentCommand )
		{
			NextRect.SetSize(10.0f);
			NextRect.GetComponentsInChildren<MidairPrimitive>()[1].SetSize(10.0f);
		}
		else
		{
			NextRect.SetSize(7.0f);
			NextRect.GetComponentsInChildren<MidairPrimitive>()[1].SetSize(7.0f);
		}
		NextRect.SetWidth(3.0f);
		NextRect.GetComponentsInChildren<MidairPrimitive>()[1].SetWidth(3.0f);
		Color themeColor = ColorManager.GetThemeColor(NextCommand.themeColor).Bright;
		GameObject effect = Instantiate(NextRectEffect, NextRect.transform.position, Quaternion.identity) as GameObject;
		effect.transform.parent = NextRect.transform;
		effect.transform.localPosition = Vector3.forward;
		effect.transform.localScale = Vector3.one;
		effect.transform.localRotation = Quaternion.identity;
		foreach( MidairPrimitive primitive in effect.GetComponentsInChildren<MidairPrimitive>() )
		{
			primitive.SetColor(themeColor);
		}
		
		targetRotation = Quaternion.Inverse(Quaternion.LookRotation(-command.transform.localPosition)) * offsetRotation;
		if( command != IntroCommand ) SEPlayer.Play("select");

		GameContext.PlayerConductor.OnSelectedCommand(command);
	}

	public void SelectInitialInvertCommand()
	{
		if( NextCommand == null || NextCommand is InvertCommand == false )
		{
			if( NextCommand != null )
			{
				Deselect();
			}
			foreach( PlayerCommand c in CurrentCommand.LinkedCommands )
			{
				if( c is InvertCommand )
				{
					if( NextCommand != c ) Select(c);
					break;
				}
			}
		}
	}

	public void CheckLinkedFromIntro()
	{
		IntroCommand.GetComponent<Renderer>().enabled = true;
		foreach( CommandEdge line in IntroCommand.linkLines )
		{
			line.SetEnabled(true);
		}
		foreach( CommandEdge commandEdge in EdgeSphere.GetComponentsInChildren<CommandEdge>() )
		{
			if( commandEdge.Command1.state == CommandState.DontKnow || commandEdge.Command2.state == CommandState.DontKnow )
			{
				commandEdge.State = EdgeState.DontKnow;
			}
			else
			{
				commandEdge.State = EdgeState.Unacquired;
			}
		}
		foreach( CommandEdge commandEdge in IntroCommand.linkLines )
		{
			if( commandEdge.GetOtherSide(IntroCommand).currentLevel > 0 )
			{
				commandEdge.State = EdgeState.Linked;
				commandEdge.GetOtherSide(IntroCommand).SetLinkedFromIntro();
			}
		}
	}

	#endregion
}
