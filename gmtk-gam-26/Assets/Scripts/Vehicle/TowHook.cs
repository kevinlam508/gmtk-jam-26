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

    public Transform targetPoint;
    private Transform cachedTargetPoint;
    public Transform originPoint;
    

    [Header("Visual Chain")]
    
    //[SerializeField, Tooltip("The Mesh of chain link to render")]
    //Mesh link;
    //[SerializeField, Tooltip("The chain link material, must have gpu instancing enabled!")]
    //Material linkMaterial;

    //Matrix4x4[] matrices;
    //Quaternion[] currentNodeRotations;

    public GameObject hookObject;
    private Quaternion prevHookRotation;

    public int maxChainPoints = 20;
    public float distanceBetweenPoints = .2f;
    public float chainDrawSpeed = .05f;
    public float chainDrawProgress = 1;
    private int currentTotalPoints = 0;


    public QuadBezier bezier;
    public LineRenderer line;
    public float chainHangMax = 1;
    public float currentChainHang;
    public float chainTightenSpeed = 10;
    public float chainMoveSpeed = 2;

    private Transform gravityPoint;
    private Transform targetGravityPoint;
    [Header("Visual Chain Fire Arc")]
    public float chainGravity = .02f;
    public float chainGravityMax = 5;
    private float currentChainGravity;
    public float chainFireArcHeight = 4;
    private float startingDistance = 0;
    private float prevDistance = 0;
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

        //matrices = new Matrix4x4[maxChainPoints];
        //Vector3 startPosition = Vector3.zero;
        //for (int i = 0; i < currentTotalPoints; i++)
        //{

        //    linePositions[i] = startPosition;
        //    currentNodeRotations[i] = Quaternion.identity;

        //    linePositions[i] = startPosition;

        //    matrices[i] = Matrix4x4.TRS(startPosition, Quaternion.identity, Vector3.one);

        //    startPosition.y -= distanceBetweenPoints;

        //}
    }

    private void FixedUpdate()
    {
        if (currentHookedTarget != null)
        {
            float dist = Vector3.Distance(currentHookedTarget.transform.position, transform.position);
            if (dist <= captureRange)
            {
                HookTarget cachedHookTarget = currentHookedTarget;
                currentHookedTarget.SetHooked(false);
                currentHookedTarget.SetInView(false);
                Debug.Log(currentHookedTarget.name + " CAPTURED!!!");
                hookTargets.Remove(currentHookedTarget);
                ActivateChainCallback(false);
                cachedHookTarget.OnCaptureComplete();
                hookTargets.Remove(cachedHookTarget);
            }
            SetChainPositions();
            DrawChain();
            //TranslateMatrices();
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
            midpoint += new Vector3(0, chainFireArcHeight, 0);
            midpoint += new Vector3(Random.Range(-1, 1),Random.Range(-1, 1), Random.Range(-1, 1)) * chainWhipRadius;
            gravityPoint.position = midpoint;
            targetGravityPoint.position = midpoint;
            currentChainHang = 0;
            currentChainGravity = 0;
            chainDrawProgress = 0;
            startingDistance = Vector3.Distance(originPoint.position, targetPoint.position);
            prevDistance = startingDistance;
            hookObject.SetActive(true);
        }
        else
        {
            currentHookedTarget = null;
            targetPoint = cachedTargetPoint;
            Vector3 midpoint = (originPoint.position + targetPoint.position) * 0.5f;
            targetGravityPoint.position = midpoint;
            gravityPoint.position = midpoint;
            hookObject.SetActive(false);
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
            prog = Mathf.Lerp(0, chainDrawProgress, prog);
            linePositions[i] = bezier.BezPos(prog);
        }
    }

    private void UpdateGravityPoint()
    {
        Vector3 midpoint = (originPoint.position + targetPoint.position) * 0.5f;
        
        if (targetGravityPoint.position.y > midpoint.y + .5f)
        {
            currentChainGravity += chainGravity;
            currentChainGravity = Mathf.Clamp(currentChainGravity, 0, chainGravityMax);
            targetGravityPoint.position = Vector3.MoveTowards(targetGravityPoint.position, midpoint, currentChainGravity);
            gravityPoint.position = targetGravityPoint.position;
        }
        else
        {
            float dist = Vector3.Distance(originPoint.position, targetPoint.position);
            float distDiff = prevDistance - dist;
            // if tightening pull taught harder
            if (distDiff < 0)
            {
                currentChainHang = Mathf.MoveTowards(currentChainHang, 0, Time.fixedDeltaTime * (chainTightenSpeed + -distDiff));
            }
            else
            {
                currentChainHang = Mathf.MoveTowards(currentChainHang, chainHangMax, Time.fixedDeltaTime * (chainMoveSpeed + distDiff));
            }

            prevDistance = dist;

            Vector3 targetPos = midpoint + Vector3.down * currentChainHang;
            targetGravityPoint.position = targetPos;
            gravityPoint.position = gravityDynamics.Update(Time.fixedDeltaTime, targetPos, Vector3.zero);
        }
        if (chainDrawProgress < 1)
        {
            chainDrawProgress += chainDrawSpeed;
            chainDrawProgress = Mathf.Clamp01(chainDrawProgress);

            hookObject.transform.position = bezier.BezPos(chainDrawProgress);
            
            // hook has arrived
            if (chainDrawProgress >= 1)
                hookObject.transform.rotation = currentHookedTarget.hookSpot.rotation;
            else
                hookObject.transform.LookAt(bezier.BezPos(chainDrawProgress + .02f));
        }
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
                    targetPoint = target.hookSpot;
                    ActivateChainCallback(true);
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

    private void Update()
    {
        //if (activateChain)
        //    Graphics.DrawMeshInstanced(link, 0, linkMaterial, matrices, currentTotalPoints);
    }

    void TranslateMatrices()
    {
        for (int i = 0; i < currentTotalPoints; i++)
        {
            //matrices[i].SetTRS(linePositions[i], currentNodeRotations[i], Vector3.one);
        }
    }

    public void DrawChain()
    {
        //SetPointNumber();
        for (int n = 0; n < linePositions.Length; n++)
        {
            if (n > currentTotalPoints)
                linePositions[n] = targetPoint.position;
            else if (n == linePositions.Length - 1 && chainDrawProgress >= 1)
                linePositions[n] = targetPoint.position;
            else if (n == 0)
                linePositions[n] = originPoint.position;
            else
                linePositions[n] = linePositions[n];
        }

        line.positionCount = currentTotalPoints;
        line.SetPositions(linePositions);
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
