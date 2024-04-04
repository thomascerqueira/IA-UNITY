using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class Attack : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float damage = 10f;
    [SerializeField] private float cooldown = 1f;
    [SerializeField] private int frameAttack = 15;

    [Header("Attack Components")]
    [SerializeField] private SphereCollider attackCollider;
    [SerializeField] private GameObject model;
    [SerializeField] private Color attackColor = Color.green;

    public bool isAttacking = false;
    public bool isInCooldown = false;
    private Material material;
    private Color originalColor;
    public Fighter opponentInCollider;

    private void Start() {
        material = GetComponent<Renderer>().material;
        originalColor = material.color;
    }

    IEnumerator FrameAttack() {
        // wait for frames
        bool hit = false;
        
        for (int i = 0; i < frameAttack; i++) {
            if (opponentInCollider != null && !hit) {
                opponentInCollider.TakeDamage(damage);
                hit = true;
            }
            yield return new WaitForEndOfFrame();
        }
        isAttacking = false;
        material.color = originalColor;
    }

    IEnumerator AttackCooldown() {
        yield return new WaitForSeconds(cooldown);
        isInCooldown = false;
    }

    public void LaunchAttack() {
        if (!isInCooldown) {
            material.color = attackColor;
            isAttacking = true;
            isInCooldown = true;
            attackCollider.enabled = true;
            StartCoroutine(FrameAttack());
            StartCoroutine(AttackCooldown());
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (other.TryGetComponent<Fighter>(out Fighter fighter)) {
            opponentInCollider = fighter;
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.TryGetComponent<Fighter>(out Fighter fighter)) {
            opponentInCollider = null;
        }
    }
}
