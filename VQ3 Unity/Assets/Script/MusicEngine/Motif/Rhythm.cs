using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Rhythm : IComparable<Rhythm>
{
    public Rhythm(int bt, params Note[] ns)
    {
        notes = new List<Note>();
        baseTime = bt;
        notes.AddRange(ns);
    }
    public Rhythm(int bt, string rhythm) : this(bt, Note.Parse(rhythm)) { }

    public List<Note> notes { get; private set; }
    public int numNote { get { return notes.Count(n => n.hasNote); } }//装飾音や伸ばし以外の発音数
    public int baseTime { get; protected set; }//一拍の何分の一を基底とするか。
    public Note this[int i]
    {
        set
        {
            if (0 <= i && i < notes.Count)
                notes[i] = value;
            else throw new ApplicationException("リズムのNoteの書き換え時に、不適切なインデックスを渡しています");
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
        else throw new NotImplementedException("特殊リズムは普通のLength()でも使ってろってこった");
    }

    /// <summary>
    /// InOnBeat == trueが前提で呼び出す関数。
    /// trueで呼び出したとしても、音がなければ、
    /// 例えば8分音符刻みで、16の裏に呼び出されたときなどは
    /// -1を返すので注意。
    /// </summary>
    /// <param name="start"></param>
    /// <param name="now"></param>
    /// <returns></returns>
    public int GetNoteIndex(int mt)
    {
        if (mt % (Music.mtBeat / baseTime) != 0) return -1;
        else mt /= (Music.mtBeat / baseTime);
        return mt;
    }
    /// <summary>
    /// 再生するべきNoteを返す。音が存在しなかった場合、nullが返される。
    /// </summary>
    /// <param name="start"></param>
    /// <param name="now"></param>
    /// <returns></returns>
    public Note GetNote(int mt)
    {
        return this[GetNoteIndex(mt)];
    }
    /// <summary>
    /// リズムのindex番目までのNoteの中で、HasNote()==trueの物を数えて
    /// 何音目かを返す。
    /// </summary>
    /// <param name="index">リズムのインデックス</param>
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

    public int CompareTo(Rhythm other)
    {
        int n1 = this.numNote, n2 = other.numNote;
        if (n1 == n2) {
            int l = Math.Min(this.MTLength(), other.MTLength());
            Note note1, note2;
            for (int i = 0; i < l; i++) {
                note1 = this.GetNote(i);
                note2 = other.GetNote(i);
                if ((note1 == null || !note1.hasNote) && (note2 == null || !note2.hasNote)) continue;
                else if (note1.hasNote && note2.hasNote) continue;
                else return note1.hasNote ? -1 : 1;//先に音があるほうが小さい。
            }
            return 0;//音の位置がすべて同じなら同値。
        } else return n1 - n2;//音が多いほうが大きい。
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