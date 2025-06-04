using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ArmyScript : MonoBehaviourPunCallbacks
{
	public int totalHealt = 25;
	public int damage = 5;
	public float attackSpeed = 1.5f;
	[SerializeField] private int amount = 10;

	public Transform target;
	public float moveSpeed = 5f;
	public float targetDistance = 2f;
	public Transform[] enemyBuildings;
	public GameObject collidersParent;
	public string enemyTag = "";
	private Rigidbody rb;
	bool canMove = true;
	bool canAttack = false;

	void Start()
	{
		rb = GetComponent<Rigidbody>();
	}

	void FixedUpdate()
	{
		if (canMove && target != null)
		{
			Vector3 direction = (target.position - transform.position).normalized;
			float distance = Vector3.Distance(transform.position, target.position);

			if (distance > targetDistance)
			{
				rb.MovePosition(transform.position + direction * moveSpeed * Time.fixedDeltaTime);
			}
			else
			{
				rb.velocity = Vector3.zero; // Yaklaştığında dur
				canMove = false;
				canAttack = true;
			}
		}
		if (target == null && enemyBuildings.Length > 0 && enemyBuildings[0] != null)
		{
			target = enemyBuildings[0];
		}

	}
	public int GetAmount()
	{
		return (amount);
	}

	public void SetEnemyBuildings(Transform[] _targets)
	{
		enemyBuildings = _targets;
	}

	private void OnTriggerEnter(Collider other)
	{
		Debug.Log(other.transform.root.gameObject.name);
		if (other.transform.root.gameObject.tag == enemyTag)
		{
			target = other.gameObject.transform;
		}
	}
	public void SetMove(bool sts)
	{
		canMove = sts;
	}
	// public void StartArmy()
	// {
	// 	collidersParent.SetActive(true);
	// 	SetMove(true);
	// }

	public void SetEnemyTag(string t)
	{
		enemyTag = t;
	}

	[PunRPC]
	public void RPC_StartArmy()
	{
		collidersParent.SetActive(true);
		SetMove(true);
	}
	[PunRPC]
	public void RPC_SetLayerAndTag(string layerName)
	{
		gameObject.layer = LayerMask.NameToLayer(layerName);
		gameObject.tag = layerName;
	}
}
