using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightForShadePrimitive : MonoBehaviour
{
	public List<ShadePrimitive> Targets = new List<ShadePrimitive>();

	public void AddTarget(ShadePrimitive shade)
	{
		if( Targets.Contains(shade) == false )
		{
			Targets.Add(shade);
		}
	}

	public void CheckUpdate()
	{
		Targets.RemoveAll((ShadePrimitive s) => s == null || s.Light != this);

		foreach( ShadePrimitive shade in Targets )
		{
			if( this.gameObject.activeInHierarchy )
			{
				shade.gameObject.SetActive(true);
				shade.RecalculatePolygon();
			}
			else
			{
				shade.gameObject.SetActive(false);
			}
		}
	}

	// Start is called before the first frame update
	void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
