using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using XNode;

public class TaskNode : BaseNode
{
    [Input] public int previous;
    [Output] public int next;

    public UnityEvent function;

    public void ExecuteTask()
    {
        function?.Invoke();
    }
}
