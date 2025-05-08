using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothCamera : MonoBehaviour
{
    protected Vector3 _offset = new Vector3(0f, 0f, -10f);
    protected Vector3 _xValOffset = new Vector3(1f, 0f, 0f);
    protected float _smoothTime = 3f;
    protected Vector3 _velocity = Vector3.zero;
    protected Vector3 _targetPos;
    public GameObject _target;

    protected virtual void FixedUpdate ()
    {
        if (Player.PlayerIns.MovingForward)
        {
            _targetPos = _target.transform.position + _xValOffset + _offset;
        } 
        else
        {
            if (Player.PlayerIns.MovingBackward)
            {
                _targetPos = _target.transform.position - _xValOffset + _offset;
            }
            else
            {
                _targetPos = _target.transform.position + _offset;
            }
        }
        
        Vector3 smoothCamPos = Vector3.Lerp(transform.position, _targetPos, _smoothTime * Time.fixedDeltaTime);

        transform.position = smoothCamPos;
    }
}
