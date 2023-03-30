using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Model;
using ModelContainer;

[CommandResolverHandler("RoutePlace")]
public class RouteCommand : CommandResolver
{
    public override async UniTask<object> Resolve(string arg, List<object> args, Dictionary<string, object> env)
    {
        var routeCompleteSource = new TaskCompletionSource<long>();
        var nearbyPlaces = new List<Place>();
        var currentPlace = await ExpressionCommandResolver.Resolve("CurrentPlace", arg, args, env) as Place;

        if (currentPlace.Parent > 0)
        {
            var parent = PlaceCollection.GetPlace(currentPlace.Parent);
            if (parent != null) nearbyPlaces.Add(parent);
        }
        foreach (var place in currentPlace.Child)
        {
            var child = PlaceCollection.GetPlace(place);
            if (child != null) nearbyPlaces.Add(child);
        }

        var selector = await SelectPanel.Show(
            "选择想要去的地点",
            nearbyPlaces.Select((place) => place.Name).ToList(),
            (idx) =>
            {
                OnRoute(nearbyPlaces[idx]);
                routeCompleteSource?.TrySetResult(nearbyPlaces[idx].ID);
            });
        selector.SetCancelable(true, () => routeCompleteSource.TrySetCanceled());
        return await routeCompleteSource.Task;
    }

    private void OnRoute(Place target)
    {
        CurrentLife.Place = target;
        CurrentLife.Next.Place = target;
        RouteTrigger.Instance.Trigger().Forget();
        LifeEngine.Instance.AfterLifeChange?.Invoke();
    }

    protected LifeNode CurrentLife => LifeEngine.Instance.lifeData.current;
}