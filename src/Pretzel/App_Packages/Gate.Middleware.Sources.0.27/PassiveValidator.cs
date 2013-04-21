using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gate;
using Gate.Middleware;
using System.Threading.Tasks;

namespace Owin
{
    internal static class ValidatorExtensions
    {
        public static IAppBuilder UsePassiveValidator(this IAppBuilder builder)
        {
            IList<string> warnings = new List<string>();
            if (!PassiveValidator.TryValidateProperties(builder.Properties, warnings))
            {
                throw new InvalidOperationException(warnings.Aggregate("builder.Properties are invalid", (a, b) => a + "\r\n" + b));
            }

            if (warnings.Count != 0)
            {
                var builderProperties = new BuilderProperties(builder.Properties);
                var output = builderProperties.TraceOutput ?? Console.Out;
                output.WriteLine(warnings.Aggregate("builder.Properties are invalid", (a, b) => a + "\r\n" + b));
            }

            return builder.UseType<PassiveValidator>();
        }
    }
}

namespace Gate.Middleware
{
    using AppFunc = Func<IDictionary<string, object>, Task>;
    using Gate.Middleware.Utils;
    using System.Threading;

    // This middleware does a passive validation of all requests and responses. If any requirements from the OWIN 
    // standard are violated, an 500 error or a warning header are returned to the client.
    // This is implemented using version 0.15.0 of the OWIN standard.
    internal class PassiveValidator
    {
        private readonly AppFunc nextApp;

        public PassiveValidator(AppFunc nextApp)
        {
            this.nextApp = nextApp;
        }

        public Task Invoke(IDictionary<string, object> env)
        {
            IList<string> warnings = new List<string>();
            if (!TryValidateCall(env, warnings))
            {
                return TaskHelpers.Completed();
            }

            var req = new Request(env);
            Stream originalStream = req.Body;
            TriggerStream triggerStream = new TriggerStream(originalStream);
            req.Body = triggerStream;

            // Validate response headers and values on first write.
            bool responseValidated = false;
            Action responseValidation = () =>
            {
                if (responseValidated)
                {
                    return;
                }

                responseValidated = true;

                if (!TryValidateResult(env, warnings))
                {
                    return;
                }

                if (warnings.Count > 0)
                {
                    req.Headers["X-OwinValidatorWarning"] = warnings.ToArray();
                }
            };
            triggerStream.OnFirstWrite = responseValidation;

            try
            {
                return nextApp(env)
                    // Run response validation explicitly in case there was no response data written.
                    .Then(responseValidation)
                    .Catch(errorInfo =>
                    {
                        SetFatalResult(env, "6.1", "An asynchronous exception was thrown from the AppFunc: <br>"
                            + errorInfo.Exception.ToString().Replace(Environment.NewLine, "<br>"));
                        return errorInfo.Handled();
                    });
            }
            catch (Exception ex)
            {
                SetFatalResult(env, "6.1", "A synchronous exception was thrown from the AppFunc: <br>"
                            + ex.ToString().Replace(Environment.NewLine, "<br>"));
                return TaskHelpers.Completed();
            }
        }

        #region Startup Rules
        internal static bool TryValidateProperties(IDictionary<string, object> properties, IList<string> warnings)
        {
            if (properties == null)
            {
                throw new ArgumentNullException("properties");
            }
            return CheckRequiredStartupData(properties, warnings);
        }

        static bool CheckRequiredStartupData(IDictionary<string, object> properties, IList<string> warnings)
        {
            var builderProperties = new BuilderProperties(properties);
            if (builderProperties.Version == null)
            {
                warnings.Add("builder.Properties should contain \"owin.Version\"");
            }
            else if (builderProperties.Version != "1.0")
            {
                warnings.Add("builder.Properties[\"owin.Version\"] should be \"1.0\"");
            }

            if (builderProperties.TraceOutput == null)
            {
                warnings.Add("builder.Properties[\"host.TraceOutput\"] should be a TextWriter");
            }

            // TODO: is this still true?
            //if (!properties.ContainsKey("server.Name"))
            //{
            //    warnings.Add("builder.Properties should contain \"server.Name\"");
            //}
            //else if (properties.Get<string>("server.Name") == null)
            //{
            //    warnings.Add("builder.Properties[\"server.Name\"] should be a string");
            //}

            return true;
        }

        #endregion


        #region Call Rules

        // Returns false for fatal errors, along with a resulting message.
        // Otherwise any warnings are appended.
        private static bool TryValidateCall(IDictionary<string, object> env, IList<string> warnings)
        {
            if (env == null)
            {
                throw new ArgumentNullException("env");
            }

            var req = new Request(env);

            // Must be mutable
            try
            {
                string key = "validator.MutableKey";
                string input = "Mutable Value";
                req.Set(key, input);
                string output = req.Get<string>(key);
                if (output == null || output != input)
                {
                    SetFatalResult(env, "3.2", "Environment is not fully mutable.");
                    return false;
                }
                req.Set<string>(key, null);
            }
            catch (Exception ex)
            {
                SetFatalResult(env, "3.2", "Environment is not mutable: \r\n" + ex.ToString());
                return false;
            }

            // Environment key names MUST be case sensitive.
            string upperKey = "Validator.CaseKey";
            string lowerKey = "validator.casekey";
            string[] caseValue = new string[] { "Case Value" };
            env[upperKey] = caseValue;
            string[] resultValue = req.Get<string[]>(lowerKey);
            if (resultValue != null)
            {
                SetFatalResult(env, "3.2", "Environment is not case sensitive.");
                return false;
            }
            env.Remove(upperKey);

            // Check for required owin.* keys and the HOST header.
            if (!CheckRequiredCallData(env, warnings))
            {
                return false;
            }

            return true;
        }

        private static bool CheckRequiredCallData(IDictionary<string, object> env, IList<string> warnings)
        {
            var req = new Request(env);
            string[] requiredKeys = new string[]
            {
                "owin.Version",
                "owin.CallCancelled",

                "owin.RequestBody",
                "owin.RequestHeaders",
                "owin.RequestMethod",
                "owin.RequestPath",
                "owin.RequestPathBase",
                "owin.RequestProtocol",
                "owin.RequestQueryString",
                "owin.RequestScheme",

                "owin.ResponseHeaders",
                "owin.ResponseBody",
            };

            object temp;
            foreach (string key in requiredKeys)
            {
                if (!env.TryGetValue(key, out temp))
                {
                    SetFatalResult(env, "3.2", "Missing required Environment key: " + key);
                    return false;
                }

                if (temp == null)
                {
                    SetFatalResult(env, "3.2", "Required Environment value is null: " + key);
                    return false;
                }
            }

            IDictionary<string, string[]> requestHeaders = req.Get<IDictionary<string, string[]>>("owin.RequestHeaders");
            IDictionary<string, string[]> responseHeaders = req.Get<IDictionary<string, string[]>>("owin.ResponseHeaders");

            if (!TryValidateHeaderCollection(env, requestHeaders, "Request", warnings))
            {
                return false;
            }

            if (!TryValidateHeaderCollection(env, responseHeaders, "Response", warnings))
            {
                return false;
            }

            string[] header;
            if (!requestHeaders.TryGetValue("HOST", out header) || header.Length == 0)
            {
                SetFatalResult(env, "5.2", "Missing Host header");
                return false;
            }

            // Validate values

            string[] stringValueTypes = new string[]
            {
                "owin.RequestMethod",
                "owin.RequestPath",
                "owin.RequestPathBase",
                "owin.RequestProtocol",
                "owin.RequestQueryString",
                "owin.RequestScheme",
                "owin.Version" 
            };

            foreach (string key in stringValueTypes)
            {
                if (!(env[key] is string))
                {
                    SetFatalResult(env, "3.2", key + " value is not of type string: " + env[key].GetType().FullName);
                    return false;
                }
            }

            if (!(env["owin.CallCancelled"] is CancellationToken))
            {
                SetFatalResult(env, "3.2.3", "owin.CallCancelled is not of type CancellationToken: " + env["owin.CallCancelled"].GetType().FullName);
                return false;
            }

            if (req.Get<CancellationToken>("owin.CallCancelled").IsCancellationRequested)
            {
                warnings.Add(CreateWarning("3.6", "The owin.CallCancelled CancellationToken was cancelled before processing the request."));
            }

            if (string.IsNullOrWhiteSpace(req.Get<string>("owin.RequestMethod")))
            {
                SetFatalResult(env, "3.2.1", "owin.RequestMethod is empty.");
                return false;
            }

            string pathBase = req.Get<string>("owin.RequestPathBase");
            if (pathBase.EndsWith("/"))
            {
                SetFatalResult(env, "5.3", "owin.RequestBasePath ends with a slash: " + pathBase);
                return false;
            }


            if (!(pathBase.StartsWith("/") || pathBase.Equals(string.Empty)))
            {
                SetFatalResult(env, "5.3", "owin.RequestBasePath is not empty and does not start with a slash: " + pathBase);
                return false;
            }

            string path = req.Get<string>("owin.RequestPath");
            if (!path.StartsWith("/"))
            {
                if (path.Equals(string.Empty))
                {
                    if (pathBase.Equals(string.Empty))
                    {
                        SetFatalResult(env, "5.3", "owin.RequestPathBase and owin.RequestPath are both empty.");
                        return false;
                    }
                }
                else
                {
                    SetFatalResult(env, "5.3", "owin.RequestPath does not start with a slash.");
                    return false;
                }
            }

            string protocol = req.Get<string>("owin.RequestProtocol");
            if (!protocol.Equals("HTTP/1.1", StringComparison.OrdinalIgnoreCase)
                && !protocol.Equals("HTTP/1.0", StringComparison.OrdinalIgnoreCase))
            {
                warnings.Add(CreateWarning("3.2.1", "Unrecognized request protocol: " + protocol));
            }

            // No query string validation.

            string scheme = req.Get<string>("owin.RequestScheme");
            if (!scheme.Equals("http", StringComparison.OrdinalIgnoreCase)
                && !scheme.Equals("https", StringComparison.OrdinalIgnoreCase))
            {
                warnings.Add(CreateWarning("5.1", "Unrecognized request scheme: " + scheme));
            }

            string version = req.Get<string>("owin.Version");
            Version parsedVersion;
            if (!Version.TryParse(version, out parsedVersion))
            {
                SetFatalResult(env, "7", "owin.Version could not be parsed: " + version);
                return false;
            }

            if (!parsedVersion.Equals(new Version(1, 0)))
            {
                warnings.Add(CreateWarning("7", "Unrecognized OWIN version: " + version));
            }

            return true;
        }

        #endregion Call Rules

        #region Result Rules

        private static bool TryValidateResult(IDictionary<string, object> env, IList<string> warnings)
        {
            var resp = new Response(env);
            IDictionary<string, string[]> responseHeaders = resp.Headers;

            // Headers
            if (!TryValidateHeaderCollection(env, responseHeaders, "Response", warnings))
            {
                return false;
            }

            // Status code
            object temp;
            if (env.TryGetValue("owin.ResponseStatusCode", out temp))
            {
                if (temp == null)
                {
                    SetFatalResult(env, "3.2.2", "The response status code value is null");
                    return false;
                }

                if (!(temp is int))
                {
                    SetFatalResult(env, "3.2.2", "Status code is not an int: " + temp.GetType().FullName);
                    return false;
                }

                int statusCode = (int)temp;
                if (statusCode <= 100 || statusCode >= 1000)
                {
                    SetFatalResult(env, "3.2.2", "Invalid status code value: " + statusCode);
                    return false;
                }
            }

            // Reason phrase
            if (env.TryGetValue("owin.ResponseReasonPhrase", out temp))
            {
                if (temp == null)
                {
                    SetFatalResult(env, "3.2.2", "The reason phrase value is null");
                    return false;
                }

                if (!(temp is string))
                {
                    SetFatalResult(env, "3.2.2", "The reason phrase is not a string: " + temp.GetType().FullName);
                    return false;
                }
            }

            // Protocol
            if (env.TryGetValue("owin.ResponseProtocol", out temp))
            {
                if (temp == null)
                {
                    SetFatalResult(env, "3.2.2", "The response protocol value is null");
                    return false;
                }

                if (!(temp is string))
                {
                    SetFatalResult(env, "3.2.2", "The response protocol is not a string: " + temp.GetType().FullName);
                    return false;
                }

                string protocol = (string)temp;
                if (!protocol.Equals("HTTP/1.1", StringComparison.OrdinalIgnoreCase)
                    && !protocol.Equals("HTTP/1.0", StringComparison.OrdinalIgnoreCase))
                {
                    warnings.Add(CreateWarning("3.2.2", "Unrecognized response protocol: " + protocol));
                }
            }

            return true;
        }

        #endregion Result Rules

        // Shared code for validating that the request and response header collections adhere to some basic requirements like mutability and casing.
        private static bool TryValidateHeaderCollection(IDictionary<string, object> env, IDictionary<string, string[]> headers,
            string headerId, IList<string> warnings)
        {
            // Must be mutable
            try
            {
                string key = "x-validator-mutate";
                string[] input = new string[] { "Mutable Value" };
                headers[key] = input;
                string[] output = headers[key];
                if (output == null || output.Length != input.Length || output[0] != input[0])
                {
                    SetFatalResult(env, "3.3", headerId + " headers are not fully mutable.");
                    return false;
                }
                headers.Remove(key);
            }
            catch (Exception ex)
            {
                SetFatalResult(env, "3.3", headerId + " headers are not mutable: \r\n" + ex.ToString());
                return false;
            }

            // Header key names MUST be case insensitive.
            string upperKey = "X-Validator-Case";
            string lowerKey = "x-validator-case";
            string[] caseValue = new string[] { "Case Value" };
            headers[upperKey] = caseValue;
            string[] resultValue = null;
            if (!headers.TryGetValue(lowerKey, out resultValue) || resultValue.Length != caseValue.Length || resultValue[0] != caseValue[0])
            {
                SetFatalResult(env, "3.3", headerId + " headers are case sensitive.");
                return false;
            }
            headers.Remove(upperKey);

            foreach (var pair in headers)
            {
                if (pair.Value == null)
                {
                    warnings.Add(CreateWarning("3.3", headerId + " header " + pair.Key + " has a null string[]."));
                }
                else
                {
                    for (int i = 0; i < pair.Value.Length; i++)
                    {
                        if (pair.Value[i] == null)
                        {
                            warnings.Add(CreateWarning("3.3", headerId + " header " + pair.Key + " has a null value at index " + i));
                        }
                    }
                }
            }

            return true;
        }

        private static void SetFatalResult(IDictionary<string, object> env, string standardSection, string message)
        {
            new Response(env)
            {
                StatusCode = 500,
                ReasonPhrase = "Internal Server Error"
            }.Write("OWIN v0.15.0 validation failure: Section#{0}, {1}", standardSection, message);
        }

        private static string CreateWarning(string standardSection, string message)
        {
            return string.Format("OWIN v0.15.0 validation warning: Section#{0}, {1}", standardSection, message);
        }
    }
}
