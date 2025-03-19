using System.Collections;
using System.Collections.Generic;
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
        gameFlags = new Dictionary<string, bool>(flags);
    }
    
    // 清除所有标志
    public void ClearAllFlags()
    {
        gameFlags.Clear();
    }
}