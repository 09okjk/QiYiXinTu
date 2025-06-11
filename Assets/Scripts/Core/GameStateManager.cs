using System.Collections;
using System.Collections.Generic;
using Manager;
using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }
    
    
    // 用于存储游戏状态标志的字典
    private Dictionary<string, bool> gameFlags = new Dictionary<string, bool>();
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        InitializeGameFlags();
    }

    private void InitializeGameFlags()
    {
        // 初始化游戏状态标志
        gameFlags["FirstEntry_"+ "女生宿舍"] = true; // 假设女生宿舍是第一个关卡
        gameFlags["CanInteract_"+"LuSleep"] = true; // LuSleep是一个对象的交互标志
        gameFlags["FirstEntry_" + "outside1"] = true; // 假设outside1是第二个关卡
    }

    public void SetPlayerPointType(PlayerPointType pointType)
    {
        // 使用Unity内置的PlayerPrefs来存储玩家的出生点类型
        Debug.Log($"SetPlayerPointType:{pointType}");
        PlayerPrefs.SetInt("PlayerPointType", (int)pointType);
        
    }
    
    // 获取当前玩家的出生点类型
    public PlayerPointType GetPlayerPointType()
    {
        // 使用Unity内置的PlayerPrefs来获取玩家的出生点类型
        if (PlayerPrefs.HasKey("PlayerPointType"))
        {
            return (PlayerPointType)PlayerPrefs.GetInt("PlayerPointType");
        }
        return PlayerPointType.Right;
    }
    
    // 获取标志值，如果标志不存在则返回false
    public bool GetFlag(string flagName)
    {
        if (gameFlags.TryGetValue(flagName, out bool value))
        {
            return value;
        }
        return false;
    }
    
    // 设置标志值
    public void SetFlag(string flagName, bool value)
    {
        gameFlags[flagName] = value;
    }
    
    // 切换标志值
    public void ToggleFlag(string flagName)
    {
        if (gameFlags.TryGetValue(flagName, out bool value))
        {
            gameFlags[flagName] = !value;
        }
        else
        {
            gameFlags[flagName] = true;
        }
    }
    
    // 检查标志是否存在
    public bool HasFlag(string flagName)
    {
        return gameFlags.ContainsKey(flagName);
    }
    
    // 移除标志
    public void RemoveFlag(string flagName)
    {
        if (gameFlags.ContainsKey(flagName))
        {
            gameFlags.Remove(flagName);
        }
    }
    
    // 获取所有标志值（用于保存）
    public Dictionary<string, bool> GetAllFlags()
    {
        return new Dictionary<string, bool>(gameFlags);
    }
    
    // 设置所有标志（用于加载）
    public void SetAllFlags(Dictionary<string, bool> flags)
    {
        if (flags == null)
        {
            Debug.LogWarning("Attempted to set game flags with a null dictionary.");
            return;
        }
        gameFlags = new Dictionary<string, bool>(flags);
    }
    
    // 清除所有标志
    public void ClearAllFlags()
    {
        gameFlags.Clear();
    }
}