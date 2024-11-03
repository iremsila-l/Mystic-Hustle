using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using TMPro;
using System.Drawing;

public class PlayerMovement : MonoBehaviour
{
    // Oyuncunun maksimum saðlýk puaný.
    public int maxHealth = 10;
    // Oyuncunun mevcut saðlýk puaný.
    public int currentHealth;

    // Saðlýk çubuðu nesnesi.
    public HealthBar healthBar;

    // Oyuncu animasyon kontrolcüsü.
    public Animator animator;
    private float horizontal; // Yatay giriþ deðeri.
    private float speed = 5f; // Oyuncu hareket hýzý.
    private float jumpingPower = 17f; // Zýplama gücü.
    public float knockbackForce = 3f; // Ýtenekli itme kuvveti.
    private bool isFacingRight = true; // Oyuncu saða bakýyor mu?

    // Fiziksel cisim ve zemin kontrol noktasý.
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;

    // Yeniden doðma noktasý.
    public Vector3 respawnPoint;
    public GameObject fallDetector; // Oyuncunun düþme kontrolü.
    public GameObject Coins; // Para objesi.

    // Bitiþ ve ölüm ekranlarý.
    public GameObject finish;
    public GameObject deathScreen;

    void Start()
    {
        // Baþlangýçta oyuncunun yeniden doðma noktasýný belirle.
        respawnPoint = transform.position;
        // Baþlangýçta oyuncunun saðlýk puanýný ayarla.
        currentHealth = maxHealth;
        healthBar.SetMaxHealth(maxHealth);
    }

    // Her karede bir kez çaðrýlan fonksiyon.
    void Update()
    {
        // Yatay giriþ deðerini al.
        horizontal = Input.GetAxisRaw("Horizontal");

        // Zýplama tuþuna basýldýðýnda ve zemindeyken zýpla.
        if (Input.GetButtonDown("Jump") && IsGrounded())
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpingPower);
            animator.SetBool("isJumping", true);
        }

        // Yere deðildiðinde zýplama animasyonunu kapat.
        if (IsGrounded())
        {
            animator.SetBool("isJumping", false);
        }

        // Zýplama tuþunu býrakýldýðýnda ve hala yukarý yönlü bir hýz varsa, zýplama hýzýný azalt.
        if (Input.GetButtonUp("Jump") && rb.velocity.y > 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
        }

        // Düþme tespitçisinin konumunu ayarla.
        fallDetector.transform.position = new Vector2(transform.position.x, fallDetector.transform.position.y);

        // Oyuncunun yönünü çevir.
        Flip();
    }

    // Hasar almayý iþleyen fonksiyon.
    void TakeDamage(int damage)
    {
        // Mevcut saðlýk puanýný azalt.
        currentHealth -= damage;
        // Saðlýk çubuðunu güncelle.
        healthBar.SetHealth(currentHealth);
        // Saðlýk puaný sýfýr veya daha azsa, oyuncuyu öldür.
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // Ölümü iþleyen fonksiyon.
    void Die()
    {
        Debug.Log("Player Died"); // Konsola ölüm mesajý yazdýr.
        Destroy(gameObject); // Oyuncuyu yok et.
        deathScreen.gameObject.SetActive(true); // Ölüm ekranýný göster.
    }

    // Tetikleyiciye çarpýldýðýnda çaðrýlan fonksiyon.
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Düþme tespitçisine çarpýldýðýnda.
        if (collision.tag == "FallDetector")
        {
            transform.position = respawnPoint; // Oyuncuyu yeniden doðma noktasýna götür.
            TakeDamage(2); // 2 hasar al.
        }
        // Kontrol noktasýna çarpýldýðýnda.
        else if (collision.tag == "Checkpoint")
        {
            respawnPoint = transform.position; // Yeniden doðma noktasýný güncelle.
        }
        // Para objesine çarpýldýðýnda.
        else if (collision.tag == "Coins")
        {
            Destroy(collision.gameObject); // Para objesini yok et.
            CoinCounter.Instance.IncreaseCoins(); // Para sayacýný artýr.
        }
        // Bitiþ noktasýna çarpýldýðýnda.
        else if (collision.gameObject.CompareTag("GameEnd"))
        {
            Time.timeScale = 0f; // Oyun zamanýný durdur.
            finish.gameObject.SetActive(true); // Bitiþ ekranýný göster.
        }
        // Düþman ölüm alanýna çarpýldýðýnda.
        else if (collision.gameObject.CompareTag("EnemyDeath"))
        {
            Destroy(collision.gameObject.transform.parent.gameObject); // Düþmaný yok et.
        }
        // Oyuncu hasar bölgesine çarpýldýðýnda.
        else if (collision.gameObject.CompareTag("PlayerDamage"))
        {
            TakeDamage(1); // 1 hasar al.

            Vector2 difference = (transform.position - collision.transform.position).normalized;
            Vector2 force = difference * knockbackForce;
            rb.AddForce(force, ForceMode2D.Impulse); // Düþmana doðru itme kuvveti uygula.
        }
    }

    // Sabit zaman aralýklarýnda çaðrýlan fonksiyon.
    private void FixedUpdate()
    {
        rb.velocity = new Vector2(horizontal * speed, rb.velocity.y); // Hareketi uygula.
    }

    // Yere basýp basmadýðýný kontrol eden fonksiyon

    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
    }

    // Saða veya sola dönüþü kontrol eden fonksiyon.
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