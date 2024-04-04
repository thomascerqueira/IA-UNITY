using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

[RequireComponent(typeof(Capsule))]
[RequireComponent(typeof(Rigidbody))]
public class MoveToGoal : Agent
{
    [SerializeField] private Transform target;
    [SerializeField] private float speed = 5f;
    [SerializeField] private float forceMultiplier = 10f;
    [SerializeField] private float spawnSize = 5f;

    [SerializeField] private BoxCollider spawnArea;
    [SerializeField] private float heightSpawn = 0.5f;

    public float timeToGoal = 0f;

    private Capsule capsule;
    private Rigidbody rb;

    void Start()
    {
        // Get the capslue game object in the children
        capsule = GetComponentInChildren<Capsule>();
        rb = GetComponent<Rigidbody>();
    }

    public static Vector3 RandomPointInBounds(Bounds bounds)
    {
        return new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            Random.Range(bounds.min.y, bounds.max.y),
            Random.Range(bounds.min.z, bounds.max.z)
        );
    }

    public override void OnEpisodeBegin()
    {
        timeToGoal = 0f;
        if (rb.transform.localPosition.y < 0)
        {
            rb.angularVelocity = Vector3.zero;
            rb.velocity = Vector3.zero;
            rb.transform.localPosition = new Vector3(0, 0.5f, 0);
        }

        // Move the target to a new spot
        Vector3 spawnTarget = RandomPointInBounds(spawnArea.bounds);
        spawnTarget.y = heightSpawn;
        target.position = spawnTarget;

        // Move the agent to a new spot
        Vector3 spawnAgent = RandomPointInBounds(spawnArea.bounds);
        spawnAgent.y = heightSpawn;
        transform.position = spawnAgent;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Target and Agent positions
        sensor.AddObservation(target.localPosition);
        sensor.AddObservation(this.transform.localPosition);
        sensor.AddObservation(capsule.IsGrounded() ? 1 : 0);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = Mathf.Clamp(actionBuffers.ContinuousActions[0], -1f, 1f);
        controlSignal.z = Mathf.Clamp(actionBuffers.ContinuousActions[1], -1f, 1f);

        transform.Translate(speed * Time.deltaTime * controlSignal);

        bool haveToJump = Mathf.Clamp(actionBuffers.ContinuousActions[2], -1f, 1f) > 0f;

        Debug.Log("Have to jump: " + haveToJump);
        Debug.Log("Is grounded: " + capsule.IsGrounded());

        AddReward(-0.0005f);
        if (haveToJump && capsule.IsGrounded())
        {
            Debug.Log("Jumping");
            rb.AddForce(Vector3.up * forceMultiplier, ForceMode.Impulse);
        }

        if (transform.position.y < -3f)
        {
            SetReward(-1f);
            EndEpisode();
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActionsOut = actionsOut.ContinuousActions;

        continuousActionsOut[0] = Input.GetAxis("Horizontal");
        continuousActionsOut[1] = Input.GetAxis("Vertical");
        continuousActionsOut[2] = Input.GetKey(KeyCode.Space) ? 1f : 0f;
    }

    private void Update() {
        timeToGoal += Time.deltaTime;
    }
}
