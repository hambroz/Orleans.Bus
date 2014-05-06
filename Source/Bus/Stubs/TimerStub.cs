using System;
using System.Linq;

namespace Orleans.Bus.Stubs
{
    public class TimerStub : IOrleansTimer
    {
        void IDisposable.Dispose()
        {}
    }
}