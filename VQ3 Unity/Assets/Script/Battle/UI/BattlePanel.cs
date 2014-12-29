using UnityEngine;
using System.Collections;

public enum EBattlePanelType
{
    DF,
    HL,
    VT,
    VP
};
public class BattlePanel : MonoBehaviour {

    static float EnhIconOffset = 0.1f;
    static int MaxEnhTurn = 4;

    public MidairPrimitive Rect;
    public CounterSprite Count;
    public SpriteRenderer EnhIcon;
    public TextMesh NameText;
    public GameObject EnhBar;
    public CounterSprite EnhCount;
    public EBattlePanelType PanelType;

    Vector3 enhIconInitialPosition_;
    EnhanceParameter currentEnhanceParam;

	// Use this for initialization
	void Start () {
        enhIconInitialPosition_ = EnhIcon.transform.position;
        EnhBar.transform.localScale = new Vector3( 0, EnhBar.transform.localScale.y, 1 );
        EnhCount.transform.localScale = Vector3.zero;
	}
	
	// Update is called once per frame
	void Update () {
        if( currentEnhanceParam != null && currentEnhanceParam.remainTurn <= MaxEnhTurn )
        {
            if( currentEnhanceParam.remainTurn <= 0 )
            {
                SetEnhance( currentEnhanceParam );
                currentEnhanceParam = null;
            }
            else if( Music.IsJustChangedBar() && Music.CurrentBlockName != "wait" )
            {
                EnhBar.transform.localScale = new Vector3( Mathf.Min( 1.0f, (float)(currentEnhanceParam.remainTurn - Music.MusicalTime/64.0f) / MaxEnhTurn ), EnhBar.transform.localScale.y, 1 );
                //if( Music.Just.beat % 2 == 0 )
                //{
                //    EnhBar.transform.localScale = new Vector3( Mathf.Min( 1.0f, (float)currentEnhanceParam.remainTurn / MaxEnhTurn ), EnhBar.transform.localScale.y, 1 );
                //}
                //else
                //{
                //    EnhBar.transform.localScale = new Vector3( Mathf.Min( 1.0f, (float)(currentEnhanceParam.remainTurn - 1.0f) / MaxEnhTurn ), EnhBar.transform.localScale.y, 1 );
                //}
            }
        }
	}

    public void Init()
    {
        Count.Count = 0;
        Rect.SetColor( ColorManager.Base.MiddleBack );
        Rect.SetAnimationSize( 1.0f, 1.53f );
        Rect.SetAnimationWidth( 0.5f, 1.53f );
    }

    public void Set( PlayerCommand command )
    {
        switch( PanelType )
        {
        case EBattlePanelType.DF:
            Count.Count = command.GetDefend();
            Rect.SetColor( command.GetDefColor() );
            break;
        case EBattlePanelType.HL:
			Count.Count = command.GetHeal();
            Rect.SetColor( command.GetHealColor() );
            break;
        case EBattlePanelType.VT:
			Count.Count = command.GetVT();
            Rect.SetColor( command.GetVTColor() );
            break;
        case EBattlePanelType.VP:
			Count.Count = command.GetVP();
            Rect.SetColor( command.GetVPColor() );
            break;
        }
        Rect.SetAnimationSize( 1.0f, 1.53f );
        Rect.SetAnimationWidth( 0.5f, 1.53f );
    }
    
    public void SetEnhance( EnhanceParameter enhParam )
    {
        currentEnhanceParam = enhParam;
        if( enhParam.phase > 0 )
        {
            EnhIcon.color = ColorManager.Accent.Buff;
            EnhCount.CounterColor = ColorManager.Accent.Buff;
            EnhBar.GetComponentInChildren<MidairPrimitive>().SetColor( ColorManager.Accent.Buff );
            NameText.renderer.enabled = false;
            EnhCount.transform.localScale = Vector3.one;
        }
        else if( enhParam.phase < 0 )
        {
            EnhIcon.color = ColorManager.Accent.DeBuff;
            EnhCount.CounterColor = ColorManager.Accent.DeBuff;
            EnhBar.GetComponentInChildren<MidairPrimitive>().SetColor( ColorManager.Accent.DeBuff );
            NameText.renderer.enabled = false;
            EnhCount.transform.localScale = Vector3.one;
        }
        else//enhParam.phase == 0
        {
            EnhIcon.color = ColorManager.Base.Middle;
            EnhIcon.transform.position = enhIconInitialPosition_;
            EnhBar.transform.localScale = new Vector3( 0, EnhBar.transform.localScale.y, 1 );
            EnhCount.transform.localScale = Vector3.zero;
            NameText.renderer.enabled = true;
            return;
        }

        EnhIcon.transform.position = enhIconInitialPosition_ + Vector3.up * EnhIconOffset;
        EnhBar.transform.localScale = new Vector3( Mathf.Min( 1.0f, (float)enhParam.remainTurn / MaxEnhTurn ), EnhBar.transform.localScale.y, 1 );
        EnhCount.Count = enhParam.currentParam;
    }
}
