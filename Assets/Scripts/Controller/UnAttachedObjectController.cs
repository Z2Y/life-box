using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Controller
{
    public class UnAttachedObjectController : MonoBehaviour
    {
        private readonly HashSet<IDisposable> _objectsClearOnDestroy = new();
        private readonly HashSet<IDisposable> _objectsClearOnDisable = new();

        public void AttachObject(IDisposable instance, bool clearOnDisable = true)
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


        public void RemoveIf(Predicate<IDisposable> func)
        {
            _objectsClearOnDestroy.RemoveWhere(func);
        }

        private void OnDisable()
        {
            foreach (var obj in _objectsClearOnDisable)
            {
                obj.Dispose();
            }
            _objectsClearOnDisable.Clear();
        }

        private void OnDestroy()
        {
            foreach (var obj in _objectsClearOnDestroy)
            {
                obj.Dispose();
            }
            _objectsClearOnDestroy.Clear();
        }
    }
}