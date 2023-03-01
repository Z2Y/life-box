using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Model;
using ModelContainer;

[CommandResolverHandler("RoutePlace")]
public class RouteCommand : CommandResolver
{
    public override async Task<object> Resolve(string arg, List<object> args, Dictionary<string, object> env)
    {
        var routeCompleteSource = new TaskCompletionSource<long>();
        var nearbyPlaces = new List<Place>();
        var currentPlace = await ExpressionCommandResolver.Resolve("CurrentPlace", arg, args, env) as Place;

        if (currentPlace.Parent > 0)
        {
            var parent = PlaceCollection.Instance.GetPlace(currentPlace.Parent);
            if (parent != null) nearbyPlaces.Add(parent);
        }
        foreach (var place in currentPlace.Child)
        {
            var child = PlaceCollection.Instance.GetPlace(place);
            if (child != null) nearbyPlaces.Add(child);
        }
        SelectPanel.Show(
            "选择想要去的地点", 
            nearbyPlaces.Select((place) => place.Name).ToList(), 
            (idx) =>
            {
                OnRoute(nearbyPlaces[idx]);
                routeCompleteSource?.TrySetResult(nearbyPlaces[idx].ID);
            }).SetCancelable(true, () => routeCompleteSource.TrySetCanceled());
        return await routeCompleteSource.Task;
    }

    private void OnRoute(Place target)
    {
        CurrentLife.Place = target;
        CurrentLife.Next.Place = target;
        RouteTrigger.Instance.Trigger().Coroutine();
        LifeEngine.Instance.AfterLifeChange?.Invoke();
    }

    protected LifeNode CurrentLife => LifeEngine.Instance.lifeData.current;
}