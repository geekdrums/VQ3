using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Chord : IEnumerable<int>
{
	//=============�R���X�g���N�^=================
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
	public Chord( string cho ) : this( Tone.Parse( cho ) ) { }

	//���g�͂��ꂾ��
	private List<int> chord;

	//===========�v���p�e�B�Ȃ�============
	public int numTones { get { return chord.Count; } }
	public int this[int i]
	{
		private set { }
		get
		{
			if ( 0 <= i && i < numTones )
				return chord[i];
			else throw new ApplicationException( "�R�[�h�̃C���f�b�N�X�O�ɃA�N�Z�X���悤�Ƃ��܂���" );
		}
	}
	public void AddTone( int t ) { if ( !chord.Contains( t ) ) { chord.Add( t ); } }
	#region IEnumerator����
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

	//============�R�[�h�ύX�̊֐�=================
	/// <summary>
	/// basechord��ŉ��t����邱�Ƃ����肵��tones�i�a���j��
	/// this.chord��ł̘a���ɕϊ�����B
	/// </summary>
	/// <param name="tones">�ύX���鉹</param>
	/// <param name="basschord">�ύX���鉹�������Ă���Motif�����̂܂܉��t�����Ƃ��̃R�[�h</param>
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

			//���̃R�[�h�ɂ����āA���I�N�^�[�u�ڂ̉���
			octave = ( tones[i] - basechord[0] + ( tones[i] - basechord[0] < 0 ? 1 : 0 ) ) / Tone.OCTAVE + ( tones[i] - basechord[0] < 0 ? -1 : 0 );
			//�l�������ɂ����Ȃ����B���Ȃ킿�A���ȒP�̂���basechrd[0]==0�Ƃ���ƁA-12�`-1�̓I�N�^�[�u���|�P�Ƃ������̂����A
			//-1�`-11��/Tone.OCTAVE�����Ƃ��ɂO�ɂȂ邯�ǁA-12�����́|�P�ɂȂ��Ă��܂��̂ŁA���������邽�߂ɂ����Đ�Ɉ���炵�Ă���B

			toneOnBasechord = 256.0f;//�K���ɁA�Đ����悤�Ƃ�����G���[���o�鉹�ɂ��Ă����B
			{
				#region ���̃R�[�h��̂ǂ̈ʒu�ɂ��邩���Z�o
				// �����basschord��̉��̈ʒu���i�[����B�ȒP�Ɍ����ƁA�Ⴆ�΃\�̉���Cmaj��̈ʒu��2�A
				// �t�@�̉��Ȃ�1.33�A�~�Ȃ�1�A����0.5�A�h��0�A�ȂǂƂ����悤�ɁA���Ԃ̉��͂��̑O��̉��Ƃ̔�ɂ���ė^������B
				//�I�N�^�[�u�����ׂđ�����B
				int t = tones[i];
				if ( t % Tone.OCTAVE == basechord[0] % Tone.OCTAVE ) toneOnBasechord = 0.0f;//�ꍇ�킯�B���̂Ƃ��͂킩�肫���Ă���̂ł����ݒ肵�Ĕ�����
				else
				{
					t -= Tone.OCTAVE * octave;

					//�R�[�h���T��
					int l = basechord.numTones;
					bool flag = false;//�ꏊ�����[�v�̒��Ō������������i�[�B�������ĂȂ���΁A��ԍ���������ɂ���̂ł��̏ꍇ�̏����ɁB
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
							float d = basechord[j] - basechord[j - 1];// j != 0 �́��ŕۏႳ��Ă���B
							toneOnBasechord = (float)( j - 1 ) + (float)( t - basechord[j - 1] ) / d;
							flag = true;
							break;
						}
					}
					//�R�[�h�̈�ԉ��̉��̃I�N�^�[�u��ƁA��ԏ�̉��Ƃ̊ԂŌv�Z�B
					if ( !flag )
					{
						float d2 = basechord[0] + Tone.OCTAVE - basechord[l - 1];
						toneOnBasechord = (float)( l - 1 ) + (float)( t - basechord[l - 1] ) / d2;
					}
				}
				#endregion
			}
			toneOnDefaultOctave = 256;//�K���ɁA�Đ����悤�Ƃ�����G���[���o�鉹�ɂ��Ă����B
			{
				#region ����ꂽ�ʒu�ɑ΂��āA���g�̃R�[�h��ł̈ʒu�̉����v�Z����B
				int l = this.numTones;
				bool flag = false;//�������[�v�̒��Ō������������i�[�B�������ĂȂ���΁A���̏�̉���ݒ肷��B
				//�R�[�h���T��
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
						float dec = toneOnBasechord - ( j - 1 );//decimal�i�͌^��������g���Ȃ��̂Łj�܂�1�ȉ��̐��ɂȂ�
						int d = this[j] - this[j - 1];
						toneOnDefaultOctave = this[j - 1] + (int)( dec * d );
						flag = true;
						break;
					}
				}
				//������Ȃ����������ƂȂ�R�[�h�̉��̕��������������i�������͓��������Ō�̃C���f�b�N�X�̉���荂�����j
				if ( !flag )
				{
					float dec2 = ( toneOnBasechord - (float)( l - 1 ) ) / (float)( basechord.numTones - l + 1 );
					int d2 = this[0] + Tone.OCTAVE - this[l - 1];
					toneOnDefaultOctave = this[l - 1] + (int)( dec2 * d2 );
				}
				#endregion
			}

			//�ȏ�̏�񂩂�A���g�̃R�[�h��̑Ή����鉹���Z�o�B
			result[i] = toneOnDefaultOctave + octave * Tone.OCTAVE;
		}

		return new Chord( result );
	}

	//===========���s�ړ��A�W�J�������̍쐬============
	public Chord Transpose( int t )
	{
		if ( t == 0 ) return new Chord( this.chord );
		else
		{
			int[] res = new int[numTones];
			for ( int i = 0; i < res.Length; i++ )
			{
				res[i] = this[i] + t;
			}
			return new Chord( res );
		}
	}
	/// <summary>
	/// �R�[�h����ɓW�J����
	/// </summary>
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
	/// <summary>
	/// �R�[�h�����ɓW�J����
	/// </summary>
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
	/// <summary>
	/// �a�������
	/// </summary>
	/// <param name="t">�d�˂鉹�Ƃ̋���</param>
	/// <returns>�V�����a��</returns>
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

	//==============static���\�b�h=================
	/// <summary>
	/// Parse("C00,C00 E00 G00,D00 F00")
	/// �݂����A�R�[�h�̉����J���}�ŕ����ēn���B
	/// </summary>
	/// <param name="chordphrase"></param>
	/// <returns></returns>
	public static Chord[] Parse( string chordphrase )
	{
		string[] strs = chordphrase.Split( Note.comma, StringSplitOptions.RemoveEmptyEntries );
		Chord[] chords = new Chord[strs.Length];
		for ( int i = 0; i < chords.Length; i++ )
			chords[i] = new Chord( strs[i] );
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
		else throw new ApplicationException( "���̃R�[�h�͎����œ��͂��Ă���" );
	}
}
