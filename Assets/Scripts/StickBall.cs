using UnityEngine;

public class StickBall : MonoBehaviour
{
    public Transform ball;        
    public Transform cue;          
    public Transform movablePart;   

    public float angleOffset = 0f;
    public float stillThreshold = 0.001f;
    public float pullBackSpeed = 2f; 
    public float maxPullBack = -1f;  

    Vector3 lastPos;
    Vector3 movableStartLocalPos; 
    bool isPulling = false;

    void Start()
    {
        lastPos = ball.position;
        if (movablePart != null)
            movableStartLocalPos = movablePart.localPosition;
    }

    void Update()
    {
        if (ball == null || cue == null || movablePart == null) return;

        bool isStationary = (ball.position - lastPos).sqrMagnitude < stillThreshold * stillThreshold;
        lastPos = ball.position;

        cue.gameObject.SetActive(isStationary);

        if (!isStationary)
        {
            movablePart.localPosition = movableStartLocalPos;
            return;
        }

        if (Input.GetMouseButton(0))
        {
            isPulling = true;

            float newX = Mathf.Max(movablePart.localPosition.x - pullBackSpeed * Time.deltaTime, maxPullBack);
            movablePart.localPosition = new Vector3(newX, movablePart.localPosition.y, movablePart.localPosition.z);
        }
        else
        {
            isPulling = false;

            movablePart.localPosition = Vector3.Lerp(movablePart.localPosition, movableStartLocalPos, Time.deltaTime * 5f);

            Vector3 m = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 dir = (m - ball.position);
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + angleOffset;
            cue.rotation = Quaternion.Euler(0, 0, angle);
        }
    }
}