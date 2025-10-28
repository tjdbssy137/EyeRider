using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;


public class EventManager
{
    private Dictionary<EEventType, List<CustomAction>> _event;
    
    public void Init()
    {
        if (_event == null)
        {
            _event = new Dictionary<EEventType, List<CustomAction>>();
        }
    }

    public void AddEvent(EEventType eventType, CustomAction listener)
    {
        List<CustomAction> thisEvent;
        if (_event.TryGetValue(eventType, out thisEvent))
        {
            thisEvent.Add(listener);
            _event[eventType] = thisEvent;
        }
        else
        {
            thisEvent = new List<CustomAction>();
            thisEvent.Add(listener);
            _event.Add(eventType, thisEvent);
        }
    }

    public void RemoveEvent(EEventType eventType, CustomAction listener)
    {
        if (_event == null)
        {
            return;
        }

        List<CustomAction> thisEvent;
        if (_event.TryGetValue(eventType, out thisEvent))
        {
            thisEvent.Remove(listener);
            _event[eventType] = thisEvent;
        }
    }

    public void TriggerEvent(EEventType eventType, Component sender = null, object param = null)
    {
        List<CustomAction> thisEvent;
        if (_event.TryGetValue(eventType, out thisEvent))
        {
            foreach(var action in thisEvent)
            {
                action.Invoke(sender, param);
            }
        }
    }

    public void Clear()
    {
        _event.Clear();
    }
}

public delegate void CustomAction(Component sender = null, object param = null);