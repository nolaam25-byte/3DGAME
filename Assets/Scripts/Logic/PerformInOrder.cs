using UnityEngine;
using UnityEngine.Events;

public class PerformInOrder : MonoBehaviour
{
    [Tooltip("A series of UnityEvents. Each event will trigger in the exact order here.")]
    public UnityEvent[] orderedEvents = new UnityEvent[1];

    public void PerformOrderedEvents()
    {
        for (int i = 0; i < orderedEvents.Length; i++)
        {
            UnityEvent each = orderedEvents[i];
            if (each != null) each.Invoke();
        }
    }
}
