/**
	SpriteStudioPlayer
	
	FPS Counter
	
	Copyright(C) 2003-2013 Web Technology Corp. 
	
*/

using UnityEngine;

public class SsFpsCounter : MonoBehaviour
{
	// A FPS counter.
	// It calculates frames/second over each updateInterval,
	// so the display does not keep changing wildly.
	float updateInterval = 0.5f;
	private double lastInterval; // Last interval end time
	private int frames = 0; // Frames over current interval
	private float fps; // Current FPS
	
	public void Start() {
	    lastInterval = Time.realtimeSinceStartup;
	    frames = 0;
	}
	
	public void OnGUI () {
	    // Display label with two fractional digits
	    GUILayout.Label("" + fps.ToString("f2"));
	} 
	
	public void Update() {
	    ++frames;
	    float timeNow = Time.realtimeSinceStartup;
	    if( timeNow > lastInterval + updateInterval )
	    {
	        fps = frames / (float)(timeNow - lastInterval);
	        frames = 0;
	        lastInterval = timeNow;
	    }
	}
}