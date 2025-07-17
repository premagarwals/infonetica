# Infonetica Task — Configurable Workflow Engine API

A lightweight and extensible .NET 8 Web API for defining and managing state-based workflows. Ideal for systems requiring dynamic state transitions—such as order processing, approvals, and custom pipelines—this engine supports runtime modification of workflows, actions, and states via clean HTTP endpoints.

---

## Project Structure

```

infonetica.sln
└── infonetica/
├── Program.cs        # Minimal API routes
├── Services/         # Main engine logic
│   └── WorkflowService.cs
├── Model/            # Data models (Workflow, State, Action)
├── workflows.json    # Persistent store (JSON-based)
└── infonetica.csproj

````

---

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)

### Running the API

```sh
git clone https://github.com/premagarwals/infonetica.git
cd infonetica
dotnet run
````

By default, Swagger UI is available at:

```
https://localhost:<PORT>/swagger
```

---

## API Endpoints

Routes are grouped under `/workflows`.

| Endpoint                                  | HTTP Method | Description                                 |
| ----------------------------------------- | ----------- | ------------------------------------------- |
| `/workflows`                              | POST        | Create a workflow                           |
| `/workflows`                              | GET         | Retrieve all workflows                      |
| `/workflows/{id}`                         | GET         | Retrieve a workflow by ID                   |
| `/workflows/{id}/execute/{actionId}`      | POST        | Execute an action—transition workflow state |
| `/workflows/{id}/states`                  | POST        | Add a new state to a workflow               |
| `/workflows/{id}/states/{stateId}/toggle` | PUT         | Enable or disable a workflow state          |
| `/workflows/{id}/actions`                 | POST        | Add an action with a single source state    |

---

## Sample Requests

### Create a Workflow

```json
POST /workflows
{
  "id": 1,
  "name": "OrderProcess",
  "states": {},
  "actions": {}
}
```

### Add a State

```json
POST /workflows/1/states
{
  "id": 10,
  "name": "Order Placed",
  "isInitial": true,
  "isFinal": false,
  "enabled": true
}
```

### Add an Action

```json
POST /workflows/1/actions
{
  "action": {
    "id": 100,
    "name": "Confirm Payment",
    "toStateId": 20
  },
  "fromStateId": 10
}
```

### Execute an Action

```
POST /workflows/1/execute/100
```

---

## App Robustness: Graceful Handling

* Prevents duplicate workflow, state, and action IDs
* Validates source state presence on action execution
* Enforces enabled state before transitions
* Supports dynamic toggling of state availability
* Safeguards against invalid transitions (e.g. final states)
* Uses JSON fallback for file-based persistence
* Separates serialization logic from HTTP-level concerns

---

## My Experience

I have never used VisualStudio/.NET/C#, as I mostly used C++/Python/Rust. Hence, I faced a lot of challenge building this project.

#### My approach:

I first created a following C++ code to create the core logic for the project. You can follow the below c++ code to go through the actual logic of my project.

```cpp
#include <bits/stdc++.h>
using namespace std;

class State {
public:
    int id;
    string name;
    bool isFinal, isInitial;
    bool enabled;

    State(int id, string name, bool isFinal, bool isInitial) : id(id), name(name), isFinal(isFinal), isInitial(isInitial), enabled(true) {};

    void disable() {
        enabled = false;
    }

    void enable() {
        enabled = true;
    }
};

class Action{
public:
    int id;
    string name;
    unordered_set<int> fromStatesIDs;
    State* toState;

    Action(int id, string name, unordered_set<int> fromStates, State* toState) : id(id), name(name), fromStatesIDs(fromStates), toState(toState) {};

    State* transition (State* from) {
        if (fromStatesIDs.find(from->id) != fromStatesIDs.end()) {
            if (toState->enabled) {
                return toState;
            } else {
                throw invalid_argument("Target state is disabled");
            }
        } else {
            return nullptr;
        }
    }
};

class Workflow{
public:
    int id;
    string name;
    unordered_map<int, State*> statesMap;
    unordered_map<int, Action*> actionsMap;

    State* currentState;

    State* initialState;
    State* finalState = new State(-1, "Final", false, true);
    vector<State*> statesHistory;

    Workflow(int id, string name, vector<pair<string,int>> states, int initID, int finalID, vector<pair<pair<string,int>, pair<int, int>>> actions) {
        this->name = name;
        this->id = id;

        for (int i = 0; i < states.size(); ++i) {
            int stateID = states[i].second;
            string stateName = states[i].first;
            bool isFinal = (stateID == finalID);
            bool isInitial = (stateID == initID);

            addState(stateID, stateName, isFinal, isInitial);
        }

        for (int i = 0; i < actions.size(); ++i) {
            int actionID = actions[i].first.second;
            string actionName = actions[i].first.first;

            int fromState = actions[i].second.first;
            int toState = actions[i].second.second;

            addAction(actionID, actionName, fromState, toState);
        }

        currentState = statesMap[initID];
        initialState = statesMap[initID];
        finalState = statesMap[finalID];
        statesHistory.push_back(currentState);
    }

    void addAction(int actionID, string actionName, int fromState, int toState) {
        if (fromState == finalState->id || fromState == toState) {
            throw invalid_argument("Cannot add action involving given state");
        }
        if (actionsMap.find(actionID) != actionsMap.end()) {
            if (actionsMap[actionID]->toState->id != toState) {
                throw invalid_argument("Action already exists with different target state");
            }
            actionsMap[actionID]->fromStatesIDs.insert(fromState);
        } else {
            if (statesMap.find(fromState) == statesMap.end() || statesMap.find(toState) == statesMap.end()) {
                throw invalid_argument("Invalid state ID in action definition");
            }
            State* toStateObj = statesMap[toState];
            Action* action = new Action(actionID, actionName, {fromState}, toStateObj);
            actionsMap[actionID] = action;
        }
    }

    void addState(int id, string name, bool isFinal, bool isInitial) {
        if (statesMap.find(id) != statesMap.end()) {
            throw invalid_argument("State with this ID already exists");
        }
        State* state = new State(id, name, isFinal, isInitial);
        statesMap[id] = state;
    }

    void disableState(int id) {
        if (statesMap.find(id) == statesMap.end()) {
            throw invalid_argument("State with this ID does not exist");
        }
        statesMap[id]->disable();
    }

    void enableState(int id) {
        if (statesMap.find(id) == statesMap.end()) {
            throw invalid_argument("State with this ID does not exist");
        }
        statesMap[id]->enable();
    }

    void performAction(int actionID) {
        if (actionsMap.find(actionID) == actionsMap.end()) {
            throw invalid_argument("Action with this ID does not exist");
        }
        Action* action = actionsMap[actionID];
        State* nextState = action->transition(currentState);
        if (nextState == nullptr) {
            throw invalid_argument("Given action cannot be performed");
        }
        currentState = nextState;
        statesHistory.push_back(currentState);
    }

    void printCurrentState() {
        cout << "Current State: " << currentState->name << endl;
    }

    void printStatesHistory() {
        cout << "States History: ";
        for (State* state : statesHistory) {
            cout << state->name << " ";
        }
        cout << endl;
    }

    void describe() {
        cout << "Workflow ID: " << id << ", Name: " << name << endl;
        cout << "Initial State: " << (initialState ? initialState->name : "None") << endl;
        cout << "Final State: " << (finalState ? finalState->name : "None") << endl;
        cout << "States:" << endl;
        for (const auto& pair : statesMap) {
            State* state = pair.second;
            cout << "  ID: " << state->id << ", Name: " << state->name 
                << ", Final: " << (state->isFinal ? "Yes" : "No") 
                << ", Initial: " << (state->isInitial ? "Yes" : "No") 
                << ", Enabled: " << (state->enabled ? "Yes" : "No") << endl;
        }
        cout << "Actions:" << endl;
        for (const auto& pair : actionsMap) {
            Action* action = pair.second;
            cout << "  ID: " << action->id << ", Name: " << action->name 
                << ", From States: ";
            for (int fromID : action->fromStatesIDs) {
                cout << fromID << " ";
            }
            cout << ", To State ID: " << action->toState->id 
                << ", To State Name: " << action->toState->name << endl;
        }
    }

    bool isRunning() {
        return currentState != finalState;
    }
};

int main() {
    
    vector<pair<string, int>> states = {
        {"Order Placed", 1},
        {"Payment Confirmed", 2},
        {"Packed", 3},
        {"Shipped", 4},
        {"Out for Delivery", 5},
        {"Delivered", 6},
        {"Cancelled", 7}
    };

    vector<pair<pair<string,int>, pair<int,int>>> actions = {
        {{"Confirm Payment", 101}, {1, 2}},
        {{"Pack Order", 102}, {2, 3}},
        {{"Ship Order", 103}, {3, 4}},
        {{"Send Out for Delivery", 104}, {4, 5}},
        {{"Mark Delivered", 105}, {5, 6}},
        {{"Cancel Order", 106}, {1, 7}},
        {{"Cancel Order", 106}, {2, 7}},
        {{"Cancel Order", 106}, {3, 7}},
        {{"Cancel Order", 106}, {4, 7}},
        {{"Cancel Order", 106}, {5, 7}},
    };

    Workflow deliveryWorkflow(1, "Delivery Workflow", states, 1, 6, actions);

    try {
        deliveryWorkflow.printCurrentState();
        deliveryWorkflow.performAction(101); 
        deliveryWorkflow.performAction(102); 
        deliveryWorkflow.performAction(103); 
        deliveryWorkflow.performAction(104); 
        deliveryWorkflow.performAction(105); 
        deliveryWorkflow.printCurrentState();
    } catch (const exception& e) {
        cout << "Error: " << e.what() << endl;
    }
    cout << endl;

    return 0;
}
```

I also had very good understanding of API calls and other backend stuffs hence the only thing I needed was to learn the basics of the language and some knowledge about dotnet framework. After coming up with the above logic, I started surfing the internet to gain some language specific knowledge. After a while, I felt comfortable to use the tool (Visual Studio) and C#. Still I struggled at some points and I took help of internet to convert my C++ code to C# without losing the core logic.

After all of these stuffs, I only had to build the endpoints and that's it.

**End Note:** I usually commit at each small checkpoint, but I was somewhat overwhelmed during this project hence I failed to create a good commit history :(

---

## Future Enhancements

* Persist data in SQLite / LiteDB or a relational database
* Add access control on action execution
* Provide an interactive web-UI for workflow design

---

## License

Licensed under the [MIT License](https://opensource.org/licenses/MIT)
