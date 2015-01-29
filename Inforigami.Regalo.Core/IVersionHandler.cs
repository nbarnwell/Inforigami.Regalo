using System;

namespace Inforigami.Regalo.Core
{
    public interface IVersionHandler
    {
        Guid GetVersion(object evt);
        void SetParentVersion(object evt, Guid? parentVersion);
    }
}
