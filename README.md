# RESTA
A simple command-line tool for automated testing of RESTfull APIs

* [Build RESTA for your platform](doc/build.md)
* [Use RESTA CLI](doc/usage.md)
* [RESTA Scripts](doc/scripts.md)



### History of changes:

**Mar, 2024: Release 1.2.09**

* Upgrade to .NET 8 Framework
* Added test summary report.
* Option to hide report about variables assignment during execution. Command switch `-private`.

**Dec, 2022: Release 1.2.07**

* No longer need to create files for default schemas `object` and `array`.
* No need to create a runbook if you run one script. Can specify the script name in the command.
* Option to create request body inside task. Do not have to create file for request body.
* Upgrade RestSharp and updated unit tests

**Jul, 2022: Release 1.2**

* Converted to .NET 6 and C# 10
* Improved validations and error messages
* Added task delay feature
* User can generate new runbook with sample script

**Oct, 2021: Release 1.1**

- Added certificates

**Oct, 2021: Release 1.1.12**

- Added request HTTP header to the test report
- Fixed command-line issue with full Windows path
- Added feature to fail fast
- Added feature to include response header to the test report
- Added dynamic variables
- Improved error handling





---

