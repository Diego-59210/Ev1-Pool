using UnityEngine;

public class Ball : MonoBehaviour 
{
    public Vector2 velocity;
    public float radius = 0.5f;

    public Vector2 Position 
    {
        get { return transform.position; }
        set { transform.position = value; }
    }

    public void SyncTransform() 
    {
        transform.position = Position;
    }
    void OnDrawGizmos() 
    {
    Gizmos.color = Color.blue;

    Gizmos.DrawWireSphere(Position, radius);
    }
}
