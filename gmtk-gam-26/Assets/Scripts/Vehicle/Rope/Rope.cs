using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(LineRenderer))]
public class Rope : MonoBehaviour
{
    [Header("Instanced Mesh Details")]
    [SerializeField, Tooltip("The Mesh of chain link to render")] 
    Mesh link;
    [SerializeField, Tooltip("The chain link material, must have gpu instancing enabled!")] 
    Material linkMaterial;

    [Space]

    [Header("Demo Parameters")]
    [SerializeField, Min(0), Tooltip("The distance to project the mouse into world space")] 
    float mouseOffset = 10f;

    [Space]

    [Header("Verlet Parameters")]

    [SerializeField, Tooltip("The distance between each link in the chain")] 
    float nodeDistance = 0.35f;
    [SerializeField, Tooltip("The radius of the sphere collider used for each chain link")] 
    float nodeColliderRadius = 0.2f;

    [SerializeField, Tooltip("Add some slack so the chain isn't perfect length")]
    int nodeSlack = 2;

    [SerializeField, Tooltip("Works best with a lower value")]
    float gravityStrength = 2;

    [SerializeField, Tooltip("The number of chain links. Decreases performance with high values and high iteration")]
    int maxNodes = 100;

    [SerializeField, Tooltip("The number of chain links. Decreases performance with high values and high iteration")] 
    int currentTotalNodes = 100;

    [SerializeField, Range(0, 1), Tooltip("Modifier to dampen velocity so the simulation can stabilize")] 
    float velocityDampen = 0.95f;

    [SerializeField, Range(0, 0.99f), Tooltip("The stiffness of the simulation. Set to lower values for more elasticity")] float stiffness = 0.8f;

    [SerializeField, Tooltip("Setting this will test collisions for every n iterations. Possibly more performance but less stable collisions")] 
    int iterateCollisionsEvery = 1;

    [SerializeField, Tooltip("Iterations for the simulation. More iterations is more expensive but more stable")] 
    int iterations = 100;

    [SerializeField, Tooltip("How many colliders to test against for every node.")] 
    int colliderBufferSize = 1;

    RaycastHit[] raycastHitBuffer;
    Collider[] colliderHitBuffer;
    public Camera cam;

    // Need a better way of stepping through collisions for high Gravity
    // And high Velocity
    Vector3 gravity;

    Vector3 startLock;
    Vector3 endLock;

    bool isStartLocked = false;
    bool isEndLocked = false;

    public bool chainActive = false;

    [Space]

    // For Debug Drawing the chain/rope
    [Header("Line Renderer")]
    [SerializeField, Tooltip("Width for the line renderer")] float ropeWidth = 0.1f;

    LineRenderer lineRenderer;
    Vector3[] linePositions;

    Vector3[] previousNodePositions;

    Vector3[] currentNodePositions;
    Quaternion[] currentNodeRotations;

    SphereCollider nodeCollider;
    GameObject nodeTester;
    Matrix4x4[] matrices;


    void Awake()
    {
        currentTotalNodes = maxNodes;
        currentNodePositions = new Vector3[currentTotalNodes];
        previousNodePositions = new Vector3[currentTotalNodes];
        currentNodeRotations = new Quaternion[currentTotalNodes];

        raycastHitBuffer = new RaycastHit[colliderBufferSize];
        colliderHitBuffer = new Collider[colliderBufferSize];
        gravity = new Vector3(0, -gravityStrength, 0);
        lineRenderer = this.GetComponent<LineRenderer>();

        // using a single dynamically created GameObject to test collisions on every node
        nodeTester = new GameObject();
        nodeTester.name = "Node Tester";
        nodeTester.layer = 8;
        nodeCollider = nodeTester.AddComponent<SphereCollider>();
        nodeCollider.radius = nodeColliderRadius;


        matrices = new Matrix4x4[currentTotalNodes];

        Vector3 startPosition = Vector3.zero;
        for (int i = 0; i < currentTotalNodes; i++)
        {

            currentNodePositions[i] = startPosition;
            currentNodeRotations[i] = Quaternion.identity;

            previousNodePositions[i] = startPosition;

            matrices[i] = Matrix4x4.TRS(startPosition, Quaternion.identity, Vector3.one);

            startPosition.y -= nodeDistance;
        }

        // for line renderer data
        linePositions = new Vector3[currentTotalNodes];
    }


    void Update()
    {
        if (!chainActive)
            return;
        
        DrawRope();

        // Instanced drawing here is really performant over using GameObjects
        //Graphics.DrawMeshInstanced(link, 0, linkMaterial, matrices, currentTotalNodes);
    }

    private void FixedUpdate()
    {
        if (!chainActive)
            return;

        DrawRope();

        Simulate();

        for (int i = 0; i < iterations; i++)
        {
            ApplyConstraint();

            if(i % iterateCollisionsEvery == 0)
            {
                AdjustCollisions();
            }
        }

        SetAngles();
        TranslateMatrices();
    }

    private void Simulate()
    {
        var fixedDt = Time.fixedDeltaTime;
        for (int i = 0; i < currentTotalNodes; i++)
        {
            Vector3 velocity = currentNodePositions[i] - previousNodePositions[i];
            velocity *= velocityDampen;

            previousNodePositions[i] = currentNodePositions[i];

            // calculate new position
            Vector3 newPos = currentNodePositions[i] + velocity;
            newPos += gravity * fixedDt;
            Vector3 direction = currentNodePositions[i] - newPos;

            currentNodePositions[i] = newPos;
        }
    }

    public void SetPoints(Vector3 originPos, Vector3 targetPos)
    {
        startLock = originPos;
        endLock = targetPos;
        isStartLocked = true;
        isEndLocked = true;

        SetPointNumber();
    }

    public void SetPointNumber()
    {
        float dist = Vector3.Distance(startLock, endLock);

        int nodeNum = (int)(dist / nodeDistance);

        // add some slack
        nodeNum += nodeSlack;

        if (nodeNum > maxNodes)
            nodeNum = maxNodes;
        else if (nodeNum <= 0)
            nodeNum = 1;
        
        currentTotalNodes = nodeNum;
    }

    public void SetRopePositions(Vector2 mousePos)
    {
        if (!isStartLocked && !isEndLocked)
        {
            startLock = cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, cam.nearClipPlane + mouseOffset));
        }
        else if (isStartLocked && !isEndLocked)
        {
            endLock = cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, cam.nearClipPlane + mouseOffset));
        }
    }
    
    private void AdjustCollisions()
    {
        for (int i = 0; i < currentTotalNodes; i++)
        {
            if(i % 2 == 0) continue;

            int result = -1;
            result = Physics.OverlapSphereNonAlloc(currentNodePositions[i], nodeColliderRadius + 0.01f, colliderHitBuffer, ~(1 << 8));

            // if (result > 0)
            // {
                for (int n = 0; n < result; n++)
                {
                    // if (colliderHitBuffer[n].gameObject.layer != 8)
                    {
                        Vector3 colliderPosition = colliderHitBuffer[n].transform.position;
                        Quaternion colliderRotation = colliderHitBuffer[n].gameObject.transform.rotation;

                        Vector3 dir;
                        float distance;

                        Physics.ComputePenetration(nodeCollider, currentNodePositions[i], Quaternion.identity, colliderHitBuffer[n], colliderPosition, colliderRotation, out dir, out distance);
                        
                        currentNodePositions[i] += dir * distance;
                    }
                }
            // }
        }    
    }

    private void ApplyConstraint()
    {
        currentNodePositions[0] = startLock;
        if(isStartLocked)
        {
            currentNodePositions[currentTotalNodes - 1] = endLock;
        }

        for (int i = 0; i < currentTotalNodes - 1; i++)
        {
            var node1 = currentNodePositions[i];
            var node2 = currentNodePositions[i + 1];

            // Get the current distance between rope nodes
            float currentDistance = (node1 - node2).magnitude;
            float difference = Mathf.Abs(currentDistance - nodeDistance);
            Vector3 direction = Vector3.zero;

            // determine what direction we need to adjust our nodes
            if (currentDistance > nodeDistance)
            {
                direction = (node1 - node2).normalized;
            }
            else if (currentDistance < nodeDistance)
            {
                direction = (node2 - node1).normalized;
            }

            // calculate the movement vector
            Vector3 movement = direction * difference;

            // apply correction
            currentNodePositions[i] -= (movement * stiffness);
            currentNodePositions[i + 1] += (movement * stiffness);
        }
    }

    void SetAngles()
    {
        for (int i = 0; i < currentTotalNodes - 1; i++)
        {
            var node1 = currentNodePositions[i];
            var node2 = currentNodePositions[i + 1];

            var dir = (node2 - node1).normalized;
            if(dir != Vector3.zero)
            {
                if( i > 0)
                {
                    Quaternion desiredRotation = Quaternion.LookRotation(dir, Vector3.right);
                    currentNodeRotations[i + 1] = desiredRotation;
                }
                else if( i < currentTotalNodes - 1)
                {
                    Quaternion desiredRotation = Quaternion.LookRotation(dir, Vector3.right);
                    currentNodeRotations[i + 1] = desiredRotation;
                }
                else
                {
                    Quaternion desiredRotation = Quaternion.LookRotation(dir, Vector3.right);
                    currentNodeRotations[i] = desiredRotation;
                }
            }

            if( i % 2 == 0 && i != 0)
            {
                currentNodeRotations[i + 1] *= Quaternion.Euler(0, 0, 90);
            }
        }
    }

    void TranslateMatrices()
    {
        for(int i = 0; i < currentTotalNodes; i++)
        {
            matrices[i].SetTRS(currentNodePositions[i], currentNodeRotations[i], Vector3.one);
        }
    }

    private void DrawRope()
    {
        lineRenderer.startWidth = ropeWidth;
        lineRenderer.endWidth = ropeWidth;
        lineRenderer.positionCount = currentTotalNodes;

        for (int n = 0; n < linePositions.Length; n++)
        {
            if (n > currentTotalNodes)
                linePositions[n] = endLock;
            else
                linePositions[n] = currentNodePositions[n];
        }
        
        lineRenderer.SetPositions(linePositions);
    }

}
