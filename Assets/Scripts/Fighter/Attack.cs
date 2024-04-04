using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class Attack : MonoBehaviour
{
    [SerializeField] private float cooldown = 1f;
    [SerializeField] private SphereCollider attackCollider;
    [SerializeField] private GameObject model;
    [SerializeField] private Color attackColor = Color.green;
    [SerializeField] private int frameAttack = 15;

    public bool isAttacking = false;
    public bool isInCooldown = false;
    private Material material;
    private Color originalColor;
    

    private void Start() {
        attackCollider.enabled = false;
        material = GetComponent<Renderer>().material;
        originalColor = material.color;
    }

    IEnumerator FrameAttack() {
        attackCollider.enabled = true;
        // wait for 5 frames
        for (int i = 0; i < frameAttack; i++) {
            yield return new WaitForEndOfFrame();
        }
        isAttacking = false;
        attackCollider.enabled = false;
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
}
