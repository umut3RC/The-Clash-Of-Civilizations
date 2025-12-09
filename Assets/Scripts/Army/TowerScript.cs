// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using TMPro;
// using UnityEngine.UI;
// using Photon.Pun;

// public class TowerScript : MonoBehaviourPunCallbacks
// {
// 	public int index = -1;
// 	public int health = 500;
// 	public Transform hpBox;
// 	private int maxHP;

// 	void Start()
// 	{
// 		maxHP = health;
// 		UpdateHealthBar();
// 	}

// 	public void DecreaseHp(int damage)
// 	{
// 		health -= damage;
// 		UpdateHealthBar();

// 		if (health <= 0)
// 		{
// 			DestroyTower();
// 		}
// 	}

// 	void UpdateHealthBar()
// 	{
// 		float ratio = (float)health / (float)maxHP;

// 		if (hpBox != null)
// 		{
// 			Vector3 scale = hpBox.localScale;
// 			scale.y = ratio * 5f;
// 			hpBox.localScale = scale;
// 		}
// 	}

// 	void DestroyTower()
// 	{
// 		if (PhotonNetwork.IsMasterClient)
// 		{
// 			PhotonView playerView = GetComponentInParent<PlayerScript>().photonView;
// 			if (playerView != null)
// 			{
// 				playerView.RPC("RPC_DestroyTowerByIndex", RpcTarget.All, index);
// 			}
// 		}
// 	}
// 	[PunRPC]
// 	public void RPC_DestroyTower()
// 	{
// 		DestroyTowerLocally();
// 	}
// 	public void DestroyTowerLocally()
// 	{
// 		Destroy(gameObject);
// 	}

// 	public int GetMyID()
// 	{
// 		return index;
// 	}
// }
using UnityEngine;
using Photon.Pun;
using System.Collections;

public class TowerScript : ArmyScript
{
	[Header("Tower Specific")]
	public int index = -1;
	public Transform hpBox;
	[Header("Attack Visuals")]
	public Transform firePoint;
	public LineRenderer lineRenderer;
	public float laserDuration = 0.15f;


	public override void Start()
	{

		base.Start();

		if (lineRenderer == null) lineRenderer = GetComponent<LineRenderer>();

		if (lineRenderer != null) lineRenderer.enabled = false;
		UpdateHealthBar();
	}

	public override void FixedUpdate()
	{
		if (!photonView.IsMine) return;

		if (target == null)
		{
			UpdateTarget();
		}
		if (canAttack && target != null)
		{
			if (attackTimer >= attackSpeed)
			{
				attackTimer = 0f;
				AttackTarget();
			}
			else
			{
				attackTimer += Time.deltaTime;
			}
		}
		else if (target != null)
		{
			float dist = Vector3.Distance(transform.position, target.position);
			if (dist > targetDistance)
			{
				target = null;
				canAttack = false;
			}
			else
			{
				canAttack = true;
			}
		}
	}
	public override void TurnToTarget()
	{
	}

	[PunRPC]
	public override void TakeDamage(int damage)
	{
		base.TakeDamage(damage);

		UpdateHealthBar();
	}

	public override void Die()
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

	void UpdateHealthBar()
	{

		if (maxHealth <= 0) return;


		float ratio = (float)health / (float)maxHealth;

		if (hpBox != null)
		{
			Vector3 scale = hpBox.localScale;

			scale.y = ratio * 5f;

			hpBox.localScale = scale;

		}
	}

	public void DestroyTowerLocally()
	{
		Destroy(gameObject);
	}

	public int GetMyID()
	{
		return index;
	}

	public override void AttackTarget()
	{
		if (canAttack && target != null)
		{
			base.AttackTarget();

			Vector3 targetCenter = target.position + Vector3.up * 1f;
			photonView.RPC("RPC_FireVisual", RpcTarget.All, targetCenter);
		}
	}

	[PunRPC]
	public void RPC_FireVisual(Vector3 targetPosition)
	{
		if (lineRenderer == null || firePoint == null) return;

		StartCoroutine(ShowLaserRoutine(targetPosition));
	}

	IEnumerator ShowLaserRoutine(Vector3 targetPos)
	{
		lineRenderer.enabled = true;
		lineRenderer.positionCount = 2;

		lineRenderer.SetPosition(0, firePoint.position);
		lineRenderer.SetPosition(1, targetPos);

		yield return new WaitForSeconds(laserDuration);

		lineRenderer.enabled = false;
	}
}