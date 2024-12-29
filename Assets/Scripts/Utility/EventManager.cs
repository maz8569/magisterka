using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class TypedEvent : UnityEvent<object> { }

public class EventManager : MonoBehaviour
{

    private Dictionary<string, TypedEvent> typedEventDictionary;
    private static EventManager eventManager;


    public static EventManager instance
    {
        get
        {
            if (!eventManager)
            {
                eventManager = FindObjectOfType(typeof(EventManager)) as EventManager;

                if (!eventManager)
                {
                    Debug.LogError("There needs to be one active EventManger script on a GameObject in your scene.");
                }
                else
                {
                    eventManager.Init();
                }
            }

            return eventManager;
        }
    }

    void Init()
    {
        if (typedEventDictionary == null)
        {
            typedEventDictionary = new Dictionary<string, TypedEvent>();
        }
    }

    public static void StartListening(string eventName, UnityAction<object> listener)
    {
        TypedEvent thisEvent = null;
        if (instance.typedEventDictionary.TryGetValue(eventName, out thisEvent))
        {
            thisEvent.AddListener(listener);
        }
        else
        {
            thisEvent = new TypedEvent();
            thisEvent.AddListener(listener);
            instance.typedEventDictionary.Add(eventName, thisEvent);
        }
    }

    public static void StopListening(string eventName, UnityAction<object> listener)
    {
        if (eventManager == null) return;
        TypedEvent thisEvent = null;
        if (instance.typedEventDictionary.TryGetValue(eventName, out thisEvent))
        {
            thisEvent.RemoveListener(listener);
        }
    }

    public static void TriggerEvent(string eventName, object data)
    {
        TypedEvent thisEvent = null;
        if (instance.typedEventDictionary.TryGetValue(eventName, out thisEvent))
        {
            thisEvent.Invoke(data);
        }
    }

}