# Atlassian.NET SDK

[![Build](https://github.com/dzaba1/Atlassian.NET/actions/workflows/build.yml/badge.svg)](https://github.com/dzaba1/Atlassian.NET/actions/workflows/build.yml)

Contains utilities for interacting with  [Atlassian JIRA](http://www.atlassian.com/software/jira).

## Download

- 

## License

This project is licensed under  [BSD](/LICENSE.md).

## Dependencies & Requirements

- [RestSharp](https://www.nuget.org/packages/RestSharp)
- [Newtonsoft.Json](https://www.nuget.org/packages/Newtonsoft.Json)
- Tested with JIRA v8.5.2

## History

- For a description changes, check out the [Change History Page](/docs/change-history.md).

- This project began in 2010 during a [ShipIt](https://www.atlassian.com/company/shipit) day at Atlassian with provider
  to query Jira issues using LINQ syntax. Over time it grew to add many more operations on top of the JIRA SOAP API.
  Support of REST API was added on v4.0 and support of SOAP API was dropped on v8.0.

## Related Projects

- [VS Jira](https://bitbucket.org/farmas/vsjira) - A VisualStudio Extension that adds tools to interact with JIRA
servers.
- [Jira OAuth CLI](https://bitbucket.org/farmas/atlassian.net-jira-oauth-cli) - Command line tool to setup OAuth on a JIRA server so that it can be used with the Atlassian.NET SDK.

## Documentation

The documentation is placed under the [docs](/docs) folder.

As a first user, here is the documentation on [how to use the SDK](/docs/how-to-use-the-sdk.md).
