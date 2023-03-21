using UnityEngine;

namespace Logic.Map
{
    public class MapEdgeFog : MonoBehaviour
    {
        public ParticleSystem[] fogPs;

        public float offset = 0.5f;

        private void Awake()
        {
            fogPs = GetComponentsInChildren<ParticleSystem>();
        }

        public void UpdateFogBounds(Bounds visible, Bounds world)
        {

            UpdateFogByDirection(1, visible.center, visible.size, visible.min.y - offset <= world.min.y);
            UpdateFogByDirection(2, visible.center, visible.size, visible.max.x + offset >= world.max.x);
            UpdateFogByDirection(3, visible.center, visible.size, visible.min.x - offset <= world.min.x);
            UpdateFogByDirection(0, visible.center, visible.size, visible.max.y + offset >= world.max.y);

        }

        private void UpdateFogByDirection(int direction, Vector3 center, Vector3 size, bool start)
        {
            if (direction >= fogPs.Length) return;

            var shape = fogPs[direction].shape;
            
            fogPs[direction].gameObject.SetActive(start);

            if (!start)
            {
                fogPs[direction].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                return;
            }

            fogPs[direction].Play();



            switch (direction)
            {
                case 0:
                    shape.scale = new Vector3(size.x * 2f, 0.01f, 0);
                    shape.position = new Vector3(center.x, center.y + size.y / 2, 0 );
                    break;
                case 1:
                    shape.scale = new Vector3(size.x * 2f, 0.01f, 0);
                    shape.position = new Vector3(center.x, center.y - size.y / 2, 0 );
                    break;
                case 2:
                    shape.scale = new Vector3(0.01f, size.y * 2f, 0);
                    shape.position = new Vector3(center.x + size.x / 2, center.y, 0 );
                    break;
                case 3:
                    shape.scale = new Vector3(0.01f, size.y * 2f, 0);
                    shape.position = new Vector3(center.x - size.x / 2, center.y, 0 );
                    break;
            }
        }
    }
}