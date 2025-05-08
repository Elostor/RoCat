#if EVENTROUTER_THROWEXCEPTIONS
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// A general event struct.
public struct GameEvent
{
    public string EventName;
    public GameEvent (string newEvent)
    {
        EventName = newEvent;
    }
    static GameEvent e;
    public static void Trigger (string newEvent)
    {
        e.EventName = newEvent;
        EventManager.TriggerEvent(e);
    }
}

/// A class for event management. Can be used for holding events, then sending those to other classes when required.
[ExecuteAlways]
public static class EventManager
{
    private static Dictionary<Type, List<ICoreEventListener>> _subsList;

    static EventManager ()
    {
        _subsList = new Dictionary<Type, List<ICoreEventListener>>();
    }

    /// Adds a new subscriber to a specific event.
    public static void AddListener<Event> (IEventListener<Event> listener) where Event : struct
    {
        Type eventType = typeof (Event);

        if (!_subsList.ContainsKey(eventType))
            _subsList[eventType] = new List<ICoreEventListener>();
        
        if (!SubExists(eventType, listener))
            _subsList[eventType].Add(listener);
    }

    /// Removes a subscriber from a specific event.
    public static void RemoveListener<Event> (IEventListener<Event> listener) where Event : struct
    {
        Type eventType = typeof (Event);

        if (!_subsList.ContainsKey(eventType))
        {
            #if EVENTROUTER_THROWEXCEPTIONS
                throw new ArguementException(string.Format("<color=blue>Unknown event type \"{1}\", Listener \"{0}\" removed.</color>", listener, eventType.ToString()));
            #else
                return;
            #endif        
        }

        List<ICoreEventListener> subsList = _subsList[eventType];
        #if EVENTROUTER_THROWEXCEPTIONS
            bool listenerPresent = false;
        #endif
    
        for (int i = 0; i<subsList.Count; i++)
        {
           if ( subsList[i] == listener)
           {
               subsList.Remove(subsList[i]);
               #if EVENTROUTER_THROWEXCEPTIONS
                   listenerPresent = true;
               #endif

               if (subsList.Count == 0)
               {
                   _subsList.Remove(eventType);
               }

               return;
           }
        }

        #if EVENTROUTER_THROWEXCEPTIONS
            if(!listenerPresent)
            {
                throw new ArguementException(string.Format("The receiver isn't subbed to event type \"{0}\".", eventType.ToString()));
            }
        #endif
    }

    /// A trigger function.

    public static void TriggerEvent<Event> (Event newEvent) where Event : struct
    {
        List<ICoreEventListener> list;
        if (!_subsList.TryGetValue(typeof(Event), out list))
        #if EVENTROUTER_REQUIRELISTENER
            throw new ArguementException( string.Format( "Attempting to send event of type \"{0}\", but no listener for this type has been found. Make sure this.Subscribe<{0}>(EventRouter) has been called, or that all listeners to this event haven't been unsubscribed.", typeof( Event ).ToString() ) );
        #else
            return;
        #endif

        //TODO / Or not TODO - Do you need to make a trigger require a listener? In other words must there be a listener for an event to trigger?

        for (int i = 0; i < list.Count; i++)
        {
            (list[i] as IEventListener<Event>).OnEvent(newEvent);
            //Debug.Log("An Event was triggered");
        }
    }

    /// Is there a potential subsriber for a specific event? If there is "true" / If it's already a sub "false 

    private static bool SubExists (Type type, ICoreEventListener receiver)
    {
        List<ICoreEventListener> receivers;

        if (!_subsList.TryGetValue(type, out receivers)) return false;

        bool present = false;

        for (int i = 0; i < receivers.Count; i++)
        {
            if (receivers[i] == receiver)
            {
                present = true;
                break;
            }
        }

        return present;
    }
}

/// Static class which allows any class to start or stop listening to events.
public static class EventRegister
{
    public delegate void Delegate<T> (T eventType);

    public static void StartListeningEvent<EventType> (this IEventListener<EventType> caller) where EventType : struct
    {
        EventManager.AddListener<EventType>(caller);
    }

    public static void StopListeningEvent<EventType> (this IEventListener<EventType> caller) where EventType : struct
    {
        EventManager.RemoveListener<EventType>(caller);
    }
}

/// A basic listener interface. Nothing fancy.
public interface ICoreEventListener { };

/// A specialized listener.
public interface IEventListener<T> : ICoreEventListener
{
    void OnEvent (T eventType);
}
