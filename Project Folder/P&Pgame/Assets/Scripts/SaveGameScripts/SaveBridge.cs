using UnityEngine;

public class SaveBridge : MonoBehaviour
{
    public void ApplyLoadedData()
    {
        if (!GameSession.instance) 
            { return; }

        var data = GameSession.instance.Data;


        //Player
        var ps = gameManager.instance.playerScript;
        if (ps)
        {
            ps.HP = data.playerHp;
            ps.woodCount = data.wood;
            ps.stoneCount = data.stone;
            ps.ApplyMiningSpeedBoost(data.PlayerMiningSpeedBoost);
            ps.ApplySpeedBoost(data.PlayerSpeedBoost);
            ps.ApplyJumpBoost(data.PlayerJumpBoost);
            ps.updatePlayerUI();
            gameManager.instance.updateResourcesUI();
        }

        //Gold
        gameManager.instance.SetGold(data.gold);

        GameData.instance.PlayerJumpBoost = data.PlayerJumpBoost;

        GameData.instance.PlayerSpeedBoost = data.PlayerSpeedBoost;

        GameData.instance.PlayerMiningSpeedBoost = data.PlayerMiningSpeedBoost;


    }

    public void CollectAndSave()
    {
        if (!GameSession.instance) 
            { return; };

        var data = GameSession.instance.Data;

        var ps = gameManager.instance.playerScript;

        if (ps)
        {
            data.playerHp = ps.HP;
            data.wood = ps.woodCount;
            data.stone = ps.stoneCount;
        }

        data.gold = gameManager.instance.GetGold();
        data.PlayerJumpBoost = GameData.instance.PlayerJumpBoost;
        data.PlayerSpeedBoost = GameData.instance.PlayerSpeedBoost;
        data.PlayerMiningSpeedBoost = GameData.instance.PlayerMiningSpeedBoost;

        GameSession.instance.SaveGame();
    }
}
