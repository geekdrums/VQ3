using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class BGEffect : MonoBehaviour
{
    protected float fade = 1;
    protected bool isActive = true;

	// Use this for initialization
    void Start()
    {
    }
    
	// Update is called once per frame
    void Update()
    {
    }

    public void Hide()
    {
        isActive = false;
    }
    protected void UpdateFade()
    {
        if( !isActive )
        {
            fade = Mathf.Lerp( fade, 0, 0.2f );
            if( fade < 0.01f )
            {
                Destroy( gameObject );
            }
        }
    }
}

