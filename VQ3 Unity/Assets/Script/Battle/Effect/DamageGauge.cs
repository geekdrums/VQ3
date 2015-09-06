using UnityEngine;
using System.Collections;

public class DamageGauge : MonoBehaviour {

	public GaugeRenderer BlackGauge;
	public GaugeRenderer RedGauge;
	public GameObject WaveOrigin;
	public DamageText DamageText;
	public Material WaveMaterial;

	float time_;
	GameObject[] vtWaves_;
	Enemy enemy_;

	// Use this for initialization
	void Start()
	{
	}
	
	// Update is called once per frame
	void Update () {
		time_ += Time.deltaTime;
		if( time_ >= 2.0f || Music.IsJustChangedAt(0) )
        {
            Destroy( transform.parent != null ? transform.parent.gameObject : gameObject );
		}
		if( Music.IsJustChanged ) RedGauge.GetComponentInChildren<Renderer>().enabled = Music.Just.MusicalTime % 3 <= 1;
	}

	public void AddDamage(int damage)
	{
		DamageText.AddDamage(damage);
		time_ = 0;
		BlackGauge.SetRate((float)enemy_.HitPoint / enemy_.MaxHP, 0.1f);
		if( vtWaves_ != null )
		{
			WaveMaterial.color = (GameContext.VoxState == VoxState.Overload ? Color.clear : ColorManager.Theme.Bright);
		}
	}

	public void Initialize(Enemy enemy, int damage, ActionResult actionResult, GameObject parent, bool useVTWave = true)
	{
		if( useVTWave )
		{
			vtWaves_ = new GameObject[VoxSystem.VPWaveNum];
			for( int i = 0; i < VoxSystem.VPWaveNum; i++ )
			{
				vtWaves_[i] = (Instantiate(WaveOrigin) as GameObject);
				vtWaves_[i].transform.parent = WaveOrigin.transform.parent;
				vtWaves_[i].transform.localPosition = WaveOrigin.transform.localPosition + (i % 2 == 0 ? Vector3.right : Vector3.left) * (int)((i + 1)/2);
				vtWaves_[i].transform.localScale = new Vector3(1, 0, 1);
			}
			Destroy(WaveOrigin.gameObject);
			WaveMaterial.color = (GameContext.VoxState == VoxState.Overload ? Color.clear : ColorManager.Theme.Bright);
			GameContext.VoxSystem.AddDamageGauge(this);
		}

		enemy_ = enemy;

		Vector3 textPos = DamageText.transform.localPosition;
		DamageText.Initialize(damage, actionResult, Vector3.zero);
		DamageText.transform.localPosition = textPos;

		RedGauge.Rate = (float)(enemy.HitPoint + damage) / enemy.MaxHP;
		BlackGauge.Rate = RedGauge.Rate;
		BlackGauge.SetRate((float)enemy.HitPoint / enemy.MaxHP, 0.1f);

		if( parent != null )
		{
			Vector3 scale = parent.transform.localScale;
			parent.transform.parent = null;
			parent.transform.localScale = scale;
			parent.transform.position = new Vector3(parent.transform.position.x, parent.transform.position.y, 5);
			transform.parent = parent.transform;
			transform.localPosition = Vector3.zero;
			transform.localScale = Vector3.one;
			transform.localRotation = Quaternion.identity;
		}
		else
		{
			transform.position = enemy.transform.position + Vector3.down * 6;
		}
    }

	public void SetVTWave(int index, float scaleY)
	{
		vtWaves_[index].transform.localScale = new Vector3(1, scaleY, 1);
	}
}
