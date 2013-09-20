using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace Inpress.Web
{
    public class ProjectService
    {
        private readonly IProjectRepository repository;

        public ProjectService()
        {
            repository = new ProjectRepository();
        }

        public Project Get(int id)
        {
            return repository.GetAll().Where(p => p.Id == id).SingleOrDefault();
        }

        public IList<Project> GetAll()
        {
            return repository.GetAll();
        }
    }
}
