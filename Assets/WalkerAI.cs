using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class WalkerAI : MonoBehaviour
{
   [Header("Movement")]
    public float moveSpeed = 1.2f;
    public float turnSpeed = 160f;

    [Header("Grounding")]
    public float rayDistance = 2.5f;
    public float stickOffset = 0.3f;
    public float maxSlopeAngle = 50f;
    public LayerMask groundMask;

    [Header("Wall Detection")]
    public float wallCheckDistance = 0.7f;  // distance to detect wall in front
    public LayerMask wallMask;

    Rigidbody rb;

    // Turning variables
    private bool isTurning = false;
    private Quaternion targetRotation;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (!Grounded(out RaycastHit groundHit))
            return;

        AlignToSurface(groundHit);
        StickToGround(groundHit);

        if (isTurning)
        {
            TurnTowardsTarget();
        }
        else
        {
            if (WallAhead())
            {
                StartTurn();
            }
            else
            {
                MoveOnSurface(groundHit);
            }
        }
    }

    // ---------------- WALL DETECTION ----------------
    bool WallAhead()
    {
        Vector3 origin = transform.position + Vector3.up * 0.5f; // slightly above ground
        return Physics.Raycast(origin, transform.forward, wallCheckDistance, wallMask);
    }

    // ---------------- MOVEMENT ----------------
    void MoveOnSurface(RaycastHit hit)
    {
        Vector3 moveDir = Vector3.ProjectOnPlane(transform.forward, hit.normal).normalized;
        rb.MovePosition(rb.position + moveDir * moveSpeed * Time.fixedDeltaTime);
    }

    void AlignToSurface(RaycastHit hit)
    {
        Quaternion targetRot = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 10f * Time.fixedDeltaTime);
    }

    void StickToGround(RaycastHit hit)
    {
        Vector3 targetPos = hit.point + hit.normal * stickOffset;
        rb.MovePosition(Vector3.Lerp(rb.position, targetPos, 12f * Time.fixedDeltaTime));
    }

    bool Grounded(out RaycastHit hit)
    {
        return Physics.Raycast(transform.position + Vector3.up, Vector3.down, out hit, rayDistance, groundMask);
    }

    // ---------------- TURNING ----------------
    void StartTurn()
    {
        // Pick a random angle to turn (90 to 180 degrees)
        float turnAngle = Random.Range(90f, 180f);
        targetRotation = Quaternion.Euler(0, transform.eulerAngles.y + turnAngle, 0);
        isTurning = true;
    }

    void TurnTowardsTarget()
    {
        // Rotate towards target rotation smoothly
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            turnSpeed * Time.fixedDeltaTime
        );

        // Check if rotation is complete
        if (Quaternion.Angle(transform.rotation, targetRotation) < 0.5f)
        {
            isTurning = false;
        }
    }

    // Optional: visualize the wall ray
    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        Gizmos.color = Color.yellow;
        Vector3 origin = transform.position + Vector3.up * 0.5f;
        Gizmos.DrawLine(origin, origin + transform.forward * wallCheckDistance);
    }
}
