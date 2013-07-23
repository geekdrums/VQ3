using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Chord : IEnumerable<int>
{
	public Chord( params int[] tones )
	{
		chord = new List<int>( tones );
		chord.TrimExcess();
		chord.Sort();
	}
	public Chord( List<int> tones )
	{
		chord = tones;
		chord.TrimExcess();
		chord.Sort();
	}
    public Chord(string cho, int baseOctave = 0) : this(Tone.Parse(cho, baseOctave)) { }

	private List<int> chord;

	public int numTones { get { return chord.Count; } }
	public int this[int i]
	{
		private set { }
		get
		{
			if ( 0 <= i && i < numTones )
				return chord[i];
			else throw new ApplicationException( "chord[i] is out of index: i = " + i );
		}
	}
	public void AddTone( int t ) { if ( !chord.Contains( t ) ) { chord.Add( t ); } }
	#region IEnumerator
	public IEnumerator<int> GetEnumerator()
	{
		foreach ( int t in chord )
			yield return t;
	}
	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
	#endregion

	/// <summary>
	/// Original transformation logic from basechord to this chord
	/// </summary>
	/// <param name="tones">tones to be transformed</param>
	/// <param name="basschord">chord that fit to original tones</param>
	/// <returns></returns>
	public Chord Fix( Chord tones, Chord basechord )
	{
		if ( basechord == null ) return tones;
		if ( numTones == basechord.numTones )
		{
			bool isSame = true;
			for ( int i = 0; i < numTones; i++ )
			{
				if ( this[i] != basechord[i] )
				{
					isSame = false;
					break;
				}
			}
			if ( isSame )
			{
				return tones;
			}
		}

		int length = tones.numTones;
		int[] result = new int[length];
		float toneOnBasechord;
		int toneOnDefaultOctave;
		int octave;
		for ( int i = 0; i < length; i++ )
		{
			octave = ( tones[i] - basechord[0] + ( tones[i] - basechord[0] < 0 ? 1 : 0 ) ) / Tone.OCTAVE + ( tones[i] - basechord[0] < 0 ? -1 : 0 );

			toneOnBasechord = 256.0f;
			{
				#region Find where this tone is in basechord
				int t = tones[i];
				if ( t % Tone.OCTAVE == basechord[0] % Tone.OCTAVE ) toneOnBasechord = 0.0f;
				else
				{
					t -= Tone.OCTAVE * octave;

					int l = basechord.numTones;
					bool flag = false;
					for ( int j = 0; j < l; j++ )
					{
						if ( basechord[j] == t )
						{
							toneOnBasechord = (float)j;
							flag = true;
							break;
						}
						else if ( t < basechord[j] )
						{
							float d = basechord[j] - basechord[j - 1];
							toneOnBasechord = (float)( j - 1 ) + (float)( t - basechord[j - 1] ) / d;
							flag = true;
							break;
						}
					}
					if ( !flag )
					{
						float d2 = basechord[0] + Tone.OCTAVE - basechord[l - 1];
						toneOnBasechord = (float)( l - 1 ) + (float)( t - basechord[l - 1] ) / d2;
					}
				}
				#endregion
			}
			toneOnDefaultOctave = 256;
			{
				#region Calcurate a tone in this chord
				int l = this.numTones;
				bool flag = false;
				for ( int j = 0; j < l; j++ )
				{
					if ( toneOnBasechord == (float)j )
					{
						toneOnDefaultOctave = this[j];
						flag = true;
						break;
					}
					else if ( toneOnBasechord < j )
					{
						float dec = toneOnBasechord - ( j - 1 );
						int d = this[j] - this[j - 1];
						toneOnDefaultOctave = this[j - 1] + (int)( dec * d );
						flag = true;
						break;
					}
				}
				//Can't find means basechord has more tones( or higher tones )
				if ( !flag )
				{
					float dec2 = ( toneOnBasechord - (float)( l - 1 ) ) / (float)( basechord.numTones - l + 1 );
					int d2 = this[0] + Tone.OCTAVE - this[l - 1];
					toneOnDefaultOctave = this[l - 1] + (int)( dec2 * d2 );
				}
				#endregion
			}

			result[i] = toneOnDefaultOctave + octave * Tone.OCTAVE;
		}

		return new Chord( result );
	}

	public Chord Transpose( int t )
	{
		if ( t == 0 ) return new Chord( this.chord );
		else
		{
            List<int> res = new List<int>();
			for ( int i = 0; i < chord.Count; i++ )
			{
				res.Add( this[i] + t );
			}
			return new Chord( res );
		}
	}
	public Chord RollUp( int time )
	{
		List<int> res = new List<int>( this );
		for ( int i = 0; i < time; i++ )
		{
			res[0] += Tone.OCTAVE;
			res.Sort();
		}
		return new Chord( res );
	}
	public Chord RollDown( int time )
	{
		List<int> res = new List<int>( this );
		for ( int i = 0; i < time; i++ )
		{
			chord[numTones - 1] -= Tone.OCTAVE;
			chord.Sort();
		}
		return new Chord( res );
	}
	public Chord Harmonize( int t )
	{
		if ( t == 0 ) return new Chord( this.chord );
		else
		{
			int[] res = new int[numTones*2];
			for ( int i = 0; i < numTones; i++ )
			{
				res[i * 2] = this[i];
				res[i * 2 + 1] = this[i] + t;
			}
			return new Chord( res );
		}
	}

	/// <summary>
	/// ex. Parse("C,C E G,o01,C E")
	/// </summary>
	/// <param name="chordphrase"></param>
	/// <returns></returns>
	public static List<Chord> Parse( string chordphrase )
	{
		string[] strs = chordphrase.Split( ",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries );
        List<Chord> chords = new List<Chord>();
        int octave = 0;
        for (int i = 0; i < strs.Length; i++)
        {
            string str = strs[i];
            if (str.StartsWith("o"))
            {
                octave = int.Parse(str.Substring(str.Length - 2, 2));
            }
            else
            {
                chords[i] = new Chord(strs[i], octave);
            }
        }
		return chords;
	}
	public static Chord MakeChord( int tone, string option )
	{
		if ( option == "maj" )
		{
			return new Chord( tone, tone + 4, tone + 7 );
		}
		else if ( option == "min" )
		{
			return new Chord( tone, tone + 3, tone + 7 );
		}
		else if ( option == "sus4" )
		{
			return new Chord( tone, tone + 5, tone + 7 );
		}
		else if ( option == "7" )
		{
			return new Chord( tone, tone + 4, tone + 7, tone + 10 );
		}
		else if ( option == "maj7" )
		{
			return new Chord( tone, tone + 4, tone + 7, tone + 11 );
		}
		else if ( option == "min7" )
		{
			return new Chord( tone, tone + 3, tone + 7, tone + 10 );
		}
		else if ( option == "dim" )
		{
			return new Chord( tone, tone + 3, tone + 6, tone + 9 );
		}
		else throw new ApplicationException( "That chord is not implemented" );
	}
}
