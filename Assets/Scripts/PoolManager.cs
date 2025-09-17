using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PoolManager : MonoBehaviour
{
    [Header("Balls")]
    public Ball cueBall;
    public List<Ball> coloredBalls;

    [Header("Shot Settings")]
    public float shotPower = 1.0f;
    public float maxShotSpeed = 10.0f;

    [Header("Table")]
    public SpriteRenderer spriteRenderer;

    [Header("Aiming & UI")]
    public LineRenderer aimLine;
    public Image powerBar;
    public float maxChargeTime = 2f;

    [Header("Physics Settings")]
    [Range(0f, 1f)] public float ballFriction = 0.95f;
    [Range(0f, 1f)] public float wallDamping = 0.6f;
    [Range(0f, 1f)] public float ballDamping = 0.75f;
    public float stopThreshold = 0.01f;

    private List<Ball> balls = new();
    private Vector2 tableMin;
    private Vector2 tableMax;

    private bool aiming;
    private bool isCharging;
    private float chargePower;
    private Vector2 aimCurrentWorld;

    void Start()
    {
        balls.Add(cueBall);
        coloredBalls.ForEach(c => balls.Add(c));

        tableMin = spriteRenderer.bounds.min;
        tableMax = spriteRenderer.bounds.max;

        foreach (var b in balls)
        {
            b.position = b.prevPosition = b.transform.position;
            b.velocity = Vector2.zero;
        }

        if (aimLine != null) aimLine.enabled = true;
        if (powerBar != null) powerBar.fillAmount = 0f;
    }

    void Update()
    {
        HandleInput();
        StepPhysics(Time.deltaTime);
        SyncTransforms(Time.deltaTime);
        UpdateAimLine();
    }

    void HandleInput()
    {
        float velocityThreshold = 0.01f;
        if (cueBall.velocity.sqrMagnitude > velocityThreshold) return;

        Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // Empezar a cargar el tiro.
        if (Input.GetMouseButtonDown(0))
        {
            aiming = true;
            isCharging = true;
            chargePower = 0f;

            aimCurrentWorld = mouseWorld;
        }

        if (aiming && isCharging)
        {
            
            chargePower += Time.deltaTime / maxChargeTime;
            chargePower = Mathf.Clamp01(chargePower);

            if (powerBar != null) powerBar.fillAmount = chargePower;

            // Tirar la bola.
            if (Input.GetMouseButtonUp(0))
            {
                Vector2 dir = aimCurrentWorld - cueBall.position;
                Vector2 v = dir * Mathf.Lerp(shotPower, maxShotSpeed, chargePower);
                if (v.magnitude > maxShotSpeed) v = v.normalized * maxShotSpeed;

                cueBall.velocity = v;

                aiming = false;
                isCharging = false;
                chargePower = 0f;
                if (powerBar != null) powerBar.fillAmount = 0f;
            }
        }
    }

    void StepPhysics(float dt)
    {
        foreach (var b in balls) b.prevPosition = b.position;

        foreach (var b in balls)
        {
            // Aplicar fricción
            if (b.velocity.sqrMagnitude > stopThreshold * stopThreshold)
                b.velocity *= Mathf.Pow(ballFriction, dt);

            // Mover la bola
            b.position += b.velocity * dt;

            ResolveWallCollision(b);

            // Detener velocidades minimas.
            if (b.velocity.magnitude < stopThreshold)
                b.velocity = Vector2.zero;
        }

        // Colisión entre bolas.
        for (int i = 0; i < balls.Count; i++)
            for (int j = i + 1; j < balls.Count; j++)
                ResolveBallCollision(balls[i], balls[j]);

        // Detener pequeños movimientos después de la colisión.
        foreach (var b in balls)
            if (b.velocity.magnitude < stopThreshold)
                b.velocity = Vector2.zero;
    }

    void ResolveWallCollision(Ball b)
    {
        Vector2 pos = b.position;
        Vector2 vel = b.velocity;

        if (pos.x - b.radius < tableMin.x || pos.x + b.radius > tableMax.x)
        {
            vel.x *= -wallDamping;
            pos.x = Mathf.Clamp(pos.x, tableMin.x + b.radius, tableMax.x - b.radius);
        }
        if (pos.y - b.radius < tableMin.y || pos.y + b.radius > tableMax.y)
        {
            vel.y *= -wallDamping;
            pos.y = Mathf.Clamp(pos.y, tableMin.y + b.radius, tableMax.y - b.radius);
        }

        b.position = pos;
        b.velocity = vel;
    }

    void ResolveBallCollision(Ball a, Ball b)
    {
        Vector2 diff = b.position - a.position;
        float dist = diff.magnitude;
        float minDist = a.radius + b.radius;

        if (dist < minDist && dist > 0f)
        {
            Vector2 normal = diff.normalized;
            float overlap = minDist - dist;

            // Empujar bolas.
            a.position -= normal * overlap * 0.5f;
            b.position += normal * overlap * 0.5f;

            // Intercambio de velocidades.
            float aProj = Vector2.Dot(a.velocity, normal);
            float bProj = Vector2.Dot(b.velocity, normal);
            float exchange = bProj - aProj;
            a.velocity += normal * exchange;
            b.velocity -= normal * exchange;

            // Aplicar damping.
            a.velocity *= ballDamping;
            b.velocity *= ballDamping;

            // Detener movimientos pequeños.
            if (a.velocity.magnitude < stopThreshold) a.velocity = Vector2.zero;
            if (b.velocity.magnitude < stopThreshold) b.velocity = Vector2.zero;
        }
    }


    void SyncTransforms(float alpha)
    {
        alpha = Mathf.Clamp01(alpha / Time.deltaTime);
        foreach (var b in balls)
        {
            Vector2 interp = Vector2.Lerp(b.prevPosition, b.position, alpha);
            b.transform.position = interp;
        }
    }

    void UpdateAimLine()
    {
        float velocityThreshold = 0.01f;

        // Ocultar línea si la bola blanca se está moviendo.
        if (cueBall.velocity.sqrMagnitude > velocityThreshold)
        {
            if (aimLine != null) aimLine.enabled = false;
            return;
        }

        if (aimLine != null)
        {
            aimLine.enabled = true;
            aimLine.positionCount = 2;
            aimLine.SetPosition(0, cueBall.position);

            Vector2 endPoint = isCharging ? aimCurrentWorld : 
                                            (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);
            aimLine.SetPosition(1, endPoint);
        }
    }

    void OnDrawGizmos()
    {
        if (spriteRenderer == null) return;

        tableMin = spriteRenderer.bounds.min;
        tableMax = spriteRenderer.bounds.max;

        Gizmos.color = Color.green;
        Vector3 size = new Vector3(tableMax.x - tableMin.x, tableMax.y - tableMin.y, 0);
        Vector3 center = new Vector3((tableMax.x + tableMin.x) * 0.5f,
                                     (tableMax.y + tableMin.y) * 0.5f, 0);
        Gizmos.DrawWireCube(center, size);

        if (balls != null)
        {
            Gizmos.color = Color.yellow;
            foreach (var b in balls)
                if (b != null)
                    Gizmos.DrawWireSphere(b.transform.position, b.radius);
        }

        if (aiming)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(cueBall.position, aimCurrentWorld);
        }
    }
}