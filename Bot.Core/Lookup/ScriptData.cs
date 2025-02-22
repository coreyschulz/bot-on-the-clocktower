namespace Bot.Core.Lookup
{
    public class ScriptData
    {
        public string Name { get; }
        public bool IsOfficial { get; }
        public string? AlmanacUrl { get; set; }
        public string? Author { get; set; }
        public ScriptData(string name, bool isOfficial)
        {
            Name = name;
            IsOfficial = isOfficial;
        }

        public override string ToString() => $"{Name} {(IsOfficial ? "(official)" : "(custom)")}";
    }
}
