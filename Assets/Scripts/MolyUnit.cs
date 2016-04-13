using UnityEngine;
using System.Collections;

public enum MolyState {
	idle, move, wait, hited
}

public enum SpriteType {
	Ppo, Ppu
}

public class MolyUnit : MonoBehaviour {

	public MolyState nowMolyState = MolyState.idle;
	SpriteType nowMolySpriteType = SpriteType.Ppo;

	public UISprite molySprite;
	public GameObject effectObj;

	TweenPosition molyTweenPos;
	TweenPosition effectTweenPos;

	Vector3 molyFromPos = new Vector3(0, -100, 0);

	public float waitTimer = 0.0f;
	float waitTimeFact = 0.5f;

	public float hitAfterTimer = 0;
	float hitAfterTimeFact = 0.5f;

	void OnEnable() {
		molyTweenPos = molySprite.GetComponent<TweenPosition> ();
		effectTweenPos = effectObj.GetComponent<TweenPosition> ();

		molyTweenPos.from = molyFromPos;
		molyTweenPos.to = Vector3.zero;
		molyTweenPos.eventReceiver = gameObject;
		molyTweenPos.enabled = false;

		effectTweenPos.enabled = false;

		effectObj.SetActive (false);

	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void FixedUpdate() {
		switch (nowMolyState) {
		case MolyState.hited:
			hitAfterTimer += Time.fixedDeltaTime;
			if (hitAfterTimer >= hitAfterTimeFact) {
				hitAfterTimer = 0;
				MolyMove (false);
			}
			break;

		case MolyState.wait:
			waitTimer += Time.fixedDeltaTime;
			if (waitTimer >= waitTimeFact) {
				waitTimer = 0;
				MolyMove (false);
			}
			break;
		}
	}

	void OnPress() {
		switch (nowMolyState) {
		case MolyState.wait:
			waitTimer = 0;
			hitAfterTimer = 0;
			nowMolyState = MolyState.hited;
			HitedMoly ();
			break;
		}
	}

	void MolyMove(bool goUpside = true) {
		nowMolyState = MolyState.move;

		switch (goUpside) {
		case true:
			{
				molyTweenPos.from = molyFromPos;
				molyTweenPos.to = Vector3.zero;
				break;
			}

		case false:
			{
				molyTweenPos.from = Vector3.zero;
				molyTweenPos.to = molyFromPos;
				effectObj.SetActive (false);
				break;
			}
		}

		molyTweenPos.ResetToBeginning ();
		molyTweenPos.callWhenFinished = "FinishMove";
		molyTweenPos.enabled = true;
	}

	void FinishMove() {
		if (molyTweenPos.from == Vector3.zero)
			nowMolyState = MolyState.idle;
		else
			nowMolyState = MolyState.wait;

		molyTweenPos.enabled = false;

	}

	public void StartUseMoly(SpriteType spriteType = SpriteType.Ppo, float waitTime = 0.5f) {
		if (nowMolyState != MolyState.idle)
			return;

		waitTimeFact = waitTime;
		nowMolySpriteType = spriteType;

		switch (spriteType) {
		case SpriteType.Ppo:
			molySprite.spriteName = "ppo";
			break;
		case SpriteType.Ppu:
			molySprite.spriteName = "ppu";
			break;
		}

		molySprite.MakePixelPerfect ();
		MolyMove (true);
	}

	void HitedMoly(){
		switch (nowMolySpriteType) {
		case SpriteType.Ppo:
			molySprite.spriteName = "ppo_hit";

			if (GameManager.instance.haveFeverMode == true)
				GameManager.instance.AddScore (100);
			else
				GameManager.instance.AddScore (10);
			break;

		case SpriteType.Ppu:
			molySprite.spriteName = "ppu_hit";
			GameManager.instance.AddScore (-5);
			break;
		}

		molySprite.MakePixelPerfect ();

		effectTweenPos.gameObject.SetActive (true);
		effectTweenPos.ResetToBeginning ();
		effectTweenPos.enabled = true;

		GameManager.instance.PlayMolyHitSound (nowMolySpriteType);
	}
}
