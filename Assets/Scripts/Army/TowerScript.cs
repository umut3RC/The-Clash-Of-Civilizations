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

// ARTIK ARMY SCRIPT'TEN MİRAS ALIYOR
public class TowerScript : ArmyScript
{
	[Header("Tower Specific")]
	public int index = -1; // Manager'daki bina indexi
	public Transform hpBox; // Can barı görseli
	[Header("Attack Visuals")]
	public Transform firePoint; // Editörden atayacağın namlu ucu
	public LineRenderer lineRenderer; // Editörden atayacağın çizgi bileşeni
	public float laserDuration = 0.15f; // Işının ekranda kalma süresi (çok kısa olmalı)

	// ArmyScript'teki 'maxHealth' serializefield olduğu için onu inspector'dan ayarla.
	// 'health' değişkeni ArmyScript'ten geliyor.

	public override void Start()
	{
		// ArmyScript'in Start'ını çağır (Değişkenleri tanımlasın)
		base.Start();
		// Kuleler doğuştan hazırdır, spawn animasyonu beklemez
		// ArmyScript'teki 'isReady' değişkenini protected yapman gerekebilir 
		// veya ArmyScript'te isReady = true olarak başlatabilirsin.
		// Eğer ArmyScript'te 'isReady' private ise onu 'protected' yap.

		// Kulelerin otomatik hedef belirlemesi için Tag ayarı:
		// Eğer kule bana aitse, düşman tag'ini ayarla
		// if (photonView.IsMine)
		// {
		// Basit mantık: Ben Army1 isem düşman Army2'dir.
		// Bu tag'i kendi oyun mantığına göre string olarak ver.
		// if (gameObject.layer == LayerMask.NameToLayer("Army1"))
		// 	SetEnemyTag("Army2");
		// else
		// 	SetEnemyTag("Army1");
		// }
		if (lineRenderer == null) lineRenderer = GetComponent<LineRenderer>();
		// Başlangıçta kapalı olduğundan emin ol
		if (lineRenderer != null) lineRenderer.enabled = false;
		UpdateHealthBar();
	}

	// EN ÖNEMLİ KISIM: Hareket etmemesi için FixedUpdate'i eziyoruz
	public override void FixedUpdate()
	{
		if (!photonView.IsMine) return;

		// Hedef yoksa ara
		if (target == null)
		{
			UpdateTarget();
		}

		// Saldırı Mantığı (Hareket kodu YOK)
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
			// Hedef menzilden çıktıysa veya saldırılamazsa
			// Kule olduğu için kovalama yapmıyoruz.
			// Sadece menzil kontrolü yapabiliriz.
			float dist = Vector3.Distance(transform.position, target.position);
			// Menzil dışıysa hedefi bırak
			if (dist > targetDistance)
			{
				target = null;
				canAttack = false;
			}
			else
			{
				// Menzildeyse saldırıya hazırlan
				canAttack = true;
			}
		}
	}
	public override void TurnToTarget()
	{
	}

	// ArmyScript'teki TakeDamage fonksiyonu RPC ile çağrıldığında çalışır.
	// Ancak Tower'ın HP barını güncellemesi lazım.
	// ArmyScript'teki TakeDamage fonksiyonunu da 'virtual' yapmalısın!
	[PunRPC]
	public override void TakeDamage(int damage)
	{
		// Önce ArmyScript'in can azaltma işlemini yap
		base.TakeDamage(damage);

		// Sonra Kule'ye özel HP barını güncelle
		UpdateHealthBar();
	}

	// ArmyScript'teki Die fonksiyonu PhotonNetwork.Destroy yapar.
	// Ama kuleler Manager listesinden silinmeli. O yüzden eziyoruz.
	public override void Die()
	{
		if (PhotonNetwork.IsMasterClient)
		{
			PhotonView playerView = GetComponentInParent<PlayerScript>().photonView;
			if (playerView != null)
			{
				// Manager üzerindeki silme fonksiyonunu çağır
				playerView.RPC("RPC_DestroyTowerByIndex", RpcTarget.All, index);
			}
		}
	}

	// Kuleye özel can barı güncelleme
	void UpdateHealthBar()
	{
		// Güvenlik: Sıfıra bölünme hatasını engelle
		if (maxHealth <= 0) return;

		// ArmyScript'ten miras alınan 'health' ve 'maxHealth' kullanılıyor.
		// İkisi de int olduğu için (float) dönüşümü ŞARTTIR, yoksa sonuç hep 0 çıkar.
		float ratio = (float)health / (float)maxHealth;

		if (hpBox != null)
		{
			Vector3 scale = hpBox.localScale;

			// Orijinal mantığınızı koruyoruz (Yükseklik 5 birim üzerinden oranlanıyor)
			scale.y = ratio * 5f;

			hpBox.localScale = scale;

			// NOT: Daha önceki konuşmamızda bahsettiğimiz pivot sorunu varsa (bar yere batıyorsa),
			// pozisyon düzeltme kodunu da buraya ekleyebilirsin. Yoksa bu hali yeterlidir.
		}
	}

	// Manager tarafından çağrılan yerel yok etme
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
			// 1. Önce hasarı ver (ArmyScript'teki temel mantık çalışsın)
			base.AttackTarget();

			// 2. Görsel efekti HERKES İÇİN tetikle
			// Hedefin o anki pozisyonunu gönderiyoruz (Hedef hareket ederse ışın kaymasın diye)
			// Hedefin "orta noktasına" (genelde transform.position ayaklarıdır, biraz yukarı) ateş edelim.
			Vector3 targetCenter = target.position + Vector3.up * 1f;
			photonView.RPC("RPC_FireVisual", RpcTarget.All, targetCenter);
		}
	}

	// Görsel efekti oynatan YENİ RPC
	[PunRPC]
	public void RPC_FireVisual(Vector3 targetPosition)
	{
		// Eğer gerekli bileşenler yoksa çalıştırma
		if (lineRenderer == null || firePoint == null) return;

		// Işını açıp kapatan Coroutine'i başlat
		StartCoroutine(ShowLaserRoutine(targetPosition));
	}

	// Işını kısa süre gösterip kapatan zamanlayıcı
	IEnumerator ShowLaserRoutine(Vector3 targetPos)
	{
		lineRenderer.enabled = true;

		// --- EKLENECEK KRİTİK SATIR ---
		// Çizginin 2 noktadan (Başlangıç ve Bitiş) oluştuğunu sisteme bildiriyoruz.
		lineRenderer.positionCount = 2;
		// ------------------------------

		lineRenderer.SetPosition(0, firePoint.position); // Index 0 (Başlangıç)
		lineRenderer.SetPosition(1, targetPos);          // Index 1 (Bitiş)

		yield return new WaitForSeconds(laserDuration);

		lineRenderer.enabled = false;
	}
}