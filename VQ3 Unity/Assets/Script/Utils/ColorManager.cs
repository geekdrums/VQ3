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
    public Color Heal;
    public Color Buff;
    public Color DeBuff;
    public Color Break;
}


[ExecuteInEditMode]
public class ColorManager : MonoBehaviour
{
    static ColorManager instance
    {
        get
        {
            if( instance_ == null )
            {
                instance_ = UnityEngine.Object.FindObjectOfType<ColorManager>();
            }
            return instance_;
        }
    }
    static ColorManager instance_;

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

    public static BaseColor Base { get; private set; }

    public static ThemeColor Theme { get; private set; }

    public static AccentColor Accent { get; private set; }

    struct HSV
    {
        public float h, s, v;
    }

    void Start()
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

        if( GameContext.VoxSystem.state != VoxState.Invert )
        {
            instance.BGMaterial.color = Theme.Light;
        }
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

    //http://ja.wikipedia.org/wiki/HSV%E8%89%B2%E7%A9%BA%E9%96%93
    static HSV GetHSV( Color c )
    {
        HSV hsv;
        float max = Mathf.Max( c.r, c.g, c.b );
        float min = Mathf.Min( c.r, c.g, c.b );
        hsv.h = max - min;
        if( hsv.h > 0.0f )
        {
            if( max == c.r )
            {
                hsv.h = (c.g - c.b) / hsv.h;
                if( hsv.h < 0.0f )
                {
                    hsv.h += 6.0f;
                }
            }
            else if( max == c.g )
            {
                hsv.h = 2.0f + (c.b - c.r) / hsv.h;
            }
            else
            {
                hsv.h = 4.0f + (c.r - c.g) / hsv.h;
            }
        }
        hsv.h /= 6.0f;
        hsv.s = (max - min);
        if( max != 0.0f )
            hsv.s /= max;
        hsv.v = max;

        return hsv;
    }
    static Color GetRGB( HSV hsv )
    {
        float r = hsv.v;
        float g = hsv.v;
        float b = hsv.v;
        if (hsv.s > 0.0f) {
            hsv.h *= 6.0f;
            int i = (int)hsv.h;
            float f = hsv.h - (float)i;
            switch (i) {
                default:
                case 0:
                    g *= 1 - hsv.s * (1 - f);
                    b *= 1 - hsv.s;
                    break;
                case 1:
                    r *= 1 - hsv.s * f;
                    b *= 1 - hsv.s;
                    break;
                case 2:
                    r *= 1 - hsv.s;
                    b *= 1 - hsv.s * (1 - f);
                    break;
                case 3:
                    r *= 1 - hsv.s;
                    g *= 1 - hsv.s * f;
                    break;
                case 4:
                    r *= 1 - hsv.s * (1 - f);
                    g *= 1 - hsv.s;
                    break;
                case 5:
                    g *= 1 - hsv.s;
                    b *= 1 - hsv.s * f;
                    break;
            }
        }
        return new Color( r, g, b );
    }

    public static Color MakeAlpha( Color color, float alpha )
    {
        return new Color( color.r, color.g, color.b, color.a * alpha );
    }
    public static float Distance( Color color1, Color color2 )
    {
        return Mathf.Abs( color1.r - color2.r ) + Mathf.Abs( color1.g - color2.g )
            + Mathf.Abs( color1.b - color2.b ) + Mathf.Abs( color1.a - color2.a );
    }
}

