using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AttackEffect : CommandEffect{
    public float interval;

    public override void UpdateAnimation()
    {
        base.UpdateAnimation();
        for( int i = 0; i < primitives.Length; i++ )
        {
            primitives[i].transform.localPosition = new Vector3( primitives[i].transform.localPosition.x, interval * (i - 1.5f), primitives[i].transform.localPosition.z );
        }
    }
}
