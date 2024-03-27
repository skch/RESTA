# RESTA

RESTA is a  simple command-line tool for automated testing of RESTfull APIs. To execute the test, use the following command:

```shell
resta {runbook} -out:{output-dir} -sc:{schema-dir} -in:{data-dir} [options]
```



**PARAMETERS**:

* `runbook`: Filename of the RESTA runbook to execute.
* `-out:{path}`: Specify the path to a folder where RESTA will save the test results (output files)
* `-in:{path}`: Specify the path to a folder where RESTA will find the data files referenced in the tasks as “body” 
* `-sc:{path}`: Specify the path to a folder where RESTA will find the schema files. Every schema file name should have prefix `schema-`
* `-env:{name}`: you can override the environment name specified in the runbook by providing the environment name in command line.
* `-script`: the option indicate that instead of the runbook file name you provided the name of the script you want to run. If you use this option, then you have to specify the environment.



**OPTIONS**:

`-keep`: To keep the result file for successful tests

`-rh`: Include the HTTP response header to the test result report

`-ff` To stop execution after first error (fail fast)

`-private` To hide variables assigned during execution



**EXAMPLE**:

```shell
Resta test/runbook.json -out:results -sc:test/schema -in:test/data -keep -ff
```





## Use Example Scripts



The examples folder contains a simple script you can run using the following commands:

```shell
cd RESTA/examples
mkdir results
Resta book1.json -out:results -sc:schema -in:results -keep
```



