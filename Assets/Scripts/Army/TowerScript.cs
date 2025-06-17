using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Photon.Pun;

public class TowerScript : MonoBehaviourPunCallbacks
{
	public int index = -1;
	public int health = 500;
	public Transform hpBox;
	int maxHP;
	void Start()
	{
		maxHP = health;
	}
	public int GetMyID()
	{
		return index;
	}

	[PunRPC]
	public void RPC_DecreaseHp(int newHp)
	{
		health = newHp;
		UpdateHealthBar();

		if (health <= 0)
		{
			DestroyTower();
		}
	}

	void UpdateHealthBar()
	{
		float ratio = (float)health / (float)maxHP;

		Vector3 scale = hpBox.localScale;
		scale.y = ratio * 5f;
		hpBox.localScale = scale;

		Vector3 position = hpBox.localPosition;
		position.y = scale.y / 2f;
		hpBox.localPosition = position;
	}


	void DestroyTower()
	{
		Debug.Log(index + " Destroyed");
	}
}
