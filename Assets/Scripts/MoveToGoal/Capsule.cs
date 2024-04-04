using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(MoveToGoal))]
public class Capsule : MonoBehaviour
{
    [SerializeField] float lengthRay = 0.45f;
    private MoveToGoal agent;

    private void Start() {
        agent = GetComponent<MoveToGoal>();
    }

    private void OnTriggerEnter(Collider other) {
        if (other.TryGetComponent<Goal>(out Goal goal)) {
            agent.SetReward(1.5f);
            agent.AddReward(Mathf.Min(-agent.timeToGoal / 100f, 0.5f));
            agent.EndEpisode();
        } else if (other.TryGetComponent<Death>(out Death death)) {
            agent.SetReward(-1f);
            agent.EndEpisode();
        }
    }

    public bool IsGrounded() {
        int layerMask = 1 << 10;
        return Physics.Raycast(transform.position, Vector3.down, lengthRay, layerMask);
    }

    private void OnCollisionEnter(Collision other) {
        if (other.gameObject.TryGetComponent<Wall>(out Wall wall)) {
            agent.SetReward(-1f);
            agent.EndEpisode();
        }
    }

    private void Update() {
        Debug.DrawRay(transform.position, Vector3.down * lengthRay, Color.red);
    }
}
