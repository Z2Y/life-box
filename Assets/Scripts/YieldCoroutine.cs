using System.Collections;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class YieldCoroutine : MonoBehaviour
{
    private static YieldCoroutine instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public static async UniTask WaitForSeconds(float t)
    {
        await UniTask.Delay(System.TimeSpan.FromSeconds(t));
    }

    public static async UniTask WaitForInstruction(YieldInstruction instruction)
    {
        var yieldTask = new YieldTask(instruction);
        await DoTask(yieldTask).ToUniTask(instance);
    }

    public static async UniTask WaitForInstruction(CustomYieldInstruction instruction)
    {
        var yieldTask = new YieldTask(instruction);
        await DoTask(yieldTask).ToUniTask(instance);
    }

    private static IEnumerator DoTask(YieldTask task)
    {
        yield return task.instruction;
    }

    private class YieldTask
    {
        public readonly object instruction;

        public YieldTask(YieldInstruction instruction)
        {
            this.instruction = instruction;
        }
        
        public YieldTask(CustomYieldInstruction instruction)
        {
            this.instruction = instruction;
        }
    }
}