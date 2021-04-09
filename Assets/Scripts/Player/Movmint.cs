using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movmint : MonoBehaviour
{
    public float speed = 5.0f;
    public float rotationSpeed = 5.0f;
    public float gravitySpeed;
    public float flJumpMult = 3.0f;

    public float flPsyBallRadius = 5.0f;
    public float flPsyHoleRadius = 7.0f;
    public float flTeleForceMult = 25.0f;

    public Camera cam;

    private float y;

    private Rigidbody rb;
    private Collider colPlayer;
    private bool bInJump;

    private float flLastPsyBallDist;
    private bool bInTK;
    private List<Collider> colsInTk = new List<Collider>();

    // anim stuff
    


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        colPlayer = GetComponent<Collider>();
    }

    void Update()
    {
        if(Input.GetKey(KeyCode.F5))
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    void FixedUpdate()
    {
        var forwardMove = Input.GetAxis("Vertical");
        var sideMove = Input.GetAxis("Horizontal");
        
        HandleMove(forwardMove, sideMove);
        HandleJump();
        //HandleTelekinesis();
        DoTK();
    }

    void HandleMove(float forwardMove, float sideMove)
    {   
        var fw = cam.transform.forward * forwardMove;
        var sd = cam.transform.right * sideMove;

        var inputDir = (fw+sd).normalized;
        inputDir.y = 0.0f;

        y = rb.velocity.y;

        var moveAmount = Mathf.Clamp01(Mathf.Abs(forwardMove) + Mathf.Abs(sideMove));
        var speedAmount = moveAmount * speed;

        // rotation shit
        var targetDir = inputDir;
        if (targetDir == Vector3.zero || (Input.GetButton("Fire1") || Input.GetButton("Fire2") || Input.GetButton("Fire3")))
        {
            targetDir = cam.transform.forward;
            targetDir.y = 0.0f;
        }
        
        var rotAmount = rotationSpeed * Time.fixedDeltaTime;

        var lookDir = Quaternion.LookRotation(targetDir);
        var targetRot = Quaternion.Slerp(transform.rotation, lookDir, rotAmount);

        transform.rotation = targetRot;

        if(forwardMove == 0.0f && sideMove == 0.0f)
            return;
        var moveDir = new Vector3(inputDir.x * speedAmount, y, inputDir.z * speedAmount);
        var vecVelDelta = moveDir - rb.velocity;
        var flSmooth = 3.0f;
        vecVelDelta /= flSmooth;
        rb.velocity += vecVelDelta;
    }

    void HandleJump()
    {
        bool bPressingJump = Input.GetButton("Jump");
        //bool bOnGround = Physics.Raycast(transform.position, -transform.up, 1.1f, ~LayerMask.GetMask("Player"));
        //bool bOnGround=Physics.BoxCast(transform.position - new Vector3(0f, -1f, 0f), new Vector3(0.5f, 0f, 0.5f), -transform.up, Quaternion.identity, 0f, ~LayerMask.GetMask("Player"));
        bool bOnGround = Physics.OverlapBox(transform.position, new Vector3(0.25f, 2f, 0.25f), Quaternion.Euler(0f, 0f, 0f), ~LayerMask.GetMask("Player")).Length > 0;
        if (bOnGround)
        {
            if (bPressingJump && !bInJump)
            {
                Vector3 vecJumpForce = new Vector3(0.0f, 1.0f, 0.0f);
                vecJumpForce *= (flJumpMult * rb.mass);
                rb.AddForce(vecJumpForce);
                bInJump = true;
            }
            else if(!bPressingJump)
            {
                bInJump = false;
            }
        }
    }


    // debug sphere stuff ...
    Vector3 vecPsyBallOrigin = Vector3.zero;
    Vector3 vecPsyHoleOrigin = Vector3.zero;
    Vector3 vecPsyTargetDbg = Vector3.zero;
    void DoTK()
    {
        Vector3 vecPsyHole = transform.TransformPoint(flPsyHoleRadius * 1.5f, flPsyHoleRadius * 1.5f, flPsyHoleRadius * 1.5f);

        int iTkDir = 0;
        if (Input.GetButton("Fire1"))
            iTkDir++;
        if (Input.GetButton("Fire2") || Input.GetButton("Fire3"))
            iTkDir--;
        if (!Input.GetButton("Fire1") && !Input.GetButton("Fire2") && !Input.GetButton("Fire3"))
        {
            colsInTk.Clear();

            foreach (var hitCol in Physics.OverlapSphere(vecPsyHole, flPsyHoleRadius, LayerMask.GetMask("TKable")))
            {
                colsInTk.Add(hitCol);

                var tkRb = hitCol.GetComponent<Rigidbody>();
                Vector3 vecPsyForce = flTeleForceMult * tkRb.mass * (vecPsyHole - hitCol.transform.position).normalized;
                vecPsyForce = vecPsyForce * 7 - (tkRb.velocity.normalized * tkRb.velocity.magnitude * 0.77f * tkRb.mass);
                vecPsyForce -= tkRb.mass * Physics.gravity;
                tkRb.AddForce(vecPsyForce);
            }

            return;
        }

        Ray rayPsyBeam = new Ray();
        rayPsyBeam.direction = cam.transform.forward;
        rayPsyBeam.origin = cam.transform.position;
        RaycastHit hitRay;
        Physics.Raycast(rayPsyBeam, out hitRay, Mathf.Infinity, ~LayerMask.GetMask("Player"));
        RaycastHit hitRayTarget;
        Physics.Raycast(rayPsyBeam, out hitRayTarget, Mathf.Infinity, ~LayerMask.GetMask("Player", "TKable"));

        Vector3 vecPsyBall = hitRay.point;

        vecPsyBallOrigin = vecPsyBall;
        vecPsyHoleOrigin = vecPsyHole;

        Collider[] hitColsTemp = Physics.OverlapSphere(vecPsyBall, flPsyBallRadius, LayerMask.GetMask("TKable"));
        foreach (var hitCol in hitColsTemp)
            if (!colsInTk.Contains(hitCol))
                if(iTkDir < 1)
                    colsInTk.Add(hitCol);

        Vector3 vecAvgTkPos = Vector3.zero;
        foreach (var colInTk in colsInTk)
            vecAvgTkPos += colInTk.transform.position;

        vecAvgTkPos /= colsInTk.Count;

        if (iTkDir == 1)
        {
            foreach (var hitCol in hitColsTemp)
            {
                if (!colsInTk.Contains(hitCol))
                {
                    var tkRb = hitCol.GetComponent<Rigidbody>();
                    Vector3 vecPsyForce = 2f * flTeleForceMult * tkRb.mass * (hitCol.transform.position - rayPsyBeam.origin).normalized;
                    vecPsyForce += flTeleForceMult * tkRb.mass * (hitCol.transform.position - vecPsyBall).normalized;
                    vecPsyForce -= tkRb.mass * Physics.gravity;
                    tkRb.AddForce(vecPsyForce);
                }
            }
        }

        int iYeetCount = 0;

        foreach (var colInTk in colsInTk)
        {
            Vector3 vecPsyTarget;
            switch (iTkDir)
            {
                case -1:
                    if((colInTk.transform.position - rayPsyBeam.origin).magnitude < (rayPsyBeam.origin - vecPsyHole).magnitude + flPsyHoleRadius)
                        vecPsyTarget = vecPsyHole;
                    else
                        vecPsyTarget = rayPsyBeam.origin;
                    break;
                case 0:
                    vecPsyTarget = rayPsyBeam.origin + rayPsyBeam.direction * (vecAvgTkPos - rayPsyBeam.origin).magnitude;
                    break;
                default:
                    vecPsyTarget = hitRayTarget.point;
                    break;
            }
            var tkRb = colInTk.GetComponent<Rigidbody>();
            Vector3 vecPsyForce = flTeleForceMult * tkRb.mass * (vecPsyTarget - colInTk.transform.position).normalized;

            vecPsyTargetDbg = vecPsyTarget;

            Debug.DrawRay(colInTk.transform.position, (vecPsyTarget - colInTk.transform.position), Color.magenta);

            if (iTkDir == 0)
            {
                vecPsyForce -= (0.027f * tkRb.velocity * tkRb.mass * (1/Time.fixedDeltaTime));
            }
            else if((colInTk.transform.position - rayPsyBeam.origin).magnitude < (rayPsyBeam.origin - vecPsyHole).magnitude + flPsyHoleRadius/*(colInTk.transform.position - vecPsyHole).magnitude < flPsyHoleRadius*/)
            {
                if (iTkDir == -1)
                {
                    vecPsyForce = vecPsyForce * 7 - (0.027f * tkRb.velocity * tkRb.mass * (1/Time.fixedDeltaTime));
                }
                else if (iTkDir == 1)
                {
                    if (iYeetCount < 7 && !Physics.Raycast(colInTk.transform.position, (vecPsyTarget - colInTk.transform.position).normalized, 1.5f * (flPsyHoleRadius - (vecPsyHole - colInTk.transform.position).magnitude), LayerMask.GetMask("TKable")))
                    {
                        vecPsyForce = ( 2*7 * vecPsyForce) - (0.14f * tkRb.velocity * tkRb.mass * (1/Time.fixedDeltaTime));
                        iYeetCount++;
                    }
                    else if((colInTk.transform.position - vecPsyHole).magnitude < flPsyHoleRadius)
                    {
                        vecPsyForce = flTeleForceMult * tkRb.mass * (vecPsyHole - colInTk.transform.position).normalized * 7 - (0.027f * tkRb.velocity * tkRb.mass * (1/Time.fixedDeltaTime));
                    }
                }
                /*
                switch(iTkDir)
                {
                    case -1:
                        vecPsyForce = vecPsyForce * 7 - (tkRb.velocity.normalized * tkRb.velocity.magnitude * 0.77f * tkRb.mass);
                        break;
                    case 1:
                        if(iYeetCount < 1 && !Physics.Raycast(colInTk.transform.position, (vecPsyTarget - colInTk.transform.position).normalized, flPsyHoleRadius - (vecPsyHole - colInTk.transform.position).magnitude, LayerMask.GetMask("TKable")))
                        {
                            vecPsyForce = (7 * vecPsyForce) - (tkRb.velocity * 2.0f * tkRb.mass);
                            iYeetCount++;
                        }
                        else
                        {
                            vecPsyForce = flTeleForceMult * tkRb.mass * (vecPsyHole - colInTk.transform.position).normalized * 7 - (tkRb.velocity * 2.0f * 0.77f * tkRb.mass);
                        }
                        break;
                    default:
                        break;
                }
                */
            }
            /*
            if(iTkDir == -1 && (colInTk.transform.position - vecPsyHole).magnitude < flPsyHoleRadius)
                vecPsyForce = vecPsyForce * 7 - (tkRb.velocity.normalized * tkRb.velocity.magnitude * 0.77f * tkRb.mass);
            else if (iTkDir == 0)
                vecPsyForce -= tkRb.velocity * 2.0f * 0.50f * tkRb.mass;
            else if(iTkDir == 1 && (colInTk.transform.position - vecPsyHole).magnitude < flPsyHoleRadius)
                if(!Physics.Raycast(colInTk.transform.position, (vecPsyTarget - colInTk.transform.position).normalized, flPsyHoleRadius - (vecPsyHole - colInTk.transform.position).magnitude, LayerMask.GetMask("TKable")))
                    vecPsyForce = (7 * vecPsyForce) - (tkRb.velocity * 2.0f * tkRb.mass);
                else
                    vecPsyForce = flTeleForceMult * tkRb.mass * (vecPsyHole - colInTk.transform.position).normalized * 7 - (tkRb.velocity * 2.0f * 0.77f * tkRb.mass);
            */
            
            vecPsyForce -= tkRb.mass * Physics.gravity;
            tkRb.AddForce(vecPsyForce);
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(vecPsyBallOrigin, flPsyBallRadius);
        Gizmos.DrawWireSphere(vecPsyHoleOrigin, flPsyHoleRadius);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(vecPsyTargetDbg, 1.0f);
    }
}
