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
            };
        }

        public bool AddFileToAnalyze(string path, Stream stream)
        {
            _options.InputAssemblies.Add(new StreamAssemblyFile(path, stream), true);
            return true;
        }

        public async Task AnalyzeAll()
        {
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
