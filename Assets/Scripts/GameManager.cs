using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour 
{
    public float friction = 0.98f;
    public float wallDamping = 0.8f;

    private List<Ball> balls = new List<Ball>();
    private Ball cueBall; 
    public LineRenderer aimLine;
    private bool isAiming = false;

    public Vector2 tableMin = new Vector2(-8f, -4f);
    public Vector2 tableMax = new Vector2(8f, 4f);

    void Start() 
    {
        Ball[] foundBalls = FindObjectsByType<Ball>(FindObjectsSortMode.None);
        foreach (Ball b in foundBalls) {
            balls.Add(b);
            if (cueBall == null) cueBall = b; 
        }
    }

    void Update() 
    {
        HandleInput();
        StepPhysics(Time.deltaTime);
        SyncTransforms();
    }

    void HandleInput() 
    {
        if (cueBall == null) return;

        // Apuntar
        if (Input.GetMouseButtonDown(0)) {
            isAiming = true;
            aimLine.enabled = true;
        }

        // Actualizar línea al apuntar
        if (isAiming && Input.GetMouseButton(0)) {
            Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 dir = (mouseWorld - cueBall.Position).normalized;
            float power = (mouseWorld - cueBall.Position).magnitude;

            Vector2 endPoint = cueBall.Position + dir * power;

            aimLine.positionCount = 2;
            aimLine.SetPosition(0, new Vector3(cueBall.Position.x, cueBall.Position.y, -1f));
            aimLine.SetPosition(1, new Vector3(endPoint.x, endPoint.y, -1f));
        }

        // Disparar
        if (Input.GetMouseButtonUp(0) && isAiming) {
            Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 dir = (mouseWorld - cueBall.Position).normalized;
            float power = (mouseWorld - cueBall.Position).magnitude;

            cueBall.velocity = dir * power * 5f;
            Debug.Log($"Velocity={cueBall.velocity.magnitude}, Angle={Vector2.SignedAngle(Vector2.right, dir)}°");

            isAiming = false;
            aimLine.enabled = false;
        }
    }

    void StepPhysics(float dt) 
    {
        foreach (Ball b in balls) 
        {
            DragBall(b);
            b.Position += b.velocity * dt;
        }

        foreach (Ball b in balls) 
        {
            ResolveWallCollision(b);
        }

        for (int i = 0; i < balls.Count; i++) 
        {
            for (int j = i + 1; j < balls.Count; j++) 
            {
                ResolveBallCollision(balls[i], balls[j]);
            }
        }
    }

    void SyncTransforms() 
    {
        foreach (Ball b in balls) 
        {
            b.SyncTransform();
        }
    }

    void DragBall(Ball b) 
    {
        b.velocity *= friction;
        if (b.velocity.magnitude < 0.01f) 
        {
            b.velocity = Vector2.zero;
        }
    }

    void ResolveWallCollision(Ball b) 
    {
        Vector2 pos = b.Position;

        if (pos.x - b.radius < tableMin.x) {
            pos.x = tableMin.x + b.radius;
            b.velocity.x = -b.velocity.x * wallDamping;
        }
        if (pos.x + b.radius > tableMax.x) {
            pos.x = tableMax.x - b.radius;
            b.velocity.x = -b.velocity.x * wallDamping;
        }

        if (pos.y - b.radius < tableMin.y) {
            pos.y = tableMin.y + b.radius;
            b.velocity.y = -b.velocity.y * wallDamping;
        }
        if (pos.y + b.radius > tableMax.y) {
            pos.y = tableMax.y - b.radius;
            b.velocity.y = -b.velocity.y * wallDamping;
        }

        b.Position = pos;
    }

    void ResolveBallCollision(Ball a, Ball b) 
    {
        Vector2 delta = b.Position - a.Position;
        float dist = delta.magnitude;
        float minDist = a.radius + b.radius;

        if (dist < minDist && dist > 0f) 
        {
            Vector2 normal = delta / dist;
            float overlap = minDist - dist;
            a.Position -= normal * overlap * 0.5f;
            b.Position += normal * overlap * 0.5f;

            Vector2 relativeVelocity = a.velocity - b.velocity;
            float velAlongNormal = Vector2.Dot(relativeVelocity, normal);

            if (velAlongNormal > 0) return;

            float impulse = -velAlongNormal;
            Vector2 impulseVector = impulse * normal;

            a.velocity += -impulseVector * 0.5f;
            b.velocity += impulseVector * 0.5f;
        }
    }
    void OnDrawGizmos() 
    {
        Gizmos.color = Color.cyan;

        Vector3 bottomLeft  = new Vector3(tableMin.x, tableMin.y, 0);
        Vector3 bottomRight = new Vector3(tableMax.x, tableMin.y, 0);
        Vector3 topRight    = new Vector3(tableMax.x, tableMax.y, 0);
        Vector3 topLeft     = new Vector3(tableMin.x, tableMax.y, 0);

        Gizmos.DrawLine(bottomLeft, bottomRight);
        Gizmos.DrawLine(bottomRight, topRight);
        Gizmos.DrawLine(topRight, topLeft);
        Gizmos.DrawLine(topLeft, bottomLeft);
    }
}
