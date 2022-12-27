# RESTA SCRIPTS



### Environment

To run the tests you will need at least one environment file. The environment file name should be in the following format:  `env-{name}.json` where `name` is the name of the environment. For example, `env-uat.json` is the file for the **uat** environment. The environment file has the following structure:

```json
{
	"title": "Environment title",
	"values": {
		"key1": "value one",
		"key2": "value two",
    
		"keyN": "value N"
	}
}
```

You can create as many environments as you like. The values property contains environment-specific values you can use in your scripts and data. Here is an example of the environment file:

```json
{
  "title": "Cloud QA",
  "values": {
    "url": "http://dummy.restapiexample.com/api",
    "key": "75486571884"
  }
}
```



#### Using Variables

Before RESTA executes a script, it creates the context – an in-memory dictionary of values that all script tasks can use. Before the first task starts, the context is populated by the variables loaded from the environment file. Tasks can use the **read** section to update variables in the context.

RESTA uses mustaches syntax for variables. To use the context value in a string, enter the variable name in the double curly brackets. Before executing the task, RESTA will replace variables with actual values. For example:

```json
{
   "url": "{{baseurl}}/api/auth"
}
```

If the context has a variable called **baseurl** and its value is `http://localhost:8080`, the RESTA execute the task that looks like this:

```json
{
   "url": "http://localhost:8080/api/auth"
}
```

Since version 1.1.12 RESTA also supports the following dynamic variables:

| Variable name | Description                                         | Example                              |
| ------------- | --------------------------------------------------- | ------------------------------------ |
| $guid         | Generates a GUID (Globally Unique Identifier) in v4 | 15aacbb1-1615-47d8-b001-e5411a044761 |
| $timestamp    | Returns the current timestamp                       | 1561013396                           |
| $randomInt    | Generates random integer between 0 and 1000         | 764                                  |



### Schemas

File name `shema-{name}.json`. For example, `schema-object.json` is the schema file you are referring to as “object”.

```json
{
  "description": "Any Object",
  "type": "object"
}
```

You can create as many schema files as you like. You do not need to create file for default schemas `object` and `array`.



### Scripts

RESTA script is a JSON file that contains one or several tasks (API calls). The script file name must be in the following format: `script-{name}.json`. For example, the `script-smoke-test.json` file contains the script with the ID “smoke-test”. The script file has the following structure:

```json
{
	"id": "{script-id}",
	"title": "{script-title}",
	"shared": {
    "timeout": {timeout},
		"header": {
			"Accept": "applicstion/json"
		}
	},

	"tasks": [
		{ ...task 1 },
		{ ...task 2 },
    ...
		{ ...task N }
	]
}
```

The **shared** section contains features that will apply to every task. For example, all key-value pairs specified in the shared header will be added to the HTTP header of every API call.

The **tasks** array contains the list of API calls. The script is an environment-agnostic flow that executes all tasks sequentially. Use script as a functional test of a particular scenario or a use case. You will be able to run multiple scripts together as defined in the runbook (see below).



#### Tasks

Every task in the **tasks** array represents a single REST API call. Here is the structure of the task object:

```json
{
	"id": "{task-id}",
	"disabled": true|false,
	"title": "{task-title}",
	"method": "GET|POST|PUT|DELETE",
	"url": "{full-path-of-the-api}",
  "timeout": {timeout},
  "wait": {pause},
  "header": {},
  "body": {}
	...segments...
}
```

Here are the primary properties of the task object:

* The `id` property is a unique ID of the task in the script.
* The `title` property is a text that is displayed on the screen and in the test report.
* The `disabled` property is a flag that can be used to skip some tasks during script execution.
* The `method` property indicates the HTTP method of the request. RESTA supports [GET](https://developer.mozilla.org/en-US/docs/Web/HTTP/Methods/GET), [POST](https://developer.mozilla.org/en-US/docs/Web/HTTP/Methods/POST), [PUT](https://developer.mozilla.org/en-US/docs/Web/HTTP/Methods/PUT), [DELETE](https://developer.mozilla.org/en-US/docs/Web/HTTP/Methods/DELETE), [HEAD](https://developer.mozilla.org/en-US/docs/Web/HTTP/Methods/HEAD), [OPTIONS](https://developer.mozilla.org/en-US/docs/Web/HTTP/Methods/OPTIONS), [PATCH](https://developer.mozilla.org/en-US/docs/Web/HTTP/Methods/PATCH), MERGE, and COPY .   
* The `url` property contains the URL of the API request.
* The `timeout` property specify the call time-out in milliseconds. The default time-out is 5 seconds.
* The `wait` property sets a delay in milliseconds. If more than zero, the script will pause before executing this task. Maximum value is 60 seconds.
* The `header` property is optional. It contains key-value pairs that RESTA will add to the HTTP header of the API request.
* The `body` property is used for the POST and PUT methods. It contains the name of the file in the data directory. RESTA will send the content of this file in the HTTP body of the API request.
* A task may also include one of the following segments: *assert*, *x509*, and *read*. You will find more details below.



Here is an example of the script task:

```json
{
	"id": "test-auth",
	"disabled": false,
	"title": "API Authentication using token",
	"method": "POST",
	"url": "{{baseurl}}/api/authenticate",
	"body": "user-id",
	"header": {
	  "Authorization": "Bearer {{authToken}}"
	},
	"assert": {
		"response": 201,
		"type": "application/json",
		"schema": "object"
	}
}
```

Notice how environment variables are used here to represent values of the script context.



#### Assert segment

The **assert** segment contains a list of tests used to determine whether the call failed or ran as expected. The segment has the following structure:

```json
{
	"assert": {
		"response": {code},
    "responses": [ {code1}, {code2}],
		"type": "{mime-type}",
    "schema": "{schema-name}"
	}
}
```

All fields of the assert object are optional. 

- The **response** field specifies the expected HTTP response code. Alternatively, you can provide an array of acceptable codes. For that, you have to use field **responses**. 

- The **type** field specifies the expected response MIME type. 

- The **schema** field contains the name of the JSON schema. If this field is provided, then the response must match the JSON schema found in the `schema-{name}.json` file. 

```json
{
	"assert": {
		"responses": [200, 201],
		"type": "application/json",
    "schema": "object"
	}
}
```

In this example, the API call will succeed if it returns either HTTP code 200 or 201.



#### x.509 certificate segment

Use this section only if the server requires the client to provide an x.509 certificate. The x509 section has the following structure:

```json
{
	"x509": {
		"file": "{file-name.pfx}",
		"password": "{pfx-password}"
	},
}
```

The **file** property contains the name of the certificate file. RESTA accepts files in PFX format only. Use the **password** property to enter the password of the certificate. Here is an example of the x509 section:

```json
{
	"x509": {
		"file": "client-qa.pfx",
		"password": "xyz123"
	},
}
```

The client-qa.pfx file must be in the same folder as the script file.



#### Read values segment

This segment allows you to read the API call's output so you can use it in other calls as parameters. The **read** section has the following structure:

```json
{
  "read": [
    {
	    "locate": "{path-to-find}",
		  "target": "{var-to-update}"
	  },
	  { ... },
	  { ... }
  ]
}
```

Every object in the array has two fields. 

- The **locate**: JSONPath query to locate the particular value in the API response. Find out more about JSONPath [here](http://goessner.net/articles/JsonPath/).

- The **target**: name of the variable to assign this value to. 

All variables are stored in the script context, along with variables loaded from the environment file. Here is an example of the read section:

```json
{
  "read": [
    {
	    "locate": "token",
		  "target": "authToken"
	  }
  ]
}
```

The script will find the property **token** in the response and assign its value to the context variable **authToken**.



### Runbooks

Runbook is a JSON file that contains instructions for executing multiple API scripts in the selected environment. The runbook file has the following structure:

```json
{
	"title": "{runbook-title}",
	"environment": "{environment-name}",
	"scripts": [
		"{script-name}", "{script-name}", "{script-name}"
	]
}
```

Here is an example of the RESTA runbook:

```json
{
	"title": "System Health Check",
	"environment": "uat",
	"scripts": [
		"ping-api",
    "read-summary"
	]
}
```

You can create multiple runbook files to support your testing needs.



### Test Results

RESTA saves the result of every test call in the API execution report. The report file name has the following structure: `api-{script}-{task}.json` where **script** is the script ID and **task** is the task ID. The report file has the following structure:

```json
{
  "scriptid": "{script-id}",
  "taskid": "{task-id}",
  "url": "{Method} {url}",
  "time": "{time-stamp}",
  "duration": {duration-ms},
  "htmlcode": {response-code},
  "type": "{response-mime}",
  "warnings": [],
  "response": { },
  "responseHeader": { },
  "raw": {raw-output},
  "input": { }
}
```

RESTA executes every script in the runbook one by one. For every script the it executes a sequence of tasks (API calls). If the task test execution succeeded, then RESTA will delete the report file. If you want to keep report files for successful calls, use `-keep` parameter in the command line.

The API test report contains the following fields:

- **scriptid**: ID of the script
- **taskid**: ID of the task inside the script
- **URL**: HTTP command in the following format: `{method} {url}`
- **time**: Timestamp when the test was executed (local time)
- **input**: the body of the request
- **duration**: Duration of the call in milliseconds
- **htmlcode**: The response HTML code
- **type**: The MIME type of the response
- **response**: The body of the response (JSON format)
- **responseHeader**: The HTTP response header variables (requires command-line option `-rh` )
- **raw**: The body of the response in raw format. This field is provided only if the body cannot be converted to JSON.
- **warnings**: List of messages generated by failed assertions. The API call is considered successful if the warning list is empty. 

Here is an example of the RESTA test report file:

```json
{
  "scriptid": "px-cspapi",
  "taskid": "auth-login-cspa",
  "url": "POST https://test-api.qa.service.com/login",
  "time": "09/21/2021 05:59:27",
  "duration": 457,
  "htmlcode": 400,
  "type": "application/json",
  "warnings": [
    "Invalid response code: 400. Expected 200"
  ],
  "response": {
    "message": "Missing required parameter: username"
  },
  "input": {
    "email": "admin@demo.com",
    "password": "bigsecret"
  }
}
```



In addition to the execution reports, RESTA will save the updated environment to the output folder. The file `env-{name}.json` will contain all key-value pairs including the ones obtained by scripts using the `read` section.



---

