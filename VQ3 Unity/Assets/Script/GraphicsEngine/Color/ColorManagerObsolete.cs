using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public enum EThemeColor
{
    Red,
    Green,
    Blue,
    Yellow,
    Black,
    White,
}

public enum EBaseColor
{
    White,
    Black,
    Red,
}

public enum EAccentColor
{
    Default,
}

[System.Serializable]
public class BaseColor
{
    public Color Back;
    public Color MiddleBack;
    public Color Middle;
    public Color Front;
    public bool isLightBack;
    public Color Dark { get { return isLightBack ? Front : Back; } }
    public Color Shade { get { return isLightBack ? Middle : MiddleBack; } }
    public Color Light { get { return isLightBack ? MiddleBack : Middle; } }
    public Color Bright { get { return isLightBack ? Back : Front; } }
}

[System.Serializable]
public class ThemeColor
{
    public Color Bright;
    public Color Light;
    public Color Shade;
}

[System.Serializable]
public class AccentColor
{
    public Color Damage;
	public Color Critical;
	public Color PlayerDamage;
    public Color Heal;
    public Color Buff;
    public Color DeBuff;
    public Color Break;
	public Color Time;
	public Color Drain;
}


[ExecuteInEditMode]
public class ColorManagerObsolete : MonoBehaviour
{
    static ColorManagerObsolete instance
    {
        get
        {
            if( instance_ == null )
            {
                instance_ = UnityEngine.Object.FindObjectOfType<ColorManagerObsolete>();
            }
            return instance_;
        }
    }
    static ColorManagerObsolete instance_;

    public BaseColor BaseWhite;
    public BaseColor BaseBlack;
    public BaseColor BaseRed;

    public ThemeColor ThemeRed;
    public ThemeColor ThemeGreen;
    public ThemeColor ThemeBlue;
    public ThemeColor ThemeYellow;
    public ThemeColor ThemeBlack;
    public ThemeColor ThemeWhite;

    public AccentColor AccentDefault;

    public Material BaseBackMaterial;
    public Material BaseMiddleBackMaterial;
    public Material BaseMiddleMaterial;
    public Material BaseFrontMaterial;

    public Material BaseDarkMaterial;
    public Material BaseShadeMaterial;
    public Material BaseLightMaterial;
    public Material BaseBrightMaterial;

    public Material ThemeBrightMaterial;
    public Material ThemeLightMaterial;
    public Material ThemeShadeMaterial;

    public Material AccentDamageMaterial;
	public Material AccentCriticalMaterial;
    public Material AccentHealMaterial;
    public Material AccentBuffMaterial;
    public Material AccentDeBuffMaterial;
    public Material AccentBreakMaterial;

    public Material BGMaterial;

    public CounterSprite[] BaseFrontCounters;
    public CounterSprite[] BaseBackCounters;
	public MidairPrimitive[] BaseBackPrimitives;
	public MidairPrimitive[] BaseFrontPrimitives;
	public MidairPrimitive[] BaseLightPrimitives;
	public Material[] BaseBackMaterials;
	public Material[] BaseFrontMaterials;
	public TextMesh[] BaseFrontTextMeshes;
	public GaugeRenderer[] BaseBackGauges;
	public GaugeRenderer[] BaseFrontGauges;
	public GaugeRenderer[] BaseMiddleGauges;
	public GaugeRenderer[] BaseMiddleBackGauges;

	public MidairPrimitive[] MoonPrimitives;

	public static BaseColor Base { get; private set; }

    public static ThemeColor Theme { get; private set; }

    public static AccentColor Accent { get; private set; }

	public static event Action<BaseColor> OnBaseColorChanged;

    void Awake()
    {
        instance_ = this;
        Base = this.BaseBlack;
        Theme = this.ThemeGreen;
        Accent = this.AccentDefault;
    }
    void Update()
    {
    }

    public static void SetBaseColor( EBaseColor baseColor )
    {
        switch( baseColor )
        {
        case EBaseColor.White:
            Base = instance.BaseWhite;
            break;
        case EBaseColor.Black:
            Base = instance.BaseBlack;
            break;
        case EBaseColor.Red:
            Base = instance.BaseRed;
            break;
        }
        instance.BaseBackMaterial.color = Base.Back;
        instance.BaseMiddleBackMaterial.color = Base.MiddleBack;
        instance.BaseMiddleMaterial.color = Base.Middle;
        instance.BaseFrontMaterial.color = Base.Front;
        instance.BaseBrightMaterial.color = Base.Bright;
        instance.BaseLightMaterial.color = Base.Light;
        instance.BaseShadeMaterial.color = Base.Shade;
        instance.BaseDarkMaterial.color = Base.Dark;

        foreach( CounterSprite counter in instance.BaseFrontCounters )
        {
            counter.CounterColor = Base.Front;
        }
        foreach( CounterSprite counter in instance.BaseBackCounters )
        {
            counter.CounterColor = Base.Back;
        }
		foreach( MidairPrimitive primitive in instance.BaseBackPrimitives )
		{
			primitive.SetColor(Base.Back);
		}
		foreach( MidairPrimitive primitive in instance.MoonPrimitives )
		{
			primitive.SetColor(baseColor != EBaseColor.White ? Base.Back : instance.BaseBlack.Back);
		}
		foreach( MidairPrimitive primitive in instance.BaseFrontPrimitives )
		{
			primitive.SetColor(Base.Front);
		}
		foreach( MidairPrimitive primitive in instance.BaseLightPrimitives )
		{
			primitive.SetColor(Base.Light);
		}
		foreach( GaugeRenderer gauge in instance.BaseBackGauges )
		{
			gauge.SetColor(Base.Back);
		}
		foreach( GaugeRenderer gauge in instance.BaseFrontGauges )
		{
			gauge.SetColor(Base.Front);
		}
		foreach( GaugeRenderer gauge in instance.BaseMiddleGauges )
		{
			gauge.SetColor(Base.Middle);
		}
		foreach( GaugeRenderer gauge in instance.BaseMiddleBackGauges )
		{
			gauge.SetColor(Base.MiddleBack);
		}
		foreach( Material material in instance.BaseBackMaterials)
		{
			material.color = Base.Back;
		}
		foreach( Material material in instance.BaseFrontMaterials )
		{
			material.color = Base.Front;
		}
		foreach( TextMesh textMesh in instance.BaseFrontTextMeshes )
		{
			textMesh.color = Base.Front;
		}

		if( OnBaseColorChanged != null )
			OnBaseColorChanged(Base);
    }
    public static void SetThemeColor( EThemeColor themeColor )
    {
        switch( themeColor )
        {
        case EThemeColor.Red:
            Theme = instance.ThemeRed;
            break;
        case EThemeColor.Green:
            Theme = instance.ThemeGreen;
            break;
        case EThemeColor.Blue:
            Theme = instance.ThemeBlue;
            break;
        case EThemeColor.Yellow:
            Theme = instance.ThemeYellow;
            break;
        case EThemeColor.Black:
            Theme = instance.ThemeBlack;
            break;
        case EThemeColor.White:
            Theme = instance.ThemeWhite;
            break;
        }
        instance.ThemeBrightMaterial.color = Theme.Bright;
        instance.ThemeLightMaterial.color = Theme.Light;
        instance.ThemeShadeMaterial.color = Theme.Shade;
    }
    public static ThemeColor GetThemeColor( EThemeColor themeColor )
    {
        switch( themeColor )
        {
        case EThemeColor.Red:
            return instance.ThemeRed;
        case EThemeColor.Green:
            return instance.ThemeGreen;
        case EThemeColor.Blue:
            return instance.ThemeBlue;
        case EThemeColor.Yellow:
            return instance.ThemeYellow;
        case EThemeColor.Black:
            return instance.ThemeBlack;
        case EThemeColor.White:
            return instance.ThemeWhite;
        default:
            return null;
        }
    }
    public static void SetAccentColor( EAccentColor accentColor )
    {
        switch( accentColor )
        {
        case EAccentColor.Default:
            Accent = instance.AccentDefault;
            break;
        }
        instance.AccentDamageMaterial.color = Accent.Damage;
        instance.AccentCriticalMaterial.color = Accent.Critical;
        instance.AccentBuffMaterial.color = Accent.Buff;
        instance.AccentDeBuffMaterial.color = Accent.DeBuff;
        instance.AccentBreakMaterial.color = Accent.Break;
        instance.AccentHealMaterial.color = Accent.Heal;
	}

	public static Color MakeAlpha(Color color, float alpha)
	{
		return new Color(color.r, color.g, color.b, color.a * alpha);
	}

	public static float Distance( Color color1, Color color2 )
    {
        return Mathf.Abs( color1.r - color2.r ) + Mathf.Abs( color1.g - color2.g )
            + Mathf.Abs( color1.b - color2.b ) + Mathf.Abs( color1.a - color2.a );
    }
}

