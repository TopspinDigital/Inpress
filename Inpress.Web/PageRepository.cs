using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inpress.Web
{
    public class PageRepository : IPageRepository
    {
        private readonly int projectId;

        public PageRepository(int projectId)
        {
            this.projectId = projectId;
        }
    
        public IList<Page> GetAll()
        {
            throw new NotImplementedException();
        }

        public void Save()
        {
 	        throw new NotImplementedException();
        }

        public void Delete()
        {
 	        throw new NotImplementedException();
        }
    }
}
