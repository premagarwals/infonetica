using System.Text.Json;
using infonetica.Model;

namespace infonetica.Services
{
    // Helper class for JSON deserialization
    public class WorkflowDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int InitialStateId { get; set; }
        public int FinalStateId { get; set; }
        public int CurrentStateId { get; set; }
        public Dictionary<int, State> States { get; set; } = new();
        public List<ActionDto> Actions { get; set; } = new();
        public List<int> StateHistory { get; set; } = new();
    }

    public class ActionDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int FromStateId { get; set; }
        public int ToStateId { get; set; }
    }

    public class WorkflowService
    {
        private readonly string _filePath = "workflows.json";
        private Dictionary<int, Workflow> workflows = new();

        public WorkflowService()
        {
            Load();
        }

        private void Load()
        {
            if (File.Exists(_filePath))
            {
                var json = File.ReadAllText(_filePath).Trim();
                if (!string.IsNullOrEmpty(json))
                {
                    workflows = JsonSerializer.Deserialize<Dictionary<int, Workflow>>(json)
                                ?? new Dictionary<int, Workflow>();
                }
            }
        }

        private void Save()
        {
            var json = JsonSerializer.Serialize(workflows, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_filePath, json);
        }

        public List<Workflow> GetAll() => workflows.Values.ToList();

        public Workflow? Get(int id) => workflows.TryGetValue(id, out var wf) ? wf : null;

        // Method to create workflow from JSON with array format
        public Workflow CreateFromJson(string json)
        {
            var workflowDto = JsonSerializer.Deserialize<WorkflowDto>(json);
            if (workflowDto == null)
                throw new Exception("Invalid JSON format");

            var workflow = new Workflow
            {
                Id = workflowDto.Id,
                Name = workflowDto.Name,
                InitialStateId = workflowDto.InitialStateId,
                FinalStateId = workflowDto.FinalStateId,
                CurrentStateId = workflowDto.CurrentStateId,
                States = workflowDto.States,
                StateHistory = workflowDto.StateHistory,
                Actions = new Dictionary<int, infonetica.Model.Action>()
            };

            return CreateWorkflowWithActions(workflow, workflowDto.Actions);
        }

        public Workflow Create(Workflow workflow)
        {
            if (workflows.ContainsKey(workflow.Id))
                throw new Exception("Workflow ID already exists");

            var actionsFromJson = workflow.Actions;
            workflow.Actions = new Dictionary<int, infonetica.Model.Action>();

            workflows[workflow.Id] = workflow;

            var actionDtos = new List<ActionDto>();
            foreach (var kvp in actionsFromJson)
            {
                var action = kvp.Value;
                foreach (var fromStateId in action.FromStatesIDs)
                {
                    actionDtos.Add(new ActionDto
                    {
                        Id = action.Id,
                        Name = action.Name,
                        FromStateId = fromStateId,
                        ToStateId = action.ToStateId
                    });
                }
            }

            return CreateWorkflowWithActions(workflow, actionDtos);
        }

        private Workflow CreateWorkflowWithActions(Workflow workflow, List<ActionDto> actionDtos)
        {
            // Group actions by ID to handle multiple fromStateId values
            var actionGroups = actionDtos.GroupBy(a => a.Id);

            foreach (var group in actionGroups)
            {
                var actionId = group.Key;
                var transitions = group.ToList();

                // Validate that all transitions for this action have the same name and toStateId
                var firstTransition = transitions.First();
                if (transitions.Any(t => t.Name != firstTransition.Name || t.ToStateId != firstTransition.ToStateId))
                {
                    throw new Exception($"Action {actionId} has conflicting properties");
                }

                // Create the action with all fromStateIds
                var action = new infonetica.Model.Action
                {
                    Id = actionId,
                    Name = firstTransition.Name,
                    ToStateId = firstTransition.ToStateId,
                    FromStatesIDs = new HashSet<int>(transitions.Select(t => t.FromStateId))
                };

                // Validate states exist
                if (!workflow.States.ContainsKey(action.ToStateId))
                {
                    throw new Exception($"Target state {action.ToStateId} not found for action {actionId}");
                }

                foreach (var fromStateId in action.FromStatesIDs)
                {
                    if (!workflow.States.ContainsKey(fromStateId))
                    {
                        throw new Exception($"From state {fromStateId} not found for action {actionId}");
                    }

                    // Validate action logic
                    if (fromStateId == action.ToStateId || fromStateId == workflow.FinalStateId)
                    {
                        throw new Exception($"Invalid action definition for action {actionId}");
                    }
                }

                workflow.Actions[actionId] = action;
            }

            workflows[workflow.Id] = workflow;
            Save();
            return workflow;
        }

        public void PerformAction(int wfId, int actionId)
        {
            if (!workflows.TryGetValue(wfId, out var wf))
                throw new Exception("Workflow not found");

            if (!wf.Actions.TryGetValue(actionId, out var action))
                throw new Exception("Action not found");

            if (!action.FromStatesIDs.Contains(wf.CurrentStateId))
                throw new Exception("Action not valid from current state");

            var toState = wf.States[action.ToStateId];
            if (!toState.Enabled)
                throw new Exception("Target state is disabled");

            wf.CurrentStateId = toState.Id;
            wf.StateHistory.Add(toState.Id);
            Save();
        }

        public void AddState(int wfId, State state)
        {
            var wf = Get(wfId) ?? throw new Exception("Workflow not found");

            if (wf.States.ContainsKey(state.Id))
                throw new Exception("State ID already exists");

            wf.States[state.Id] = state;
            Save();
        }

        public void ToggleState(int wfId, int stateId, bool enable)
        {
            var wf = Get(wfId) ?? throw new Exception("Workflow not found");

            if (!wf.States.TryGetValue(stateId, out var state))
                throw new Exception("State not found");

            state.Enabled = enable;
            Save();
        }

        public void AddAction(int wfId, infonetica.Model.Action action, int fromState)
        {
            var wf = Get(wfId) ?? throw new Exception("Workflow not found");

            if (action.ToStateId == fromState || fromState == wf.FinalStateId)
                throw new Exception("Invalid action definition");

            if (!wf.States.ContainsKey(fromState) || !wf.States.ContainsKey(action.ToStateId))
                throw new Exception("Invalid state ID");

            if (wf.Actions.TryGetValue(action.Id, out var existing))
            {
                if (existing.ToStateId != action.ToStateId)
                    throw new Exception("Action ID conflict with different target state");

                existing.FromStatesIDs.Add(fromState);
            }
            else
            {
                action.FromStatesIDs = new HashSet<int> { fromState };
                wf.Actions[action.Id] = action;
            }

            Save();
        }
    }
}