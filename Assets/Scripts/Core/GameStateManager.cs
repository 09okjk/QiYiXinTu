using System.Collections;
using System.Collections.Generic;
using Manager;
using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }
    
    
    // 用于存储游戏状态标志的字典
    private Dictionary<string, bool> gameFlags = new Dictionary<string, bool>();
    private int currentSaveSlot = -1; // 当前使用的存档槽位，默认为-1表示未选择
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

        #region 场景切换触发器-Flag
        
        gameFlags["canEnter_"+ "outside1_1"] = true; // 假设outside1_1是一个场景
        gameFlags["canEnter_"+ "In_LiDe_2"] = true; 
        gameFlags["canEnter_"+ "In_LiDe_3"] = true; 
        gameFlags["canEnter_" + "In_LiDe_4"] = true; // 假设In_LiDe_4是一个场景
        gameFlags["canEnter_" + "Space_Time"] = true; // 假设In_LiDe_4是一个场景
        gameFlags["canEnter_" + "DaYinDian"] = true; // 假设In_LiDe_4是一个场景
        gameFlags["canEnter_" + "DinnerHall"] = true; // 假设In_LiDe_4是一个场景

        #endregion

        #region 第一次进入关卡-Flag

        gameFlags["firstEntry_"+ "女生宿舍"] = true; // 假设女生宿舍是第一个关卡
        gameFlags["firstEntry_" + "outside1"] = true; // 假设outside1是第二个关卡
        gameFlags["firstEntry_" + "outside1_1"] = true; // 假设outside1是第二个关卡
        gameFlags["firstEntry_" + "In_LiDe"] = true; // 假设outside1是第二个关卡
        gameFlags["firstEntry_" + "In_LiDe_2"] = true; // 假设outside1是第二个关卡
        gameFlags["firstEntry_" + "In_LiDe_3"] = true; // 假设outside1是第二个关卡
        gameFlags["firstEntry_" + "In_LiDe_4"] = true; // 假设outside1是第二个关卡
        gameFlags["firstEntry_" + "Space_Time"] = true; // 假设outside1是第二个关卡
        
        #endregion

        #region 对话触发器—Flag

        gameFlags["canInteract_"+"fang_dialogue"] = true; 
        gameFlags["canInteract_"+"shi_dialogue"] = true; 
        gameFlags["canInteract_"+"li_dialogue"] = true; 
        gameFlags["canInteract_"+"zhang_dialogue"] = true; 
        gameFlags["canInteract_"+"xiao_dialogue"] = true; 
        // gameFlags["canInteract_"+"rift_1955_dialogue"] = true; 
        gameFlags["canInteract_"+"silence_dialogue"] = true; 
        
        gameFlags["canInteract_"+"quan-05-2"] = true; 
        gameFlags["canInteract_"+"chenyan-05-2"] = true; 
        gameFlags["canInteract_"+"xiaoxiao-05-3"] = true; 
        gameFlags["canInteract_"+"dayindian_dialogue"] = true; 
        
        // school_npc
        gameFlags["canInteract_"+"quschoolnpc_dialogue"] = true;
        gameFlags["canInteract_"+"ruischoolnpc_dialogue"] = true; 
        gameFlags["canInteract_"+"xingschoolnpc_dialogue"] = true; 
        gameFlags["canInteract_"+"jin_dayin"] = true; 
        
        #endregion

        #region 特殊触发器-Flag
        
        gameFlags["canInteract_"+"LuSleep"] = true; // LuSleep是一个对象的交互标志
        gameFlags["canInteract_"+"milk_tea"] = true; // milk_tea是一个对象的交互标志
        gameFlags["canInteract_"+"card"] = true; // milk_tea是一个对象的交互标志
        gameFlags["isNewGame"] = true; // 标志是否为新游戏
        
        #endregion
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
    
    public int GetCurrentSaveSlot()
    {
        var saveSlot = currentSaveSlot;
        currentSaveSlot = -1; // 重置当前存档槽位
        return saveSlot;
    }

    public void SetCurrentSaveSlot(int slot)
    {
        currentSaveSlot = slot;
    }
    
    // 获取标志值，如果标志不存在则返回false
    public bool GetFlag(string flagName)
    {
        if (gameFlags.TryGetValue(flagName, out bool value))
        {
            // Debug.Log($"GetFlag:{flagName}, Value:{value}");
            return value;
        }
        // Debug.Log($"GetFlag:{flagName}, Value:false (not found)");
        return false;
    }
    
    // 设置标志值
    public void SetFlag(string flagName, bool value)
    {
        Debug.Log($"SetFlag:{flagName}, Value:{value}");
        gameFlags[flagName] = value;
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
    public bool SetAllFlags(Dictionary<string, bool> flags)
    {
        if (flags == null)
        {
            Debug.LogWarning("Attempted to set game flags with a null dictionary.");
            return false;
        }
        gameFlags = new Dictionary<string, bool>(flags);
        return true;
    }
    
    // 清除所有标志
    public void ClearAllFlags()
    {
        gameFlags.Clear();
    }
}