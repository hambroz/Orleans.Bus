﻿using System;
using System.IO;
using System.Linq;
using System.Net;

using NUnit.Framework;

using Orleans.IoC;
using Orleans.Host.SiloHost;

[assembly: OrleansSiloForTestingAction]

namespace Orleans.IoC
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public class OrleansSiloForTestingActionAttribute : TestActionAttribute
    {
        OrleansSiloForTesting silo;

        public override void BeforeTest(TestDetails details)
        {
            if (details.IsSuite)
                silo = new OrleansSiloForTesting();
        }

        public override void AfterTest(TestDetails details)
        {
            if (details.IsSuite)
                silo.Dispose();
        }

        public override ActionTargets Targets
        {
            get { return ActionTargets.Suite; }
        }
    }

    public class OrleansSiloForTesting : IDisposable
    {
        static OrleansSiloHost host;
        static AppDomain domain;

        public OrleansSiloForTesting()
        {
            domain = AppDomain.CreateDomain("OrleansSiloForTesting", null, new AppDomainSetup
            {
                AppDomainInitializer = Start,
                AppDomainInitializerArguments = new string[0],
                ApplicationBase = AppDomain.CurrentDomain.BaseDirectory
            });

            var clientConfigFileName = ConfigurationFilePath("OrleansClientConfigurationForTesting.xml");
            OrleansClient.Initialize(clientConfigFileName);
        }

        public void Dispose()
        {
            domain.DoCallBack(Shutdown);
            AppDomain.Unload(domain);
        }

        static void Start(string[] args)
        {
            var serverConfigFileName = ConfigurationFilePath("OrleansServerConfigurationForTesting.xml");
            host = new OrleansSiloHost(Dns.GetHostName()) { ConfigFileName = serverConfigFileName };

            host.LoadOrleansConfig();
            host.InitializeOrleansSilo();

            host.Config.Globals.ReminderServiceType =
                GlobalConfiguration.ReminderServiceProviderType.ReminderTableGrain;

            host.StartOrleansSilo();
        }

        static void Shutdown()
        {
            if (host == null)
                return;

            host.StopOrleansSilo();
            host.Dispose();

            host = null;
        }

        static string ConfigurationFilePath(string configFileName)
        {
            var outputDirectory = AppDomain.CurrentDomain.BaseDirectory;
            return Path.Combine(outputDirectory, @"Utility\" + configFileName);
        }
    }
}
