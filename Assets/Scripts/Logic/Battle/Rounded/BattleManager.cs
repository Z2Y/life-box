using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;


public class BattleManager : Singleton<BattleManager>
{
    private BattleTurnManager m_turnManager;
    private Scene originScene;

    public BattleTurnManager TurnManager
    {
        get
        {
            return m_turnManager;
        }
    }

    public BattleActionManager ActionManager
    {
        get
        {
            return BattleActionManager.Instance;
        }
    }

    public async Task<BattleState> DoBattle(BattleStartConfig config)
    {
        await EnterBattleScene();

        m_turnManager = new BattleTurnManager();

        foreach (var character in config.Roles)
        {
            AddBattleCharacter(character, config);
        }

        m_turnManager.InitFirstTurn();

        return await new BattleLoop(this).DoLoop();
    }

    public async Task EnterBattleScene()
    {
        originScene = SceneManager.GetActiveScene();
        await GameLoader.Instance.LoadSceneWithAnimation("BattleScene", LoadSceneMode.Additive);
    }

    public void OnBattleEnd(BattleState state)
    {
        BattleResultPanel.Show(state == BattleState.Win ? "胜利" : "失败", () =>
        {
            GameLoader.Instance.SwitchSceneWithAnimation(SceneManager.GetActiveScene(), originScene).Forget();
        });
    }

    public void AddBattleCharacter(BattleCharacter instance, BattleStartConfig config)
    {
        Vector3 initalPos = new Vector3();
        if (instance.TeamID < config.Positions.Count)
        {
            initalPos = config.Positions[instance.TeamID];
        }
        BattlePositionBlock initalBlock = FindBattlePosition(initalPos);

        if (initalBlock == null)
        {
            Debug.LogError($"Can't Add Character {instance.character.ID}");
            return;
        }

        instance.Position = initalBlock.Position;
        instance.CreateView(initalBlock.WorldPosition);

        m_turnManager.AddBattleCharacter(instance);
    }

    BattlePositionBlock FindBattlePosition(Vector3 pos)
    {
        var blocks = BattleBlockManager.Instance.GetBattlePositonBlocks();
        var minDist = float.MaxValue;
        BattlePositionBlock result = null;
        foreach (var block in blocks)
        {
            var dist = (pos - block.Position).sqrMagnitude;
            if (dist < minDist)
            {
                minDist = dist;
                result = block;
            }
        }
        return result;
    }
}

public class BattleStartConfig
{
    public List<BattleCharacter> Roles = new List<BattleCharacter>();

    public List<Vector2> Positions = new List<Vector2>(); // inital position for team

    public string Scene;
}