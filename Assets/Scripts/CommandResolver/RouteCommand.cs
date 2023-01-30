using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Model;
using ModelContainer;

[CommandResolverHandler("RoutePlace")]
public class RouteCommand : CommandResolver
{
    protected TaskCompletionSource<long> routeCompleteSource;
    public List<Place> nearbyPlaces = new List<Place>();
    public override async Task<object> Resolve(string arg, List<object> args, Dictionary<string, object> env)
    {
        routeCompleteSource = new TaskCompletionSource<long>();
        nearbyPlaces.Clear();
        Place currentPlace = await ExpressionCommandResolver.GetResolver("CurrentPlace").Resolve(arg, args, env) as Place;

        if (currentPlace.Parent > 0)
        {
            Place parent = PlaceCollection.Instance.GetPlace(currentPlace.Parent);
            if (parent != null) nearbyPlaces.Add(parent);
        }
        foreach (var place in currentPlace.Child)
        {
            Place child = PlaceCollection.Instance.GetPlace(place);
            if (child != null) nearbyPlaces.Add(child);
        }
        SelectPanel.Show("选择想要去的地点", nearbyPlaces.Select((place) => place.Name).ToList(), OnRoute).SetCancelable(true, OnCancel);
        return await routeCompleteSource.Task;
    }

    public void OnCancel() {
        routeCompleteSource?.TrySetCanceled();
    }

    public void OnRoute(int selectedIndex)
    {

        Place target = nearbyPlaces[selectedIndex];
        CurrentLife.Place = target;
        CurrentLife.Next.Place = target;
        RouteTrigger.Instance.Trigger().Coroutine();
        LifeEngine.Instance?.AfterLifeChange?.Invoke();
        routeCompleteSource?.TrySetResult(target.ID);
    }

    public LifeNode CurrentLife
    {
        get
        {
            return LifeEngine.Instance?.lifeData.current;
        }
    }
}