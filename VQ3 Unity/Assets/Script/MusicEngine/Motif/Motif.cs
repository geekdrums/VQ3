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
    /// 次のタイミングで演奏すべき音と音程を取得します。
    /// 演奏し終わった場合falseを返します。
    /// </summary>
    /// <param name="mt">再生開始からの音楽時間</param>
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
                    } else nextTones = null;// 次は音がストップされるか伸ばされるかもしくは、toneを使わない音(cuttingなど)だけ。
                } else nextTones = null;// 今のタイミングはbaseTime上にない。
                return true;
            }
        } else throw new ApplicationException("IsOnBeatでないリズムでこの関数は使えません");
    }
    public Chord baseChord { get { return phrase.baseChord; } }
    public int baseTime { get { return rhythm != null ? rhythm.baseTime : 4; } }
    public Motif MakeTranspose(int t) { return new Motif(rhythm, phrase.MakeTranspose(t)); }

    public static readonly Motif REST_MOTIF = new Motif(new Rhythm(1, "nn"), new Phrase(""));
    public static readonly Motif ONE_NOTE_MOTIF = new Motif(new Rhythm(4, "ta"), new Phrase(null, new Chord(0)));
}