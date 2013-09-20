using System.Collections.Generic;

namespace Inpress.Web
{
    internal interface IRepository <tEntity>
    {
        IList<tEntity> GetAll();
        void Save();
        void Delete();
    }
}
