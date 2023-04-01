using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Utils;

public class CommandSnapshot : PoolObject, IDisposable
{
    public string commandName;
    private string arg;
    public List<object> args;
    private Dictionary<string, object> env;
    private readonly List<IDisposable> stuff = new ();

    public CommandSnapshot Capture(string name, string arg0, IEnumerable<object> args0, IDictionary<string, object> env0)
    {
        commandName = name;
        arg = arg0;
        args = new List<object>(args0);
        env = new Dictionary<string, object>(env0);
        return this;
    }

    public void AddDisposeStuff(params IDisposable[] stuffs)
    {
        stuff.AddRange(stuffs);
    }
    
    public override void Dispose()
    {
        base.Dispose();
        foreach (var item in stuff)
        {
            item.Dispose();
        }
        stuff.Clear();
        commandName = null;
        arg = null;
        args.Clear();
        env.Clear();
    }
    
}