using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class YieldCoroutine : MonoBehaviour
{
    protected static readonly List<YieldTask> Tasks = new ();
    
    private void Update() {
        if (Tasks.Count > 0)
        {
            DOWait();
        }
    }

    public static async Task WaitForSeconds(float t)
    {
        var yieldTask = new YieldTask(new WaitForSeconds(t));
        Tasks.Add(yieldTask);
        await yieldTask.Wait();
    }

    public static async Task WaitForInstruction(YieldInstruction instruction, CancellationTokenSource cancel = null)
    {
        var yieldTask = new YieldTask(instruction, cancel);
        Tasks.Add(yieldTask);
        await yieldTask.Wait();
    }

    public static async Task WaitForInstruction(CustomYieldInstruction instruction)
    {
        var yieldTask = new YieldTask(instruction);
        Tasks.Add(yieldTask);
        await yieldTask.Wait();
    }

    private void DOWait()
    {
        foreach(var task in Tasks)
        {
            StartCoroutine(DoTask(task));
        }
        Tasks.Clear();
    }

    private static IEnumerator DoTask(YieldTask task)
    {
        yield return task.instruction;
        task.source.TrySetResult(true);
    }

    protected class YieldTask
    {
        public readonly object instruction;
        public readonly TaskCompletionSource<bool> source;

        public YieldTask(YieldInstruction instruction, CancellationTokenSource cancel = null)
        {
            this.instruction = instruction;
            source = new TaskCompletionSource<bool>();
            cancel?.Token.Register(Cancel);
        }
        
        public YieldTask(CustomYieldInstruction instruction, CancellationTokenSource cancel = null)
        {
            this.instruction = instruction;
            source = new TaskCompletionSource<bool>();
            cancel?.Token.Register(Cancel);
        }

        private void Cancel()
        {
            source.TrySetCanceled();
        }

        public async Task Wait()
        {
            await source.Task;
        }
    }
}