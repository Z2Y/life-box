using System.Collections.Generic;
using System.Linq;
using Logic.Detector;
using Logic.Detector.Config;
using Logic.Detector.Scriptable;
using ModelContainer;
using ShortCuts;
using UI;
using UnityEngine;

namespace Controller
{
    public class NPCInteractController : MonoBehaviour
    {
        private CollisionDetector collisionDetector;

        [SerializeField] 
        private ScriptableDetectorBase[] detectors;

        [SerializeField] private Vector3 tipOffset;

        public readonly HashSet<KeyValuePair<IDetector, Collider2D>> activeDetectors = new ();

        private InteractTip tip;

        private void OnEnable()
        {
            collisionDetector = GetComponent<CollisionDetector>();
            foreach (var item in detectors)
            {
                Debug.Log(item.GetDetector());
                Debug.Log($"Add Detector {item.GetDetector().GetType().Name}");
                var detector = item.GetDetector();
                detector.onDetect(onDetect);
                detector.onEndDetect(onEndDetect);
                collisionDetector.AddDetector(detector);
            }
        }

        private void onEndDetect(IDetector detector, Collider2D collision)
        {
            activeDetectors.Remove(new KeyValuePair<IDetector, Collider2D>(detector, collision));
            if (activeDetectors.Count <= 0)
            {
                hideInteractMenu();
            }
        }

        private void onDetect(IDetector detector, Collider2D collision)
        {
            Debug.Log($"Detect {detector.GetType().Name} ${collision.gameObject.name}");
            activeDetectors.Add(new KeyValuePair<IDetector, Collider2D>(detector, collision));
            if (activeDetectors.Count > 0)
            {
                showInteractMenu();
            }
        }

        private async void showInteractMenu()
        {
            var items = InteractMenuConfig.buildMenuItems(activeDetectors.ToList());
            var targets = items.Select((item) => item.collision).Distinct();

            var target = ReferenceEquals(tip, null)
                ? targets.FirstOrDefault()
                : targets.FirstOrDefault(t => t.GetInstanceID() == tip.targetID);

            if (ReferenceEquals(target, null)) return;
            
            var menuTypes = items.Where((item) => item.collision.GetInstanceID() == target.GetInstanceID())
                .Select((item) => item.menuID).Distinct().ToList();
            
            if (menuTypes.Count <= 0)
            {
                return;
            }
            var menuID = menuTypes.Count > 1  ? 0 : menuTypes.First();

            var offset = new Vector3(0, target.bounds.size.y, 0);

            if (ReferenceEquals(tip, null))
            {
                tip = await InteractTip.Show(menuID);
                
                tip.SetPosition(target.transform.position  + offset, target.GetInstanceID());
            }
            else
            {
                tip.UpdateMenu(menuID);
                tip.Show();
                tip.SetPosition(target.transform.position + offset, target.GetInstanceID());
            }
            
            var menuConfig = InteractMenuConfigCollection.Instance.GetConfig(menuID);

            InputCommandResolver.Instance.Register(tip.keyCode, new InteractMenuHandler(this, menuConfig.MenuResolver));

        }

        private void hideInteractMenu()
        {
            if (ReferenceEquals(tip, null)) return;
            UIManager.Instance.Hide(tip.GetInstanceID());
            InputCommandResolver.Instance.UnRegister(tip.keyCode);
            tip = null;
        }

        private void OnDisable()
        {
            foreach (var item in detectors)
            {
                collisionDetector.RemoveDetector(item.GetDetector());
            }
        }
    }
}