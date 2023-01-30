using System;
using System.Threading.Tasks;

public class BattleLoop
{
    private BattleManager m_battleManager;
    private BattleTurnManager m_turnManager;

    public BattleLoop(BattleManager manager)
    {
        m_battleManager = manager;
        m_turnManager = manager.TurnManager;
    }

    public async Task<BattleState> DoLoop()
    {
        while (true)
        {
            try
            {
                UnityEngine.Debug.Log("Update Battle loop");
                var result = m_turnManager.State;

                if (result != BattleState.InProgress)
                {
                    m_battleManager.OnBattleEnd(result);
                    return result;
                }

                var character = m_turnManager.GetNextCharacter();

                if (character == null)
                {
                    m_turnManager.OnBeforeTurnEnd();
                    m_turnManager.OnTurnEnd();
                    continue;
                }

                await m_turnManager.OnBeforeAction(character);

                await m_turnManager.OnAction(character);

                m_turnManager.OnAfterAction(character);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError(e);
                return BattleState.Cancled;
            }

        }
    }

}