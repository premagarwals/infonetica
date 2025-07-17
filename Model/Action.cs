namespace infonetica.Model
{
    public class Action
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public HashSet<int> FromStatesIDs { get; set; } = new();
        public int ToStateId { get; set; }
    }

}
