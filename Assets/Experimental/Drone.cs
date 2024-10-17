using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drone : MonoBehaviour
{
    Rigidbody rb;
    [SerializeField]
    GameObject leftRotor;
    [SerializeField]
    GameObject rightRotor;

    [SerializeField]
    Transform target;

    [SerializeField]
    GameObject leftWing;

    [SerializeField]
    GameObject rightWing;

    static List<Transform> droneTargets;
    float leftRotorTorque, rightRotorTorque;

    public float targetHeight = 10f; // Target height for the drone
    public float targetPitch = 0f; // Target pitch angle
    public float targetYaw = 0f; // Target yaw angle
    float kpAltitude = 1f; // Proportional gain for altitude control
    float kiAltitude = 0.1f; // Integral gain for altitude control
    float kdAltitude = 0.2f; // Derivative gain for altitude control


    public Transform propeller1; // Transform of propeller 1
    public Transform propeller2; // Transform of propeller 2


    private float integralAltitude = 0f;
    private float lastErrorAltitude = 0f;
    [Range(0,40f)]
    public float forward = 0f;
    [Range (-1f, 1f)] public float turn;
    // Start is called before the first frame update
    void Start()
        
    {
        targetHeight = transform.position.y;
        StartCoroutine(targetBots());
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        
        // thanks ChatGPT for the below 10 lines! :D
        // Calculate altitude error (difference between target height and current height)
        float errorAltitude = targetHeight - transform.position.y;

        // Proportional term for altitude control
        float proportionalAltitude = kpAltitude * errorAltitude;

        // Integral term for altitude control
        integralAltitude += errorAltitude * Time.deltaTime;
        float integralTermAltitude = kiAltitude * integralAltitude;

        // Derivative term for altitude control
        float derivativeAltitude = (errorAltitude - lastErrorAltitude) / Time.deltaTime;
        float derivativeTermAltitude = kdAltitude * derivativeAltitude;
        lastErrorAltitude = errorAltitude;

        // Total control signal for altitude
        float controlSignalAltitude = proportionalAltitude + integralTermAltitude + derivativeTermAltitude;

        // Calculate differential force for each propeller for altitude control
        float forcePropeller1 = controlSignalAltitude;
        float forcePropeller2 = controlSignalAltitude; 

        float differential = propeller1.position.y - propeller2.position.y;
        differential += turn * 5f;
        // Apply forces to propellers for altitude control
        Vector3 force1 = propeller1.up * forcePropeller1 * (500f + differential * -20f); ;
        Vector3 force2 = propeller1.up * forcePropeller2 * (500f + differential * 20f);
        rb.AddForceAtPosition(force1, propeller1.position);
        rb.AddForceAtPosition(force2, propeller2.position);
        leftRotorTorque = force1.magnitude;
        rightRotorTorque = force2.magnitude;

        Vector3 currRot = rb.rotation.eulerAngles;
        currRot.x = forward;
        
        rb.rotation = Quaternion.Euler(currRot);

        rb.AddTorque(new Vector3(0f, 1000f * turn, 0f));

        rb.AddForce(transform.forward * forward * 50f);

    }

    private void Update()
    {
        if (target)
        {
            Vector3 toTargetButOutsideArena = target.transform.position + 30f*(target.transform.position - new Vector3(120f, 0f, 120f)).normalized;
            Vector3 toTarget = (toTargetButOutsideArena - transform.position);
            float distanceFromTarget = Mathf.Sqrt((toTarget.x * toTarget.x) + (toTarget.z * toTarget.z));
          
            toTarget.y = transform.position.y;
            float dot = Vector3.Dot(toTarget.normalized, transform.forward);
            if(distanceFromTarget>20f) turn = dot - 1f;
            if (Vector3.Cross(transform.forward, toTarget).y > 0)
            {
                turn = -1f * turn;
            }

          
            if (distanceFromTarget > 20f && dot > 0.7f)
            {
                forward = Mathf.Lerp(forward, 40f, Time.deltaTime);
            }
            else forward = Mathf.Lerp(forward, 0, Time.deltaTime);
        }
        else forward = Mathf.Lerp(forward, 0, Time.deltaTime);

        //horizontal stabilisation
        float horizon = Mathf.Abs(Vector3.SignedAngle(Vector3.up, transform.up, transform.forward));
        leftWing.transform.localRotation = Quaternion.Euler(new Vector3(horizon, 0f, 0f));
        rightWing.transform.localRotation = Quaternion.Euler(new Vector3(horizon, 0f, 0f));
       
        // Update rotor speed based on rotor torque
        leftRotor.GetComponent<SimpleRotator>().speed = leftRotorTorque;
        rightRotor.GetComponent<SimpleRotator>().speed = rightRotorTorque;
    }


    IEnumerator targetBots()
    {
        if (droneTargets == null) droneTargets = new List<Transform>();
        yield return new WaitForSeconds(4f);
        float timer = 0f;
        while (true)
        {
           // yield return new WaitUntil(() => target == null);
           
            Transform randomTarget = BattleBot.allBots[Random.Range(0, BattleBot.allBots.Count)].GetHeadTransform();
            if (!droneTargets.Contains(target))
            {
                target = randomTarget;
                droneTargets.Add(target);
            }
            yield return new WaitForSeconds(Random.Range(10f, 20f));
            droneTargets.Remove(target);
            target = null;
            yield return new WaitForFixedUpdate();
        }
    }
}
