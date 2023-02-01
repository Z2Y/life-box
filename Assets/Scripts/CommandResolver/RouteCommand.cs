using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Model;
using ModelContainer;

[CommandResolverHandler("RoutePlace")]
public class RouteCommand : CommandResolver
{
    protected TaskCompletionSource<long> routeCompleteSource;
    public override async Task<object> Resolve(string arg, List<object> args, Dictionary<string, object> env)
    {
        routeCompleteSource = new TaskCompletionSource<long>();
        var nearbyPlaces = new List<Place>();
        var currentPlace = await ExpressionCommandResolver.GetResolver("CurrentPlace").Resolve(arg, args, env) as Place;

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
            (idx) => OnRoute(nearbyPlaces[idx])).SetCancelable(true, OnCancel);
        return await routeCompleteSource.Task;
    }

    private void OnCancel() {
        routeCompleteSource?.TrySetCanceled();
    }

    private void OnRoute(Place target)
    {
        CurrentLife.Place = target;
        CurrentLife.Next.Place = target;
        RouteTrigger.Instance.Trigger().Coroutine();
        LifeEngine.Instance?.AfterLifeChange?.Invoke();
        routeCompleteSource?.TrySetResult(target.ID);
    }

    protected LifeNode CurrentLife => LifeEngine.Instance?.lifeData.current;
}