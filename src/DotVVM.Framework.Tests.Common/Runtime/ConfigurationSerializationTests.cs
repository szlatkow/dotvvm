using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using CheckTestOutput;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.ResourceManagement;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotVVM.Framework.Tests.Runtime
{
    [TestClass]
    public class ConfigurationSerializationTests
    {
        OutputChecker check = new OutputChecker("config-tests");

        public ConfigurationSerializationTests()
        {
            DotvvmTestHelper.EnsureCompiledAssemblyCache();
        }

        void checkConfig(DotvvmConfiguration config, string checkName = null, string fileExtension = "json", [CallerMemberName] string memberName = null, [CallerFilePath] string sourceFilePath = null)
        {
            var serialized = DotVVM.Framework.Hosting.VisualStudioHelper.SerializeConfig(config);
            serialized = Regex.Replace(serialized, "Version=[0-9.]+", "Version=***");
            //serialized = Regex.Replace(serialized, "DotVVM\\.Framework, Version=[0-9.]+,", "DotVVM.Framework, Version=2.4.0.2,");
            check.CheckString(serialized, checkName, fileExtension, memberName, sourceFilePath);
        }

        [TestMethod]
        public void SerializeDefaultConfig()
        {
            checkConfig(DotvvmConfiguration.CreateDefault());
        }

        [TestMethod]
        public void SerializeEmptyConfig()
        {
            checkConfig(new DotvvmConfiguration());
        }

        [TestMethod]
        public void SerializeRoutes()
        {
            var c = new DotvvmConfiguration();

            c.RouteTable.Add("route1", "url1", "file1.dothtml", new { a = "ccc" });
            c.RouteTable.Add("route2", "url2/{int}", "file1.dothtml", new { a = "ccc" });
            c.RouteTable.Add("custom presenter", "url3", "", presenterFactory: LocalizablePresenter.BasedOnQuery("lang"));

            c.RouteTable.AddGroup("group1", "group-{lang}/", "", rg => {

                rg.Add("r1", "r1", "g-r1.dothtml");

                rg.AddGroup("g", "g/", "", rg => {
                    rg.Add("r2", "r2", "g-g-r2.dothtml");
                });

            }, presenterFactory: LocalizablePresenter.BasedOnParameter("lang"));

            checkConfig(c);
        }

        [TestMethod]
        public void SerializeResources()
        {
            var c = new DotvvmConfiguration();
            c.Resources.Register("r1", new ScriptResource(
                new UrlResourceLocation("x")) {
                    IntegrityHash = "hash, maybe",
                    VerifyResourceIntegrity = true,
                    RenderPosition = ResourceRenderPosition.Head
                });
            c.Resources.Register("r2", new ScriptResource(
                new UrlResourceLocation("x")) {
                    Dependencies = new [] { "r1" },
                    LocationFallback = new ResourceLocationFallback("window.x", new IResourceLocation [] {
                        new UrlResourceLocation("y"),
                        new UrlResourceLocation("z"),
                        new FileResourceLocation("some-script.js")
                    })
                }
            );
            c.Resources.Register("r3", new StylesheetResource(
                new UrlResourceLocation("s")) {
                    IntegrityHash = "hash, maybe",
                    VerifyResourceIntegrity = true
                }
            );
            c.Resources.Register("r4", new InlineStylesheetResource("body { display: none }"));
            c.Resources.Register("r5", new InlineStylesheetResource(new FileResourceLocation("some-style.css")));
            c.Resources.Register("r6", new InlineScriptResource("alert(1)"));
            c.Resources.Register("r7", new InlineScriptResource(new FileResourceLocation("some-script.js")));
            c.Resources.Register("r8", new NullResource());
            c.Resources.Register("r9", new TemplateResource("<div></div>"));


            checkConfig(c);            
        }

        [TestMethod]
        public void ExperimentalFeatures()
        {
            var c = new DotvvmConfiguration();

            c.ExperimentalFeatures.LazyCsrfToken.EnableForAllRoutesExcept(new [] { "r1", "r2" });
            c.ExperimentalFeatures.ServerSideViewModelCache.EnableForRoutes(new [] { "r1", "r2" });

            checkConfig(c);            
        }

        [TestMethod]
        public void Markup()
        {
            var c = new DotvvmConfiguration();

            c.Markup.DefaultDirectives.Add("dir1", "MyDirective");
            c.Markup.AddCodeControls("myControls", typeof(ControlLifeCycleMock));
            c.Markup.AddMarkupControl("myControls", "C1", "./Controls/C1.dotcontrol");
            c.Markup.AddServiceImport("myService", typeof(ConfigurationSerializationTests));
            c.Markup.AddServiceImport("secondService", typeof(Func<IEnumerable<Lazy<IServiceProvider>>>));
            c.Markup.ImportedNamespaces.Add(new NamespaceImport("System"));
            c.Markup.ImportedNamespaces.Add(new NamespaceImport("System.Collections.Generic"));
            c.Markup.ImportedNamespaces.Add(new NamespaceImport("System.Collections", "Collections"));
            c.Markup.HtmlAttributeTransforms.Add(new HtmlTagAttributePair { AttributeName = "data-uri", TagName = "div" }, new HtmlAttributeTransformConfiguration { Type = typeof(TranslateVirtualPathHtmlAttributeTransformer) });

            checkConfig(c);            
        }

        [TestMethod]
        public void RestAPI()
        {
            var c = new DotvvmConfiguration();

            c.RegisterApiClient(typeof(Binding.TestApiClient), "http://server/api", "./apiscript.js", "_testApi");

            checkConfig(c);            
        }

        [TestMethod]
        public void AuxOptions()
        {
            var c = new DotvvmConfiguration();

            c.Debug = true;
            c.ApplicationPhysicalPath = "/opt/myApp";
            c.ClientSideValidation = false;
            c.DefaultCulture = "cs-CZ";
            c.UseHistoryApiSpaNavigation = true;

            checkConfig(c);            
        }

    }
}
