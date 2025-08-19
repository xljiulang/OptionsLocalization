using System.Collections.Generic;

namespace WebApp
{
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
}
