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

    private Vector3 initialPosition;
    private Rigidbody rb;
    private Attack attackComponent;

    [ContextMenu("Reset")]

    private void Start() {
        initialPosition = transform.localPosition;
        attackComponent = GetComponent<Attack>();
        rb = GetComponent<Rigidbody>();
    }

    public override void OnEpisodeBegin()
    {
        transform.localPosition = new Vector3(0, initialPosition.y, 0);

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

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        Vector3 direction = Move(actionBuffers);
        Attack(actionBuffers);

        // Reward for moving
        if (direction != Vector3.zero) {
            AddReward(0.001f);
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
