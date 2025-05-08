using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BezierFollower : MonoBehaviour
{
    [Range(0.01f, 5f)]
    public float val1 = 0.05f;
    [Range(0.05f, 5f)]
    public float val2 = 0.25f;
    public GameObject RefPoint;
    
    [SerializeField]
    private Transform[] controlPoints;
    private Vector2 gizmosPosition;
    protected Vector3 _lastDotPoint0;
    protected Vector3 _lastDotPoint1;
    protected Vector3 _lastDotPoint2; 
    protected Vector3 _lastDotPoint3;

    protected virtual void Awake ()
    {
        _lastDotPoint0 = controlPoints[0].transform.position;
        _lastDotPoint1 = controlPoints[1].transform.position;
        _lastDotPoint2 = controlPoints[2].transform.position;
        _lastDotPoint3 = controlPoints[3].transform.position;
    }

    protected virtual void LateUpdate ()
    {
        //controlPoints[0].transform.position = new Vector3(Player.PlayerIns.transform.position.x, Player.PlayerIns.transform.position.y, 0);
        //controlPoints[1].transform.position = new Vector3(Player.PlayerIns.transform.position.x + 1.243f, Player.PlayerIns.transform.position.y + 0.982f, 0);
        //controlPoints[2].transform.position = new Vector3(Player.PlayerIns.transform.position.x + 0.74f, Player.PlayerIns.transform.position.y + 3.5f, 0);
        controlPoints[3].transform.position = new Vector3(RefPoint.transform.position.x, RefPoint.transform.position.y, 0);
    }
    private void OnDrawGizmos()
    {
        for (float t = 0; t <= 1; t += val1)
        {
            gizmosPosition = Mathf.Pow(1 - t, 3) * controlPoints[0].position + 3 * Mathf.Pow(1 - t, 2) * t * controlPoints[1].position + 3 * (1 - t) * Mathf.Pow(t, 2) * controlPoints[2].position + Mathf.Pow(t, 3) * controlPoints[3].position;
            Gizmos.DrawSphere(gizmosPosition, val2);
        }

        Gizmos.DrawLine(new Vector2(controlPoints[0].position.x, controlPoints[0].position.y), new Vector2(controlPoints[1].position.x, controlPoints[1].position.y));
        Gizmos.DrawLine(new Vector2(controlPoints[2].position.x, controlPoints[2].position.y), new Vector2(controlPoints[3].position.x, controlPoints[3].position.y));
    }
}
