namespace infonetica.Model
{
    public class Workflow
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public Dictionary<int, State> States { get; set; } = new();
        public Dictionary<int, Action> Actions { get; set; } = new();
        public int CurrentStateId { get; set; }
        public int InitialStateId { get; set; }
        public int FinalStateId { get; set; }
        public List<int> StateHistory { get; set; } = new();
    }

}
