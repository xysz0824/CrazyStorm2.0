/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Script.Serialization;

namespace CrazyStorm.McpServer
{
    internal sealed class McpProtocol
    {
        private const string ServerName = "crazystorm-mcp";
        private const string ServerVersion = "0.1.0";
        private const string DefaultProtocolVersion = "2025-06-18";

        private readonly CrazyStormTools tools;
        private readonly JavaScriptSerializer json = new JavaScriptSerializer();

        public McpProtocol(CrazyStormTools tools)
        {
            this.tools = tools;
        }

        public void Run(TextReader input, TextWriter output, TextWriter error)
        {
            string line;
            while ((line = input.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                try
                {
                    var request = json.DeserializeObject(line) as Dictionary<string, object>;
                    if (request == null) continue;

                    object id;
                    bool hasId = request.TryGetValue("id", out id);
                    string method = GetString(request, "method");
                    if (string.IsNullOrEmpty(method)) continue;
                    if (!hasId || method.StartsWith("notifications/", StringComparison.Ordinal)) continue;

                    WriteJson(output, HandleRequest(id, method, GetMap(request, "params")));
                }
                catch (Exception ex)
                {
                    error.WriteLine(ex);
                    WriteJson(output, CreateError(null, -32700, "Parse error", ex.Message));
                }
            }
        }

        private Dictionary<string, object> HandleRequest(object id, string method, Dictionary<string, object> parameters)
        {
            switch (method)
            {
                case "initialize":
                    return CreateResult(id, new Dictionary<string, object>
                    {
                        { "protocolVersion", ChooseProtocolVersion(parameters) },
                        { "capabilities", new Dictionary<string, object>
                            {
                                { "tools", new Dictionary<string, object>() }
                            }
                        },
                        { "serverInfo", new Dictionary<string, object>
                            {
                                { "name", ServerName },
                                { "version", ServerVersion }
                            }
                        }
                    });

                case "ping":
                    return CreateResult(id, new Dictionary<string, object>());

                case "tools/list":
                    return CreateResult(id, new Dictionary<string, object>
                    {
                        { "tools", tools.ListTools() }
                    });

                case "tools/call":
                    return CreateResult(id, tools.Call(GetString(parameters, "name"), GetMap(parameters, "arguments")));

                default:
                    return CreateError(id, -32601, "Method not found", method);
            }
        }

        private string ChooseProtocolVersion(Dictionary<string, object> parameters)
        {
            string requested = GetString(parameters, "protocolVersion");
            if (string.IsNullOrEmpty(requested)) return DefaultProtocolVersion;
            return requested;
        }

        private void WriteJson(TextWriter output, object value)
        {
            output.WriteLine(json.Serialize(value));
            output.Flush();
        }

        private static Dictionary<string, object> CreateResult(object id, object result)
        {
            return new Dictionary<string, object>
            {
                { "jsonrpc", "2.0" },
                { "id", id },
                { "result", result }
            };
        }

        private static Dictionary<string, object> CreateError(object id, int code, string message, object data)
        {
            var error = new Dictionary<string, object>
            {
                { "code", code },
                { "message", message }
            };
            if (data != null) error["data"] = data;

            return new Dictionary<string, object>
            {
                { "jsonrpc", "2.0" },
                { "id", id },
                { "error", error }
            };
        }

        internal static string GetString(Dictionary<string, object> source, string name)
        {
            if (source == null) return null;

            object value;
            if (!source.TryGetValue(name, out value) || value == null) return null;
            return value as string ?? Convert.ToString(value);
        }

        internal static bool GetBool(Dictionary<string, object> source, string name, bool defaultValue)
        {
            if (source == null) return defaultValue;

            object value;
            if (!source.TryGetValue(name, out value) || value == null) return defaultValue;
            if (value is bool) return (bool)value;

            bool parsed;
            if (bool.TryParse(Convert.ToString(value), out parsed)) return parsed;
            return defaultValue;
        }

        internal static Dictionary<string, object> GetMap(Dictionary<string, object> source, string name)
        {
            if (source == null) return new Dictionary<string, object>();

            object value;
            if (!source.TryGetValue(name, out value) || value == null) return new Dictionary<string, object>();

            var map = value as Dictionary<string, object>;
            return map ?? new Dictionary<string, object>();
        }
    }
}
