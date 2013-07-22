using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// 音程、コードの配列。Rhythmと合わせて使用。
/// </summary>
public class Phrase
{
    //==================コンストラクタ=========================
    public Phrase(Chord bc, params Chord[] phrase)
    {
        baseChord = bc;
        chords = new List<Chord>();
        chords.AddRange(phrase);
        chords.TrimExcess();
    }
    public Phrase(Chord bc, string phrase) : this(bc, Chord.Parse(phrase)) { }
    public Phrase( string phrase ) : this(null, Chord.Parse(phrase)) { }

    public List<Chord> chords { get; private set; }
    public int Length { get { return chords.Count; } }
    public Chord this[int i]
    {
        set
        {
            if (0 <= i && i < Length) {
                chords[i] = value;
            } else throw new ApplicationException("フレーズのchordsの書き換え時に、不適切なインデックスを渡しています");
        }
        get
        {
            if (0 <= i && i < Length)
                return chords[i];
            else return null;
        }
    }
    public Chord baseChord { get; private set; }//コード進行に合わせて使用したい場合は設定する。しない場合はnullにしておく。

    /// <summary>
    /// 自分自身は変更せずに新たに平行移動したフレーズを作る。
    /// baseChordもトランスポーズされる。
    /// </summary>
    /// <param name="t">移動量</param>
    /// <returns></returns>
    public Phrase MakeTranspose(int t) 
    {
        List<Chord> transposed = new List<Chord>();
        foreach (Chord c in this.chords) {
            transposed.Add(c.Transpose(t));
        }
        return new Phrase((baseChord != null ? baseChord.Transpose(t) : null), transposed.ToArray()); 
    }
    /// <summary>
    /// 自分自身は変更せずに新たに和音を加えたフレーズを作る。
    /// </summary>
    /// <param name="t">重ねる音の距離</param>
    /// <returns></returns>
    public Phrase MakeHarmonize(int t)
    {
        List<Chord> harmonized = new List<Chord>();
        foreach (Chord c in this.chords) {
            harmonized.Add(c.Harmonize(t));
        }
        return new Phrase(baseChord, harmonized.ToArray());
    }
}

