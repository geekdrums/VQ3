using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

[ExecuteInEditMode]
public class PlayerCommand : CommandBase, IVoxNode
{
    public string MusicBlockName;
    public float latitude;
    public float longitude;
    public int AcquireLevel = 1;
    public string[] DescribeTexts;
    public string[] AcquireTexts;
    public List<MonoBehaviour> links;
    public float radius = 1.0f;
    public float Radius()
    {
        return radius;
    }
    public Transform Transform()
    {
        return transform;
    }
    public IEnumerable<IVoxNode> LinkedNodes()
    {
        return links.ConvertAll<IVoxNode>( ( MonoBehaviour mb ) => mb as IVoxNode );
    }
    public bool IsLinkedTo( IVoxNode node )
    {
        return LinkedNodes().Contains<IVoxNode>( node );
    }

    public Strategy ParentStrategy { get; protected set; }
    public bool IsLinked { get; protected set; }
    public bool IsCurrent { get; protected set; }
    public bool IsSelected { get; protected set; }
    public bool IsAcquired { get; protected set; }
    public bool IsTargetSelectable { get { return _skillList.Find( ( Skill s ) => s.IsTargetSelectable ) != null; } }
    public PlayerCommand ParentCommand { get; protected set; }
    public int NumLoopVariations { get; protected set; }

    public override void Parse()
    {
        base.Parse();

<<<<<<< HEAD:VQ3 Unity/Assets/Script/Battle/ComandGraph/Command/PlayerCommand.cs
        if( !UnityEditor.EditorApplication.isPlaying ) return;
=======
>>>>>>> 85910d9ad2d94a2b40c99570530e553b0e0057ec:VQ3 Unity/Assets/Script/Battle/ComandGraph/Command/PlayerCommand.cs
        IsLinked = true;
        IsAcquired = AcquireLevel <= GameContext.PlayerConductor.Level;
        if( !IsAcquired )
        {
            GetComponent<TextMesh>().color = Color.clear;
        }
<<<<<<< HEAD:VQ3 Unity/Assets/Script/Battle/ComandGraph/Command/PlayerCommand.cs
        NumLoopVariations = 1;
        foreach( PlayerCommand command in GetComponentsInChildren<PlayerCommand>() )
        {
            if( command != this )
            {
                command.ParentCommand = this;
                int index = -1;
                if( int.TryParse( command.name.Replace( this.name, "" ), out index ) )
                {
                    NumLoopVariations++;
                }
            }
        }
=======
>>>>>>> 85910d9ad2d94a2b40c99570530e553b0e0057ec:VQ3 Unity/Assets/Script/Battle/ComandGraph/Command/PlayerCommand.cs
    }

    void Start()
    {
        Parse();
        IsLinked = false;
        IsCurrent = false;
        IsSelected = false;
    }

    void Update()
    {
#if UNITY_EDITOR
        if( UnityEditor.EditorApplication.isPlaying ) return;
        transform.localPosition = Quaternion.AngleAxis( latitude, Vector3.right ) * Quaternion.AngleAxis( longitude, Vector3.down ) * Vector3.back * 7.5f;
        transform.localRotation = Quaternion.LookRotation( -transform.localPosition );
#endif
    }

    public virtual GameObject GetCurrentSkill()
    {
        if( GameContext.VoxSystem.state == VoxState.Eclipse && Music.Just.bar >= 2 ) return null;
        return SkillDictionary.ContainsKey( Music.Just.totalUnit ) ? SkillDictionary[Music.Just.totalUnit].gameObject : null;
    }
    public void SetParent( Strategy parent )
    {
        ParentStrategy = parent;
    }

    public int GetWillGainVP()
    {
        int sum = 0;
        for( int i = 0; i < SkillDictionary.Count; ++i )
        {
            if( SkillDictionary.Keys.ElementAt( i ) < 3 * Music.mtBar )
            {
                Skill skill = SkillDictionary.Values.ElementAt( i );
                if( skill.Actions == null ) skill.Parse();
                foreach( ActionSet a in skill.Actions )
                {
                    if( a.GetModule<AttackModule>() != null )
                    {
                        sum += a.GetModule<AttackModule>().VP;
                    }
                }
            }
            else break;
        }
        return sum;
    }

    public virtual string GetBlockName()
    {
        return MusicBlockName;
    }
    public bool IsUsable()
    {
        return IsAcquired && IsLinked;
    }


    public IEnumerable<PlayerCommand> LinkedCommands
    {
        get
        {
            if( ParentStrategy != null )
            {
                foreach( PlayerCommand c in ParentStrategy.LinkedCommands )
                {
                    yield return c;
                }
            }
            foreach( IVoxNode link in links )
            {
                Strategy linkedStrategy = link as Strategy;
                PlayerCommand linkedCommand = link as PlayerCommand;
                if( linkedStrategy != null )
                {
                    foreach( PlayerCommand c in linkedStrategy.Commands )
                    {
                        yield return c;
                    }
                }
                else if( linkedCommand != null )
                {
                    yield return linkedCommand;
                }
            }
        }
    }
    public PlayerCommand FindVariation( string suffix )
    {
        return GetComponentsInChildren<PlayerCommand>().First<PlayerCommand>( ( PlayerCommand command ) => command.name == this.name + suffix );
    }
    public PlayerCommand FindLoopVariation( int numLoop )
    {
        int index = numLoop % NumLoopVariations;
        return ( index == 0 ? this : FindVariation( index.ToString() ) );
    }

    public virtual void SetLink( bool linked )
    {
        IsCurrent = false;
        IsSelected = false;
        IsLinked = linked;
        if( IsUsable() )
        {
            GetComponent<TextMesh>().color = Color.black;
        }
        else
        {
            if( IsAcquired )
            {
                GetComponent<TextMesh>().color = Color.gray;
            }
        }
    }
    public void SetCurrent()
    {
        if( IsAcquired )
        {
            IsCurrent = true;
            GetComponent<TextMesh>().color = Color.red;
        }
    }
    public void Select()
    {
        if( IsAcquired )
        {
            IsSelected = true;
            if( !IsCurrent )
            {
                GetComponent<TextMesh>().color = Color.magenta;
            }
        }
    }
    public void Deselect()
    {
        if( IsAcquired )
        {
            IsSelected = false;
            if( !IsCurrent )
            {
                GetComponent<TextMesh>().color = Color.black;
            }
        }
    }
    public void Acquire()
    {
        IsAcquired = true;
        GetComponent<TextMesh>().color = Color.gray;
    }
}