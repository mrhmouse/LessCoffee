﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using DotSmart.Properties;
using System.IO;
using System.Reflection;
using System.Diagnostics;

namespace DotSmart
{
    public class CoffeeScriptHandler : ScriptHandlerBase, IHttpHandler
    {
        static string _coffeeWsf;
        static string _coffeeScriptJs;

        static CoffeeScriptHandler()
        {
            _coffeeWsf = Path.Combine(TempDirectory, "coffee.wsf");
            _coffeeScriptJs = Path.Combine(TempDirectory, "coffee-script.js");

            ExportResourceIfNewer(_coffeeWsf, Resources.CoffeeWsf);
            ExportResourceIfNewer(_coffeeScriptJs, Resources.CoffeeScriptJs);
        }

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "application/x-javascript";

            context.Response.Write("// Generated by DotSmart LessCoffee on " + DateTime.Now + " - http://dotsmart.net\r\n");
            context.Response.Write("// Using CoffeeScript Compiler v1.1.1 - http://coffeescript.org - Copyright 2011, Jeremy Ashkenas\r\n\r\n");

            string scriptFileName = context.Server.MapPath(context.Request.FilePath);

            renderScript(scriptFileName, context.Response.Output);
            SetCacheability(context.Response, scriptFileName);
        }

        void renderScript(string scriptFileName, TextWriter output)
        {
            using (var scriptFile = new StreamReader(scriptFileName, Encoding.UTF8))
            using (var stdErr = new StringWriter())
            {
                int exitCode = ProcessUtil.Exec("cscript.exe", "//U //nologo \"" + _coffeeWsf + "\" - ", scriptFile, output, stdErr, Encoding.Unicode);
                if (exitCode != 0)
                {
                    output.WriteLine("throw \"Error in " + Path.GetFileName(scriptFileName).JsEncode() + ": " 
                        + stdErr.ToString().Trim().JsEncode() + "\";");
                }
            }
        }

        public bool IsReusable
        {
            get { return false; }
        }

    }
}
