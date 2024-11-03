using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using TMPro;
using System.Drawing;

public class PlayerMovement : MonoBehaviour
{
    // Oyuncunun maksimum sa�l�k puan�.
    public int maxHealth = 10;
    // Oyuncunun mevcut sa�l�k puan�.
    public int currentHealth;

    // Sa�l�k �ubu�u nesnesi.
    public HealthBar healthBar;

    // Oyuncu animasyon kontrolc�s�.
    public Animator animator;
    private float horizontal; // Yatay giri� de�eri.
    private float speed = 5f; // Oyuncu hareket h�z�.
    private float jumpingPower = 17f; // Z�plama g�c�.
    public float knockbackForce = 3f; // �tenekli itme kuvveti.
    private bool isFacingRight = true; // Oyuncu sa�a bak�yor mu?

    // Fiziksel cisim ve zemin kontrol noktas�.
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;

    // Yeniden do�ma noktas�.
    public Vector3 respawnPoint;
    public GameObject fallDetector; // Oyuncunun d��me kontrol�.
    public GameObject Coins; // Para objesi.

    // Biti� ve �l�m ekranlar�.
    public GameObject finish;
    public GameObject deathScreen;

    void Start()
    {
        // Ba�lang��ta oyuncunun yeniden do�ma noktas�n� belirle.
        respawnPoint = transform.position;
        // Ba�lang��ta oyuncunun sa�l�k puan�n� ayarla.
        currentHealth = maxHealth;
        healthBar.SetMaxHealth(maxHealth);
    }

    // Her karede bir kez �a�r�lan fonksiyon.
    void Update()
    {
        // Yatay giri� de�erini al.
        horizontal = Input.GetAxisRaw("Horizontal");

        // Z�plama tu�una bas�ld���nda ve zemindeyken z�pla.
        if (Input.GetButtonDown("Jump") && IsGrounded())
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpingPower);
            animator.SetBool("isJumping", true);
        }

        // Yere de�ildi�inde z�plama animasyonunu kapat.
        if (IsGrounded())
        {
            animator.SetBool("isJumping", false);
        }

        // Z�plama tu�unu b�rak�ld���nda ve hala yukar� y�nl� bir h�z varsa, z�plama h�z�n� azalt.
        if (Input.GetButtonUp("Jump") && rb.velocity.y > 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
        }

        // D��me tespit�isinin konumunu ayarla.
        fallDetector.transform.position = new Vector2(transform.position.x, fallDetector.transform.position.y);

        // Oyuncunun y�n�n� �evir.
        Flip();
    }

    // Hasar almay� i�leyen fonksiyon.
    void TakeDamage(int damage)
    {
        // Mevcut sa�l�k puan�n� azalt.
        currentHealth -= damage;
        // Sa�l�k �ubu�unu g�ncelle.
        healthBar.SetHealth(currentHealth);
        // Sa�l�k puan� s�f�r veya daha azsa, oyuncuyu �ld�r.
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // �l�m� i�leyen fonksiyon.
    void Die()
    {
        Debug.Log("Player Died"); // Konsola �l�m mesaj� yazd�r.
        Destroy(gameObject); // Oyuncuyu yok et.
        deathScreen.gameObject.SetActive(true); // �l�m ekran�n� g�ster.
    }

    // Tetikleyiciye �arp�ld���nda �a�r�lan fonksiyon.
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // D��me tespit�isine �arp�ld���nda.
        if (collision.tag == "FallDetector")
        {
            transform.position = respawnPoint; // Oyuncuyu yeniden do�ma noktas�na g�t�r.
            TakeDamage(2); // 2 hasar al.
        }
        // Kontrol noktas�na �arp�ld���nda.
        else if (collision.tag == "Checkpoint")
        {
            respawnPoint = transform.position; // Yeniden do�ma noktas�n� g�ncelle.
        }
        // Para objesine �arp�ld���nda.
        else if (collision.tag == "Coins")
        {
            Destroy(collision.gameObject); // Para objesini yok et.
            CoinCounter.Instance.IncreaseCoins(); // Para sayac�n� art�r.
        }
        // Biti� noktas�na �arp�ld���nda.
        else if (collision.gameObject.CompareTag("GameEnd"))
        {
            Time.timeScale = 0f; // Oyun zaman�n� durdur.
            finish.gameObject.SetActive(true); // Biti� ekran�n� g�ster.
        }
        // D��man �l�m alan�na �arp�ld���nda.
        else if (collision.gameObject.CompareTag("EnemyDeath"))
        {
            Destroy(collision.gameObject.transform.parent.gameObject); // D��man� yok et.
        }
        // Oyuncu hasar b�lgesine �arp�ld���nda.
        else if (collision.gameObject.CompareTag("PlayerDamage"))
        {
            TakeDamage(1); // 1 hasar al.

            Vector2 difference = (transform.position - collision.transform.position).normalized;
            Vector2 force = difference * knockbackForce;
            rb.AddForce(force, ForceMode2D.Impulse); // D��mana do�ru itme kuvveti uygula.
        }
    }

    // Sabit zaman aral�klar�nda �a�r�lan fonksiyon.
    private void FixedUpdate()
    {
        rb.velocity = new Vector2(horizontal * speed, rb.velocity.y); // Hareketi uygula.
    }

    // Yere bas�p basmad���n� kontrol eden fonksiyon

    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
    }

    // Sa�a veya sola d�n��� kontrol eden fonksiyon.
    private void Flip()
    {
        if (isFacingRight && horizontal < 0f || !isFacingRight && horizontal > 0f)
        {
            isFacingRight = !isFacingRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }
    }
}