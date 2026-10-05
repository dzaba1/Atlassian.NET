# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

Fork of the Atlassian.NET SDK (a .NET client for Jira, published as the NuGet package `Dzaba.AtlassianSDK`). The repo is **mid-migration** between two parallel library projects in `Atlassian.slnx`:

- `src/Atlassian.Jira` — the **legacy** SDK.
- `src/Dzaba.AtlassianSdk.Jira` — the **new** SDK being built to replace it.

## Writing V3 clients

The V3 client is generated from `src/contracts/swagger_v3.json`. When writing or porting code that calls a V3 endpoint, also check the official reference at https://developer.atlassian.com/cloud/jira/platform/rest/v3 for that operation.

- **Paging (`maxResults`)**: the maximum page size differs per endpoint and is often lower than the default or than what the swagger suggests. Look up the endpoint's documented `maxResults` limit (and its default) in the reference, and use that value as the `pageSize` passed to `PageExpander.ExpandAsync`. Define it as a named constant in the service (e.g. `MaxPrioritiesResults = 50`). Requesting more than the limit makes Jira silently clamp the page, which breaks offset arithmetic.
- **Paging style**: check whether the endpoint is offset based (`startAt`/`maxResults`, `isLast`/`total`) or token based (`nextPageToken`), and whether it returns a bare collection with no "last page" flag.
- **Parameters and behavior**: verify required/optional parameters, `expand` values, permissions required and any deprecation notes in the reference, and prefer the non-deprecated operation when both exist.
- If the reference and `swagger_v3.json` disagree, follow the reference for runtime behavior and leave a short comment in the code explaining why.

## Conventions

- Check `.editorconfig`
- File-scoped namespaces
- XML doc comments on public members
- null, empty string/collections, numeric ranges check in constructor/method arguments with `ArgumentNullException` or `ArgumentException`
- `ConfigureAwait(false)` on awaits in library code.
- User-facing documentation is in `docs/` (indexed by `docs/README.md`). Update it when public behavior changes.
