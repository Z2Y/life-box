namespace Logic.Map
{
    public interface IRouteResponder
    {
        public void OnEnter(long mapID, long placeID);

        public void OnLeave(long mapID, long placeID);
    }
}