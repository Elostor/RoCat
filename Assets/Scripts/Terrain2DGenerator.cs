using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Terrain2DGenerator : MonoBehaviour, IEventListener<LevelEvent>
{
    [Header("Endless Settings")]
    public Transform groundContainer;
    public Terrain2D startGround;
    public Transform follow;
    protected Terrain2D _leftGround;
    protected Terrain2D _middleGround;
    protected Terrain2D _rightGround;
    protected bool _flightModeActive = false;
    [Header("Parallax Scrolling")]
    public bool parallaxScrolling;
    public float xSpeed = 1f;
    public float ySpeed = 1f;
    public float yOffset;

    public enum Mode { Sky, Ground };
    public Mode mode;

    protected virtual void Start()
    {
        GetParent();
        if (CheckForErrors())
        {
            this.enabled = false;
            return;
        }
        if (!startGround) _middleGround = GetRandomGround();
        else _middleGround = startGround;
        _middleGround.transform.parent = transform;
        _middleGround.cacheTransform.position = transform.position;
        _leftGround = GetRandomGround();
        _leftGround.transform.position = new Vector2(_middleGround.cacheTransform.position.x - _leftGround.width, transform.position.y);
        _rightGround = GetRandomGround();
        _rightGround.transform.position = new Vector2(_middleGround.cacheTransform.position.x + _middleGround.width, transform.position.y);
        if (mode == Mode.Sky)
        {
            _middleGround.enabled = false;
            _leftGround.enabled = false;
            _rightGround.enabled = false;
        }
        DisableAll();
    }

    protected bool CheckForErrors()
    {
        bool b = false;
        if (groundContainer.childCount < 3)
        {
            Debug.LogError(this + " Not enough terrains in container to generate endless terrain.");
            b = true;
        }
        return b;
    }

    protected Terrain2D GetRandomGround()
    {
        Terrain2D r = groundContainer.GetChild(Random.Range(0, groundContainer.childCount)).GetComponent<Terrain2D>(); ;
        r.transform.parent = transform;
        r.gameObject.SetActive(true);
        return r;
    }

    protected virtual void GetParent()
    {
        Terrain2D[] t = groundContainer.GetComponentsInChildren<Terrain2D>();
        for (int i = 0; i < t.Length; i++)
        {
            t[i].parent = t[i].transform.parent;
        }
    }

    protected virtual void DisableAll()
    {
        Terrain2D[] t = groundContainer.GetComponentsInChildren<Terrain2D>();
        for (int i = 0; i < t.Length; i++)
        {
            t[i].gameObject.SetActive(false);
        }
    }

    protected virtual void Update()
    {
        if (mode == Mode.Sky)
        {
            if (_flightModeActive)
            {
                if (follow.position.x >= _rightGround.cacheTransform.position.x)
                {
                    // Left ground is no longer needed, it is will be replaced by the middle ground.
                    // Change back to the original parent. (GroundContainer)

                    /*for (int i = 0; i < _leftGround.transform.childCount; i++)
                    {
                        _leftGround.transform.GetChild(i).GetComponent<ScoreItem>().ResetItem();
                    }*/
                    _leftGround.transform.BroadcastMessage("ResetItem");
                    _leftGround.cacheTransform.parent = _leftGround.parent;
                    // Disable it so it doesn't show up if the player moves left.
                    _leftGround.gameObject.SetActive(false);
                    // Rearrange terrains to the proper variables
                    _leftGround = _middleGround;
                    _middleGround = _rightGround;
                    // Get a random ground from the GroundContainer	to place at the right side.	
                    _rightGround = GetRandomGround();
                    // Positions the new right terrain piece next to the middle terrain.
                    _rightGround.transform.position = new Vector2(_middleGround.cacheTransform.position.x + _middleGround.width, transform.position.y);
                }
                else if (follow.position.x <= _middleGround.cacheTransform.position.x)
                {
                    // Pretty much the same as above.
                    _rightGround.cacheTransform.parent = _rightGround.parent;
                    _rightGround.gameObject.SetActive(false);
                    _rightGround = _middleGround;
                    _middleGround = _leftGround;
                    _leftGround = GetRandomGround();
                    _leftGround.transform.position = new Vector2(_middleGround.cacheTransform.position.x - _middleGround.width, transform.position.y);
                }
                if (!parallaxScrolling) return;
                transform.position = new Vector2(follow.transform.position.x * xSpeed, follow.transform.position.y * ySpeed - yOffset);
            }
            else
            {
                return;
            }
        }
        if (mode == Mode.Ground)
        {
            if (follow.position.x >= _rightGround.cacheTransform.position.x)
            {
                // Left ground is no longer needed, it is will be replaced by the middle ground.
                // Change back to the original parent. (GroundContainer)
                _leftGround.cacheTransform.parent = _leftGround.parent;
                // Disable it so it doesn't show up if the player moves left.
                _leftGround.gameObject.SetActive(false);
                // Rearrange terrains to the proper variables
                _leftGround = _middleGround;
                _middleGround = _rightGround;
                // Get a random ground from the GroundContainer	to place at the right side.	
                _rightGround = GetRandomGround();
                // Positions the new right terrain piece next to the middle terrain.
                _rightGround.transform.position = new Vector2(_middleGround.cacheTransform.position.x + _middleGround.width, transform.position.y);
            }
            else if (follow.position.x <= _middleGround.cacheTransform.position.x)
            {
                // Pretty much the same as above.
                _rightGround.cacheTransform.parent = _rightGround.parent;
                _rightGround.gameObject.SetActive(false);
                _rightGround = _middleGround;
                _middleGround = _leftGround;
                _leftGround = GetRandomGround();
                _leftGround.transform.position = new Vector2(_middleGround.cacheTransform.position.x - _middleGround.width, transform.position.y);
            }
            if (!parallaxScrolling) return;
            transform.position = new Vector2(follow.transform.position.x * xSpeed, follow.transform.position.y * ySpeed - yOffset);
        }
		else
		{
			Debug.Log("Generator mode isn't specified.");
			return;
		}
    }

    public virtual void OnEvent(LevelEvent levelEvent)
    {
        switch (levelEvent.EventType)
        {
            case LevelEventType.Ascending:
                _middleGround.enabled = true;
                _leftGround.enabled = true;
                _rightGround.enabled = true;
                break;
            case LevelEventType.FlightOn:
                _flightModeActive = true;
                break;
            case LevelEventType.Descending:
                _flightModeActive = false;
                break;
            case LevelEventType.FlightOff:
                _leftGround.enabled = false;
                _middleGround.enabled = false;
                _rightGround.enabled = false;
                break;
        }
    }

    protected virtual void OnEnable()
    {
        this.StartListeningEvent<LevelEvent>();
    }

    protected virtual void OnDisable()
    {
        this.StopListeningEvent<LevelEvent>();
    }
}
