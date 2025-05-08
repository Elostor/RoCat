using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum LevelEventType 
{
    Ascending, 
    FlightOn,
    Descending,
    FlightOff, 
    OnGround,
    InAir, 
    TokenPicked, 
    CatnipPicked, 
    FuelPicked,
    PlayerFlipped,
    PlayerReverseFlipped,
    PlayerDeadge
}

/// A type of event which inform other managers, of occurances between game-start and game-end.
public struct LevelEvent
{
    public LevelEventType EventType;

    // Initializing an instance.
    public LevelEvent(LevelEventType occuranceType)
    {
        EventType = occuranceType;
    }

    static LevelEvent e;
    public static void TriggerEvent(LevelEventType occuranceType)
    {
        e.EventType = occuranceType;
        EventManager.TriggerEvent(e);
        Debug.Log("A LevelEvent was triggered");
    }
}
