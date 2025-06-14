namespace Core
{
    /// <summary>
    /// 数据重置接口 - 确保新游戏开始时所有数据都是干净的
    /// </summary>
    public interface IDataResettable
    {
        /// <summary>
        /// 重置所有数据到初始状态
        /// </summary>
        void ResetData();
        
        /// <summary>
        /// 检查数据是否已被修改
        /// </summary>
        /// <returns>如果数据被修改返回true</returns>
        bool IsDataModified();
    }
}