using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ArmyScript : MonoBehaviourPunCallbacks
{
	public int health = 25;
	public int damage = 5;
	public float attackSpeed = 1.5f;
	public int amount = 10;

	public Transform target;
	private List<Transform> enemiesInRange = new List<Transform>();
	public float moveSpeed = 5f;
	public float rotationSpeed = 10f;
	public float targetDistance = 2f;
	public Transform[] enemyBuildings;
	public GameObject collidersParent;
	public string enemyTag = "";
	public Rigidbody rb;
	public Animator animator;
	// bool canMove = true;
	public bool canAttack = false;
	public float attackTimer = 0f;
	PhotonView enemyPlayerPv = null;
	void Start()
	{
		rb = GetComponent<Rigidbody>();
		rb.useGravity = false;
		animator = GetComponent<Animator>();
		// animator.SetTrigger("idle");
	}

	void FixedUpdate()
	{
		if (!photonView.IsMine)
			return;
		if (target == null)
		{
			UpdateTarget();
		}
		else if (!canAttack && target != null)
		{
			Vector3 direction = target.position - transform.position;
			float distance = direction.magnitude;

			Vector3 flatDirection = new Vector3(direction.x, 0f, direction.z).normalized;

			if (flatDirection != Vector3.zero)
			{
				Quaternion targetRotation = Quaternion.LookRotation(flatDirection);
				transform.rotation = targetRotation;
			}

			if (distance > targetDistance)
			{
				animator.SetTrigger("walk");
				rb.MovePosition(transform.position + flatDirection * moveSpeed * Time.fixedDeltaTime);
				canAttack = false;
			}
			else
			{
				rb.velocity = Vector3.zero;
				canAttack = true;
				attackTimer = 0f;
				animator.SetTrigger("attack");
			}
		}
		else if (canAttack && target != null)
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
	}

	public int GetAmount()
	{
		return (amount);
	}

	// public void SetEnemyBuildings(Transform[] _targets)
	// {
	// 	enemyBuildings = _targets;
	// }

	[PunRPC]
	public void RPC_SetEnemyBuildings(int[] viewIDs)
	{
		List<Transform> foundBuildings = new List<Transform>();
		foreach (int id in viewIDs)
		{
			PhotonView view = PhotonView.Find(id);
			if (view != null)
			{
				foundBuildings.Add(view.transform);
			}
		}
		enemyBuildings = foundBuildings.ToArray();
	}

	// private void OnTriggerEnter(Collider other)
	// {
	// 	if (other.transform.root.gameObject.tag == enemyTag)
	// 	{
	// 		if (target == null || Vector3.Distance(transform.position, other.gameObject.transform.position) < Vector3.Distance(transform.position, target.position))
	// 			target = other.transform.root.gameObject.transform;
	// 	}
	// }
	private void OnTriggerEnter(Collider other)
	{
		if (!photonView.IsMine || enemyTag == null)
		{
			return;
		}
		if (other.transform.root.CompareTag(enemyTag))
		{
			Transform enemy = other.transform.root;
			if (!enemiesInRange.Contains(enemy))
			{
				enemiesInRange.Add(enemy);
			}
			UpdateTarget();
		}
	}
	private void OnTriggerExit(Collider other)
	{
		if (!photonView.IsMine || enemyTag == null)
		{
			return;
		}
		if (other.transform.root.CompareTag(enemyTag))
		{
			Transform enemy = other.transform.root;
			enemiesInRange.Remove(enemy);

			if (target == enemy)
			{
				target = null;
				UpdateTarget();
			}
		}
	}

	public void UpdateTarget()
	{
		float closestDistance = Mathf.Infinity;
		Transform closestEnemy = null;

		foreach (Transform enemy in enemiesInRange)
		{
			if (enemy == null) continue; // Ölmüş olabilir
			float dist = Vector3.Distance(transform.position, enemy.position);
			if (dist < closestDistance)
			{
				closestDistance = dist;
				closestEnemy = enemy;
			}
		}
		if (enemiesInRange.Count < 1 || closestEnemy == null)
		{
			closestEnemy = GetBuildingTarget();
		}

		target = closestEnemy;
		canAttack = false;
	}

	public void SetEnemyTag(string t)
	{
		enemyTag = t;
	}

	[PunRPC]
	public void RPC_StartArmy()
	{
		collidersParent.SetActive(true);
		rb.useGravity = true;
		canAttack = false;
	}
	[PunRPC]
	public void RPC_SetLayerAndTag(string layerName)
	{
		gameObject.layer = LayerMask.NameToLayer(layerName);
		gameObject.tag = layerName;
	}
	Transform GetBuildingTarget()
	{
		Transform _nearestTarget = null;
		float distance;

		if (enemyBuildings.Length < 1)
		{
			GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
			foreach (GameObject player in players)
			{
				PhotonView playerView = player.GetComponent<PhotonView>();
				if (!playerView.IsMine)
				{
					playerView.RPC("RPC_RequestEnemyBuildings", playerView.Owner, photonView.ViewID);
					break;
				}
			}
			return null;
		}

		if (enemyBuildings != null && enemyBuildings.Length > 0)
		{
			for (int i = 0; i < enemyBuildings.Length; i++)
			{
				if (_nearestTarget == null)
				{
					_nearestTarget = enemyBuildings[i];
				}
				else
				{
					distance = Vector3.Distance(transform.position, enemyBuildings[i].position);
					if (distance < Vector3.Distance(transform.position, _nearestTarget.position))
					{
						_nearestTarget = enemyBuildings[i];
					}
				}
			}
		}
		return _nearestTarget;
	}
	[PunRPC]
	public void TakeDamage(int damage)
	{
		health -= damage;
		animator.SetTrigger("damage");
		if (health <= 0)
		{
			Die();
		}
	}

	void Die()
	{
		if (photonView.IsMine)
		{

			PhotonNetwork.Destroy(gameObject);
		}
	}
	public void OnEnemyDeath(Transform enemy)
	{
		enemiesInRange.Remove(enemy);

		if (target == enemy)
		{
			target = null;
			UpdateTarget();
		}
	}
	public virtual void AttackTarget()
	{
		if (canAttack && target != null)
		{
			if (target.CompareTag("Tower"))
			{
				TowerScript tower = target.GetComponent<TowerScript>();
				if (tower != null)
				{
					int towerId = tower.GetMyID();

					// Tüm oyuncuları bul
					if (enemyPlayerPv == null)
					{
						GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
						foreach (GameObject p in players)
						{
							enemyPlayerPv = p.GetComponent<PhotonView>();
							break;
						}
					}
					if (enemyPlayerPv != null && !enemyPlayerPv.IsMine)
					{
						enemyPlayerPv.RPC("RPC_DealDamageToTower", RpcTarget.Others, towerId, damage);
					}
				}
			}

			else
			{
				ArmyScript enemy = target.GetComponent<ArmyScript>();
				if (enemy != null)
				{
					animator.SetTrigger("attack");
					enemy.photonView.RPC("TakeDamage", RpcTarget.AllBuffered, damage);
				}
				else
				{
					UpdateTarget();
				}
			}
		}
	}
}
