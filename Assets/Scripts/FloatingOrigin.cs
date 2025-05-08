// Based on the Unity Wiki FloatingOrigin script by Peter Stirling
// URL: http://wiki.unity3d.com/index.php/Floating_Origin
// Optimized by https://gist.github.com/brihernandez

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;

public class FloatingOrigin : MonoBehaviour, IEventListener<LevelEvent>
{
    [Tooltip("Point of reference from which to check the distance to origin.")]
    public Transform ReferenceObject = null;

    [Tooltip("Distance from the origin the reference object must be in order to trigger an origin shift.")]
    public float Threshold = 3000f;

    [Header("Options")]
    [Tooltip("When true, origin shifts are considered only from the horizontal distance to orign.")]
    public bool Use2DDistance = true;

    [Tooltip("Should ParticleSystems be moved with an origin shift.")]
    public bool UpdateParticles = true;

    [Tooltip("Should TrailRenderers be moved with an origin shift.")]
    public bool UpdateTrailRenderers = true;

    [Tooltip("Should LineRenderers be moved with an origin shift.")]
    public bool UpdateLineRenderers = true;

    [Tooltip("Should Cinemachine Cameras be moved with an origin shift.")]
    public bool UpdateCinemachine = true;

    [Header("Ignored/Children Objects")]
    public GameObject[] IgnoredObjects;
    public GameObject[] ChildObjects;

    private CinemachineVirtualCameraBase _cinemachineVC;
    private ParticleSystem.Particle[] parts = null;
    private List<GameObject> _list = new List<GameObject>();
    private bool _isActive = true;

    protected virtual void Start()
    {
        foreach (GameObject g in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            if (!IgnoredObjects.Contains(g))
            {
                _list.Add(g);
            }
        }

        foreach (GameObject g in ChildObjects)
        {
            _list.Add(g);
        }
    }

    protected virtual void FixedUpdate()
    {
        if (ReferenceObject == null)
            return;

        Vector3 referencePosition = ReferenceObject.position;

        if (Use2DDistance)
            referencePosition.y = 0f;

        if (_isActive)
        {
            if (referencePosition.magnitude > Threshold)
            {
                MoveRootTransforms(referencePosition);

                if (UpdateParticles)
                    MoveParticles(referencePosition);

                if (UpdateTrailRenderers)
                    MoveTrailRenderers(referencePosition);

                if (UpdateLineRenderers)
                    MoveLineRenderers(referencePosition);

                if (UpdateCinemachine)
                    MoveCinemachine(referencePosition);
            }
        }
    }

    protected virtual void MoveRootTransforms(Vector3 offset)
    {
        foreach (GameObject g in _list)
            g.transform.position -= offset;
    }

    protected virtual void MoveTrailRenderers(Vector3 offset)
    {
        var trails = FindObjectsOfType<TrailRenderer>() as TrailRenderer[];
        foreach (var trail in trails)
        {
            Vector3[] positions = new Vector3[trail.positionCount];

            int positionCount = trail.GetPositions(positions);
            for (int i = 0; i < positionCount; ++i)
                positions[i] -= offset;

            trail.SetPositions(positions);
        }
    }

    protected virtual void MoveLineRenderers(Vector3 offset)
    {
        var lines = FindObjectsOfType<LineRenderer>() as LineRenderer[];
        foreach (var line in lines)
        {
            Vector3[] positions = new Vector3[line.positionCount];

            int positionCount = line.GetPositions(positions);
            for (int i = 0; i < positionCount; ++i)
                positions[i] -= offset;

            line.SetPositions(positions);
        }
    }

    protected virtual void MoveParticles(Vector3 offset)
    {
        var particles = FindObjectsOfType<ParticleSystem>() as ParticleSystem[];
        foreach (ParticleSystem system in particles)
        {
            if (system.main.simulationSpace != ParticleSystemSimulationSpace.World)
                continue;

            int particlesNeeded = system.main.maxParticles;

            if (particlesNeeded <= 0)
                continue;

            // ensure a sufficiently large array in which to store the particles
            if (parts == null || parts.Length < particlesNeeded)
            {
                parts = new ParticleSystem.Particle[particlesNeeded];
            }

            // now get the particles
            int num = system.GetParticles(parts);

            for (int i = 0; i < num; i++)
            {
                parts[i].position -= offset;
            }

            system.SetParticles(parts, num);
        }
    }

    protected virtual void MoveCinemachine(Vector3 offset)
    {
        _cinemachineVC = CinemachineCore.Instance.GetVirtualCamera(0);

        _cinemachineVC.OnTargetObjectWarped(_cinemachineVC.Follow, -offset);
    }

    public virtual void OnEvent(LevelEvent levelEvent)
    {
        switch (levelEvent.EventType)
        {
            case LevelEventType.Ascending:
                _isActive = false;
                break;
            case LevelEventType.FlightOn:
                _isActive = true;
                break;
        }
    }
}
