using System;
using System.Linq;
using System.Threading.Tasks;

namespace Orleans.Bus
{
    class GrainObserverService
    {
        public static readonly GrainObserverService Instance = new GrainObserverService();

        GrainObserverService()
        {}

        public Task<Observes> CreateProxy(Observes client)
        {
            return ObservesFactory.CreateObjectReference(client);
        }

        public void DeleteProxy(Observes proxy) 
        {
            ObservesFactory.DeleteObjectReference(proxy);
        }
    }
}
