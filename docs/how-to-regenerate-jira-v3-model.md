# How to regenerate the Jira V3 model classes

`Atlassian.Jira/Model/V3/JiraModelV3.g.cs` contains the `Atlassian.Jira.Model.V3` DTO classes generated from `Atlassian.Jira/Model/V3/swagger.json` (Jira's V3 REST API OpenAPI document) using [NSwag](https://github.com/RicoSuter/NSwag). The classes are `public`, serialized with `System.Text.Json`, and the generated file is committed to git like normal source.

## Regenerating

Generation does **not** run on a normal `dotnet build` — it's opt-in, because regenerating overwrites the manual fixes described below. To regenerate after updating `swagger.json`, pass the `RegenerateJiraModelV3` MSBuild property:

```
dotnet build Atlassian.Jira/Atlassian.Jira.csproj -p:RegenerateJiraModelV3=true
```

This requires the .NET 10 SDK (the generator runs via NSwag.MSBuild's `net10.0` tool). It works the same way on Windows and Linux.

## Known issue: manual fixes required after every regeneration

Regenerating **will reintroduce compile errors** and needs manual patching afterward. Three schemas in `swagger.json` are "bare" `oneOf` + `discriminator` unions with no properties of their own:

| Schema | Referenced from | Generated (broken) property type |
|---|---|---|
| `CustomFieldContextDefaultValue` | `CustomFieldContextDefaultValueUpdate.defaultValues`, `IssueTypeDefaultValue.value`, `PageBeanCustomFieldContextDefaultValue.values` | `DefaultValues` |
| `CustomContextVariable` | `JiraExpressionEvalContextBean.custom`, `JiraExpressionEvaluateContextBean.custom` | `Custom` |
| `WorkflowCondition` | `WorkflowCompoundCondition.conditions`, `WorkflowRules.conditionsTree` | `Conditions` |

NSwag/NJsonSchema can't generate a real class for schemas like these, but instead of failing cleanly it emits a reference to a type named after the *referencing property* (`DefaultValues`, `Custom`, `Conditions`) that is never actually generated, causing `CS0246` ("type or namespace not found"). This is a confirmed, still-open upstream bug: [NSwag#3738](https://github.com/RicoSuter/NSwag/issues/3738).

After regenerating, fix each of the 7 affected properties by changing the bogus generated type to `System.Text.Json.JsonElement` (a weakly-typed but valid stand-in for these unmodeled unions). For example:

```diff
-public System.Collections.Generic.ICollection<DefaultValues> DefaultValues { get; set; }
+public System.Collections.Generic.ICollection<System.Text.Json.JsonElement> DefaultValues { get; set; }
```

Each affected spot in the generated file has a `// Manual fix for https://github.com/RicoSuter/NSwag/issues/3738 ...` comment directly above it before regeneration — that comment is lost on regenerate (NSwag overwrites the whole file), so re-add it too for the next person. Once NSwag fixes the underlying bug, these manual patches (and this doc) can be removed and `swagger.json` should generate cleanly on its own.
