using UnityEngine;
using System.Collections;


public class SimpleEffect : MonoBehaviour
{
	Animation anim_;

	void Start()
	{
		anim_ = GetComponentInChildren<Animation>();
		if( anim_ == null )
		{
			Debug.LogError(name + " effect, No anim found.");
			Destroy(gameObject);
		}
		anim_.Play();
	}

	void Update()
	{
		if( anim_ != null && anim_.isPlaying == false )
		{
			Destroy(gameObject);
		}
	}
}
