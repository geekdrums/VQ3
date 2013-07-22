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

    public readonly uint flag = 0;

    public bool hasNote { get { return (flag & NOTE) != 0; } }
    public bool isRest { get { return (flag & REST) != 0; } }
    public bool isCutting { get { return (flag & CUTTING) != 0; } }
    public bool isSustain { get { return (flag & SUSTAIN) != 0; } }
    /// <summary>
    /// HasNoteだったときに呼び出す。が、実際IsSustainを判定するのと変わらんのでいらないのか……？
    /// </summary>
    /// <returns></returns>
    //public bool IsTie{get { return (flag & TIE) != 0; }
    public bool isForte { get { return (flag & _FORTE) != 0; } }
    public bool isPiano { get { return (flag & _PIANO) != 0; } }
    public bool isStaccato { get { return (flag & _STACCATO) != 0; } }
    public bool isDrumRoll { get { return (flag & _DRUMROLL) != 0; } }
    public bool isFlam { get { return (flag & _FLAM) != 0; } }
    public bool isToneSoul { get { return (flag & _TONESOUL) != 0; } }
	public bool hasAccent { get { return ( flag & _ACCENT ) != 0; } }

    public override string ToString()
    {
        string res = "nn";
        if (hasNote) {
            if (isForte) {
                res = hasAccent ? (isToneSoul ? "po" : "ga") : "da";
            } else if (isPiano) {
                res = "ti";
            } else if (isSustain) {
                res = "ra";
            } else res = "ta";
            if (isStaccato) {
                res += "st";
            } else if (isDrumRoll) {
                res = "za";
            } else if (isCutting) {
                res = "ka";
            }
        } else if (isSustain) {
            res = "aa";
        }
        return res;
    }

    //フラグ
    public static readonly uint NOTE = 1;
    public static readonly uint REST = 2;
    public static readonly uint SUSTAIN = 4;
    public static readonly uint TIE = SUSTAIN | NOTE;

    private static readonly uint _FORTE = 8;
    public static readonly uint FORTE = _FORTE | NOTE;
    private static readonly uint _PIANO = 16;
    public static readonly uint PIANO = _PIANO | NOTE;

    private static readonly uint _STACCATO = 32;
    public static readonly uint STACCATO = _STACCATO | NOTE;
    private static readonly uint _CUTTING = 64;
    public static readonly uint CUTTING = REST | _CUTTING;

    private static readonly uint _ACCENT = 128;
    public static readonly uint ACCENT = _ACCENT | FORTE;

    public static readonly uint _DRUMROLL = 256;
    public static readonly uint DRUMROLL = _DRUMROLL | NOTE;

    public static readonly uint _FLAM = 512;
    public static readonly uint FLAM = _FLAM | NOTE;

    public static readonly uint _TONESOUL = 1024;
    public static readonly uint TONESOUL = _TONESOUL | ACCENT;


    //汎用的な基本ノート
    public static readonly Note note = new Note(NOTE);
    public static readonly Note sustain = new Note(SUSTAIN);
    public static readonly Note pianoNote = new Note(PIANO);
    public static readonly Note forteNote = new Note(FORTE);
    public static readonly Note restNote = new Note(REST);
    public static readonly Note accentNote = new Note(ACCENT);
    public static readonly Note staccatoNote = new Note(STACCATO);

    public static char[] space = { ' ' };
    public static char[] comma = { ',' };
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
    /// CUTTING : ka
    /// DRUMROLL : za
    /// FLAM : tr
    /// TONESOUL : po
    /// etc...
    /// </summary>
    /// <param name="rhythm"></param>
    /// <returns></returns>
    public static Note[] Parse(string rhythm)
    {
        string[] stringNotes = rhythm.Split(space, StringSplitOptions.RemoveEmptyEntries);
        Note[] notes = new Note[stringNotes.Length];
        for (int i = 0; i < stringNotes.Length; i++) {
            int l = stringNotes[i].Length / 2;
            uint[] flags = new uint[l];
            for (int j = 0; j < l; j++) {
                uint f = 0;
                string sign = stringNotes[i].Substring(j * 2, 2);
                if (sign == "ta") f = NOTE;
                else if (sign == "nn") f = REST;
                else if (sign == "aa") f = SUSTAIN;
                else if (sign == "ra") f = TIE;
                else if (sign == "ga") f = ACCENT;
                else if (sign == "da") f = FORTE;
                else if (sign == "ti") f = PIANO;
                else if (sign == "st") f = STACCATO;
                else if (sign == "ka") f = CUTTING;
                else if (sign == "za") f = DRUMROLL;
                else if (sign == "tr") f = FLAM;
                else if (sign == "po") f = TONESOUL;
                //else if(...
                flags[j] = f;
            }
            notes[i] = new Note(flags);
        }
        return notes;
    }
}