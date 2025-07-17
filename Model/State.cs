namespace infonetica.Model
{
    public class State
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public bool IsFinal { get; set; }
        public bool IsInitial { get; set; }
        public bool Enabled { get; set; } = true;
    }
}
