using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public enum ResultState
{
    Command,
    End
}
public enum EncounterListProperty
{
    Level,
    Enemy,
    State,
    Turn,
    Purpose,
    Tutorial,
    ApproveMessages,
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

    public int encounterCount;
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
        case GameState.Field:
            if( encounterCount >= CurrentLevel.Encounters.Count )
            {
                if( GameContext.PlayerConductor.Level >= LevelEncounters.Length ) return;
                else
                {
                    GameContext.PlayerConductor.Level++;
                    GameContext.PlayerConductor.OnLevelUp();
                    encounterCount = 0;
                    RState = ResultState.Command;
                    GameContext.ChangeState( GameState.Result );
                }
            }
            else
            {
                CheckEncount();
            }
            break;
        case GameState.Result:
            GameContext.PlayerConductor.UpdateResult();
            break;
        default:
            break;
        }
	}

    void CheckEncount()
    {
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
            GameContext.ChangeState( GameState.Field );
        }
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
                string[] enemyNames = propertyTexts[(int)EncounterListProperty.Enemy].Split( spaceSeparator, System.StringSplitOptions.None );
                encounter.Enemies = new GameObject[enemyNames.Length];
                for( int i = 0; i < enemyNames.Length; i++ )
                {
                    encounter.Enemies[i] = EnemyPrefabs.Find( ( GameObject e ) => e.name == enemyNames[i] );
                }
                encounter.StateSets = new EnemyConductor.StateSet[1];
                encounter.StateSets[0] = new EnemyConductor.StateSet( propertyTexts[(int)EncounterListProperty.State] );

                if( propertyTexts[(int)EncounterListProperty.Tutorial] != "" )
                {
                    encounter.tutorialMessage = new TutorialMessage();
                    encounter.tutorialMessage.Type = TutorialMessageType.Const;
                    encounter.tutorialMessage.Texts = new string[1];
                    encounter.tutorialMessage.Texts[0] = propertyTexts[(int)EncounterListProperty.Tutorial];
                    encounter.tutorialMessage.ApproveMessageTypes = new List<BattleMessageType>();
                    encounter.tutorialMessage.BaseColor = Color.cyan;
                    if( propertyTexts[(int)EncounterListProperty.ApproveMessages] != "" )
                    {
                        foreach( string approveMessage in propertyTexts[(int)EncounterListProperty.ApproveMessages].Split( spaceSeparator, System.StringSplitOptions.None ) )
                        {
                            encounter.tutorialMessage.ApproveMessageTypes.Add( (BattleMessageType)System.Enum.Parse( typeof( BattleMessageType ), approveMessage ) );
                        }
                    }
                }

            }
        }
    }
}
