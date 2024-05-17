using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EventManager
{
    public delegate void UnitInitializationEvent(int count);

    public static event UnitInitializationEvent AgentInitializationEvent;

    public delegate void AcquisitionEvent();
    
    public static event AcquisitionEvent AcquisitionStartEvent;
    public static event AcquisitionEvent AcquisitionPauseEvent;
    public static event AcquisitionEvent AcquisitionEndEvent;

    public static void InvokeAgentInitializationEvent(int agentCount) { AgentInitializationEvent?.Invoke(agentCount); }
    public static void InvokeAcquisitionStartEvent() { AcquisitionStartEvent?.Invoke(); }
    public static void InvokeAcquisitionPauseEvent() { AcquisitionPauseEvent?.Invoke(); }
    public static void InvokeAcquisitionEndEvent() { AcquisitionEndEvent?.Invoke(); }
}
