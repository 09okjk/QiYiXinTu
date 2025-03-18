# GameManager 中 Start 方法与 OnSceneLoaded 的详细解析

## Start() 方法的作用

`Start()` 方法在 GameManager 初始化时执行，主要完成两个重要任务：

1. **隐藏加载屏幕**
   ```csharp
   if (loadingScreen != null)
   {
       loadingScreen.SetActive(false);
   }
   ```
   这段代码确保游戏刚开始时不会显示加载界面。这很重要，因为加载界面应该只在场景转换过程中显示。

2. **注册场景加载事件监听**
   ```csharp
   SceneManager.sceneLoaded += OnSceneLoaded;
   ```
   这行代码为 Unity 的场景管理系统添加了一个事件监听。每当有新场景加载完成时，系统会自动调用 `OnSceneLoaded` 方法。

## OnSceneLoaded 方法的使用与工作原理

`OnSceneLoaded` 方法是一个事件回调函数，它在每次场景加载完成后自动执行。在代码中，它执行以下操作：

1. **隐藏加载界面**
   ```csharp
   if (loadingScreen != null)
   {
       loadingScreen.SetActive(false);
   }
   ```
   场景加载完成后，不再需要显示加载屏幕。

2. **初始化新场景**
   ```csharp
   InitializeScene(scene.name);
   ```
   调用 `InitializeScene` 方法，根据场景名称执行特定的初始化逻辑。

## 使用方式与工作流程

这套系统的工作流程是：

1. 当需要跳转到新场景时，调用 `GameManager.Instance.LoadScene("场景名")`
2. `LoadScene` 方法启动协程 `LoadSceneAsync`，显示加载界面并异步加载场景
3. 场景加载完成后，Unity 自动触发 `sceneLoaded` 事件
4. `OnSceneLoaded` 回调被执行，隐藏加载界面并初始化新场景

## 实际应用示例

```csharp
// 在游戏中某个按钮点击事件
public void OnStartGameButtonClicked()
{
    // 这会触发整个流程：显示加载界面 -> 加载场景 -> 场景加载完成 -> OnSceneLoaded 回调 -> 初始化场景
    GameManager.Instance.LoadScene("GameScene");
}
```

## 注意事项

1. **必须取消订阅**：在 `OnDestroy` 中，正确地取消了事件订阅，避免内存泄漏：
   ```csharp
   private void OnDestroy()
   {
       SceneManager.sceneLoaded -= OnSceneLoaded;
   }
   ```

2. **场景特定初始化**：在 `InitializeScene` 方法中，您可以为每个场景添加特定的初始化逻辑，例如放置玩家角色、初始化NPC等。

这种实现方式确保了场景加载过程的统一管理，并提供了集中处理场景初始化逻辑的机制，是游戏架构中的重要组成部分。