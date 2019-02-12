// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability;
using System;
using System.Collections.Generic;

namespace ManifestValidation
{
    public class NullProgressReporter : IProgressReporter
    {
        private readonly List<string> _issues = new List<string>();

        public IReadOnlyCollection<string> Issues => _issues.AsReadOnly();

        public void ReportIssue(string issue)
        {
            Console.WriteLine($"***** Issue reported: '{issue}'");
            _issues.Add(issue);
        }

        public void Resume()
        {
        }

        public IProgressTask StartTask(string taskName, int totalUnits)
        {
            Console.WriteLine($"Starting '{taskName}'");
            return new NullProgressTask();
        }

        public IProgressTask StartTask(string taskName)
        {
            Console.WriteLine($"Starting '{taskName}'");
            return new NullProgressTask();
        }

        public void Suspend()
        {
        }

        protected virtual void Dispose(bool disposing)
        {
            // Nothing to do for this type.
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private class NullProgressTask : IProgressTask
        {
            public void Abort()
            {
            }

            public void ReportUnitComplete()
            {
            }

            protected virtual void Dispose(bool disposing)
            {
                // Nothing to do for this type.
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
        }
    }
}
