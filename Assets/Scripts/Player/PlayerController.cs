using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // public movement vars
    public float flMoveSpeed = 7f;
    public float flTurnSpeed = 5f;
    public float flJumpVel = 7f;

    // public telekinesis vars
    public float flPsyBallRadius = 5f;
    public float flPsyHoleRadius = 7f;
    public float flTkForce = 25f;

    // player components
    Rigidbody rb;
    BoxCollider col;
    Animator anim;

    // public globals
    public int iHealth;
    [HideInInspector]
    public float flForwardMove;
    [HideInInspector]
    public float flSideMove;
    [HideInInspector]
    public bool bJumpButton;
    [HideInInspector]
    public bool bTkPushButton;
    [HideInInspector]
    public bool bTkPullButton;

    // private globals
    float flMoveAmount;
    bool bInJump;
    bool bOnGround;
    bool bDead;
    bool bDeathHandled;
    List<Collider> colsInTk = new List<Collider>();
    UnityEngine.UI.Slider uiHealthBar;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<BoxCollider>();
        anim = GetComponentInChildren<Animator>();

        uiHealthBar = GameObject.FindGameObjectWithTag("HealthBar").GetComponent<UnityEngine.UI.Slider>();

        iHealth = 100;

        foreach(var erb in GetComponentsInChildren<Rigidbody>())
        {
            erb.isKinematic = true;
        }
        foreach(var ecol in GetComponentsInChildren<Collider>())
        {
            if(ecol.tag != "Weapon")
                ecol.enabled = false;
        }
        rb.isKinematic = false;
        GetComponent<CapsuleCollider>().enabled = true;
        col.enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        // Get player input
        flForwardMove = Input.GetAxis("Vertical");
        flSideMove = Input.GetAxis("Horizontal");
        bJumpButton = Input.GetButton("Jump");
        bTkPushButton = Input.GetButton("Fire1");
        bTkPullButton = Input.GetButton("Fire2") || Input.GetButton("Fire3");

        // ghetto restart level button
        if(Input.GetKey(KeyCode.F5))
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        
        if(bDeathHandled)
            return;
        
        if(bDead)
        {
            anim.enabled = false;

            foreach (var rb in GetComponentsInChildren<Rigidbody>())
            {
                rb.interpolation = RigidbodyInterpolation.Interpolate;
                rb.isKinematic = false;
            }
            foreach (var col in GetComponentsInChildren<Collider>())
            {
                col.enabled = true;
            }

            GetComponent<Rigidbody>().isKinematic = true;
            GetComponent<Collider>().enabled = false;
            bDeathHandled = true;
        }
    }
    

    public float dbgSpeed;
    void FixedUpdate()
    {
        HandleJump();
        HandleMove();
        HandleTelekinesis();
        HandleAnims();
        dbgSpeed = new Vector3(rb.velocity.x, 0f, rb.velocity.z).magnitude;
    }

    void HandleJump()
    {
        Vector3 vecFeetCenter = col.bounds.center - new Vector3(0f, col.bounds.extents.y, 0f);
        Vector3 vecFeetHalfExts = col.bounds.extents;
        vecFeetHalfExts.y = ((vecFeetHalfExts.x + vecFeetHalfExts.z) - (vecFeetHalfExts.x + vecFeetHalfExts.z) * .5f) / 2;
        vecFeetHalfExts.x *= .5f;
        vecFeetHalfExts.z *= .5f;
        bOnGround = Physics.OverlapBox(vecFeetCenter, vecFeetHalfExts, col.transform.rotation, ~LayerMask.GetMask("Player")).Length > 0;
        if(bOnGround)
        {
            if(bJumpButton && !bInJump)
            {
                rb.velocity = new Vector3(rb.velocity.x * 1.75f, flJumpVel, rb.velocity.z * 1.75f);
                bInJump = true;
            }
            else if(!bJumpButton)
            {
                bInJump = false;
            }
        }
    }

    void HandleMove()
    {
        Vector3 vecInputDir = ((Camera.main.transform.forward * flForwardMove) + (Camera.main.transform.right * flSideMove)).normalized;
        vecInputDir.y = 0f;

        flMoveAmount = Mathf.Clamp01(Mathf.Abs(flForwardMove) + Mathf.Abs(flSideMove));
        float flMoveSpeedCur = flMoveAmount * flMoveSpeed;

        Vector3 vecTargetDir = vecInputDir;
        if(vecTargetDir == Vector3.zero || bTkPushButton || bTkPullButton)
            vecTargetDir = Camera.main.transform.forward;
        vecTargetDir.y = 0f;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(vecTargetDir), flTurnSpeed * Time.fixedDeltaTime);

        if(!bOnGround)
            return;

        if(flForwardMove == 0f && flSideMove == 0f)
            return;
        Vector3 vecVelTarget = new Vector3(vecInputDir.x * flMoveSpeedCur, 0f, vecInputDir.z * flMoveSpeedCur);
        if(flMoveAmount == 1)
        {// clamp velocity to flMoveSpeed
            vecVelTarget = vecVelTarget.normalized * flMoveSpeed;
        }
        vecVelTarget.y = rb.velocity.y;
        Vector3 vecVelDelta = vecVelTarget - rb.velocity;
        float flSmooth = 3f;

        if(anim.GetCurrentAnimatorStateInfo(0).IsName("DamageReceived"))
            return;

        rb.velocity += vecVelDelta / flSmooth;
    }

    void HandleTelekinesis()
    {
        int iTkDir = 0;
        if(bTkPushButton)
            iTkDir++;
        if(bTkPullButton)
            iTkDir--;
        
        // r = sqrt((rx)^2 + (ry)^2 + (rz)^2)
        // r^2 = (rx)^2 + (yr)^2 + (zr)^2
        // (rx)^2 = r^2 - (yr)^2 - (zr)^2
        // rx = sqrt(r^2 - (yr)^2 - (zr)^2)
        // x = (sqrt(r^2 - (yr)^2 - (zr)^2))/r

        Vector3 vecPsyHole = transform.TransformPoint(flPsyHoleRadius * .7f, Mathf.Sqrt(Mathf.Pow(flPsyHoleRadius, 2) - Mathf.Pow(0.7f * flPsyHoleRadius, 2) - Mathf.Pow(0.5f * flPsyHoleRadius, 2))/flPsyHoleRadius, flPsyHoleRadius * .5f);

        if(!(bTkPushButton || bTkPullButton))
        {
            List<Collider> colsKeepInTk = new List<Collider>();
            foreach(var hitCol in Physics.OverlapSphere(vecPsyHole, flPsyHoleRadius, LayerMask.GetMask("TKable")))
            {
                if(colsInTk.Contains(hitCol))
                    colsKeepInTk.Add(hitCol);
                else
                    continue;
                
                Rigidbody rbTk = hitCol.GetComponent<Rigidbody>();
                Vector3 vecPsyForce = flTkForce * rbTk.mass * (vecPsyHole - hitCol.transform.position).normalized;
                vecPsyForce = vecPsyForce * 7 - (rbTk.velocity.normalized * rbTk.velocity.magnitude * rbTk.mass * .77f);
                vecPsyForce -= rbTk.mass * Physics.gravity;
                rbTk.AddForce(vecPsyForce);
            }
            colsInTk.Clear();
            colsInTk = colsKeepInTk;
            return;
        }

        Ray rayPsyBeam = new Ray();
        rayPsyBeam.direction = Camera.main.transform.forward;
        rayPsyBeam.origin = Camera.main.transform.position;
        RaycastHit hitRay;
        Physics.Raycast(rayPsyBeam, out hitRay, Mathf.Infinity, ~LayerMask.GetMask("Player"));
        RaycastHit hitRayTarget;
        Physics.Raycast(rayPsyBeam, out hitRayTarget, Mathf.Infinity, ~LayerMask.GetMask("Player", "TKable"));

        Vector3 vecPsyBall = hitRay.point;
        
        Collider[] arrHitColsTemp = Physics.OverlapSphere(vecPsyBall, flPsyBallRadius, LayerMask.GetMask("TKable"));
        if(iTkDir < 0)
            foreach(var hitCol in arrHitColsTemp)
                if(!colsInTk.Contains(hitCol))
                    colsInTk.Add(hitCol);
        
        Vector3 vecAvgTkPos = Vector3.zero;
        foreach(var colInTk in colsInTk)
            vecAvgTkPos += colInTk.transform.position;
        vecAvgTkPos /= colsInTk.Count;

        if(iTkDir > 0)
        {
            foreach(var hitCol in arrHitColsTemp)
            {
                if(!colsInTk.Contains(hitCol))
                {
                    Rigidbody rbTk = hitCol.GetComponent<Rigidbody>();
                    Vector3 vecPsyForce = .5f * flTkForce * rbTk.mass * (hitCol.transform.position - rayPsyBeam.origin).normalized;
                    vecPsyForce += .5f * flTkForce * rbTk.mass * (hitCol.transform.position - vecPsyBall).normalized;
                    vecPsyForce -= rbTk.mass * Physics.gravity;
                    rbTk.AddForce(vecPsyForce);
                }
            }
        }

        foreach(var colInTk in colsInTk)
        {
            Vector3 vecPsyTarget = Vector3.zero;
            switch(iTkDir)
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
                case 1:
                    vecPsyTarget = hitRayTarget.point;
                    break;
            }

            Rigidbody rbTk = colInTk.GetComponent<Rigidbody>();
            Vector3 vecPsyForce = flTkForce * rbTk.mass * (vecPsyTarget - colInTk.transform.position).normalized;

            if (iTkDir == 0)
                vecPsyForce -= (0.027f * rbTk.velocity * rbTk.mass * (1/Time.fixedDeltaTime));
            else if((colInTk.transform.position - rayPsyBeam.origin).magnitude < (rayPsyBeam.origin - vecPsyHole).magnitude + flPsyHoleRadius)
                if (iTkDir == -1)
                    vecPsyForce = vecPsyForce * 7 - (0.027f * rbTk.velocity * rbTk.mass * (1/Time.fixedDeltaTime));
                else if (iTkDir == 1)
                    if (!Physics.Raycast(colInTk.transform.position, (vecPsyTarget - colInTk.transform.position).normalized, 1.5f * (flPsyHoleRadius - (vecPsyHole - colInTk.transform.position).magnitude), LayerMask.GetMask("TKable")))
                        vecPsyForce = ( 2*7 * vecPsyForce) - (0.14f * rbTk.velocity * rbTk.mass * (1/Time.fixedDeltaTime));
                    else if((colInTk.transform.position - vecPsyHole).magnitude < flPsyHoleRadius)
                        vecPsyForce = flTkForce * rbTk.mass * (vecPsyHole - colInTk.transform.position).normalized * 7 - (0.027f * rbTk.velocity * rbTk.mass * (1/Time.fixedDeltaTime));
            
            vecPsyForce -= rbTk.mass * Physics.gravity;
            rbTk.AddForce(vecPsyForce);
        }
    }

    void HandleAnims()
    {
        if (bTkPushButton || bTkPullButton)
        {
            anim.SetFloat("Forward", flForwardMove, 0.1f, Time.deltaTime);
            anim.SetFloat("Turn", flSideMove, 0.1f, Time.deltaTime);
        }
        else
        {
            anim.SetFloat("Turn", 0f, 0.1f, Time.deltaTime);
            if (flMoveAmount > 0)
                anim.SetFloat("Forward", Mathf.Clamp01(new Vector3(rb.velocity.x, 0f, rb.velocity.z).magnitude / flMoveSpeed), 0.01f, Time.deltaTime);
            else
                anim.SetFloat("Forward", 0f, 0.1f, Time.deltaTime);
        }
        anim.SetBool("OnGround", bOnGround);
    }

    public void ApplyDamage(int iDamage)
    {
        if(bDead)
            return;
        
        iHealth -= iDamage;
        if(iHealth <= 0)
        {
            bDead = true;
            iHealth = 0;
        }
        rb.velocity = new Vector3(rb.velocity.x * 0.5f, rb.velocity.y, rb.velocity.z * 0.5f);
        anim.SetTrigger("TakeDamage");
        uiHealthBar.value = iHealth/100f;
    }
}
