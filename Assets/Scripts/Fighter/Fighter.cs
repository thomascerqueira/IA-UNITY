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

    private Rigidbody rb;
    private Attack attackComponent;
    private Fighter opponent;

    [ContextMenu("Reset")]

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

    public override void OnEpisodeBegin()
    {
        transform.localPosition = spawnPoint.transform.localPosition;

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
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

    private void Attack(ActionBuffers actionBuffers)
    {
        bool haveToAttack = Mathf.Clamp(actionBuffers.ContinuousActions[2], -1f, 1f) > 0f;
    
        if (haveToAttack) {
            attackComponent.LaunchAttack();
        }
    }

    private float getDistanceToOpponent() {
        return Vector3.Distance(transform.localPosition, opponent.transform.localPosition);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        Vector3 direction = Move(actionBuffers);
        Attack(actionBuffers);
        float distanceToOpponent = getDistanceToOpponent();

        // Reward for moving
        if (direction != Vector3.zero) {
            AddReward(0.001f);
        }

        // Reward for being close to the opponent
        if (distanceToOpponent < goodDistance) {
            AddReward(0.01f);
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
}
