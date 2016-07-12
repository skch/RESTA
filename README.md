# RESTA
A simple command-line tool for automated testing of RESTfull APIs

## USAGE


Prepare the environment XML file:
```
<environment name="...">
  <variables>
    <var id="base" value="http://api-url"  />
    <var id="key" value="value"  />
    ...
  </variables>
  <header-all>
    <header id="key" value="value" />
    <header id="key" value="value" />
    ...
  </header-all>
</environment>
```

Prepare the one or several test cases XML files. You can use GET, POST, PUT, or DELETE request types:
```
<rest type="GET" url="{{base}}/path/parameters">
  <data>
    ~~~ message body in JSON format (optional) ~~~
  </data>
  <result code="200" type="application/json; charset=utf-8">
    ~~~ JSON SCHEMA to validate response~~~
  </result>
</rest>
```

Prepare the runbook XML file that refers one or several use cases:
```
<runbook>
  <header>
    <header id="key" value="value" />
    ...
  </header>
  <sequence id="name">
    <test src="case one" />
    <test src="case two" />
  </sequence>
  ...
</runbook>
```

Execute the test script:
```
rtest runbook.xml environment.xml
```

For each test sequence it will create file called _report-case.xml_ where the _case_ is the name of the test case.