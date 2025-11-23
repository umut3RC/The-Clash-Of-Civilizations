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
	public Animator animator;
	// bool canMove = true;
	public bool canAttack = false;
	public float attackTimer = 0f;
	PhotonView enemyPlayerPv = null;
	bool isReady = false;
	[SerializeField] private int maxHealth = 25;
	private int health = 25;
	void Start()
	{
		rb = GetComponent<Rigidbody>();
		rb.useGravity = false;
		animator = GetComponent<Animator>();
		health = maxHealth;
	}

	void FixedUpdate()
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
			closestEnemy = GetBuildingTarget();
		}

		//for buildings
		if (target == null || Vector3.Distance(transform.position, closestEnemy.position) < Vector3.Distance(transform.position, target.position))
		{
			target = closestEnemy;
			canAttack = false;
		}
	}

	public void SetEnemyTag(string t)
	{
		enemyTag = t;
	}

	[PunRPC]
	public void RPC_StartArmy()
	{
		collidersParent.SetActive(true);
		isReady = true;
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
		float closestDistance = Mathf.Infinity;

		GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
		foreach (GameObject player in players)
		{
			PhotonView pv = player.GetComponent<PhotonView>();

			if (pv != null && !pv.IsMine)
			{
				Transform castle = player.transform.Find("Castle");
				if (castle == null) continue;

				List<Transform> targets = new List<Transform>();
				targets.Add(castle);
				foreach (Transform child in castle)
				{
					if (child.gameObject.CompareTag("Tower"))
					{
						targets.Add(child);
					}
				}

				foreach (Transform t in targets)
				{
					if (t == null) continue;

					float distance = Vector3.Distance(transform.position, t.position);
					if (distance < closestDistance)
					{
						closestDistance = distance;
						_nearestTarget = t;
					}
				}
				break;
			}
		}
		return _nearestTarget;
	}
	[PunRPC]
	public void TakeDamage(int damage)
	{
		health -= damage;
		// animator.SetTrigger("damage");
		if (health <= 0)
		{
			Die();
		}
	}
	[PunRPC]
	public void RPC_Heal(int amount)
	{
		// Canı sadece maksimum cana kadar artır
		health += amount;
		health = Mathf.Min(health, maxHealth);

		// (İsteğe bağlı) Eğer bir can barınız (HP Bar) varsa,
		// onu burada güncelleyebilirsiniz.
		// UpdateHealthBar(health, maxHealth);
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
						animator.SetTrigger("attack");
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
					animator.SetTrigger("attack");
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
	void TurnToTarget()
	{
		Vector3 direction = target.position - transform.position;

		Vector3 flatDirection = new Vector3(direction.x, 0f, direction.z).normalized;

		if (flatDirection != Vector3.zero)
		{
			Quaternion targetRotation = Quaternion.LookRotation(flatDirection);
			transform.rotation = targetRotation;
		}
	}
}

