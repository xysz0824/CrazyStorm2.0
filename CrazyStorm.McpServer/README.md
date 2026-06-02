# CrazyStorm MCP Server

This project exposes CrazyStorm project-file utilities through the Model Context Protocol over stdio.

## Build

Build the solution or just this project:

```powershell
dotnet msbuild .\CrazyStorm.McpServer\CrazyStorm.McpServer.csproj /p:Configuration=Debug /p:Platform=AnyCPU
```

## Client configuration

Use the built executable as a stdio MCP server:

```json
{
  "mcpServers": {
    "crazystorm": {
      "command": "D:\\Csharp\\crazystorm\\CrazyStorm2.0\\CrazyStorm.McpServer\\bin\\Debug\\CrazyStorm.McpServer.exe"
    }
  }
}
```

## Tools

- `crazy_storm_project_summary`: loads a `.bgp` or legacy `.mbg` project and returns project counts plus invalid resources.
- `crazy_storm_validate_project`: validates loadability and can compile play data in memory with `compile=true`.
- `crazy_storm_export_play_data`: exports playable `.bg` data. Existing output files require `overwrite=true`.
- `crazy_storm_component_types`: lists component types available from `CrazyStorm.Core`.
