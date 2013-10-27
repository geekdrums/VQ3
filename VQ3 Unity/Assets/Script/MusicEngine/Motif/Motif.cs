using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Motif
{
    public Motif(Rhythm r, Phrase ph)
    {
		rhythm = r;
		phrase = ph;
    }
    
	public Rhythm rhythm { get; protected set; }
	public Phrase phrase { get; protected set; }
    public bool isOnBeat()
    {
        return rhythm.IsOnBeat();
    }
    public bool isEmpty()
    {
        return rhythm.MTLength() == 0;
    }


    /// <summary>
    /// Get next Note and Chord.
    /// </summary>
    /// <param name="mt">musical time from started this motif</param>
    /// <returns>is finished playing</returns>
    public bool PrepareNext(int mt, out Chord nextTones, out Note nextNote)
    {
        if (rhythm.IsOnBeat()) {
            if (mt >= rhythm.MTLength()) { //モチーフを最後まで再生し終わっている
                nextTones = null;
                nextNote = null;
                return false;
            } else {
                int toneIndex = -1;
                nextNote = rhythm.GetNoteandToneIndex(mt, out toneIndex);
                if (nextNote != null) {
                    if (nextNote.hasNote) {
                        nextTones = phrase[toneIndex];
                    } else nextTones = null;// tone will be sustained or stopped.
                } else nextTones = null;// not in baseTime.
                return true;
            }
        } else throw new ApplicationException("this motif is not OnBeat.");
    }
    public Chord baseChord { get { return phrase.baseChord; } }
    public int baseTime { get { return rhythm != null ? rhythm.baseTime : 4; } }
    public Motif MakeTranspose(int t) { return new Motif(rhythm, phrase.MakeTranspose(t)); }

    public static readonly Motif REST_MOTIF = new Motif(new Rhythm(1, "nn"), new Phrase(""));
    public static readonly Motif ONE_NOTE_MOTIF = new Motif(new Rhythm(4, "ta"), new Phrase(null, new Chord(0)));
}