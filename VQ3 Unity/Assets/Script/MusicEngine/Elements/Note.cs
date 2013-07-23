using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Note
{
    public Note(params uint[] flags)
    {
        foreach (uint ui in flags)
            flag |= ui;
    }
    public Note(params ENote[] flags)
    {
        foreach (ENote f in flags)
            flag |= (uint)f;
    }

    public readonly uint flag = 0;

    public bool hasNote { get { return (flag & (uint)ENote.NOTE) != 0; } }
    public bool isRest { get { return (flag & (uint)ENote.REST) != 0; } }
    public bool isSustain { get { return (flag & (uint)ENote.SUSTAIN) != 0; } }
    public bool isTie { get { return (flag & (uint)ENote.TIE) != 0; } }
    public bool isForte { get { return (flag & (uint)ENote._FORTE) != 0; } }
    public bool isPiano { get { return (flag & (uint)ENote._PIANO) != 0; } }
    public bool isStaccato { get { return (flag & (uint)ENote._STACCATO) != 0; } }
    public bool isDrumRoll { get { return (flag & (uint)ENote._DRUMROLL) != 0; } }
    public bool hasAccent { get { return (flag & (uint)ENote._ACCENT) != 0; } }

    public override string ToString()
    {
        string res = "nn";
        if (hasNote)
        {
            if (isForte)
            {
                res = hasAccent ? "ga" : "da";
            }
            else if (isPiano)
            {
                res = "ti";
            }
            else if (isSustain)
            {
                res = "ra";
            }
            else res = "ta";
            if (isStaccato)
            {
                res += "st";
            }
            else if (isDrumRoll)
            {
                res = "za";
            }
        }
        else if (isSustain)
        {
            res = "aa";
        }
        return res;
    }

    [Flags]
    public enum ENote
    {
        NOTE = 1,
        REST = 2,
        SUSTAIN = 4,
        TIE = ENote.SUSTAIN | ENote.NOTE,
        _FORTE = 8,
        FORTE = ENote._FORTE | ENote.NOTE,
        _PIANO = 16,
        PIANO = ENote._PIANO | ENote.NOTE,
        _STACCATO = 32,
        STACCATO = ENote._STACCATO | ENote.NOTE,
        _ACCENT = 64,
        ACCENT = ENote._ACCENT | ENote.FORTE,
        _DRUMROLL = 128,
        DRUMROLL = ENote._DRUMROLL | ENote.NOTE,
    }


    //汎用的な基本ノート
    public static readonly Note note = new Note(ENote.NOTE);
    public static readonly Note sustain = new Note(ENote.SUSTAIN);
    public static readonly Note pianoNote = new Note(ENote.PIANO);
    public static readonly Note forteNote = new Note(ENote.FORTE);
    public static readonly Note restNote = new Note(ENote.REST);
    public static readonly Note accentNote = new Note(ENote.ACCENT);
    public static readonly Note staccatoNote = new Note(ENote.STACCATO);

    /// <summary>
    /// 二文字ずつのフラグに分けて合成。
    /// NOTE : ta
    /// REST : nn
    /// SUSTAIN : aa
    /// TIE : ra
    /// ACCENT : ga
    /// FORTE : da
    /// PIANO : ti
    /// STACCATO : st
    /// DRUMROLL : za
    /// etc...
    /// </summary>
    /// <param name="rhythm"></param>
    /// <returns></returns>
    public static Note[] Parse(string rhythm)
    {
        string[] stringNotes = rhythm.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        Note[] notes = new Note[stringNotes.Length];
        for (int i = 0; i < stringNotes.Length; i++) {
            int l = stringNotes[i].Length / 2;
            uint[] flags = new uint[l];
            for (int j = 0; j < l; j++) {
                uint f = 0;
                string sign = stringNotes[i].Substring(j * 2, 2);
                if (sign == "ta") f = (uint)ENote.NOTE;
                else if (sign == "nn") f = (uint)ENote.REST;
                else if (sign == "aa") f = (uint)ENote.SUSTAIN;
                else if (sign == "ra") f = (uint)ENote.TIE;
                else if (sign == "ga") f = (uint)ENote.ACCENT;
                else if (sign == "da") f = (uint)ENote.FORTE;
                else if (sign == "ti") f = (uint)ENote.PIANO;
                else if (sign == "st") f = (uint)ENote.STACCATO;
                else if (sign == "za") f = (uint)ENote.DRUMROLL;
                //else if(...
                flags[j] = f;
            }
            notes[i] = new Note(flags);
        }
        return notes;
    }
}