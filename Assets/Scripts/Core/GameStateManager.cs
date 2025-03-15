using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }
    
    // Dictionary to store game state flags
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
    
    // Get a flag value, returns false if flag doesn't exist
    public bool GetFlag(string flagName)
    {
        if (gameFlags.TryGetValue(flagName, out bool value))
        {
            return value;
        }
        return false;
    }
    
    // Set a flag value
    public void SetFlag(string flagName, bool value)
    {
        gameFlags[flagName] = value;
    }
    
    // Toggle a flag value
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
    
    // Check if a flag exists
    public bool HasFlag(string flagName)
    {
        return gameFlags.ContainsKey(flagName);
    }
    
    // Remove a flag
    public void RemoveFlag(string flagName)
    {
        if (gameFlags.ContainsKey(flagName))
        {
            gameFlags.Remove(flagName);
        }
    }
    
    // Get all flags (used for saving)
    public Dictionary<string, bool> GetAllFlags()
    {
        return new Dictionary<string, bool>(gameFlags);
    }
    
    // Set all flags (used for loading)
    public void SetAllFlags(Dictionary<string, bool> flags)
    {
        gameFlags = new Dictionary<string, bool>(flags);
    }
    
    // Clear all flags
    public void ClearAllFlags()
    {
        gameFlags.Clear();
    }
}