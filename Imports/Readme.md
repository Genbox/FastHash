## Locals

Add a file to the locals folder named the same as the file you want to override. Locals as included after the base includes.

Currently supports:

* Directory.Build.props
* Directory.Build.targets
* Directory.Packages.props
* Directory.Packages.Analyzers.props
* Benchmarks.props
* Console.props
* Examples.props
* Library.props
* Tests.props

## Switches

#### IncludeBaseProject (default: true)

When set to true, tests, examples and benchmarks include their base projects. The base project is determined by project name.
For example:

* MyProject <- Base project
* MyProject.Tests <- Automatically references MyProject

#### IncludeInternalsVisibleTo (default: true)

When set to true, library projects will set InternalsVisibleTo as an assembly attribute, and automatically include benchmark and test projects.

#### IncludeAnalyzers (default: true)

When set to true, analyzers are installed in the different project types. It also disable the embedded .NET analyzers.