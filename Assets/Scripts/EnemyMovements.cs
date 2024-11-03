using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyMovements : MonoBehaviour
{
    // Rigidbody bileþeni.
    [SerializeField] private Rigidbody2D rb;
    // Hedef pozisyonlar dizisi.
    [SerializeField] Transform[] Positions;
    // Nesne hýzý.
    [SerializeField] float ObjectSpeed;
    // Sonraki pozisyonun dizideki indeksi.
    int NextPosIndex;
    // Sonraki hedef nokta.
    Transform NextPoint;
    // Yönü kontrol eden boolean deðiþken.
    private bool isFacingRight = false;

    // Baþlangýçta çaðrýlan fonksiyon.
    void Start()
    {
        NextPoint = Positions[0];
    }

    // Her karede bir kez çaðrýlan fonksiyon.
    void Update()
    {
        MoveEnemyMovements();
    }

    // Düþmanýn hareketini saðlayan fonksiyon.
    void MoveEnemyMovements()
    {
        // Eðer mevcut pozisyon hedef noktaya ulaþýrsa
        if (transform.position == NextPoint.position)
        {
            Flip(); // Yönü çevir.

            NextPosIndex++; // Sonraki pozisyon indeksini artýr.
            if (NextPosIndex == Positions.Length)
            {
                NextPosIndex = 0; // Sonraki pozisyon indeksi dizinin sýnýrýna ulaþýrsa sýfýrla.
            }
            NextPoint = Positions[NextPosIndex]; // Yeni hedef noktayý belirle.
        }
        else
        {
            // Hedefe doðru hareket et.
            transform.position = Vector3.MoveTowards(transform.position, NextPoint.position, ObjectSpeed * Time.deltaTime);
        }
    }

    // Yönü çeviren fonksiyon
    private void Flip()
    {
        // Yerel ölçeði çevirerek yönü deðiþtir.
        Vector3 localScale = transform.localScale;
        localScale.x *= -1f;
        transform.localScale = localScale;
    }
}