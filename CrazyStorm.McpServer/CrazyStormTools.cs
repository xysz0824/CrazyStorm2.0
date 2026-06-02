/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
 */
using CrazyStorm.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Script.Serialization;
using System.Xml;
using CsFile = CrazyStorm.Core.File;
using IoFile = System.IO.File;

namespace CrazyStorm.McpServer
{
    internal sealed class CrazyStormTools
    {
        private readonly JavaScriptSerializer json = new JavaScriptSerializer();

        public object[] ListTools()
        {
            return new object[]
            {
                Tool(
                    "crazy_storm_project_summary",
                    "Load a CrazyStorm .bgp/.mbg project and return counts for particle systems, layers, components, resources, and invalid resource references.",
                    new Dictionary<string, object>
                    {
                        { "path", StringProperty("Project file path. Relative paths resolve from the MCP server working directory.") }
                    },
                    new[] { "path" }),

                Tool(
                    "crazy_storm_validate_project",
                    "Validate that a CrazyStorm project can be loaded. Optionally compile it in memory to catch expression and event errors.",
                    new Dictionary<string, object>
                    {
                        { "path", StringProperty("Project file path. Relative paths resolve from the MCP server working directory.") },
                        { "compile", new Dictionary<string, object>
                            {
                                { "type", "boolean" },
                                { "description", "When true, also generates play data in memory to compile expressions and events." },
                                { "default", false }
                            }
                        }
                    },
                    new[] { "path" }),

                Tool(
                    "crazy_storm_export_play_data",
                    "Load a CrazyStorm project and export playable .bg data.",
                    new Dictionary<string, object>
                    {
                        { "path", StringProperty("Project file path. Relative paths resolve from the MCP server working directory.") },
                        { "outputPath", StringProperty("Optional output .bg path. Defaults to the project file name with a .bg extension.") },
                        { "overwrite", new Dictionary<string, object>
                            {
                                { "type", "boolean" },
                                { "description", "Set true to overwrite an existing output file." },
                                { "default", false }
                            }
                        }
                    },
                    new[] { "path" },
                    new Dictionary<string, object>
                    {
                        { "destructiveHint", true },
                        { "idempotentHint", false }
                    }),

                Tool(
                    "crazy_storm_component_types",
                    "List component types available in CrazyStorm.Core.",
                    new Dictionary<string, object>(),
                    new string[0])
            };
        }

        public Dictionary<string, object> Call(string name, Dictionary<string, object> arguments)
        {
            try
            {
                switch (name)
                {
                    case "crazy_storm_project_summary":
                        return TextResult(ToJson(BuildSummary(LoadProject(GetRequiredPath(arguments)))));

                    case "crazy_storm_validate_project":
                        return TextResult(ToJson(ValidateProject(arguments)));

                    case "crazy_storm_export_play_data":
                        return TextResult(ToJson(ExportPlayData(arguments)));

                    case "crazy_storm_component_types":
                        return TextResult(ToJson(GetComponentTypes()));

                    default:
                        return ErrorResult("Unknown tool: " + name);
                }
            }
            catch (Exception ex)
            {
                return ErrorResult(ex.Message);
            }
        }

        private Dictionary<string, object> ValidateProject(Dictionary<string, object> arguments)
        {
            string path = GetRequiredPath(arguments);
            bool compile = McpProtocol.GetBool(arguments, "compile", false);

            var loaded = LoadProject(path);
            byte[] playBytes = null;
            if (compile) playBytes = loaded.Project.GeneratePlayFile();

            var summary = BuildSummary(loaded);
            summary["valid"] = true;
            summary["compiled"] = compile;
            if (playBytes != null) summary["playDataBytes"] = playBytes.Length;
            return summary;
        }

        private Dictionary<string, object> ExportPlayData(Dictionary<string, object> arguments)
        {
            string path = GetRequiredPath(arguments);
            string outputPath = McpProtocol.GetString(arguments, "outputPath");
            bool overwrite = McpProtocol.GetBool(arguments, "overwrite", false);

            var loaded = LoadProject(path);
            string resolvedOutputPath = ResolveOutputPath(loaded.Path, outputPath);
            if (IoFile.Exists(resolvedOutputPath) && !overwrite)
            {
                throw new InvalidOperationException("Output already exists. Pass overwrite=true to replace it: " + resolvedOutputPath);
            }

            byte[] bytes = loaded.Project.GeneratePlayFile();
            IoFile.WriteAllBytes(resolvedOutputPath, bytes);

            return new Dictionary<string, object>
            {
                { "projectPath", loaded.Path },
                { "outputPath", resolvedOutputPath },
                { "bytes", bytes.Length },
                { "overwritten", overwrite }
            };
        }

        private LoadedProject LoadProject(string path)
        {
            string fullPath = ResolvePath(path);
            if (!IoFile.Exists(fullPath)) throw new FileNotFoundException("Project file not found.", fullPath);

            var project = new CsFile();
            project.Load(fullPath);

            return new LoadedProject
            {
                Path = fullPath,
                Project = project,
                Version = ReadProjectVersion(fullPath),
                IsLegacyCs1 = CsFile.IsCS1(fullPath)
            };
        }

        private Dictionary<string, object> BuildSummary(LoadedProject loaded)
        {
            var systems = new List<object>();
            int layerCount = 0;
            int componentCount = 0;
            int noteCount = 0;
            var componentTypes = new Dictionary<string, int>();

            foreach (var system in loaded.Project.ParticleSystems)
            {
                int systemComponentCount = 0;
                var layerSummaries = new List<object>();

                foreach (var layer in system.Layers)
                {
                    layerCount++;
                    componentCount += layer.Components.Count;
                    systemComponentCount += layer.Components.Count;

                    foreach (var component in layer.Components)
                    {
                        string typeName = component.GetType().Name;
                        if (!componentTypes.ContainsKey(typeName)) componentTypes[typeName] = 0;
                        componentTypes[typeName]++;
                    }

                    layerSummaries.Add(new Dictionary<string, object>
                    {
                        { "name", layer.Name },
                        { "visible", layer.Visible },
                        { "beginFrame", layer.BeginFrame },
                        { "totalFrame", layer.TotalFrame },
                        { "componentCount", layer.Components.Count }
                    });
                }

                noteCount += system.Notes.Count;
                systems.Add(new Dictionary<string, object>
                {
                    { "name", system.Name },
                    { "totalFrame", system.TotalFrame },
                    { "layerCount", system.Layers.Count },
                    { "componentCount", systemComponentCount },
                    { "customTypeCount", system.CustomTypes.Count },
                    { "customDistortTypeCount", system.CustomDistortTypes.Count },
                    { "customMaskTypeCount", system.CustomMaskTypes.Count },
                    { "noteCount", system.Notes.Count },
                    { "layers", layerSummaries.ToArray() }
                });
            }

            var invalidResources = GetInvalidResources(loaded.Project);

            return new Dictionary<string, object>
            {
                { "path", loaded.Path },
                { "format", loaded.IsLegacyCs1 ? "Crazy Storm 1.x mbg" : "Crazy Storm 2 bgp" },
                { "version", loaded.Version },
                { "particleSystemCount", loaded.Project.ParticleSystems.Count },
                { "layerCount", layerCount },
                { "componentCount", componentCount },
                { "noteCount", noteCount },
                { "imageCount", loaded.Project.Images.Count },
                { "soundCount", loaded.Project.Sounds.Count },
                { "globalVariableCount", loaded.Project.Globals.Count },
                { "invalidResourceCount", invalidResources.Count },
                { "invalidResources", invalidResources.ToArray() },
                { "componentTypes", componentTypes },
                { "particleSystems", systems.ToArray() }
            };
        }

        private List<object> GetInvalidResources(CsFile project)
        {
            var invalid = new List<object>();
            AppendInvalidResources(invalid, "image", project.Images);
            AppendInvalidResources(invalid, "sound", project.Sounds);
            return invalid;
        }

        private void AppendInvalidResources(List<object> invalid, string kind, IEnumerable<FileResource> resources)
        {
            foreach (var resource in resources)
            {
                if (resource.IsValid) continue;
                invalid.Add(new Dictionary<string, object>
                {
                    { "kind", kind },
                    { "id", resource.ID },
                    { "label", resource.Label },
                    { "path", resource.AbsolutePath }
                });
            }
        }

        private Dictionary<string, object> GetComponentTypes()
        {
            var types = typeof(Component).Assembly.GetTypes()
                .Where(t => t != typeof(Component) && !t.IsAbstract && typeof(Component).IsAssignableFrom(t))
                .OrderBy(t => t.Name)
                .Select(t => t.Name)
                .ToArray();

            return new Dictionary<string, object>
            {
                { "componentTypes", types }
            };
        }

        private string ReadProjectVersion(string path)
        {
            if (CsFile.IsCS1(path)) return "1.01";

            var doc = new XmlDocument();
            doc.Load(path);
            var root = (XmlElement)doc.SelectSingleNode(VersionInfo.AppName.Replace(" ", ""));
            if (root == null) return null;
            return root.GetAttribute("version");
        }

        private static string ResolvePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("path is required.");
            return Path.GetFullPath(Environment.ExpandEnvironmentVariables(path));
        }

        private static string ResolveOutputPath(string projectPath, string outputPath)
        {
            if (string.IsNullOrWhiteSpace(outputPath))
            {
                return Path.ChangeExtension(projectPath, ".bg");
            }

            return Path.GetFullPath(Environment.ExpandEnvironmentVariables(outputPath));
        }

        private static string GetRequiredPath(Dictionary<string, object> arguments)
        {
            string path = McpProtocol.GetString(arguments, "path");
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("path is required.");
            return path;
        }

        private string ToJson(object value)
        {
            return json.Serialize(value);
        }

        private static Dictionary<string, object> TextResult(string text)
        {
            return new Dictionary<string, object>
            {
                { "content", new object[]
                    {
                        new Dictionary<string, object>
                        {
                            { "type", "text" },
                            { "text", text }
                        }
                    }
                }
            };
        }

        private static Dictionary<string, object> ErrorResult(string text)
        {
            var result = TextResult(text);
            result["isError"] = true;
            return result;
        }

        private static Dictionary<string, object> Tool(string name, string description, Dictionary<string, object> properties, string[] required)
        {
            return Tool(name, description, properties, required, null);
        }

        private static Dictionary<string, object> Tool(string name, string description, Dictionary<string, object> properties, string[] required, Dictionary<string, object> annotations)
        {
            var tool = new Dictionary<string, object>
            {
                { "name", name },
                { "description", description },
                { "inputSchema", new Dictionary<string, object>
                    {
                        { "type", "object" },
                        { "properties", properties },
                        { "required", required },
                        { "additionalProperties", false }
                    }
                }
            };

            if (annotations != null) tool["annotations"] = annotations;
            return tool;
        }

        private static Dictionary<string, object> StringProperty(string description)
        {
            return new Dictionary<string, object>
            {
                { "type", "string" },
                { "description", description }
            };
        }

        private sealed class LoadedProject
        {
            public string Path { get; set; }
            public CsFile Project { get; set; }
            public string Version { get; set; }
            public bool IsLegacyCs1 { get; set; }
        }
    }
}
