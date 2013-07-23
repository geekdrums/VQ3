using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Phrase
{
    public Phrase(Chord bc, params Chord[] phrase)
    {
        baseChord = bc;
        chords = new List<Chord>();
        chords.AddRange(phrase);
        chords.TrimExcess();
    }
    public Phrase(Chord bc, List<Chord> phrase)
    {
        baseChord = bc;
        chords = phrase;
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
            } else throw new ApplicationException("phrase[i] is out of index: i = " + i);
        }
        get
        {
            if (0 <= i && i < Length)
                return chords[i];
            else return null;
        }
    }
    public Chord baseChord { get; private set; }

    public Phrase MakeTranspose(int t) 
    {
        List<Chord> transposed = new List<Chord>();
        foreach (Chord c in this.chords) {
            transposed.Add(c.Transpose(t));
        }
        return new Phrase((baseChord != null ? baseChord.Transpose(t) : null), transposed); 
    }
    public Phrase MakeHarmonize(int t)
    {
        List<Chord> harmonized = new List<Chord>();
        foreach (Chord c in this.chords) {
            harmonized.Add(c.Harmonize(t));
        }
        return new Phrase(baseChord, harmonized);
    }
}

