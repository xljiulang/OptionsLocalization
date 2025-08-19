## OptionsLocalization
小有意思的一个项目，使用 json 配置文件做 [Options](https://learn.microsoft.com/zh-cn/dotnet/core/extensions/options) 本地化

### 使用步骤
#### 1 定义你的选项
```c#
public class AppOptions
{
    public string Key1 { get; set; } = "Hello";

    public string Key2 { get; set; } = "World";

    public Dictionary<string, string> DynamicData { get; set; } = new Dictionary<string, string>
    {
        { "Data1", "hello" },
        { "Data2", "world" }        
    };
}
```

#### 2 服务注册
```c#
static void Main(string[] args)
{
    var builder = WebApplication.CreateBuilder(args);

    // 添加本地化工具，默认语言区域为 "en"
    builder
        .AddOptionsLocalizer("en", ["en", "fr", "zh"])
        .Configure<AppOptions>();

    builder.Services.AddControllers();
    var app = builder.Build();

    // 中间件设置当前线程的语言区域
    app.Use(next => context =>
    {
        context.Request.Query.TryGetValue("culture", out var culture);
        OptionsLocalizer.SetCurrentThreadCulture(culture, "en");
        return next(context);
    });

    app.MapControllers();
    app.Run();
} 
```


#### 3 本地化数据注入
```c#
public class HomeController
{
    /// <summary>
    /// 本地化工具示例
    /// </summary>
    /// <param name="appOptions">当前线程对应的本地化选项</param>
    /// <param name="localizer">本地化工具</param>
    /// <returns></returns>
    [HttpGet("/")]
    public Locale Index(
        [FromServices] AppOptions appOptions,
        [FromServices] IOptionsLocalizer<AppOptions> localizer)
    {
        Debug.Assert(appOptions == localizer.CurrentValue);
        return appOptions;
    }
}
```

#### 4 添加语言文件
1. 新建`localizations`目录
2. 新建`AppOptions`子目录(保持与选项类型名一致)
3. 新建`zh.json`、`zh-CN.json`等包含`{}` json 结构的语言文件
4. 启动运行你的项目，在`AppOptions`子目录找到`.value`文件，粘贴到对应的`.json`语言文件并翻译修改
