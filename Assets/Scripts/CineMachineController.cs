using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using System;

public class CineMachineController : MonoBehaviour, IEventListener<LevelEvent>
{
    public float HighScopeSize;
    public float MediumScopeSize;
    public float FlightModeSize;
    public float SmoothTimeForOrthographicChanges;
    public float SmoothTimeForHorizontalChanges;
    public float CameraOffsetTarget;

    protected CinemachineVirtualCamera _cinemachineVM;
    protected CinemachineComponentBase _cinemachineCB;
    protected CinemachineFramingTransposer _cinemachineFT;
    protected CinemachineCameraOffset _cinemachineOffset;
    protected float _cameraDefaultSize;
    protected float _cameraCurrentSize;
    protected float _screenXDefault;
    protected float _screenXCurrent;
    protected float _xDampingDefault;
    protected float _yDampingDefault;
    protected float _lookAheadTimeDefault;
    protected float _deadZoneHeightDefault;
    protected bool _isAscending = false;
    protected bool _isDescending = false;
    protected bool _flightModeActive = false;
    protected bool _isworking = false;

    protected virtual void Awake()
    {
        _cinemachineVM = GetComponent<CinemachineVirtualCamera>();
        _cinemachineCB = _cinemachineVM.GetCinemachineComponent(CinemachineCore.Stage.Body);
        _cinemachineFT = _cinemachineVM.GetCinemachineComponent<CinemachineFramingTransposer>();
        _cinemachineOffset = GetComponent<CinemachineCameraOffset>();

        _cameraDefaultSize = _cinemachineVM.m_Lens.OrthographicSize;
        _screenXDefault = _cinemachineFT.m_ScreenX;
        _xDampingDefault = _cinemachineFT.m_XDamping;
        _yDampingDefault = _cinemachineFT.m_YDamping;
        _lookAheadTimeDefault = _cinemachineFT.m_LookaheadTime;
        _deadZoneHeightDefault = _cinemachineFT.m_DeadZoneHeight;
    }

    protected virtual void Update()
    {
        _cameraCurrentSize = _cinemachineVM.m_Lens.OrthographicSize;
        _screenXCurrent = _cinemachineFT.m_ScreenX;
    }

    protected virtual void LateUpdate()
    {
        if (!_flightModeActive && _isAscending && !_isDescending)
        {
            _cinemachineVM.m_Lens.OrthographicSize = Mathf.Lerp(_cameraCurrentSize, FlightModeSize, SmoothTimeForOrthographicChanges * Time.deltaTime);
            _cinemachineFT.m_ScreenX = Mathf.Lerp(_screenXCurrent, _screenXDefault, SmoothTimeForHorizontalChanges * Time.deltaTime);
            _cinemachineFT.m_LookaheadTime = 0f;
            _cinemachineFT.m_DeadZoneHeight = 0f;
            _cinemachineFT.m_YDamping = 0f;
        }
        else if (_flightModeActive && !_isAscending && !_isDescending)
        {
            _cinemachineOffset.m_Offset.y = CameraOffsetTarget - _cinemachineVM.transform.position.y;
            _cinemachineFT.m_LookaheadTime = _lookAheadTimeDefault;
            _cinemachineFT.m_DeadZoneHeight = 1f;
            _cinemachineFT.m_XDamping = 0f;
            _cinemachineFT.m_YDamping = _yDampingDefault;

        }
        else if (!_flightModeActive && !_isAscending && _isDescending)
        {
            _cinemachineFT.m_DeadZoneHeight = _deadZoneHeightDefault;
            _cinemachineFT.m_XDamping = _xDampingDefault;
        }
        else if (!_flightModeActive && !_isAscending && !_isDescending)
        {
            _cinemachineOffset.m_Offset.y = 0f;
            if (!Player.PlayerIns.IsGrounded)
            {
                return;
            }

            if (Player.PlayerIns.MovingForward)
            {
                _cinemachineVM.m_Lens.OrthographicSize = Mathf.Lerp(_cameraCurrentSize, HighScopeSize, SmoothTimeForOrthographicChanges * Time.deltaTime);
                _cinemachineFT.m_ScreenX = Mathf.Lerp(_screenXCurrent, _screenXDefault, SmoothTimeForHorizontalChanges * Time.deltaTime);
            }
            else
            {
                if (!Player.PlayerIns.MovingBackward)
                {
                    _cinemachineVM.m_Lens.OrthographicSize = Mathf.Lerp(_cameraCurrentSize, MediumScopeSize, SmoothTimeForHorizontalChanges
             * Time.deltaTime);
                    _cinemachineFT.m_ScreenX = Mathf.Lerp(_screenXCurrent, _screenXDefault, SmoothTimeForHorizontalChanges
             * Time.deltaTime);
                }

                if (Player.PlayerIns.MovingBackward)
                {
                    _cinemachineVM.m_Lens.OrthographicSize = Mathf.Lerp(_cameraCurrentSize, MediumScopeSize, SmoothTimeForOrthographicChanges * Time.deltaTime);
                    _cinemachineFT.m_ScreenX = Mathf.Lerp(_screenXCurrent, 0.25f, SmoothTimeForHorizontalChanges
             * Time.deltaTime);
                }
            }
        }
    }

    public virtual void OnEvent(LevelEvent levelEvent)
    {
        switch (levelEvent.EventType)
        {
            case LevelEventType.Ascending:
                _isAscending = true;
                break;
            case LevelEventType.FlightOn:
                _isAscending = false;
                _flightModeActive = true;
                break;
            case LevelEventType.Descending:
                _isDescending = true;
                break;
            case LevelEventType.FlightOff:
                _isDescending = false;
                _flightModeActive = false;
                break;
        }
    }

    public virtual void OnEnable()
    {
        this.StartListeningEvent<LevelEvent>();
    }

    public virtual void OnDisable()
    {
        this.StopListeningEvent<LevelEvent>();
    }
}
