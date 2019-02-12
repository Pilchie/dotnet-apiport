// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability;
using Microsoft.Fx.Portability.Analyzer;
using Microsoft.Fx.Portability.ObjectModel;
using Microsoft.Fx.Portability.Reporting;
using System;
using System.Collections.Immutable;
using System.IO;
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
            var progressReporter = new NullProgressReporter();
            var service = new FileOutputApiPortService(progressReporter);
            var assemblyFilter = new DotNetFrameworkFilter();
            var dependencyFinder = new ReflectionMetadataDependencyFinder(
                assemblyFilter,
                new SystemObjectFinder(assemblyFilter));

            _client = new ApiPortClient(
                service,
                progressReporter,
                new TargetMapper(),
                dependencyFinder,
                new ReportGenerator(),
                new IgnoreAssemblyInfo[0],
                new ReportFileWriter(
                    new WindowsFileSystem(),
                    progressReporter));

            _options = new ReadWriteApiPortOptions
            {
                Targets = new[] { "netcoreapp3.0", "netstandard2.0" },
                OutputFormats = new[] { "json" },
                InvalidInputFiles = new string[0],
                OutputFileName = @"C:\code\Setup-Engine\src\Setup.Operations\bin\Debug\api-port-results.json",
                OverwriteOutputFile = true,
                RequestFlags = AnalyzeRequestFlags.NoTelemetry,
            };
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
