// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability;
using System;
using System.IO;

namespace ManifestValidation
{
    internal class StreamAssemblyFile : IAssemblyFile, IDisposable
    {
        private Stream _stream;

        public StreamAssemblyFile(string path, Stream stream)
        {
            Name = path;
            _stream = new MemoryStream();
            stream.CopyTo(_stream);
            _stream.Seek(0L, SeekOrigin.Begin);
        }

        public string Name { get; }

        public string Version => string.Empty;

        public bool Exists => true;

        public Stream OpenRead()
        {
            return _stream;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _stream?.Dispose();
            }

            _stream = null;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
