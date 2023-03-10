using System.Threading.Tasks;
using Utils;

namespace UI
{
    public static class UIFactory<T> where T : UIBase
    {
        public static T Create()
        {
            var ui = PrefabLoader<T>.Create(UIManager.Instance.transform);
            if (!ReferenceEquals(ui, null))
            {
                UIManager.Instance.PushUI(ui);
            }
            return ui;
        }

        public static async Task<T> CreateAsync()
        {
            var ui = await PrefabLoader<T>.CreateAsync(UIManager.Instance.transform);
            if (!ReferenceEquals(ui, null))
            {
                UIManager.Instance.PushUI(ui);
            }
            return ui;
        }
    }
}