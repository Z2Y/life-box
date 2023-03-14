using System;
using UnityEngine;

namespace ShortCuts
{
    [InputCommandResolverHandler(KeyCode.K)]
    public class OpenKnapsackResolver : IInputCommandResolver
    {
        public async void Resolve(KeyCode code)
        {
            var knapsack = LifeEngine.Instance.lifeData?.knapsackInventory;
            
            var panel = await KnapsackPanel.Show(knapsack);
            if (!ReferenceEquals(panel, null))
            {
                LifeEngine.Instance.MainCharacter.disableAllShortCuts();
                panel.OnClose(() =>
                {
                    LifeEngine.Instance.MainCharacter.enableAllShortCuts();
                });
            }
        }
    }
}