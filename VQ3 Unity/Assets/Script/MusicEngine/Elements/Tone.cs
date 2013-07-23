using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public static class Tone
{
	public static readonly int Ces = C - 1;
	public static readonly int C = 0;//recursive definition based on this note.
	public static readonly int Cis = C + 1, Des = C + 1;
	public static readonly int D = Des + 1;
	public static readonly int Dis = D + 1, Es = D + 1;
	public static readonly int E = Es + 1;
	public static readonly int Fes = E;
	public static readonly int Eis = E + 1;
	public static readonly int F = E + 1;
	public static readonly int Fis = F + 1, Ges = F + 1;
	public static readonly int G = Ges + 1;
	public static readonly int Gis = G + 1, As = G + 1;
	public static readonly int A = As + 1;
	public static readonly int Ais = A + 1, Hes = A + 1, B = Hes;
	public static readonly int H = Hes + 1;
	public static readonly int His = H + 1;
	public static readonly int OCTAVE = H - C + 1;
	
	/// <summary>
    /// ex. Parse( "C E G o01 C" )
	/// </summary>
	/// <param name="phrase"></param>
	/// <returns></returns>
	public static List<int> Parse( string phrase, int baseOctave = 0 )
	{
		string[] stringTones = phrase.Split( " ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries );
        List<int> tones = new List<int>();
        int octave = baseOctave;
		int t;
		foreach ( string str in stringTones )
		{
			if ( str.StartsWith( "o" ) )
			{
                octave = baseOctave + int.Parse(str.Substring(str.Length - 2, 2));
                continue;
			}
			else if ( str.StartsWith( "C" ) )
			{
				if ( str[1] == 'e' ) t = Ces;
				else if ( str[1] == 'i' ) t = Cis;
				else t = C;
			}
			else if ( str.StartsWith( "D" ) )
			{
				if ( str[1] == 'e' ) t = Des;
				else if ( str[1] == 'i' ) t = Dis;
				else t = D;
			}
			else if ( str.StartsWith( "E" ) )
			{
				if ( str[1] == 's' ) t = Es;
				else if ( str[1] == 'i' ) t = Eis;
				else t = E;
			}
			else if ( str.StartsWith( "F" ) )
			{
				if ( str[1] == 'e' ) t = Fes;
				else if ( str[1] == 'i' ) t = Fis;
				else t = F;
			}
			else if ( str.StartsWith( "G" ) )
			{
				if ( str[1] == 'e' ) t = Ges;
				else if ( str[1] == 'i' ) t = Gis;
				else t = G;
			}
			else if ( str.StartsWith( "A" ) )
			{
				if ( str[1] == 's' ) t = As;
				else if ( str[1] == 'i' ) t = Ais;
				else t = A;
			}
			else if ( str.StartsWith( "H" ) )
			{
				if ( str[1] == 'e' ) t = Hes;
				else if ( str[1] == 'i' ) t = His;
				else t = H;
			}
            else if ( str.StartsWith( "B" ) )
            {
                t = B;
            }
            else throw new ApplicationException("Invalid tone name: " + str);
			tones.Add( t + octave * OCTAVE );
		}
		return tones;
	}
}
