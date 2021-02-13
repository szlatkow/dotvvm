﻿using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;
using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using Riganti.Selenium.Core;
using Riganti.Selenium.Core.Abstractions;
using Xunit;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests.Feature
{
    public class PageModuleTests : AppSeleniumTest
    {
        public PageModuleTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void Feature_PageModules_ModuleInMarkupControl()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_PageModules_ModuleInMarkupControl);
                browser.Wait();

                var log = browser.Single("#log");
                browser.WaitFor(() => AssertLastLogEntry(log, "testPageModule: init"), 5000);

                var moduleButtons = browser.FindElements("input[type=button]");
                var incrementValue = browser.First(".increment-value");
                var result = browser.First(".named-command-result");
                TestModule(browser, log, moduleButtons, incrementValue, result, "testPageModule");
            });
        }

        [Fact]
        public void Feature_PageModules_ModuleInMarkupControlTwice()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_PageModules_ModuleInMarkupControlTwice);
                browser.Wait();

                var log = browser.Single("#log");
                browser.WaitFor(() => AssertLastLogEntry(log, "testPageModule: init"), 5000);

                var toggleButton = browser.Single(".toggle input[type=button]");

                // test first instance
                var moduleButtons = browser.FindElements(".control1 input[type=button]");
                var incrementValue = browser.First(".control1 .increment-value");
                var result = browser.First(".control1 .named-command-result");
                TestModule(browser, log, moduleButtons, incrementValue, result, "testPageModule");

                // show second instance
                toggleButton.Click();
                browser.WaitFor(() => AssertLastLogEntry(log, "testPageModule: init"), 5000);

                // test second instance
                moduleButtons = browser.FindElements(".control2 input[type=button]");
                incrementValue = browser.First(".control2 .increment-value");
                result = browser.First(".control2 .named-command-result");
                TestModule(browser, log, moduleButtons, incrementValue, result, "testPageModule");

                // hide second instance
                toggleButton.Click();
                browser.WaitFor(() => AssertLastLogEntry(log, "testPageModule: dispose"), 5000);

                // show second instance
                toggleButton.Click();
                browser.WaitFor(() => AssertLastLogEntry(log, "testPageModule: init"), 5000);
            });
        }

        [Fact]
        public void Feature_PageModules_ModuleInPage()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_PageModules_ModuleInPage);
                browser.Wait();

                var log = browser.Single("#log");
                browser.WaitFor(() => AssertLastLogEntry(log, "testPageModule: init"), 5000);

                var moduleButtons = browser.FindElements("input[type=button]");
                var incrementValue = browser.First(".increment-value");
                var result = browser.First(".named-command-result");
                TestModule(browser, log, moduleButtons, incrementValue, result, "testPageModule");
            });
        }

        [Fact]
        public void Feature_PageModules_ModuleInPageCommandAmbiguous()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_PageModules_ModuleInPageCommandAmbiguous);
                browser.Wait();

                var log = browser.Single("#log");
                browser.WaitFor(() => AssertLogEntry(log, "testPageModule: init"), 5000);
                browser.WaitFor(() => AssertLogEntry(log, "testPageModule2: init"), 5000);

                browser.First("input[type=button]").Click();
                browser.Wait(5000);
                AssertUI.InnerText(log, t => !t.Contains("testPageModule: commands.noArgs()"));
                AssertUI.InnerText(log, t => !t.Contains("testPageModule2: commands.noArgs()"));
            });
        }

        [Fact]
        public void Feature_PageModules_ModuleInPageMasterPage()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_PageModules_ModuleInPageMasterPage);
                browser.Wait();

                var log = browser.Single("#log");
                browser.WaitFor(() => AssertLogEntry(log, "testPageModule: init"), 5000);
                browser.WaitFor(() => AssertLogEntry(log, "testPageModule2: init"), 5000);

                var moduleButtons = browser.FindElements(".master input[type=button]");
                var incrementValue = browser.First(".master .increment-value");
                var result = browser.First(".master .named-command-result");
                TestModule(browser, log, moduleButtons, incrementValue, result, "testPageModule");

                moduleButtons = browser.FindElements(".page input[type=button]");
                incrementValue = browser.First(".page .increment-value");
                result = browser.First(".page .named-command-result");
                TestModule(browser, log, moduleButtons, incrementValue, result, "testPageModule2");
            });
        }

        [Fact]
        public void Feature_PageModules_ModuleInPageSpaMasterPage()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_PageModules_ModuleInPageSpaMasterPage2);
                browser.Wait();

                var links = browser.FindElements("a");

                var log = browser.Single("#log");
                browser.WaitFor(() => AssertLastLogEntry(log, "testPageModule: init"), 5000);
                browser.Wait(5000);
                AssertUI.InnerText(log, t => !t.Contains("testPageModule2: init"));

                links[0].Click();
                browser.WaitFor(() => AssertLastLogEntry(log, "testPageModule2: init"), 5000);

                var moduleButtons = browser.FindElements(".master input[type=button]");
                var incrementValue = browser.First(".master .increment-value");
                var result = browser.First(".master .named-command-result");
                TestModule(browser, log, moduleButtons, incrementValue, result, "testPageModule");

                moduleButtons = browser.FindElements(".page input[type=button]");
                incrementValue = browser.First(".page .increment-value");
                result = browser.First(".page .named-command-result");
                TestModule(browser, log, moduleButtons, incrementValue, result, "testPageModule2");

                links[1].Click();
                browser.WaitFor(() => AssertLastLogEntry(log, "testPageModule2: dispose"), 5000);

                links[0].Click();
                browser.WaitFor(() => AssertLastLogEntry(log, "testPageModule2: init"), 5000);
            });
        }

        private void TestModule(IBrowserWrapper browser, IElementWrapper log, IElementWrapperCollection<IElementWrapper, IBrowserWrapper> moduleButtons, IElementWrapper incrementValue, IElementWrapper result, string prefix)
        {
            moduleButtons[0].Click();
            browser.WaitFor(() => AssertLastLogEntry(log, prefix + ": commands.noArgs()"), 5000);
            moduleButtons[1].Click();
            browser.WaitFor(() => AssertLastLogEntry(log, prefix + ": commands.oneArg(10)"), 5000);
            moduleButtons[2].Click();
            browser.WaitFor(() => AssertLastLogEntry(log, prefix + @": commands.twoArgs(10, {""Test"":""Hello""})"), 5000);

            AssertUI.InnerTextEquals(incrementValue, "0");
            moduleButtons[3].Click();
            browser.WaitFor(() => AssertLastLogEntry(log, prefix + ": commands.syncIncrement(0)"), 5000);
            AssertUI.InnerTextEquals(incrementValue, "1");
            moduleButtons[4].Click();
            browser.WaitFor(() => AssertLastLogEntry(log, prefix + ": commands.asyncIncrement(1) begin"), 5000);
            browser.WaitFor(() => AssertLastLogEntry(log, prefix + ": commands.asyncIncrement(1) end"), 5000);
            AssertUI.InnerTextEquals(incrementValue, "2");
            moduleButtons[5].Click();
            browser.WaitFor(() => AssertLastLogEntry(log, prefix + ": commands.callIncrementCommand(2)"), 5000);
            AssertUI.InnerTextEquals(incrementValue, "3");

            moduleButtons[6].Click();
            browser.WaitFor(() => AssertLastLogEntry(log, prefix + ": commands.callSetResultCommand()"), 5000);
            AssertUI.InnerTextEquals(result, "1_test_abc");
        }


        private void AssertLastLogEntry(IElementWrapper log, string entry)
        {
            AssertUI.InnerText(log, t => t.Substring(t.LastIndexOf("\n") + 1).Contains(entry));
        }
        private void AssertLogEntry(IElementWrapper log, string entry)
        {
            AssertUI.InnerText(log, t => t.Contains(entry));
        }
    }
}
