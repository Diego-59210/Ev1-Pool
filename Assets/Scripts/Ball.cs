using UnityEngine;

public class Ball : MonoBehaviour 
{
    [HideInInspector] public Vector2 position;
    [HideInInspector] public Vector2 prevPosition;

    public Vector2 velocity;

    [HideInInspector] public float radius { get; private set; }

    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        position = transform.position;
        prevPosition = position;

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            radius = spriteRenderer.bounds.extents.x;
    }

    public Vector2 Velocity
    {
        get => velocity;
        set
        {
            velocity = value;
            if (velocity.sqrMagnitude < 0.0001f)
                velocity = Vector2.zero;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}