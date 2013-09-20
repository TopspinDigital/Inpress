using System;
using System.Collections.Generic;

namespace Inpress.Web
{
    internal class ProjectRepository : IProjectRepository
    {
        public IList<Project> GetAll()
        {
            var project = new Project()
            {
                Id = 1,
                Name = "Etisalat",
                Location = "/Source/Etisalat"
            };

            var list = new List<Project>();
            list.Add(project);

            return list;
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
