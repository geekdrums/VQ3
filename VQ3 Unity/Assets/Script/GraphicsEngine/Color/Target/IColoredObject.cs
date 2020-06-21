using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public interface IColoredObject
{
	void SetColor(Color color);
	Color GetColor();
}
