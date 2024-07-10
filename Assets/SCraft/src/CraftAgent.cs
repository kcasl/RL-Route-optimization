using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class CraftAgent : Agent
{
    public float fuel = 15f;
    public GravityOrbit Gravity;
    public GravityOrbit Gravity2;
    Rigidbody rBody;
    public float RotationSpeed = 10;

    void Start()
    {
        rBody = GetComponent<Rigidbody>();
    }

    public Transform Target;
    private float timer = 0f;
    private float interval = 1f;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= interval)
        {
            fuel -= 1f;
            timer = 0f;
        }

        else if (fuel <= 6)
        {
            AddReward(-0.05f);
        }
    }
    private void FixedUpdate()
    {
        Vector3 gravityUp = Vector3.zero;
        gravityUp = (transform.position - Gravity.transform.position).normalized;
        rBody.AddForce((-gravityUp * Gravity.Gravity) * rBody.mass);
    }
    public override void OnEpisodeBegin()
    {
        timer = 0f;
        fuel = 15f;
        this.rBody.angularVelocity = Vector3.zero;
        this.rBody.velocity = Vector3.zero;
        this.transform.localPosition = new Vector3(0, 0.5f, 0);
        
        Target.localPosition = new Vector3(Random.value * 8 - 4, 0.5f, -20f);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(Target.localPosition);
        sensor.AddObservation(this.transform.localPosition);

        sensor.AddObservation(rBody.velocity.x);
        sensor.AddObservation(rBody.velocity.z);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Planet")
        {
            SetReward(-0.75f);
            EndEpisode();
        }
    }

    public float forceMultiplier = 10;
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = actionBuffers.ContinuousActions[0];
        controlSignal.z = actionBuffers.ContinuousActions[1];
        rBody.AddForce(controlSignal * forceMultiplier);

        float distanceToTarget = Vector3.Distance(this.transform.localPosition, Target.localPosition);

        if (distanceToTarget < 1.72f)
        {
            if (fuel >= 7.5)
            {
                SetReward(1.75f);
                EndEpisode();
            }
            else
            {
                SetReward(1.00f);
                EndEpisode();
            }
        }

        else if (distanceToTarget < 12.2f)
        {   
            if ((distanceToTarget < 10.1f) && (fuel >= 9))
            {
                if ((distanceToTarget < 5.05f) && (fuel >= 7.5))
                {
                    SetReward(0.075f);
                }
                else if ((distanceToTarget < 10.0f) && (distanceToTarget > 5.05f) && (fuel >= 7.5))
                {
                    SetReward(0.05f);
                }
            }
        }

        else if (fuel <= 0)
        {
            SetReward(-0.75f);
            fuel = 15f;
            EndEpisode();
        }
        else if (this.transform.localPosition.y < 0)
        {
            SetReward(-1.0f);
            EndEpisode();
        }
    }
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Horizontal");
        continuousActionsOut[1] = Input.GetAxis("Vertical");
    }
}
