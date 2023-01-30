using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Model;
using ModelContainer;

[CommandResolverHandler("BackPlace")]
public class RouteBackCommand : RouteCommand {
    public override async Task<object> Resolve(string arg, List<object> args, Dictionary<string, object> env)
    {
        Place place = await ExpressionCommandResolver.GetResolver("CurrentPlace").Resolve(arg, args, env) as Place;
        
        if (place.Parent > 0)
        {
            routeCompleteSource = new TaskCompletionSource<long>();
            OnBack(place.Parent);
            return await routeCompleteSource.Task; 
        }
        return null;
    }

    private void OnBack(long placeID) {
        Place target = PlaceCollection.Instance.GetPlace(placeID);
        if (target != null) {
            CurrentLife.Place = target;
            CurrentLife.Next.Place = target;
            RouteTrigger.Instance.Trigger().Coroutine();
            LifeEngine.Instance?.AfterLifeChange?.Invoke();
            routeCompleteSource?.TrySetResult(target.ID);
        } else {
            routeCompleteSource?.TrySetResult(0);
        }
    }
}