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
			if( Primitive.animParam == MidairPrimitive.AnimationParams.None )
			{
				gameObject.SetActive(false);
				Mode = ButtonMode.Hide;
			}
			break;
		case ButtonMode.Showing:
			if( Primitive.animParam == MidairPrimitive.AnimationParams.None )
			{
				Mode = ButtonMode.Active;
			}
			break;
		}


		UpdateInput();


		switch( State )
		{
		case PushState.None:
		case PushState.PressingOther:
			switch( Mode )
			{
			case ButtonMode.Blink:
				Primitive.SetColor(Color.Lerp(ColorManager.Base.Shade, ColorManager.Base.Light, Music.MusicalCos(8)));
				Text.color = ColorManager.Base.Bright;
				break;
			case ButtonMode.Disable:
				Primitive.SetColor(ColorManager.Base.Dark);
				Text.color = ColorManager.Base.Shade;
				break;
			default:
				Primitive.SetColor(ColorManager.Base.Shade);
				Text.color = ColorManager.Base.Bright;
				break;
			}
			Primitive.SetTargetSize(initialRadius_);
			break;
		case PushState.Pressing:
			Primitive.SetColor(ColorManager.Base.Light);
			Text.color = Color.black;
			Primitive.SetTargetSize(initialRadius_ - 0.5f);
			break;
		case PushState.Leaving:
			Primitive.SetColor(ColorManager.Base.Shade);
			Text.color = Color.white;
			Primitive.SetTargetSize(initialRadius_);
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
			ButtonMode oldMode = Mode;
			Mode = mode;
			if( withAnim )
			{
				if( Mode == ButtonMode.Hide )
				{
					Primitive.SetAnimationSize(initialRadius_, 0);
					Mode = ButtonMode.Hiding;
				}
				else
				{
					gameObject.SetActive(true);
					Primitive.SetAnimationSize(0, initialRadius_);
					Mode = ButtonMode.Showing;
				}
			}
			else
			{
				gameObject.SetActive(Mode != ButtonMode.Hide);
			}
			HitCollider.enabled = (Mode != ButtonMode.Hide && Mode != ButtonMode.Disable);
			if( Mode == ButtonMode.Disable )
			{
				Primitive.SetColor(ColorManager.Base.Dark);
				Text.color = ColorManager.Base.Shade;
			}
		}
	}

	public void SetText(string text)
	{
		Text.text = text;
	}
}