using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class PlayerController : MonoBehaviour
{

    public LayerMask collisionMask;

    private const float skinWidth = .015f;

    [SerializeField]
    private float maxSlope = 80;

    [SerializeField]
    private const float distanceBetweenRays = .25f;

    private int horzRayCount;
    private int vertRayCount;

    private float horzRaySpacing;
    private float vertRaySpacing;

    BoxCollider2D collider;
    RaycastOrigins raycastOrigins;
    public CollisionInfo collisions;

    public float facing = 1;

    private void Start()
    {
        collider = GetComponent<BoxCollider2D>();
        CalculateRaySpacing();
    }

    public void Move(Vector2 amount)
    {
        UpdateRaycastOrigins();
        collisions.Reset();
        collisions.oldVel = amount;

        if (amount.y < 0)
        {
            DescendSlope(ref amount);
        }
        if (amount.x != 0)
        {
            HorizontalCollisions(ref amount);
        }
        if (amount.y != 0)
        {
            VerticalCollisions(ref amount);
        }
        transform.Translate(amount);
    }

    private void HorizontalCollisions(ref Vector2 amount) //'ref' keyword makes a link/reference rather than copying the variable
    {
        float dirX = Mathf.Sign(amount.x);
        facing = dirX;
        gameObject.GetComponent<Player>().FlipFace(facing);
        float rayLength = Mathf.Abs(amount.x) + skinWidth;

        for (int i = 0; i < horzRayCount; i++)
        {
            Vector2 rayOrigin = (dirX == -1) ? raycastOrigins.botLeft : raycastOrigins.botRight;
            rayOrigin += Vector2.up * (horzRaySpacing * i);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * dirX, rayLength, collisionMask);

            Debug.DrawRay(rayOrigin, Vector2.right * dirX, Color.red);

            if (hit)
            {
                float angle = Vector2.Angle(hit.normal, Vector2.up);

                if (i == 0 && angle <= maxSlope)
                {
                    float distToSlope;
                    if (angle != collisions.oldSlopeAngle)
                    {
                        distToSlope = hit.distance - skinWidth;
                        amount.x -= distToSlope * dirX;
                    }
                    ClimbSlope(ref amount, angle);
                }

                if (!collisions.climbingSlope || angle > maxSlope)
                {
                    amount.x = (hit.distance - skinWidth) * dirX;
                    rayLength = hit.distance;

                    if (collisions.climbingSlope)
                    {
                        amount.y = Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(amount.x);
                    }

                    collisions.left = (dirX == -1);
                    collisions.right = (dirX == 1);
                }
            }
        }
    }

    private void VerticalCollisions(ref Vector2 amount) //'ref' keyword makes a link/reference rather than copying the variable
    {
        float dirY = Mathf.Sign(amount.y);
        float rayLength = Mathf.Abs(amount.y) + skinWidth;

        for (int i = 0; i < vertRayCount; i++)
        {
            Vector2 rayOrigin = (dirY == -1) ? raycastOrigins.botLeft : raycastOrigins.topLeft;
            rayOrigin += Vector2.right * (vertRaySpacing * i + amount.x);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * dirY, rayLength, collisionMask);

            Debug.DrawRay(rayOrigin, Vector2.up * dirY, Color.red);

            if (hit)
            {
                amount.y = (hit.distance - skinWidth) * dirY;
                rayLength = hit.distance;

                if (collisions.climbingSlope)
                {
                    amount.x = amount.y / Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Sign(amount.x);
                }

                collisions.below = (dirY == -1);
                collisions.above = (dirY == 1);
            }

            if (collisions.climbingSlope)
            {
                float dirX = Mathf.Sign(amount.x);
                rayLength = Mathf.Abs(amount.x) + skinWidth;
                Vector2 _rayOrigin = ((dirX == -1) ? raycastOrigins.botLeft : raycastOrigins.botRight) + Vector2.up * amount.y;
                RaycastHit2D _hit = Physics2D.Raycast(_rayOrigin, Vector2.right * dirX, rayLength, collisionMask);

                if (_hit)
                {
                    float angle = Vector2.Angle(_hit.normal, Vector2.up);
                    if (angle != collisions.slopeAngle)
                    {
                        amount.x = (hit.distance = skinWidth) * dirX;
                        collisions.slopeAngle = angle;
                    }
                }
            }
        }
    }

    private void ClimbSlope(ref Vector2 amount, float angle)
    {
        float dist = Mathf.Abs(amount.x);
        float velY = Mathf.Sin(angle * Mathf.Deg2Rad) * dist;
        if (amount.y > velY) //jumping
        {

        } else
        {
            amount.y = velY;
        }
        amount.x = Mathf.Cos(angle * Mathf.Deg2Rad) * dist * Mathf.Sign(amount.x);
        collisions.below = true;
        collisions.climbingSlope = true;
        collisions.slopeAngle = angle;
    }

    void DescendSlope(ref Vector2 velocity)
    {
        float dirX = Mathf.Sign(velocity.x);
        Vector2 rayOrigin = (dirX == -1) ? raycastOrigins.botRight : raycastOrigins.botLeft;
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, -Vector2.up, Mathf.Infinity, collisionMask);

        if (hit)
        {
            float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
            if (slopeAngle != 0 && slopeAngle <= maxSlope)
            {
                if (Mathf.Sign(hit.normal.x) == dirX)
                {
                    if (hit.distance - skinWidth <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x))
                    {
                        float moveDistance = Mathf.Abs(velocity.x);
                        float descendVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;
                        velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);
                        velocity.y -= descendVelocityY;

                        collisions.slopeAngle = slopeAngle;
                        collisions.descendingSlope = true;
                        collisions.below = true;
                    }
                }
            }
        }
    }

    /* private void DescendSlope(ref Vector2 amount)
    {
        RaycastHit2D maxSlopeHitLeft = Physics2D.Raycast(raycastOrigins.botLeft, Vector2.down, Mathf.Abs(amount.y) + skinWidth, collisionMask);
        RaycastHit2D maxSlopeHitRight = Physics2D.Raycast(raycastOrigins.botRight, Vector2.down, Mathf.Abs(amount.y) + skinWidth, collisionMask);
        Slide(maxSlopeHitLeft, ref amount);
        Slide(maxSlopeHitRight, ref amount);

        float dirX = Mathf.Sign(amount.x);
        Vector2 rayOrigin = (dirX == -1) ? raycastOrigins.botRight : raycastOrigins.botLeft;
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, -Vector2.up, Mathf.Infinity, collisionMask);

        if (!collisions.sliding)
        {
            if (hit)
            {
                float angle = Vector2.Angle(hit.normal, Vector2.up);
                if (angle != 0 && angle <= maxSlope)
                {
                    if (Mathf.Sign(hit.normal.x) == dirX)
                    {
                        if (hit.distance - skinWidth <= Mathf.Tan(angle * Mathf.Deg2Rad * Mathf.Abs(amount.x)))
                        {
                            float dist = Mathf.Abs(amount.x);
                            float velY = Mathf.Sin(angle * Mathf.Deg2Rad) * dist;
                            amount.x = Mathf.Cos(angle * Mathf.Deg2Rad) * dist * Mathf.Sign(amount.x);
                            amount.y -= velY;

                            collisions.slopeAngle = angle;
                            collisions.descendingSlope = true;
                            collisions.below = true;
                        }
                    }
                }
            }
        }
    } */

    void Slide(RaycastHit2D hit, ref Vector2 amount)
    {
        if (hit)
        {
            float angle = Vector2.Angle(hit.normal, Vector2.up);
            if (angle > maxSlope)
            {
                amount.x = hit.normal.x * (Mathf.Abs(amount.y) - hit.distance) / Mathf.Tan(angle * Mathf.Deg2Rad);
                collisions.slopeAngle = angle;
                collisions.sliding = true;
            }
        }
    }

    private void UpdateRaycastOrigins()
    {
        Bounds bounds = collider.bounds;
        bounds.Expand(skinWidth * -2);

        raycastOrigins.botLeft = new Vector2(bounds.min.x, bounds.min.y);
        raycastOrigins.botRight = new Vector2(bounds.max.x, bounds.min.y);
        raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
        raycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);
    }

    private void CalculateRaySpacing()
    {
        Bounds bounds = collider.bounds;
        bounds.Expand(skinWidth * -2);

        float boundsWidth = bounds.size.x;
        float boundsHeight = bounds.size.y;

        horzRayCount = Mathf.RoundToInt(boundsHeight / distanceBetweenRays);
        horzRayCount = Mathf.RoundToInt(boundsWidth / distanceBetweenRays);

        horzRayCount = Mathf.Clamp(horzRayCount, 2, int.MaxValue);
        vertRayCount = Mathf.Clamp(vertRayCount, 2, int.MaxValue);

        horzRaySpacing = bounds.size.y / (horzRayCount - 1);
        vertRaySpacing = bounds.size.x / (vertRayCount - 1);
    }

    struct RaycastOrigins
    {
        public Vector2 topLeft, topRight;
        public Vector2 botLeft, botRight;
    }

    public struct CollisionInfo
    {
        public bool above, below;
        public bool left, right;

        public bool climbingSlope;
        public bool descendingSlope;
        public bool sliding;

        public float slopeAngle, oldSlopeAngle;

        public Vector2 oldVel;

        public void Reset()
        {
            above = below = false;
            left = right = false;
            climbingSlope = false;
            descendingSlope = false;
            oldSlopeAngle = slopeAngle;
            slopeAngle = 0;
        }
    }
}

//Adapted from Sebastian Lague's tutorials: https://www.youtube.com/user/Cercopithecan