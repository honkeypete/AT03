using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectiveItem : MonoBehaviour, IInteractable
{
    public delegate void ObjectiveDelegate();

    private static bool active = false;

    public static event ObjectiveDelegate ObjectiveActivatedEvent = delegate { ObjectiveActivatedEvent = delegate { }; };

    private void Awake()
    {
        active = false;
    }

    public void Activate()
    {
        if (active == false)
        {
            active = true;
            ObjectiveActivatedEvent.Invoke();
        }
    }
}