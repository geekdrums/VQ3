using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public enum ResultState
{
	StarPoint,
    Command,
    End
}
public enum EncounterListProperty
{
    Level,
    Enemy1,
    Enemy2,
    Enemy3,
    State1,
    State2,
    State3,
    Turn,
}

[ExecuteInEditMode]
public class FieldConductor : MonoBehaviour {

    [System.Serializable]
    public class LevelEncounter
    {
        public List<Encounter> Encounters = new List<Encounter>();
    }

    public List<GameObject> EnemyPrefabs;
    public bool UPDATE_BUTTON;
    public float guiHeight;
    public Color guiColor;
	public CommandExplanation CommandExp;

    public int encounterCount;
    public bool UseDebugPlay;
    int targetPlayerLevel;
    int targetEncounterCount;
    LevelEncounter[] LevelEncounters;

    public ResultState RState { get; private set; }
    LevelEncounter CurrentLevel { get { return LevelEncounters[GameContext.PlayerConductor.Level - 1]; } }

	// Use this for initialization
    void Start()
    {
		GameContext.FieldConductor = this;

        LevelEncounters = new LevelEncounter[50];
        for( int i = 0; i < 50; i++ )
        {
            LevelEncounters[i] = new LevelEncounter();
        }
        foreach( Encounter encounter in GetComponentsInChildren<Encounter>() )
        {
            LevelEncounters[encounter.Level-1].Encounters.Add( encounter );
        }


#if UNITY_EDITOR
        if( UnityEditor.EditorApplication.isPlaying )
        {
            targetPlayerLevel = GameContext.PlayerConductor.Level;
			targetEncounterCount = encounterCount;
        }
#endif
	}

    void OnGUI()
    {
        if( UseDebugPlay )
        {
            GUI.color = guiColor;
            GUI.Label( new Rect( 0, guiHeight, 50, 20 ), "Level" );
            if( GUI.Button( new Rect( 50, guiHeight, 20, 20 ), "<" ) )
            {
                --targetPlayerLevel;
            }
            GUI.Label( new Rect( 80, guiHeight, 30, 20 ), targetPlayerLevel.ToString() );
            if( GUI.Button( new Rect( 100, guiHeight, 20, 20 ), ">" ) )
            {
                ++targetPlayerLevel;
            }
            targetPlayerLevel = Mathf.Clamp( targetPlayerLevel, 1, 50 );

            GUI.Label( new Rect( 150, guiHeight, 50, 20 ), "Encount" );
            if( GUI.Button( new Rect( 200, guiHeight, 20, 20 ), "<" ) )
            {
                --targetEncounterCount;
            }
            GUI.Label( new Rect( 230, guiHeight, 30, 20 ), targetEncounterCount.ToString() );
            if( GUI.Button( new Rect( 250, guiHeight, 20, 20 ), ">" ) )
            {
                ++targetEncounterCount;
            }
            int maxEncounterCount = LevelEncounters[targetPlayerLevel].Encounters.Count;
            if( maxEncounterCount > 0 )
            {
                targetEncounterCount += maxEncounterCount;
                targetEncounterCount %= maxEncounterCount;
            }
            else
            {
                targetEncounterCount = 0;
            }

            if( GUI.Button( new Rect( 300, guiHeight, 50, 20 ), "Play" ) )
            {
                GameContext.EnemyConductor.OnPlayerWin();
                GameContext.PlayerConductor.OnPlayerWin();
                GameContext.PlayerConductor.Level = targetPlayerLevel;
                GameContext.PlayerConductor.OnLevelUp();
                GameContext.PlayerConductor.CheckAcquireCommands();
                encounterCount = targetEncounterCount;
                CheckEncount();
            }
        }
    }

	// Update is called once per frame
    void Update()
    {
#if UNITY_EDITOR
        if( !UnityEditor.EditorApplication.isPlaying )
        {
            if( UPDATE_BUTTON )
            {
                UPDATE_BUTTON = false;
                UpdateEncounterList();
            }
            return;
        }
#endif

        switch( GameContext.CurrentState )
        {
		case GameState.Init:
			if( GameContext.PlayerConductor.Level == 1 )
			{
				GameContext.ChangeState(GameState.Field);
			}
			else
			{
				foreach( Encounter encounter in GetComponentsInChildren<Encounter>() )
				{
					if( encounter.Level < GameContext.PlayerConductor.Level )
					{
						GameContext.PlayerConductor.TotalSP += encounter.AcquireStars;
						GameContext.PlayerConductor.RemainSP += encounter.AcquireStars;
					}
				}
				GameContext.ChangeState(GameState.SetMenu);
				CommandExp.SetEnemy(CurrentLevel.Encounters[encounterCount].Enemies[0].GetComponent<Enemy>());
			}
			break;
        case GameState.Field:
			//if( Input.GetMouseButtonUp(0) )
			//{
			CheckEncount();
			//}
            break;
        case GameState.Result:
            GameContext.PlayerConductor.UpdateResult();
            break;
        default:
            break;
        }
	}

	public void CheckResult()
	{
		GameContext.PlayerConductor.CheckResult(CurrentLevel.Encounters[encounterCount-1].AcquireStars);
		if( encounterCount >= CurrentLevel.Encounters.Count )
		{
			GameContext.PlayerConductor.Level++;
			GameContext.PlayerConductor.OnLevelUp();
			encounterCount = 0;
		}
		RState = ResultState.StarPoint;
	}

    void CheckEncount()
	{
		if( GameContext.PlayerConductor.Level >= LevelEncounters.Length ) return;

        Music.Stop();
        TextWindow.SetNextCursor( false );
        GameContext.EnemyConductor.SetEncounter( CurrentLevel.Encounters[encounterCount] );
        GameContext.ChangeState( GameState.Intro );
        ++encounterCount;
    }

    public void MoveNextResult()
    {
        ++RState;
        if( RState == ResultState.End )
        {
			GameContext.ChangeState(GameState.SetMenu);
			CommandExp.SetEnemy(CurrentLevel.Encounters[encounterCount].Enemies[0].GetComponent<Enemy>());
        }
    }

	public void OnContinue()
	{
		//encounterCount--;
	}

	public void OnPlayerLose()
	{
		encounterCount--;
		CommandExp.SetEnemy(CurrentLevel.Encounters[encounterCount].Enemies[0].GetComponent<Enemy>());
	}

    void UpdateEncounterList()
    {
        foreach( Encounter encounter in GetComponentsInChildren<Encounter>() )
        {
            DestroyImmediate( encounter.gameObject );
        }

        string path = Application.streamingAssetsPath + "/VQ3List - Battle.csv";
        StreamReader reader = File.OpenText( path );
        if( reader != null )
        {
            string line = reader.ReadLine();
            char[] commaSeparator = new char[] { ',' };
            char[] spaceSeparator = new char[] { ' ' };
            int level = 1;
            int order = 1;
            while( (line = reader.ReadLine()) != null )
            {
                string[] propertyTexts = line.Split( commaSeparator, System.StringSplitOptions.None );
                if( propertyTexts[(int)EncounterListProperty.Level] != "" )
                {
                    level = int.Parse( propertyTexts[(int)EncounterListProperty.Level] );
                    order = 1;
                }
                else
                {
                    ++order;
                }

                GameObject encounterObj = new GameObject( "Encounter" + (level < 10 ? "0" : "") + level.ToString() + (order < 10 ? "0" : "") + order.ToString() );
                encounterObj.transform.parent = this.transform;
                Encounter encounter = encounterObj.AddComponent<Encounter>();

                encounter.Level = level;
                List<string> enemyNames = new List<string>();
                for( int i = 0; i < 3; i++ )
                {
                    string nameProp = propertyTexts[i+(int)EncounterListProperty.Enemy1];
                    if( nameProp != "" )
                    {
                        enemyNames.Add( nameProp );
                    }
                    else break;
                }
                encounter.Enemies = new GameObject[enemyNames.Count];
                encounter.StateSets = new EnemyConductor.StateSet[1];
                string stateSet = "";
                for( int i = 0; i < enemyNames.Count; i++ )
                {
                    encounter.Enemies[i] = EnemyPrefabs.Find( ( GameObject e ) => e.name == enemyNames[i] );
                    stateSet += ( propertyTexts[i + (int)EncounterListProperty.State1] != "" ? propertyTexts[i + (int)EncounterListProperty.State1] : "Default" ) + " ";
                }
                encounter.StateSets[0] = new EnemyConductor.StateSet( stateSet );
            }
        }
    }
}
