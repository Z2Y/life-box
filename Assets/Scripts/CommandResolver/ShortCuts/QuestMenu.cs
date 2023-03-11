using NPBehave;
using UI;
using UnityEngine;

namespace ShortCuts
{
    [InputCommandResolverHandler(KeyCode.J)]
    public class QuestMenuResolver : IInputCommandResolver
    {
        public async void Resolve(KeyCode code)
        {
            try
            {
                var panel = await QuestPanel.ShowPanel();
                disableShortCuts();
                panel.OnClose(enableShortCuts);
            }
            catch (Exception e)
            {
                // nothing
                enableShortCuts();
            }
            
        }

        void disableShortCuts()
        {
            Time.timeScale = 0;
            LifeEngine.Instance.MainCharacter.disableAllShortCuts();
        }

        void enableShortCuts()
        {
            Time.timeScale = 1;
            LifeEngine.Instance.MainCharacter.enableAllShortCuts();
        }
    }
}