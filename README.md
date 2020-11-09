# RESTA
A simple command-line tool for automated testing of RESTfull APIs

## USAGE


Prepare the environment JSON file. The values property contains environment-specific values you can use in your scripts and data:
```json
{
  "title": "Environment Name",
  "values": {
    "url": "http://dummy.restapiexample.com/api",
    "key": "71884"
  }
}
```

Prepare the one or several test cases JSON files. You can use GET, POST, PUT, or DELETE request types:
```json
{
  "id": "CASEID",
  "title": "Test Case Title",
  "shared": {
    "header": {
      "Accept": "applicstion/json"
    }
    
  },

  "tasks": [
    {
      "id": "task-id",
      "title": "Task Title",
      "method": "GET",
      "url": "{{url}}/v1/employees",
      "assert": {
        "response": 200,
        "type": "text/html",
        "schema": "elist"
      },
      "read": {
        "locate": "[0].id",
        "target": "employee"
      }
    }
  ]
}
```

Prepare the runbook JSON file that refers one or several use cases:
```json
{
  "title": "Runbook title",
  "environment": "env-id",
  "scripts": [
    "case1", "case2", ...
  ]
}
```

Execute the test script:
```
resta path/runbook.json" -out:path/results -sc:path/schema -in:path/data -keep
```

For each test case it will create file called `api-{case-id}-{task-id}.json`.

Here are the command-line syntax:

```
resta runbook [options]

OPTIONS:

  -out:path     Specify the path to store output files

  -in:path      Specify the path where to get the input files 

  -sc:path      Specify the path where to get the schema files. Every schema file name should be `schema-{id}.json`

  -keep         To keep the result file for successful tests.
```



Another option for task:

```json
{
  "id": "task-id",
  "title": "Task Title",
  "method": "GET",
  "url": "{{url}}/v1/employees",
  "assert": {
    "responses": [200, 206],
    "type": "text/html",
    "schema": "elist"
  },
  "read": {
    "locate": "[0].id",
    "target": "employee"
  }
}
```

