using System.Collections.Generic;
using System.Threading.Tasks;

[CommandResolverHandler("Talk")]
public class TalkCommand : CommandResolver
{
    public override Task<object> Resolve(string arg, List<object> args, Dictionary<string, object> env)
    {
        throw new System.NotImplementedException();
    }
}
