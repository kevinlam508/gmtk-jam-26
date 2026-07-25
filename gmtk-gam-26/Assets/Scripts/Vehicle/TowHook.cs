using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class TowHook : MonoBehaviour
{
    public float visualRange;
    public float targetRange;
    public float escapeRange;
    public float captureRange;

    public SphereCollider visualRangeDetector;

    private List<HookTarget> hookTargets;
    public HookTarget currentHookedTarget;
 
    public float minRopeNodeDistance;

    public bool activateChain;    

    [Header("Visual Chain")]
    public Transform targetPoint;
    private Transform cachedTargetPoint;
    public Transform originPoint;
    private Transform gravityPoint;
    private Transform targetGravityPoint;

    public QuadBezier bezier;
    public LineRenderer line;
    public float chainGravity;

    public int maxChainPoints = 20;
    private int currentTotalPoints;
    public float distanceBetweenPoints = .2f;


    [Header("Gravity Point Dynamics")]
    public float gravityFrequency = 2f;
    public float gravityDamping = 0.5f;
    public float gravityResponse = 0f;

    public float chainWhipRadius;

    private SecondOrderDynamics gravityDynamics;
    
    private Vector3[] linePositions;

    private Camera mainCam;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (originPoint == null)
        {
            Debug.LogError("No origin point set for Tow Hook... Disabling Script");
            enabled = false;
            return;
        }

        GameObject gravityPointObject = new GameObject();
        gravityPointObject.name = "Gravity Point";
        gravityPoint = gravityPointObject.transform;

        GameObject targetGravityPointObject = new GameObject();
        targetGravityPointObject.name = "Target Gravity Point";
        targetGravityPoint = targetGravityPointObject.transform;

        mainCam = Camera.main;

        bezier.controlPoint = originPoint;
        bezier.endPoint = originPoint;

        visualRangeDetector.radius = visualRange;
        cachedTargetPoint = targetPoint;

        linePositions = new Vector3[maxChainPoints];
        gravityDynamics = new SecondOrderDynamics(gravityFrequency, gravityDamping, gravityResponse, gravityPoint.position);
        line.enabled = false;

        hookTargets = new List<HookTarget>();
    }

    private void FixedUpdate()
    {
        if (currentHookedTarget != null)
        {
            SetChainPositions();
            DrawChain();

            float dist = Vector3.Distance(currentHookedTarget.transform.position, transform.position);
            if (dist <= captureRange)
            {
                currentHookedTarget.SetHooked(false);
                Debug.Log(currentHookedTarget.name + " CAPTURED!!!");
                ActivateChainCallback(false);
            }
        }

        TargetsInRangeCheck();
    }

    private void TargetsInRangeCheck()
    {
        for (int i = 0; i < hookTargets.Count; i++)
        {
            float dist = Vector3.Distance(hookTargets[i].gameObject.transform.position, transform.position);
            if (!hookTargets[i].isHooked)
            {
                if (dist <= targetRange)
                {
                    hookTargets[i].SetInRange(true);
                }
                else
                {
                    hookTargets[i].SetInRange(false);
                }
            }
        }
    }
    public void ActivateChainCallback(bool activate)
    {
        activateChain = activate;
        if (activateChain)
        {
            SetPointNumber();
            Vector3 midpoint = (originPoint.position + targetPoint.position) * 0.5f;
            midpoint += new Vector3(Random.Range(-1, 1),Random.Range(-1, 1), Random.Range(-1, 1)) * chainWhipRadius;
            gravityPoint.position = midpoint;
        }
        else
        {
            currentHookedTarget = null;
            targetPoint = cachedTargetPoint;
            Vector3 midpoint = (originPoint.position + targetPoint.position) * 0.5f;
            targetGravityPoint.position = midpoint;
            gravityPoint.position = midpoint;
        }
            
        line.enabled = activateChain;
    }

    public void SetChainPositions()
    {
        bezier.startPoint = originPoint;
        bezier.endPoint = targetPoint;
        bezier.controlPoint = gravityPoint;

        UpdateGravityPoint();

        for (int i = 0; i < currentTotalPoints; i++)
        {
            float prog = (float)i / currentTotalPoints;

            linePositions[i] = bezier.BezPos(prog);
        }
    }

    private void UpdateGravityPoint()
    {
        Vector3 midpoint = (originPoint.position + targetPoint.position) * 0.5f;
        targetGravityPoint.position = midpoint + Vector3.down * chainGravity;

        gravityPoint.position = gravityDynamics.Update(Time.fixedDeltaTime, targetGravityPoint.position, Vector3.zero);
    }

    public void DrawChain()
    {
        for (int n = 0; n < linePositions.Length; n++)
        {
            if (n > currentTotalPoints)
                linePositions[n] = targetPoint.position;
            else
                linePositions[n] = linePositions[n];
        }

        line.positionCount = currentTotalPoints;
        line.SetPositions(linePositions);
    }

    public void SetRopePositionsCallback(InputAction.CallbackContext context)
    {
        if (!context.started)
        {
            return;
        }

        //chain.SetRopePositions(context.ReadValue<Vector2>());

        RaycastHit hit;
        LayerMask mask = LayerMask.GetMask("Hookable");

        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = mainCam.ScreenPointToRay(new Vector3(mousePos.x, mousePos.y, mainCam.nearClipPlane));

        // Does the ray intersect any objects excluding the player layer
        if (Physics.Raycast(ray, out hit, targetRange, mask))
        {
            HookTarget target = hit.collider.gameObject.GetComponent<HookTarget>();
            if (target != null)
            {
                if (target.isInRange)
                {
                    target.SetHooked(true);
                    ActivateChainCallback(true);
                    targetPoint = target.hookSpot;
                    currentHookedTarget = target;
                }
            }
        }
        else
        {
            if (currentHookedTarget != null)
            {
                currentHookedTarget.SetHooked(false);
                if (hookTargets.Contains(currentHookedTarget))
                {
                    currentHookedTarget.SetInView(true);
                }
            }
            ActivateChainCallback(false);
        }
    }

    //Upon collision with another GameObject, this GameObject will reverse direction
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.name + " Entered Range...");
        HookTarget target = other.gameObject.GetComponent<HookTarget>();
        if (target != null)
        {
            hookTargets.Add(target);
            target.SetInView(true);
        }
    }

    //Upon collision with another GameObject, this GameObject will reverse direction
    private void OnTriggerExit(Collider other)
    {
        Debug.Log(other.name + " Exited Range...");
        HookTarget target = other.gameObject.GetComponent<HookTarget>();
        if (target != null)
        {
            hookTargets.Remove(target);
            if (target == currentHookedTarget)
                ActivateChainCallback(false);
            target.SetInView(false);
        }
    }

    public void SetPointNumber()
    {
        float dist = Vector3.Distance(originPoint.position, targetPoint.position);

        int nodeNum = (int)(dist / distanceBetweenPoints);

        if (nodeNum > maxChainPoints)
            nodeNum = maxChainPoints;
        else if (nodeNum <= 0)
            nodeNum = 1;

        currentTotalPoints = maxChainPoints;
    }
}
