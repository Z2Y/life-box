using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class YieldCoroutine : MonoBehaviour
{
    private static YieldCoroutine _instance;
    public static YieldCoroutine Instance {
        get {
            if (_instance == null) {
                _instance = (new GameObject("YieldCoroutine")).AddComponent<YieldCoroutine>();
            }
            return _instance;
        }
     }

    protected List<YieldTask> Tasks = new List<YieldTask>();

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Update() {
        if (Tasks.Count > 0)
        {
            DOWait();
        }
    }

    public async Task WaitForSeconds(float t)
    {
        var yieldTask = new YieldTask(new WaitForSeconds(t));
        Tasks.Add(yieldTask);
        await yieldTask.Wait();
    }

    public async Task WaitForInstruction(YieldInstruction instruction)
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

    private IEnumerator DoTask(YieldTask task)
    {
        yield return task.instruction;
        task.source.TrySetResult(true);
    }

    protected class YieldTask
    {
        public YieldInstruction instruction;
        public TaskCompletionSource<bool> source;

        public YieldTask(YieldInstruction instruction)
        {
            this.instruction = instruction;
            this.source = new TaskCompletionSource<bool>();
        }

        public async Task Wait()
        {
            await source.Task;
        }
    }
}