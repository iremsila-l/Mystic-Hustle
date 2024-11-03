using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyMovements : MonoBehaviour
{
    // Rigidbody bile�eni.
    [SerializeField] private Rigidbody2D rb;
    // Hedef pozisyonlar dizisi.
    [SerializeField] Transform[] Positions;
    // Nesne h�z�.
    [SerializeField] float ObjectSpeed;
    // Sonraki pozisyonun dizideki indeksi.
    int NextPosIndex;
    // Sonraki hedef nokta.
    Transform NextPoint;
    // Y�n� kontrol eden boolean de�i�ken.
    private bool isFacingRight = false;

    // Ba�lang��ta �a�r�lan fonksiyon.
    void Start()
    {
        NextPoint = Positions[0];
    }

    // Her karede bir kez �a�r�lan fonksiyon.
    void Update()
    {
        MoveEnemyMovements();
    }

    // D��man�n hareketini sa�layan fonksiyon.
    void MoveEnemyMovements()
    {
        // E�er mevcut pozisyon hedef noktaya ula��rsa
        if (transform.position == NextPoint.position)
        {
            Flip(); // Y�n� �evir.

            NextPosIndex++; // Sonraki pozisyon indeksini art�r.
            if (NextPosIndex == Positions.Length)
            {
                NextPosIndex = 0; // Sonraki pozisyon indeksi dizinin s�n�r�na ula��rsa s�f�rla.
            }
            NextPoint = Positions[NextPosIndex]; // Yeni hedef noktay� belirle.
        }
        else
        {
            // Hedefe do�ru hareket et.
            transform.position = Vector3.MoveTowards(transform.position, NextPoint.position, ObjectSpeed * Time.deltaTime);
        }
    }

    // Y�n� �eviren fonksiyon
    private void Flip()
    {
        // Yerel �l�e�i �evirerek y�n� de�i�tir.
        Vector3 localScale = transform.localScale;
        localScale.x *= -1f;
        transform.localScale = localScale;
    }
}