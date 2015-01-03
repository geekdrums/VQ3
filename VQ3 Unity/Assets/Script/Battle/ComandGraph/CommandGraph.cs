using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

public enum VoxButton
{
    None,
    Ball,
	OK,
    Count
}

[ExecuteInEditMode]
public class CommandGraph : MonoBehaviour {

	public static Timing AllowInputEnd = new Timing(3, 3, 3);
	public static Timing WaitInputEnd = new Timing(0, 3, 3);

	public GameObject CommandIconPrefab;
	public GameObject IntroCommandIconPrefab;
    public GameObject RevertCommandIconPrefab;
    public GameObject InvertCommandIconPrefab;
    public GameObject EdgePrefab;
	public GameObject CommandDataPrefab;
    public List<GameObject> SkillPrefabs;

    public GameObject CommandSphere;
    public GameObject EdgeSphere;
    public GameObject SelectSpot;
    public GameObject AreaRect;

    public MidairPrimitive CurrentRect;
    public MidairPrimitive TouchRect;
	public MidairPrimitive NextRect;

    public CommandPanel Panel;

    public Vector3 MaxScale = new Vector3( 0.24f, 0.24f, 0.24f );
    public float ScaleCoeff = 0.05f;
    public float MaskColorCoeff = 0.06f;
    public float MaskStartPos = 3.0f;
    public float SphereRadius = 6.5f;
    public float TouchRectCoeff = 1.7f;
    public float MAX_LATITUDE;
    public float ROTATE_COEFF;
    public float BUTTON_RADIUS;
    public bool UPDATE_BUTTON;
    public MidairPrimitive AxisRing;
	public TextMesh ButtonText;

    public PlayerCommand IntroCommand;
    public List<PlayerCommand> CommandNodes { get; private set; }
    public PlayerCommand NextCommand { get; private set; }
    public PlayerCommand PushingCommand { get; private set; }
    public PlayerCommand CurrentCommand { get; private set; }
    public PlayerCommand OldCommand { get; private set; }
    public VoxButton CurrentButton { get; private set; }

    bool IsInvert { get { return CurrentCommand is InvertCommand; } }
    bool IsLastInvert { get { return IsInvert && (GameContext.VoxSystem.InvertTime == 1 || (CurrentCommand as InvertCommand).IsLast); } }
	bool IsOKButtonVisible { get { return GameContext.CurrentState == GameState.Endro || GameContext.CurrentState == GameState.Result || GameContext.CurrentState == GameState.Continue; } }
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

    Vector3 ballTouchStartPosition;
    Vector3 oldMousePosition;
    Quaternion targetRotation;
    Quaternion offsetRotation;

	// Use this for initialization
	void Start () {
        CommandNodes = new List<PlayerCommand>();
        CommandNodes.AddRange( GetComponentsInChildren<PlayerCommand>() );
        offsetRotation = Quaternion.LookRotation( transform.position - SelectSpot.transform.position );
        CurrentButton = VoxButton.None;
        CurrentCommand = IntroCommand;

        CurrentRect.transform.parent = this.transform;
        CurrentRect.transform.localPosition = Vector3.zero;
		CurrentRect.transform.localScale = Vector3.zero;
		CurrentRect.transform.localRotation = Quaternion.identity;
		NextRect.transform.localScale = Vector3.zero;
		TouchRect.SetColor(Color.clear);
		ButtonText.text = "";
    }

    void InitializeLinks()
    {
        List<Pair<PlayerCommand, PlayerCommand>> commandPairs = new List<Pair<PlayerCommand, PlayerCommand>>();
        foreach( PlayerCommand command in CommandNodes )
        {
            command.SetLink( false );
            foreach( PlayerCommand link in command.links )
            {
                if( null == commandPairs.Find( ( Pair<PlayerCommand, PlayerCommand> pair ) =>
                        (pair.first == command && pair.second == link) || (pair.first == command && pair.second == link) ) )
                {
                    commandPairs.Add( new Pair<PlayerCommand, PlayerCommand>( link, command ) );
                }
            }
        }
		//foreach( PlayerCommand link in IntroCommand.links )
		//{
		//	commandPairs.Add( new Pair<PlayerCommand, PlayerCommand>( link, IntroCommand ) );
		//}
        foreach( Pair<PlayerCommand, PlayerCommand> pair in commandPairs )
        {
            InstantiateLine( pair.first, pair.second );
        }
    }

    void InstantiateLine( PlayerCommand from, PlayerCommand to )
    {
        LineRenderer edge = (Instantiate( EdgePrefab ) as GameObject).GetComponent<LineRenderer>();
        edge.transform.position = EdgeSphere.transform.position;
        edge.transform.parent = EdgeSphere.transform;
        edge.SetVertexCount( 2 );
        edge.SetPosition( 0, from.transform.localPosition );
        edge.SetPosition( 1, to.transform.localPosition );
		from.OnEdgeCreated(edge.GetComponent<CommandEdge>());
		to.OnEdgeCreated(edge.GetComponent<CommandEdge>());
        //edge.SetVertexCount( 8 );
        //for( int i = 0; i < 8; i++ )
        //{
        //    edge.SetPosition( i, transform.rotation * Vector3.Slerp( from.Transform().localPosition, to.Transform().localPosition, (3 + i) / 12.0f ) );
        //}
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
            DestroyImmediate( command.gameObject );
        }
        foreach( LineRenderer commandEdge in EdgeSphere.GetComponentsInChildren<LineRenderer>() )
        {
            DestroyImmediate( commandEdge.gameObject );
		}
		CommandNodes = new List<PlayerCommand>();

        string path = Application.streamingAssetsPath + "/VQ3List - Command.csv";
        StreamReader reader = File.OpenText( path );
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
					Skill skill = SkillPrefabs.Find(( GameObject obj ) => obj.name == skillName).GetComponent<Skill>();
					if( skill != null )
					{
						commandData._skillList.Add(skill);
						commandData._timingStr += skillText.Substring(skillText.IndexOf(" ")) + ",";
					}
					else
					{
						Debug.Log("Can't find " + skillText);
					}
				}
				commandData.DescribeText = propertyTexts[(int)CommandListProperty.DescribeText].Replace("\"","");
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
					playerCommand.numSP = int.Parse( propertyTexts[(int)CommandListProperty.InitialStar] );

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
                        PlayerCommand linkNode = CommandNodes.Find( ( PlayerCommand command ) => command.name == linkStr );
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
            Debug.LogError( path + " not found!" );
        }
    }
#endif

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


        switch( GameContext.CurrentState )
        {
		case GameState.Continue:
			if( Music.IsJustChangedAt(0) )
			{
				ShowOKButton();
				ButtonText.text = "Continue";
			}
            break;
		case GameState.Intro:
			if( Music.IsJustChangedAt(0) )
			{
				AxisRing.SetWidth(0.8f);
				ButtonText.text = "";
			}
			//if( Music.NextBlockName == "intro" && Music.Just.totalUnit > 4 )
			//{
			//	//if( Music.isJustChanged ) SelectNearestNode();
			//	if( Music.isJustChanged && NextCommand != null && NextCommand != IntroCommand && !Input.GetMouseButton( 0 ) )
			//	{
			//		SetNextBlock();
			//	}
			//}
            if( Music.IsJustChangedAt( AllowInputEnd ) )
            {
                SetNextBlock();
            }
			AxisRing.SetArc( (float)(1.0f - Music.MusicalTime / 64.0) );
            break;
        case GameState.Battle:
            if( Music.IsJustChangedAt( AllowInputEnd ) ||
                (Music.CurrentBlockName == "wait" && Music.IsJustChangedAt( WaitInputEnd )) )
            {
                SetNextBlock();
            }
			if( Music.CurrentBlockName == "wait" )
			{
				AxisRing.SetArc( 0.0f );
			}
			else
			{
				AxisRing.SetArc( (float)(1.0f - Music.MusicalTime / 64.0) );
			}
            break;
		case GameState.Endro:
			if( Music.IsJustChangedAt(0) )
			{
				ShowOKButton();
				ButtonText.text = "OK";
			}
			break;
		case GameState.SetMenu:
			if( Music.IsJustChangedAt(0) )
			{
				AxisRing.SetWidth(0.8f);
				AxisRing.SetTargetArc(0.0f);
				ButtonText.text = "";
			}
			break;
        }
		if( NextCommand != null )
		{
			NextRect.SetSize(6 + Music.MusicalSin(4));
			NextRect.SetColor(Color.Lerp(Color.white, Color.clear, Music.MusicalSin(4) * 0.5f));
		}
    }

    void UpdateInput()
    {
        Ray ray = GameContext.MainCamera.ScreenPointToRay( Input.mousePosition );
        RaycastHit hit;
        Physics.Raycast( ray.origin, ray.direction, out hit, Mathf.Infinity );

        if( Input.GetMouseButtonDown( 0 ) )
		{
			CurrentButton = VoxButton.None;
			if( IsOKButtonVisible )
			{
				if( hit.collider == CommandSphere.collider )
				{
					CurrentButton = VoxButton.OK;
				}
			}
			else
			{
				oldMousePosition = Input.mousePosition;
				if( hit.collider == CommandSphere.collider && GameContext.VoxSystem.IsInverting == false )
				{
					CurrentButton = VoxButton.Ball;
					if( Panel.state != CommandPanel.State.Show )
					{
						PushCommandButton(hit.point);
					}
					ballTouchStartPosition = hit.point;
				}
			}
        }
        else if( Input.GetMouseButtonUp( 0 ) )
        {
			if( IsOKButtonVisible && hit.collider == CommandSphere.collider  && CurrentButton == VoxButton.OK )
			{
				PushOKButton();
			}
			else if( CurrentButton == VoxButton.Ball && (Music.Just < AllowInputEnd || GameContext.CurrentState == GameState.SetMenu) )
			{
				if( Panel.state == CommandPanel.State.Show )
				{
					PushingCommand = null;
					Panel.Hide();
				}
				else if( PushingCommand != null )
				{
					TouchRect.GrowSize = 0;
					TouchRect.SetSize(0);
					TouchRect.SetWidth(0);
					PlayerCommand pushCommand = PushingCommand;
					//if( PushingCommand == CurrentCommand )
					//{
					//	pushCommand = PushingCommand.FindLoopVariation(CommandLoopCount+1);
					//}
					if( NextCommand != null )
					{
						if( GameContext.CurrentState != GameState.Intro )
						{
							NextCommand.Deselect();
							NextCommand = null;
							Panel.Show(TouchRect.transform.position, pushCommand);
							NextRect.transform.localScale = Vector3.zero;
						}
					}
					else
					{
						Panel.Show(TouchRect.transform.position, pushCommand);
					}
				}
			}
			else if( hit.collider == AreaRect.collider )
			{
				if( Panel.state == CommandPanel.State.Show )
				{
					Panel.Hide();
				}
			}
        }
        else if( Input.GetMouseButton( 0 ) )
        {
			if( IsOKButtonVisible )
			{
				if( hit.collider == CommandSphere.collider && CurrentButton == VoxButton.OK )
				{
					AxisRing.SetTargetSize(7.0f);
					AxisRing.SetTargetColor(ColorManager.Base.Light);
					if( AxisRing.Width > 1.0f )
					{
						ButtonText.color = Color.black;
					}
				}
				else
				{
					AxisRing.SetTargetSize(7.5f);
					AxisRing.SetTargetColor(ColorManager.Base.Shade);
					if( AxisRing.Width > 1.0f )
					{
						ButtonText.color = Color.white;
					}
				}
			}
            else if( CurrentButton == VoxButton.Ball )
            {
                Quaternion oldRotation = transform.rotation;

                Vector3 deltaV = Input.mousePosition - oldMousePosition;
                transform.rotation *= (Quaternion.Inverse( transform.rotation )
                    * Quaternion.AngleAxis( deltaV.y * ROTATE_COEFF, Vector3.right )
                    * Quaternion.AngleAxis( deltaV.x * ROTATE_COEFF, Vector3.down ) * transform.rotation);
                oldMousePosition = Input.mousePosition;

                SelectSpot.transform.parent = transform;
                transform.rotation = Quaternion.Inverse( Quaternion.LookRotation( -SelectSpot.transform.localPosition ) ) * offsetRotation;
                SelectSpot.transform.parent = transform.parent;
				if( SphereLatitude > MAX_LATITUDE )
				{
                    transform.rotation = oldRotation;
                }

                //touch rect
                Vector3 rectPos = new Vector3( hit.point.x, hit.point.y, TouchRect.transform.position.z ) - TouchRect.transform.parent.position;
                rectPos.x = Mathf.Clamp( rectPos.x, -AxisRing.Radius + TouchRect.Radius * TouchRectCoeff, AxisRing.Radius - TouchRect.Radius * TouchRectCoeff );
                rectPos.y = Mathf.Clamp( rectPos.y, -AxisRing.Radius + TouchRect.Radius * TouchRectCoeff, AxisRing.Radius - TouchRect.Radius * TouchRectCoeff );
                TouchRect.transform.localPosition = rectPos;

                if( (ballTouchStartPosition - hit.point).magnitude > BUTTON_RADIUS / 2 )
                {
                    if( PushingCommand != null )
                    {
                        PushingCommand.SetPush( false );
                        PushingCommand = null;
                        TouchRect.GrowSize = 0;
                        TouchRect.SetTargetSize( 2.0f );
                        TouchRect.SetTargetWidth( 0 );
                        //TouchRect.GetComponentInChildren<TextMesh>().text = "";
                    }
                    else if( Panel.state == CommandPanel.State.Show )
                    {
                        Panel.Hide();
                    }
                }
            }
        }
        else
        {
            CurrentButton = VoxButton.None;
			if( NextCommand != null || GameContext.CurrentState == GameState.Result )
            {
				transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, 0.1f);
				if( NextCommand == IntroCommand && Quaternion.Angle(transform.rotation,targetRotation) < 0.1f )
				{
					NextCommand = null;
					transform.rotation = targetRotation;
				}
            }
        }

        EdgeSphere.transform.rotation = transform.rotation;
    }

    void PushCommandButton( Vector3 pushingPosition )
    {
        PushingCommand = null;
        PlayerCommand selectedCommand = null;
        float minDistance = 99999;
		foreach( PlayerCommand command in GetLinkedCommands() )
        {
            float d = (pushingPosition - command.transform.position).magnitude;
            if( d < minDistance )
            {
                minDistance = d;
                selectedCommand = command;
            }
        }
        if( selectedCommand != null && minDistance <= BUTTON_RADIUS )
        {
            selectedCommand.SetPush( true );
            PushingCommand = selectedCommand;
            SEPlayer.Play( "tickback" );
            TouchRect.SetColor( ColorManager.GetThemeColor( PushingCommand.themeColor ).Light );
            TouchRect.GrowSize = 0.2f;
            TouchRect.SetAnimationWidth( 2.0f, 0.1f );
            TouchRect.SetAnimationSize( 2.0f, 4.17f );
            Vector3 rectPos = new Vector3( pushingPosition.x, pushingPosition.y, TouchRect.transform.position.z ) - TouchRect.transform.parent.position;
            rectPos.x = Mathf.Clamp( rectPos.x, -AxisRing.Radius + TouchRect.Radius * TouchRectCoeff, AxisRing.Radius - TouchRect.Radius * TouchRectCoeff );
            rectPos.y = Mathf.Clamp( rectPos.y, -AxisRing.Radius + TouchRect.Radius * TouchRectCoeff, AxisRing.Radius - TouchRect.Radius * TouchRectCoeff );
            TouchRect.transform.localPosition = rectPos;
        }
    }

	void PushOKButton()
	{
		AxisRing.SetTargetSize(7.5f);
		AxisRing.SetTargetWidth(7.5f);
		AxisRing.SetTargetColor(ColorManager.Base.Shade);
		ButtonText.color = Color.white;
		switch( GameContext.CurrentState )
		{
		case GameState.Endro:
			if( !Music.IsPlaying() || Music.Just.totalUnit > 8 )
			{
				GameContext.BattleConductor.ClearSkills();
				GameContext.ChangeState(GameState.Result);
				ColorManager.SetBaseColor(EBaseColor.Black);
				CurrentRect.transform.localScale = Vector3.zero;
				NextRect.transform.localScale = Vector3.zero;
			}
			break;
		case GameState.Result:
			GameContext.PlayerConductor.ProceedResult();
			if( GameContext.CurrentState == GameState.SetMenu )
			{
				NextCommand = null;
				CurrentRect.transform.localScale = Vector3.zero;
				NextRect.transform.localScale = Vector3.zero;
			}
			break;
		case GameState.Continue:
			if( !Music.IsPlaying() || Music.Just.totalUnit > 4 )
			{
				GameContext.ChangeState(GameState.SetMenu);
				GameContext.FieldConductor.OnPlayerLose();
				NextCommand = null;
				CurrentRect.transform.localScale = Vector3.zero;
				NextRect.transform.localScale = Vector3.zero;
			}
			break;
		}
	}

	void ShowOKButton()
	{
		AxisRing.SetTargetWidth(7.5f);
		AxisRing.SetTargetArc(1.0f);
		AxisRing.SetColor(ColorManager.Base.Shade);
		Panel.Hide();
		CommandSphere.collider.enabled = true;
	}

	public void ShowAcquireCommand( PlayerCommand command )
	{
		targetRotation = Quaternion.Inverse(Quaternion.LookRotation(-command.transform.localPosition)) * offsetRotation;
		AxisRing.SetTargetWidth(0.8f);
		ButtonText.color = Color.clear;
	}

    IEnumerable<PlayerCommand> GetLinkedCommands()
    {
		if( GameContext.CurrentState == GameState.SetMenu )
		{
			foreach( PlayerCommand c in CommandNodes )
			{
				if( c.state != CommandState.DontKnow )
				{
					yield return c;
				}
			}
		}
        else if( GameContext.VoxSystem.state == VoxState.Invert && GameContext.VoxSystem.InvertTime == 1 )
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

    void SetNextBlock()
    {
        if( Music.NextBlockName == "endro" )
        {
            return;
        }

        if( NextCommand == null || NextCommand == IntroCommand )
        {
            if( GameContext.VoxSystem.state == VoxState.Eclipse && GameContext.VoxSystem.IsReadyEclipse )
            {
                foreach( PlayerCommand c in CurrentCommand.LinkedCommands )
                {
                    if( c is InvertCommand )
                    {
                        Select( c );
                        break;
                    }
                }
            }
            else if( IsInvert && !IsLastInvert )
            {
				NextCommand = CurrentCommand;
            }
            else
            {
                if( IsInvert ) CurrentCommand = IntroCommand;
                NextCommand = null;
                OldCommand = null;
                Music.SetNextBlock( "wait" );
            }
        }

        if( NextCommand != null )
        {
            OldCommand = CurrentCommand;
            if( OldCommand == NextCommand )
            {
                CommandLoopCount++;
                //NextCommand = NextCommand.FindLoopVariation( CommandLoopCount );
                //if( NextCommand == null ) Debug.LogError( OldCommand.name + CommandLoopCount.ToString() + "variation not found!" );
            }
            else
            {
                CommandLoopCount = 0;
            }
            //TODO: Add variation logic here( former block, parameter, etc... )

            Music.SetNextBlock( NextCommand.GetBlockName() );
        }
        
    }

    public void CheckCommand()
    {
        EThemeColor themeColor = CurrentCommand.themeColor;
        if( OldCommand != null )
        {
            OldCommand.SetLink( false );
            foreach( PlayerCommand c in OldCommand.LinkedCommands )
            {
				foreach( CommandEdge edge in c.linkLines )
				{
					if( edge.State < EdgeState.Unacquired )
					{
						edge.State = EdgeState.Unlinked;
					}
				}
                c.SetLink( false );
            }
        }
        if( NextCommand != null )
        {
            CurrentCommand = NextCommand;
            themeColor = CurrentCommand.themeColor;
            CurrentRect.transform.parent = CurrentCommand.transform;
            CurrentRect.transform.localPosition = Vector3.forward;
			CurrentRect.transform.localScale = Vector3.one;
			CurrentRect.transform.localRotation = Quaternion.identity;
			NextRect.transform.localScale = Vector3.zero;
            foreach( PlayerCommand c in GetLinkedCommands() )
            {
				c.SetLink(true);
            }
            CurrentCommand.SetCurrent();
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
			if( Music.CurrentBlockName != "wait" )
			{
				AxisRing.SetAnimationSize(6.7f, 7.5f);
				Panel.Hide();
			}
			CommandSphere.collider.enabled = true;
        }

        VoxState desiredState = GetDesiredVoxState();
        if( GameContext.VoxSystem.state != desiredState )
        {
            GameContext.VoxSystem.SetState( desiredState );
        }
		ColorManager.SetThemeColor(themeColor);

		if( CurrentCommand is InvertCommand )
		{
			AxisRing.SetColor(Color.black);
		}
		else
		{
			AxisRing.SetColor(ColorManager.Theme.Bright);
		}
    }

    public PlayerCommand CheckAcquireCommand( int Level )
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
    public PlayerCommand CheckForgetCommand( int Level )
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
            command.SetLink( false );
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
        Select( IntroCommand );
        CheckCommand();
		transform.rotation = Quaternion.Inverse(Quaternion.LookRotation(-IntroCommand.transform.localPosition)) * offsetRotation;
		CurrentRect.transform.parent = IntroCommand.transform;
		CurrentRect.transform.localPosition = Vector3.forward;
		CurrentRect.transform.localScale = Vector3.one;
		CurrentRect.transform.localRotation = Quaternion.identity;
    }

	public void Deselect()
	{
		NextCommand.Deselect();
		NextCommand = null;
		CommandSphere.collider.enabled = true;
		NextRect.transform.localScale = Vector3.zero;
	}

    public void Select( PlayerCommand command )
    {
		if( command != IntroCommand )
		{
			CommandSphere.collider.enabled = false;
		}
		if( ( CurrentCommand == IntroCommand || ( CurrentCommand is InvertCommand /*&& (CurrentCommand as InvertCommand).IsLast */) ) && command != IntroCommand )
		{
			IntroCommand.renderer.enabled = false;
			foreach( CommandEdge line in IntroCommand.linkLines )
			{
				line.SetEnabled(false);
			}
		}
		else if( command == IntroCommand )
		{
			IntroCommand.renderer.enabled = true;
			foreach( CommandEdge line in IntroCommand.linkLines )
			{
				line.SetEnabled(true);
			}
		}
        if( NextCommand != null ) NextCommand.Deselect();
        NextCommand = command;
		NextCommand.Select();
		NextRect.transform.parent = NextCommand.transform;
		NextRect.transform.localPosition = Vector3.forward;
		NextRect.transform.localScale = Vector3.one;
		NextRect.transform.localRotation = Quaternion.identity;

        targetRotation = Quaternion.Inverse( Quaternion.LookRotation( -command.transform.localPosition ) ) * offsetRotation;
		if( command != IntroCommand ) SEPlayer.Play("select");
    }

	public void SelectInitialInvertCommand()
	{
		if( NextCommand != null )
		{
			Deselect();
		}
		foreach( PlayerCommand c in CurrentCommand.LinkedCommands )
		{
			if( c is InvertCommand )
			{
				if( NextCommand != c ) Panel.Show(SelectSpot.transform.position, c);
				break;
			}
		}
		//CommandSphere.collider.enabled = false;
	}

    VoxState GetDesiredVoxState()
    {
        if( !GameContext.PlayerConductor.CanUseInvert || Music.CurrentBlockName == "wait" )
        {
            return VoxState.Sun;
        }
        else if( CurrentCommand is RevertCommand )
        {
            return VoxState.Eclipse;
        }
        else if( IsInvert )
        {
            return VoxState.Invert;
        }
        else
        {
            return VoxState.Sun;
        }
    }

	public void CheckLinkedFromIntro()
	{
		IntroCommand.renderer.enabled = true;
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
}
