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
	private int maxHP;

	void Start()
	{
		maxHP = health;
		UpdateHealthBar();
	}

	public void DecreaseHp(int damage)
	{
		health -= damage;
		UpdateHealthBar();

		if (health <= 0)
		{
			DestroyTower();
		}
	}

	void UpdateHealthBar()
	{
		float ratio = (float)health / (float)maxHP;

		// Bu kontrolü yap: hpBox sahneye ait olmayabilir
		if (hpBox != null)
		{
			Vector3 scale = hpBox.localScale;
			scale.y = ratio * 5f;
			hpBox.localScale = scale;

			Vector3 position = hpBox.localPosition;
			position.y = scale.y / 2f;
			hpBox.localPosition = position;
		}
	}

	void DestroyTower()
	{
		if (PhotonNetwork.IsMasterClient)
		{
			PhotonView playerView = GetComponentInParent<PlayerScript>().photonView;
			if (playerView != null)
			{
				playerView.RPC("RPC_DestroyTowerByIndex", RpcTarget.All, index);
			}
		}
	}
	[PunRPC]
	public void RPC_DestroyTower()
	{
		DestroyTowerLocally();
	}
	public void DestroyTowerLocally()
	{
		Destroy(gameObject);
	}

	public int GetMyID()
	{
		return index;
	}
}
