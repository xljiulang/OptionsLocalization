namespace OptionsLocalization
{
    /// <summary>
    /// 本地化选项构建器接口
    /// </summary>
    public interface IOptionsLocalizerBuilder
    {
        /// <summary>
        /// 绑定配置到到指定的选项类型
        /// </summary>
        /// <typeparam name="TOptions">选项类型</typeparam>
        /// <returns></returns>
        IOptionsLocalizerBuilder Configure<TOptions>() where TOptions : class, new();
    }
}
