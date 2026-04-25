using System;
using System.Runtime.InteropServices;
using CONTROLBUILDERLib;

namespace Fin
{
    internal sealed class ControlBuilderSession : IDisposable
    {
        private CBOpenIF _client;
        private bool _disposed;

        public ControlBuilderSession()
        {
            Log.Info("Initializing CONTROLBUILDERLib...");
            _client = new CBOpenIF();
        }

        public CBOpenIF Client
        {
            get
            {
                if (_disposed) throw new ObjectDisposedException(nameof(ControlBuilderSession));
                return _client;
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            if (_client != null)
            {
                Marshal.ReleaseComObject(_client);
                _client = null;
                Log.Info("Released Control Builder COM object.");
            }
        }
    }
}
