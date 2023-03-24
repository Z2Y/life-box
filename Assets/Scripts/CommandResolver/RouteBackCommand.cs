using System.Threading.Tasks;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Model;
using ModelContainer;

[CommandResolverHandler("BackPlace")]
public class RouteBackCommand : RouteCommand {
    public override async UniTask<object> Resolve(string arg, List<object> args, Dictionary<string, object> env)
    {
        var place = await ExpressionCommandResolver.Resolve("CurrentPlace", arg, args, env) as Place;

        if (place != null && place.Parent > 0)
        {
            var routeCompleteSource = new TaskCompletionSource<long>();
            var target = PlaceCollection.Instance.GetPlace(place.Parent);
            if (target != null) {
                CurrentLife.Place = target;
                CurrentLife.Next.Place = target;
                RouteTrigger.Instance.Trigger().Coroutine();
                LifeEngine.Instance.AfterLifeChange?.Invoke();
                routeCompleteSource?.TrySetResult(target.ID);
            } else {
                routeCompleteSource?.TrySetResult(0);
            }
            return await routeCompleteSource.Task; 
        }
        return null;
    }
}