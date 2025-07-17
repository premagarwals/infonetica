# Infonetica Task Workflow API - Test JSONs

This document includes JSON payloads and test case descriptions for validating various scenarios of the workflow engine. Use these with Swagger UI to manually verify robustness.

---

## TEST 1: Valid Workflow Execution

**Endpoint:** `POST /workflows`  
**Method:** `POST`

```json
{
  "id": 1,
  "name": "Delivery Workflow",
  "states": {
    "1": { "id": 1, "name": "Order Placed", "isInitial": true, "isFinal": false, "enabled": true },
    "2": { "id": 2, "name": "Payment Confirmed", "isInitial": false, "isFinal": false, "enabled": true },
    "3": { "id": 3, "name": "Packed", "isInitial": false, "isFinal": false, "enabled": true },
    "4": { "id": 4, "name": "Shipped", "isInitial": false, "isFinal": false, "enabled": true },
    "5": { "id": 5, "name": "Out for Delivery", "isInitial": false, "isFinal": false, "enabled": true },
    "6": { "id": 6, "name": "Delivered", "isInitial": false, "isFinal": true, "enabled": true },
    "7": { "id": 7, "name": "Cancelled", "isInitial": false, "isFinal": false, "enabled": true }
  },
  "actions": {
    "101": { "id": 101, "name": "Confirm Payment", "fromStatesIDs": [1], "toStateId": 2 },
    "102": { "id": 102, "name": "Pack Order", "fromStatesIDs": [2], "toStateId": 3 },
    "103": { "id": 103, "name": "Ship Order", "fromStatesIDs": [3], "toStateId": 4 },
    "104": { "id": 104, "name": "Send Out for Delivery", "fromStatesIDs": [4], "toStateId": 5 },
    "105": { "id": 105, "name": "Mark Delivered", "fromStatesIDs": [5], "toStateId": 6 },
    "106": { "id": 106, "name": "Cancel Order", "fromStatesIDs": [1, 2, 3, 4, 5], "toStateId": 7 }
  },
  "currentStateId": 1,
  "initialStateId": 1,
  "finalStateId": 6,
  "stateHistory": [1]
}
````

---

## TEST 2: Invalid Action ID

**Endpoint:** `POST /workflows/1/execute/999`
**Method:** `POST`

*No JSON body required.*

---

## TEST 3: Action from Wrong State

**Endpoint:** `POST /workflows/2/execute/102`
**Method:** `POST`

*This should throw since action 102 cannot be executed from the initial state.*

---

## TEST 4: Adding Duplicate State

**Endpoint:** `POST /workflows/2/states`
**Method:** `POST`

```json
{
  "id": 1,
  "name": "Duplicate Order Placed",
  "isInitial": false,
  "isFinal": false,
  "enabled": true
}
```

---

## TEST 5: Disabling Non-existent State

**Endpoint:** `PUT /workflows/2/states/999/toggle`
**Method:** `PUT`

*No JSON body required.*

---

## TEST 6: Enabling Non-existent State

**Endpoint:** `PUT /workflows/2/states/999/toggle`
**Method:** `PUT`

*No JSON body required.*

---

## TEST 7: Constructor with Invalid State IDs in Actions

**Endpoint:** `POST /workflows`
**Method:** `POST`

```json
{
  "id": 3,
  "name": "Bad Workflow",
  "states": {
    "1": { "id": 1, "name": "Valid State", "isInitial": true, "isFinal": false, "enabled": true },
    "2": { "id": 2, "name": "Another Valid", "isInitial": false, "isFinal": true, "enabled": true }
  },
  "actions": {
    "201": { "id": 201, "name": "Invalid Action", "fromStatesIDs": [999], "toStateId": 2 }
  },
  "currentStateId": 1,
  "initialStateId": 1,
  "finalStateId": 2,
  "stateHistory": [1]
}
```

---

## TEST 8: Adding Action with Invalid States

**Endpoint:** `POST /workflows/2/actions`
**Method:** `POST`

```json
{
  "action": {
    "id": 202,
    "name": "Invalid Action",
    "toStateId": 2
  },
  "fromStateId": 999
}
```

---

## TEST 9: Adding Action from Final State

**Endpoint:** `POST /workflows/2/actions`
**Method:** `POST`

```json
{
  "action": {
    "id": 203,
    "name": "Final State Action",
    "toStateId": 2
  },
  "fromStateId": 6
}
```

---

## TEST 10: Adding Action with Same From and To State

**Endpoint:** `POST /workflows/2/actions`
**Method:** `POST`

```json
{
  "action": {
    "id": 204,
    "name": "Self Loop",
    "toStateId": 2
  },
  "fromStateId": 2
}
```

---

## TEST 11: Adding Action with Existing ID but Different Target

**Endpoint:** `POST /workflows/2/actions`
**Method:** `POST`

```json
{
  "action": {
    "id": 106,
    "name": "Conflict Cancel",
    "toStateId": 3
  },
  "fromStateId": 2
}
```

---

## TEST 12: Valid State Operations

**Endpoint:** `POST /workflows/2/states`
**Method:** `POST`

```json
{
  "id": 10,
  "name": "New State",
  "isInitial": false,
  "isFinal": false,
  "enabled": true
}
```

---

**Toggle State**

**Endpoint:** `PUT /workflows/2/states/10/toggle`
**Method:** `PUT`

*No JSON body required.*

---

**Add Action to New State**

**Endpoint:** `POST /workflows/2/actions`
**Method:** `POST`

```json
{
  "action": {
    "id": 205,
    "name": "To New State",
    "toStateId": 10
  },
  "fromStateId": 2
}
```

---

## TEST 13: Cancel After Partial Transition

**Endpoint:** `POST /workflows/4/execute/106`
**Method:** `POST`

*No JSON body required.*

---

## TEST 14: Final State Reached and History Check (Visual)

**Endpoint:** `GET /workflows/1` or view in Swagger
**Method:** `GET`

*No JSON body required.*