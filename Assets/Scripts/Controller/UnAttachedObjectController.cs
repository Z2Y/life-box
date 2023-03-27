using System.Collections.Generic;
using UnityEngine;

namespace Controller
{
    public class UnAttachedObjectController : MonoBehaviour
    {
        private readonly List<Object> _objectsClearOnDestroy = new();
        private readonly List<Object> _objectsClearOnDisable = new();

        public void AttachObject(Object instance, bool clearOnDisable = true)
        {
            if (clearOnDisable)
            {
                _objectsClearOnDisable.Add(instance);
            }
            else
            {
                _objectsClearOnDestroy.Add(instance);
            }
        }

        private void OnDisable()
        {
            foreach (var obj in _objectsClearOnDisable)
            {
                Destroy(obj);
            }
            _objectsClearOnDisable.Clear();
        }

        private void OnDestroy()
        {
            foreach (var obj in _objectsClearOnDestroy)
            {
                Destroy(obj);
            }
            _objectsClearOnDestroy.Clear();
        }
    }
}