// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Autofac;
using Microsoft.Fx.Portability;
using Microsoft.Fx.Portability.Analysis;
using Microsoft.Fx.Portability.Analyzer;
using Microsoft.Fx.Portability.ObjectModel;
using Microsoft.Fx.Portability.Reporting;
using Microsoft.Fx.Portability.Reports;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Runtime.Versioning;
using System.Threading.Tasks;

namespace ManifestValidation
{
    public class AssemblyValidator
    {
        public static AssemblyValidator Instance { get; } = new AssemblyValidator();

        private readonly ApiPortClient _client;
        private readonly ReadWriteApiPortOptions _options;

        private ImmutableDictionary<IAssemblyFile, bool> _assemblies = ImmutableDictionary<IAssemblyFile, bool>.Empty;

        private AssemblyValidator()
        {
            _options = new ReadWriteApiPortOptions
            {
                Targets = new[]
                {
                    new FrameworkName(".NET Core", new Version(3, 0)).ToString(),
                    new FrameworkName(".NET Standard", new Version(2, 0)).ToString(),
                },
                OutputFormats = new[] { "json" },
                InvalidInputFiles = new string[0],
                OutputFileName = @"C:\code\Setup-Engine\src\Setup.Operations\bin\Debug\api-port-results.json",
                OverwriteOutputFile = true,
                RequestFlags = AnalyzeRequestFlags.NoTelemetry,
            };

            var builder = new ContainerBuilder();
            builder.RegisterType<TargetMapper>()
                .As<ITargetMapper>()
                .SingleInstance();

            builder.RegisterInstance(new ProductInformation("ManifestValidator"));

            builder.RegisterType<FileIgnoreAssemblyInfoList>()
                .As<IEnumerable<IgnoreAssemblyInfo>>()
                .SingleInstance();

            builder.RegisterType<ReflectionMetadataDependencyFinder>()
                .As<IDependencyFinder>()
                .SingleInstance();

            builder.RegisterType<DotNetFrameworkFilter>()
                .As<IDependencyFilter>()
                .SingleInstance();

            builder.RegisterType<ReportGenerator>()
                .As<IReportGenerator>()
                .SingleInstance();

            builder.RegisterType<ApiPortClient>()
                .SingleInstance();

            builder.RegisterType<ApiPortService>()
                .SingleInstance();

            builder.RegisterType<WindowsFileSystem>()
                .As<IFileSystem>()
                .SingleInstance();

            builder.RegisterType<ReportFileWriter>()
                .As<IFileWriter>()
                .SingleInstance();

            builder.RegisterType<RequestAnalyzer>()
                .As<IRequestAnalyzer>()
                .SingleInstance();

            builder.RegisterType<AnalysisEngine>()
                .As<IAnalysisEngine>()
                .SingleInstance();

            builder.RegisterType<SystemObjectFinder>()
                .SingleInstance();

            builder.RegisterInstance<IApiPortOptions>(_options);

            ////builder.RegisterType<DocIdSearchRepl>();

            ////builder.RegisterType<ApiPortServiceSearcher>()
            ////    .As<ISearcher<string>>()
            ////    .SingleInstance();

            builder.RegisterType<NullProgressReporter>()
                .As<IProgressReporter>()
                .SingleInstance();

            builder.RegisterType<JsonReportWriter>()
                .As<IReportWriter>()
                .SingleInstance();

            builder.RegisterModule(new OfflineDataModule("json"));

            var container = builder.Build();

            _client = container.Resolve<ApiPortClient>();
        }

        public bool AddFileToAnalyze(string path, Stream stream)
        {
            ImmutableInterlocked.AddOrUpdate(ref _assemblies, new StreamAssemblyFile(path, stream), _ => true, (k, v) => true);
            return true;
        }

        public async Task AnalyzeAll()
        {
            _options.InputAssemblies = _assemblies;
            await _client.WriteAnalysisReportsAsync(_options);

            foreach (var (asm, _) in _options.InputAssemblies)
            {
                if (asm is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }
    }
}
