using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum GameState
{
	ready,
	idle,
	gameover,
	wait
}

public class GameManager : MonoBehaviour {
	public static GameManager instance;

	public GameState nowGameState = GameState.ready;
	public UISlider sliderTimeBar;
	public UILabel labelTime;
	public UISprite spriteReady;
	public UILabel labelScore;

	float fGameTimer = 0.0f;
	int nGameScore = 0;

	public List<float> ppoMolyProbability = new List<float>();
	public List<MolyUnit> molyUnitList = new List<MolyUnit> ();
	public List<float> molyWaitTime = new List<float> ();
	public List<float> molyAppearTime = new List<float>();
	public List<int> molyAppearCount = new List<int> ();

	int molySpawnCount = 0;
	float molySpawnTimer = 0;
	float molySpawnRandom = 0;
	int molySelectRandom = 0;
	int listIndex = 0;

	public AudioClip readyClip;
	public AudioClip goClip;
	public AudioClip ppoHitClip;
	public AudioClip ppuHitClip;

	public AudioSource audioSource;

	public GameObject ResultPopupWindow;
	public UILabel ResultScoreLabel;

	int ComboCount = 0;
	public bool haveFeverMode = false;
	float fever = 0;

	public UILabel comboText;
	public UILabel feverText;
	public UISlider feverSlider;

	// Use this for initialization
	void Start () {
		Debug.Log ("game start");
	}
	
	// Update is called once per frame
	void Update () {

		switch (nowGameState) {
		case GameState.idle:
			{
				fGameTimer += Time.deltaTime;
				if (fGameTimer >= 60.0f) {
					fGameTimer = 60.0f;
					nowGameState = GameState.gameover;

					CancelInvoke ("RandomMolySpawn");
					CancelInvoke ("RepeatAddListIndex");

					ResultPopupWindow.SetActive (true);
					ResultScoreLabel.text = nGameScore.ToString ();
				}

				sliderTimeBar.value = (60.0f - fGameTimer) / 60.0f;
				labelTime.text = string.Format ("{0:f0}", (60.0f - fGameTimer));

				break;
			}
		}
	}

	void OnEnable() {
		Debug.Log ("game Enable");
		InitReady ();
	}

	public void InitReady() {
		spriteReady.spriteName = "Ready";
		spriteReady.MakePixelPerfect ();
		spriteReady.gameObject.SetActive (true);
		Invoke ("ReadyToGo", 2.0f);

		audioSource.PlayOneShot (readyClip);

		ResultPopupWindow.SetActive (false);
		ResultScoreLabel.text = "0";

		if (IsInvoking ("ResetCombo"))
			CancelInvoke ("ResetCombo");
		
		if (IsInvoking ("ResetFever"))
			CancelInvoke ("ResetFever");

		ResetCombo ();
		ResetFever ();
	}

	void ReadyToGo() {
		spriteReady.spriteName = "Go";
		spriteReady.MakePixelPerfect ();
		Invoke ("GoToIdle", 1.0f);

		audioSource.PlayOneShot (goClip);
	}

	void GoToIdle() {
		spriteReady.gameObject.SetActive (false);
		fGameTimer = 0.0f;
		nowGameState = GameState.idle;

		listIndex = 0;

		if (IsInvoking ("RandomMolySpawn")) {
			CancelInvoke ("RandomMolySpawn");
		}

		InvokeRepeating ("RandomMolySpawn", 0.01f, molyAppearTime[listIndex]);
		InvokeRepeating ("RepeatAddListIndex", 10f, 10f);

	}


	public void AddScore(int addScore) {
		if (addScore > 0) {
			ComboCount++;
			nGameScore += (addScore * ComboCount);

			comboText.text = string.Format ("{0}[ba4926]COMBOS[-]", ComboCount);
			comboText.gameObject.SetActive (true);

			if (IsInvoking ("ResetCombo")) {
				CancelInvoke ("ResetComobo");
			}
			Invoke ("ResetCombo", 2.0f);
		} else {
			nGameScore += addScore;
		}

		if (addScore > 0 && haveFeverMode == false) {
			fever += 0.05f;
			if (fever >= 1.0f) {
				fever = 1.0f;
				haveFeverMode = true;
				feverText.gameObject.SetActive (true);
				Invoke ("ResetFever", 5.0f);
			}

			feverSlider.value = fever;
		}

		if (nGameScore < 0) {
			nGameScore = 0;
		}
		labelScore.text = nGameScore.ToString ();
	}

	void ResetCombo() {
		ComboCount = 0;
		comboText.gameObject.SetActive (false);
	}

	void ResetFever() {
		fever = 0;
		haveFeverMode = false;
		feverText.gameObject.SetActive (false);
		feverSlider.value = 0;
	}

	void Awake() {
		if (instance == null)
			instance = this;
	}

	void RandomMolySpawn()
	{
		if(nowGameState != GameState.idle) return;

		molySpawnCount = Random.Range (0, molyAppearCount [listIndex]);

		for (int i = 0; i < molySpawnCount; ++i) {
			molySpawnRandom = Random.Range (0f, 100f);
			molySelectRandom = Random.Range (0, 16);

			while (molyUnitList [molySelectRandom].nowMolyState != MolyState.idle) {
				molySelectRandom = Random.Range (0, 16);
			}

			if (molySpawnRandom <= ppoMolyProbability [listIndex]) {
				molyUnitList [molySelectRandom].StartUseMoly (SpriteType.Ppo, molyWaitTime[listIndex]);
			} else {
				molyUnitList [molySelectRandom].StartUseMoly (SpriteType.Ppu, molyWaitTime[listIndex]);
			}
		}
	}

	void RepeatAddListIndex()
	{
		if (nowGameState != GameState.idle) {
			return;
		}

		listIndex++;

		if (listIndex >= ppoMolyProbability.Count)
			listIndex = ppoMolyProbability.Count - 1;
	}

	public void PlayMolyHitSound(SpriteType isPpo)
	{
		switch (isPpo) {
		case SpriteType.Ppo:
			audioSource.PlayOneShot (ppoHitClip);
			break;

		case SpriteType.Ppu:
			audioSource.PlayOneShot (ppuHitClip);
			break;
		}
	}
}
