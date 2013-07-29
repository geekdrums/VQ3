using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Rhythm
{
    public Rhythm(int bt, params Note[] ns)
    {
        notes = new List<Note>();
        baseTime = bt;
        notes.AddRange(ns);
    }
    public Rhythm(int bt, string rhythm) : this(bt, Note.Parse(rhythm)) { }

    public List<Note> notes { get; private set; }
    public int numNote { get { return notes.Count(n => n.hasNote); } }//actual note with tones.
    public int baseTime { get; protected set; }// Beat * 1/baseTime is the basical time of this rhythm.
    public Note this[int i]
    {
        set
        {
            if (0 <= i && i < notes.Count)
                notes[i] = value;
            else throw new ApplicationException("rhythm[i] is out of index: i = " + i);
        }
        get
        {
            if (0 <= i && i < notes.Count)
                return notes[i];
            else return null;
        }
    }

    public bool IsOnBeat() { return Music.mtBeat % baseTime == 0; }
    public int Length() { return notes.Count; }
    public int MTLength()
    {
        if (IsOnBeat())
            return notes.Count * Music.mtBeat / baseTime;
        else throw new NotImplementedException("This rhythm is not OnBeat.");
    }

    /// <summary>
    /// if time is not on thi beat, returns -1.
    /// </summary>
    /// <param name="mt">musical time from start playing this rhythm</param>
    /// <returns></returns>
    public int GetNoteIndex(int mt)
    {
        if (mt % (Music.mtBeat / baseTime) != 0) return -1;
        else mt /= (Music.mtBeat / baseTime);
        return mt;
    }
    /// <summary>
    /// if time is not on this beat, returns null.
    /// </summary>
    /// <param name="mt">musical time from start playing this rhythm</param>
    /// <returns></returns>
    public Note GetNote(int mt)
    {
        return this[GetNoteIndex(mt)];
    }
    /// <summary>
    /// Count up HasTone notes from 0 to index, and return what index of phrase you should access.
    /// </summary>
    /// <param name="index">note index</param>
    /// <returns></returns>
    public int GetToneIndex(int index)
    {
        int toneIndex = -1;
        for (int i = 0; i <= ((index > notes.Count - 1) ? notes.Count - 1 : index); i++)
            if (notes[i].hasNote)
                toneIndex++;
        return toneIndex;
    }
    public Note GetNoteandToneIndex(int mt, out int toneIndex)
    {
        int index = GetNoteIndex(mt);
        toneIndex = GetToneIndex(index);
        return this[index];
    }

    public override string ToString()
    {
        StringBuilder res = new StringBuilder();
        foreach (Note n in notes) {
            res.Append(n.ToString() + " ");
        }
        return res.ToString();
    }

    public static readonly Rhythm REST_RHYTHM = new Rhythm(1, "nn");
    public static readonly Rhythm ONE_NOTE_RHYTHM = new Rhythm(4, "ta");
}