using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingObject : MonoBehaviour
{
    public float Speed = 0;
    public Vector3 Direction = Vector3.left;

    public bool DirectionCanChange = true;
    public Space MovementSpace = Space.World;

    public Vector3 Movement {get {return _movement;}}

    protected Vector3 _movement;
    protected float _initalSpeed;

    protected virtual void Awake ()
    {
        _initalSpeed = Speed;
    }

    protected virtual void OnEnable()
    {
        Speed = _initalSpeed;
    }

    protected virtual void FixedUpdate ()
    {
        Direction = -LevelManager.Instance.Player.CurrentDirection;

        if (!LevelManager.Instance)
        {
            _movement = Vector3.zero;
        }
        else
        {
            _movement = Direction * LevelManager.Instance.LevelSpeed * 2f * Time.deltaTime;
        }

        transform.Translate(_movement, MovementSpace);
    }
}
