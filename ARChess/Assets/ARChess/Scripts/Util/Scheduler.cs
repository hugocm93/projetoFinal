using System;
using System.Collections;

namespace Util
{

//Source
//https://forum.unity.com/threads/schedule-command.7838/
public class Scheduler
{
    private ArrayList ScheduledEvents = new ArrayList();
    private object ThreadLocker = new object();

    public Scheduler()
    {
    }
    /// <summary>
    /// Registers the event.
    /// </summary>
    /// <param name='ScheduledIn'>
    /// Scheduled in.
    /// </param>
    /// <param name='functionPointer'>
    /// Function pointer to the event i.e. new FunctionPointer(MyFunction).
    /// </param>
    public void RegisterEvent(double ScheduledIn, FunctionPointer functionPointer)
    {
        ScheduledEvents.Add(new ScheduledEvent(functionPointer, ScheduledIn));
    }
    /// <summary>
    /// Registers an event.
    /// </summary>
    /// <param name='ScheduledIn'>
    /// The time from now to execute the event.
    /// </param>
    /// <param name='functionPointer'>
    /// Function pointer to the event i.e. new ParamiterizedFunctionPointer(MyFunction).
    /// </param>
    /// <param name='paramiters'>
    /// Paramiters to pass in as an object array.
    /// </param>
    public void RegisterEvent(double ScheduledIn, ParamiterizedFunctionPointer functionPointer, params object[] paramiters)
    {
        ScheduledEvents.Add(new ScheduledEvent(functionPointer, paramiters, ScheduledIn));
    }

    public void Clear()
    {
        lock(ThreadLocker)
        {
            ScheduledEvents.Clear();
        }
    }

    /// <summary>
    /// Executes the functions in the ScheduledEvents list.
    /// </summary>
    /// <param name='sender'>
    /// Sender.
    /// </param>
    /// <param name='e'>
    /// E.
    /// </param>
    public void ExecuteSchedule()
    {
        //Lock the thread so we dont overrun
        lock(ThreadLocker)
        {
            //Only if we have events because we need to initalize a control group
            if(ScheduledEvents.Count > 0)
            {
                //control group records the execute events for remove later
                ArrayList executedEvents = new ArrayList();

                //Test all our events
                foreach(ScheduledEvent nEvent in  ScheduledEvents)
                {
                    if(System.DateTime.Now >= nEvent.TargetTime)
                    {
                        executedEvents.Add(nEvent);
                        if(nEvent.xPointer != null)
                        {
                            nEvent.xPointer();
                        }
                        else if(nEvent.yPointer != null)
                        {
                            nEvent.yPointer(nEvent.Paramiters);
                        }
                    }
                }

                //remove the executed events
                foreach(ScheduledEvent nEvent in executedEvents)
                {
                    ScheduledEvents.Remove(nEvent);
                }

                executedEvents.Clear();
            }
        }
    }
    };

//Origem: https://forum.unity.com/threads/schedule-command.7838/
//Autor: lodendsg
/// <summary>
/// Scheduled event object.
/// </summary>
public class ScheduledEvent
{
    /// <summary>
    /// The x pointer indicats a paramiterless function call.
    /// </summary>
    public FunctionPointer xPointer;
    /// <summary>
    /// The y pointer indicates a paramiterized function call.
    /// </summary>
    public ParamiterizedFunctionPointer yPointer;
    /// <summary>
    /// The target time to exectute the call.
    /// </summary>
    public DateTime TargetTime;
    /// <summary>
    /// The paramiters to use for paramiterized calls.
    /// </summary>
    public object[] Paramiters;
    /// <summary>
    /// Initializes a new instance of the <see cref="DarkStarGames.ScheduledEvent"/> class.
    /// </summary>
    /// <param name='pointer'>
    /// Pointer.
    /// </param>
    /// <param name='time'>
    /// Time.
    /// </param>
    public ScheduledEvent(FunctionPointer pointer, double time)
    {
        xPointer = pointer;
        TargetTime = DateTime.Now.AddMilliseconds(time);
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="DarkStarGames.ScheduledEvent"/> class.
    /// </summary>
    /// <param name='pointer'>
    /// Pointer.
    /// </param>
    /// <param name='parmaiters'>
    /// Parmaiters.
    /// </param>
    /// <param name='time'>
    /// Time.
    /// </param>
    public ScheduledEvent(ParamiterizedFunctionPointer pointer, object[] parmaiters, double time)
    {
        this.yPointer = pointer;
        this.TargetTime = DateTime.Now.AddMilliseconds(time);
        this.Paramiters = parmaiters;
    }
}

//Source
//https://forum.unity.com/threads/schedule-command.7838/
/// <summary>
/// Function pointer.
/// </summary>
public delegate void FunctionPointer();
/// <summary>
/// Paramiterized function pointer.
/// </summary>
public delegate void ParamiterizedFunctionPointer(params object[] paramiters);
}