using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ArmyScript : MonoBehaviourPunCallbacks
{
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
	protected Animator animator;
	// bool canMove = true;
	public bool canAttack = false;
	public float attackTimer = 0f;
	PhotonView enemyPlayerPv = null;
	bool isReady = false;
	[SerializeField]
	protected int maxHealth = 25;
	protected int health = 25;
	public virtual void Start()
	{
		rb = GetComponent<Rigidbody>();
		if (rb != null) rb.useGravity = false;

		animator = GetComponent<Animator>();
		health = maxHealth;
	}

	public virtual void FixedUpdate()
	{
		if (!photonView.IsMine || !isReady)
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
				if (animator != null) animator.SetBool("isWalking", true);
				rb.MovePosition(transform.position + flatDirection * moveSpeed * Time.fixedDeltaTime);
				canAttack = false;
			}
			else
			{
				rb.velocity = Vector3.zero;
				canAttack = true;
				attackTimer = 0f;
				if (animator != null) animator.SetBool("isWalking", false);
				photonView.RPC("RPC_TriggerAnimation", RpcTarget.All, "attack");
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
	private void OnTriggerEnter(Collider other)
	{
		if (!photonView.IsMine || string.IsNullOrEmpty(enemyTag))
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
			if (enemy == null) continue;
			float dist = Vector3.Distance(transform.position, enemy.position);
			if (dist < closestDistance)
			{
				closestDistance = dist;
				closestEnemy = enemy;
			}
		}
		if (enemiesInRange.Count < 1 || closestEnemy == null)
		{
			if (gameObject.tag != "Tower")
				closestEnemy = GetBuildingTarget();
		}
		if (target == null || Vector3.Distance(transform.position, closestEnemy.position) < Vector3.Distance(transform.position, target.position))
		{
			target = closestEnemy;
			canAttack = false;
		}
	}

	public void SetEnemyTag(string t)
	{
		if (photonView.IsMine)
			Debug.Log(gameObject.name + " -T-> " + t);
		enemyTag = t;
	}

	[PunRPC]
	public void RPC_StartArmy()
	{
		collidersParent.SetActive(true);

		int targetLayer = gameObject.layer;
		collidersParent.layer = targetLayer;
		foreach (Transform child in collidersParent.transform)
		{
			child.gameObject.layer = targetLayer;
		}

		isReady = true;
		rb.useGravity = true;
		canAttack = false;
	}
	[PunRPC]
	public void RPC_SetLayerAndTag(string layerName)
	{
		gameObject.layer = LayerMask.NameToLayer(layerName);
		if (gameObject.tag != "Tower")
			gameObject.tag = layerName;
		Debug.Log("Changing layer-tag: " + layerName);
	}
	Transform GetBuildingTarget()
	{
		Transform _nearestTarget = null;
		float closestDistance = Mathf.Infinity;

		GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
		foreach (GameObject player in players)
		{
			PhotonView pv = player.GetComponent<PhotonView>();

			if (pv != null && !pv.IsMine)
			{
				foreach (Transform child in player.transform)
				{
					if (child.gameObject.CompareTag("Tower"))
					{
						if (child.gameObject.activeSelf)
						{
							float distance = Vector3.Distance(transform.position, child.position);
							if (distance < closestDistance)
							{
								closestDistance = distance;
								_nearestTarget = child;
							}
						}
					}
				}
				break;
			}
		}
		return _nearestTarget;
	}
	[PunRPC]
	public virtual void TakeDamage(int damage)
	{
		health -= damage;
		if (health <= 0)
		{
			Die();
		}
	}
	[PunRPC]
	public void RPC_Heal(int amount)
	{
		health += amount;
		health = Mathf.Min(health, maxHealth);

	}

	public virtual void Die()
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
			photonView.RPC("RPC_TriggerAnimation", RpcTarget.All, "attack");
			if (target.gameObject.CompareTag("Tower"))
			{
				TowerScript tower = target.GetComponent<TowerScript>();
				if (tower != null)
				{
					int towerId = tower.GetMyID();

					if (enemyPlayerPv == null)
					{
						GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
						foreach (GameObject p in players)
						{
							PhotonView view = p.GetComponent<PhotonView>();
							if (!view.IsMine)
							{
								enemyPlayerPv = view;
								break;
							}
						}
					}

					if (enemyPlayerPv != null)
					{
						AnimationTrigger("attack");
						enemyPlayerPv.RPC("RPC_DealDamageToTower", RpcTarget.All, towerId, damage);
						TurnToTarget();
					}
				}
			}
			else
			{
				ArmyScript enemy = target.GetComponent<ArmyScript>();
				if (enemy != null)
				{
					AnimationTrigger("attack");
					enemy.photonView.RPC("TakeDamage", RpcTarget.AllBuffered, damage);
					TurnToTarget();
				}
				else
				{
					UpdateTarget();
				}
			}
		}
	}
	public virtual void TurnToTarget()
	{
		Vector3 direction = target.position - transform.position;

		Vector3 flatDirection = new Vector3(direction.x, 0f, direction.z).normalized;

		if (flatDirection != Vector3.zero)
		{
			Quaternion targetRotation = Quaternion.LookRotation(flatDirection);
			transform.rotation = targetRotation;
		}
	}
	bool AnimationTrigger(string triggerName)
	{
		if (animator != null)
		{
			animator.ResetTrigger(triggerName);
			animator.SetTrigger(triggerName);
			return true;
		}
		else
		{
			return false;
		}
	}
	[PunRPC]
	public void RPC_TriggerAnimation(string triggerName)
	{
		if (animator != null)
		{
			animator.ResetTrigger(triggerName);
			animator.SetTrigger(triggerName);
		}
	}
}
