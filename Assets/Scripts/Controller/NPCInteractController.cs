using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Logic.Detector;
using Logic.Detector.Config;
using Logic.Detector.Scriptable;
using Logic.Loot;
using ModelContainer;
using ShortCuts;
using UI;
using UnityEngine;
using Exception = NPBehave.Exception;

namespace Controller
{
    public class NPCInteractController : MonoBehaviour
    {
        private CollisionDetector collisionDetector;

        [SerializeField] 
        private ScriptableDetectorBase[] detectors;

        public readonly HashSet<KeyValuePair<IDetector, Collider2D>> activeDetectors = new ();

        private readonly List<IDetector> _detectors = new ();

        public ScriptableLootItemDetector lootDetector;

        private bool tipUpdating;

        private bool interactDisabled;

        private InteractTip tip;

        private void Awake()
        {
            collisionDetector = GetComponent<CollisionDetector>();
            IDetector detector;
            foreach (var item in detectors)
            {
                detector = item.GetDetector();
                detector.onDetect(onDetect);
                detector.onEndDetect(onEndDetect);
                _detectors.Add(detector);
            }

            detector = lootDetector.GetDetector();
            detector.onDetect(onLootItem);
            _detectors.Add(detector);
        }

        private void OnEnable()
        {
            foreach (var item in _detectors)
            {
                collisionDetector.AddDetector(item);
            }
        }
        
        private void OnDisable()
        {
            foreach (var item in _detectors)
            {
                collisionDetector.RemoveDetector(item);
            }
        }

        private void onLootItem(IDetector detector, Collider2D collision)
        {
            var loot = collision.gameObject.GetComponent<ILootItem>();
            var items = loot?.GetLootItemStack();
            if (items is { Empty: false })
            {
                if (loot.IsAutoLoot())
                {
                    LifeEngine.Instance.lifeData.knapsackInventory.StoreItem(items.item, items.Count);
                    loot.OnLoot(gameObject);
                }
                else
                {
                    // loot by interact button pressed
                }
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
            // Debug.Log($"Detect {detector.GetType().Name} {collision.gameObject.name}");
            activeDetectors.Add(new KeyValuePair<IDetector, Collider2D>(detector, collision));
            if (activeDetectors.Count > 0)
            {
                showInteractMenu();
            }
        }

        private async void showInteractMenu()
        {
            activeDetectors.RemoveWhere((pair) => pair.Value == null);
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
            
            try
            {
                var offset = new Vector3(0, target.bounds.size.y, 0);
                
                while (tipUpdating)
                {
                    await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
                }
                tipUpdating = true;
                if (ReferenceEquals(tip, null))
                {
                    tip = await InteractTip.Show(menuID);

                    tip.SetPosition(target.transform.position + offset, target.GetInstanceID());
                }
                else
                {
                    tip.UpdateMenu(menuID);
                    tip.Show();
                    tip.SetPosition(target.transform.position + offset, target.GetInstanceID());
                }

                var menuConfig = InteractMenuConfigCollection.GetConfig(menuID);
                
                InputCommandResolver.Instance.Register(tip.keyCode,
                    new InteractMenuHandler(this, menuConfig.MenuResolver));

                if (interactDisabled)
                {
                    tip.Hide();
                    InputCommandResolver.Instance.DisableKeyCode(tip.keyCode);
                }
                else
                {
                    InputCommandResolver.Instance.EnableKeyCode(tip.keyCode);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
            }
            tipUpdating = false;
        }

        public void disableInteract()
        {
            interactDisabled = true;
            if (ReferenceEquals(tip, null)) return;
            tip.Hide();
            InputCommandResolver.Instance.DisableKeyCode(tip.keyCode);
        }

        public void enableInteract()
        {
            interactDisabled = false;
            if (ReferenceEquals(tip, null)) return;
            if (!tip.gameObject.activeSelf)
            {
                tip.Show();
            }
            InputCommandResolver.Instance.EnableKeyCode(tip.keyCode);
        }
        
        public void removeInteract()
        {
            if (ReferenceEquals(tip, null)) return;
            InputCommandResolver.Instance.UnRegister(tip.keyCode);
        }


        private void hideInteractMenu()
        {
            if (ReferenceEquals(tip, null)) return;
            UIManager.Instance.Hide(tip.GetInstanceID());
            tip.Hide();
            InputCommandResolver.Instance.UnRegister(tip.keyCode);
            tip = null;
        }
    }
}