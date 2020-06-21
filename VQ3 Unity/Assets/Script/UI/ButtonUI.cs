using UnityEngine;
using System;
using System.Collections;

public enum ButtonMode
{
	Hide,
	Hiding,
	Showing,
	Disable,
	Active,
	Blink
}

public enum PushState
{
	None,
	Pressing,
	Leaving,
	PressingOther
}

public class ButtonUI : MonoBehaviour
{
	public Collider HitCollider;
	public MidairPrimitive Primitive;
	public TextMesh Text;

	public PushState State { get; private set; }
	public ButtonMode Mode { get; private set; }

	public event EventHandler OnPushed;

	float initialRadius_;
	
	void Awake()
	{
		State = PushState.None;
		Mode = ButtonMode.Active;
		initialRadius_ = Primitive.Radius;
	}

	void Update()
	{
		switch( Mode )
		{
		case ButtonMode.Disable:
			return;
		case ButtonMode.Hiding:
			if( Primitive.Radius <= 0 )
			{
				gameObject.SetActive(false);
				Mode = ButtonMode.Hide;
			}
			return;
		case ButtonMode.Showing:
			if( Primitive.Radius >= initialRadius_ )
			{
				Mode = ButtonMode.Active;
			}
			return;
		}


		UpdateInput();

		if( Mode == ButtonMode.Disable || Mode == ButtonMode.Hide || Mode == ButtonMode.Hiding ) return;

		switch( State )
		{
		case PushState.None:
		case PushState.PressingOther:
			switch( Mode )
			{
			case ButtonMode.Blink:
				Primitive.SetColor(Color.Lerp(ColorManagerObsolete.Base.Shade, ColorManagerObsolete.Base.Light, Music.MusicalCos(8)));
				Text.color = ColorManagerObsolete.Base.Bright;
				break;
			case ButtonMode.Disable:
				Primitive.SetColor(ColorManagerObsolete.Base.Dark);
				Text.color = ColorManagerObsolete.Base.Shade;
				break;
			default:
				Primitive.SetColor(ColorManagerObsolete.Base.Shade);
				Text.color = ColorManagerObsolete.Base.Bright;
				break;
			}
			Primitive.AnimateRadius(initialRadius_);// AnimType.Linear
			break;
		case PushState.Pressing:
			Primitive.SetColor(ColorManagerObsolete.Base.Light);
			Text.color = Color.black;
			Primitive.AnimateRadius(initialRadius_ - 0.5f);// AnimType.Linear
			break;
		case PushState.Leaving:
			Primitive.SetColor(ColorManagerObsolete.Base.Shade);
			Text.color = Color.white;
			Primitive.AnimateRadius(initialRadius_);// AnimType.Linear
			break;
		}
	}

	void UpdateInput()
	{
		Ray ray = GameContext.MainCamera.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		Physics.Raycast(ray.origin, ray.direction, out hit, Mathf.Infinity);

		if( Input.GetMouseButtonDown(0) )
		{
			if( hit.collider == HitCollider )
			{
				State = PushState.Pressing;
			}
			else
			{
				State = PushState.PressingOther;
			}
		}
		else if( Input.GetMouseButton(0) )
		{
			if( State != PushState.PressingOther )
			{
				if( hit.collider == HitCollider )
				{
					State = PushState.Pressing;
				}
				else
				{
					State = PushState.Leaving;
				}
			}
		}
		else if( Input.GetMouseButtonUp(0) )
		{
			if( State == PushState.Pressing && hit.collider == HitCollider && OnPushed != null )
			{
				OnPushed(this, null);
			}
			State = PushState.None;
		}
		else
		{
			State = PushState.None;
		}
	}

	public void SetMode(ButtonMode mode, bool withAnim = false)
	{
		if( mode != Mode )
		{
			Mode = mode;
			if( withAnim )
			{
				if( Mode == ButtonMode.Hide )
				{
					Primitive.AnimateRadius(0);// AnimType.Linear
					Text.color = Color.clear;
					Mode = ButtonMode.Hiding;
				}
				else
				{
					gameObject.SetActive(true);
					Primitive.AnimateRadius(initialRadius_);// AnimType.Linear
					Mode = ButtonMode.Showing;
				}
			}
			else
			{
				gameObject.SetActive(Mode != ButtonMode.Hide);
			}
			HitCollider.enabled = (Mode != ButtonMode.Hide && Mode != ButtonMode.Disable && Mode != ButtonMode.Hiding);
			if( Mode == ButtonMode.Disable )
			{
				Primitive.SetColor(ColorManagerObsolete.Base.Dark);
				Text.color = ColorManagerObsolete.Base.Shade;
			}
		}
	}

	public void SetText(string text)
	{
		Text.text = text;
	}
}