using System;
using UnityEngine;
using System.Collections;

namespace Util
{
    
public enum ButtonEnum
{
    NewGame, LoadGame, SaveGame, Undo, Redo, File1, File2, File3
};

public class Constants
{
    public static readonly float _scale = 10.0f;
    public static readonly float _tile_size = 1f;
    public static readonly Vector2 _tile_offset = new Vector2(_tile_size / 2, _tile_size / 2);
    public static readonly Vector3 _origin = GameObject.Find("ChessBoard").transform.position;
    public static readonly Vector2Int _none = new Vector2Int(-1, -1);
     
    public static Vector3 getTileCenter(Vector2Int position)
    {
        var positionFloat = new Vector2(position.x, position.y);
        var center2D = _tile_size * positionFloat + _tile_offset;
        return _origin + _scale * new Vector3(center2D.x, 0, center2D.y);
    }

    public static Vector3 getBoardCenter()
    {
            var offset = _tile_size * _scale * 4;
            return new Vector3(_origin.x + offset, _origin.y, _origin.z + offset);
    }

    public static Vector2Int getTile(Vector3 position3d)
    {
        var positionInBoard = position3d - _origin;
        if(positionInBoard.x < 0 || positionInBoard.z < 0)
            return _none;
            
        return new Vector2Int((int)(positionInBoard.x / Util.Constants._scale), 
                              (int)(positionInBoard.z / Util.Constants._scale));
    }

    public static string ButtonEnumToString(ButtonEnum e)
    {
        switch(e)
        {
            case ButtonEnum.NewGame: 
                return "NewGame";

            case ButtonEnum.LoadGame:
                return "LoadGame";

            case ButtonEnum.SaveGame:
                return "SaveGame";

            case ButtonEnum.Undo:
                return "Undo";

            case ButtonEnum.Redo:
                return "Redo";

            case ButtonEnum.File1:
                return "File1";

            case ButtonEnum.File2:
                return "File2";

            case ButtonEnum.File3:
                return "File3";

            default:
                return "";
        }
    }
};
        
public class Pair<T, U>
{
    public Pair(){}

    public Pair(T first, U second)
    {
        this.first = first;
        this.second = second;
    }

    public T first { get; set; }
    public U second { get; set; }
};


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

//Source
//https://forum.unity.com/threads/schedule-command.7838/
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