using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Policies;
using System.Buffers.Text;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Attack))]
public class Fighter : Agent
{
    [Header("Fighter Settings")]
    [SerializeField] float speed = 5f;
    [SerializeField] GameObject spawnPoint;
    [SerializeField] private float goodDistance = 0.5f;
    [SerializeField] private float meaningfulDistance = 0.5f;

    private Rigidbody rb;
    private Attack attackComponent;
    private Fighter opponent;

    public float health = 100f;

    Vector3 lastPosition;

    private void Start() {
        attackComponent = GetComponent<Attack>();
        rb = GetComponent<Rigidbody>();

        foreach (Transform child in transform.parent) {
            if (child != transform && child.TryGetComponent<Fighter>(out Fighter fighter)) {
                opponent = fighter;
            }
        }
        if (opponent == null) {
            Debug.LogError("Fighter: Start: No opponent found");
        }
    }

    IEnumerator FrameTakeDamage() {
        Material baseMaterial = GetComponent<Renderer>().material;
        GetComponent<Renderer>().material.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        GetComponent<Renderer>().material.color = baseMaterial.color;
    }

    public void TakeDamage(float damage) {
        StartCoroutine(FrameTakeDamage());
        health -= damage;

        AddReward(-0.1f);
        if (health <= 0) {
            AddReward(-1f);
            opponent.Win();
        }
    }

    public override void OnEpisodeBegin()
    {
        transform.localPosition = spawnPoint.transform.localPosition;

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        health = 100f;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(attackComponent.isAttacking);
        sensor.AddObservation(attackComponent.isInCooldown);
    }


    private Vector3 Move(ActionBuffers actionBuffers) {
        Vector3 direction = Vector3.zero;
        direction.x = Mathf.Clamp(actionBuffers.ContinuousActions[0], -1f, 1f);
        direction.z = Mathf.Clamp(actionBuffers.ContinuousActions[1], -1f, 1f);

        transform.Translate(speed * Time.fixedDeltaTime * direction);
        return direction;
    }

    private bool Attack(ActionBuffers actionBuffers)
    {
        bool haveToAttack = Mathf.Clamp(actionBuffers.ContinuousActions[2], -1f, 1f) > 0f;
    
        if (haveToAttack) {
            attackComponent.LaunchAttack();
        }
        return haveToAttack;
    }

    private float GetDistanceToOpponent() {
        return Vector3.Distance(transform.localPosition, opponent.transform.localPosition);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        Move(actionBuffers);
        bool haveToAttack = Attack(actionBuffers);
        float distanceToOpponent = GetDistanceToOpponent();

        // Reward for being close to the opponent
        if (distanceToOpponent < goodDistance) {
            AddReward(0.01f);
        } else {
            // Punish for being far from the opponent and attacking for no reason
            if (attackComponent.isAttacking) {
                AddReward(-0.003f);
            }
            AddReward(-0.001f);
        }

        if (attackComponent.opponentInCollider) {
            if (haveToAttack) {
                AddReward(0.1f);
            } else {
                AddReward(-0.13f);
            }
        }

        // Reward for moving a meaningful distance
        float distanceMoved = Vector3.Distance(transform.localPosition, lastPosition);
        if (distanceMoved >= meaningfulDistance) {
            AddReward(0.004f);
        } else {
            AddReward(-0.002f);
        }

        // Reward if the other have less health
        if (opponent.health < health) {
            AddReward(0.001f * (health - opponent.health));
        } else {
            AddReward(-0.001f);
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;

        continuousActions[0] = Input.GetAxis("Horizontal");
        continuousActions[1] = Input.GetAxis("Vertical");
        continuousActions[2] = Input.GetKey(KeyCode.Space) ? 1f : 0f;
    }

    private void OnCollisionEnter(Collision other) {
        if (other.gameObject.TryGetComponent<Wall>(out Wall wall)) {
            Debug.Log("Fighter: OnCollisionEnter: Wall");
            AddReward(-0.1f);
        }
    }

    public void Win() {
        AddReward(1f);
        EndEpisode();
    }

    private void Update() {
        if (transform.position.y < -3f) {
            AddReward(-1f);
            opponent.Win();
            EndEpisode();
        }
    }
}
