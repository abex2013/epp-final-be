using Excellerent.ProjectManagement.Domain.Interfaces.RepositoryInterface;
using Excellerent.ProjectManagement.Domain.Models;
using Excellerent.ProjectManagement.Domain.Services.Helpers;
using Excellerent.SharedInfrastructure.Context;
using Excellerent.SharedInfrastructure.Repository;
using Excellerent.SharedModules.Seed;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Excellerent.ProjectManagement.Infrastructure.Repositories
{
    public class ProjectRepository : AsyncRepository<Project>, IProjectRepository
    {
        private readonly EPPContext _context;
        private  IUnitOfWork _unitOfWork;
        public ProjectRepository(EPPContext context ) : base(context)
        {
            _context = context;
            _unitOfWork = UnitOfWork;
        }

        public async Task<IEnumerable<Project>> GetProjectByName(string ProjectName)
        {
            try
            {
                IEnumerable<Project> project = (await base.GetQueryAsync(x => x.ProjectName == ProjectName)).ToList();
                return project;
            }
            catch (Exception)
            {

                throw;
            }
        }
        public async Task<Project> GetProjectById(Guid id)
        {
            try
            {
                return (await _context.Project.FindAsync(id));
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<IEnumerable<Project>> GetProjectFullData()
        {
            try
            {
                try
                {
                    return _context.Project.Where(p => !p.IsDeleted).Include(x => x.Client).Include(x => x.ProjectStatus).ToList();

                }
                catch (Exception)
                {

                    throw;
                }

            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<IEnumerable<Project>> GetPaginatedProject(Expression<Func<Project, bool>> predicate, PaginationParams paginationParams)

        {
            paginationParams.pageSize = paginationParams.pageSize ?? 10;
            paginationParams.pageIndex = paginationParams.pageIndex ?? 1;
            if (paginationParams.SortField != null)
            {
                switch (paginationParams.SortField)
                {
                    case "Project":
                        if (paginationParams.sortOrder == SharedModules.Seed.SortOrder.Descending)
                        {
                            return predicate == null ? (await _context.Project.OrderByDescending(x => x.ProjectName).Where(x=>x.IsDeleted.Equals(false)).Skip((int)((paginationParams.pageIndex - 1) * paginationParams.pageSize)).Take((int)paginationParams.pageSize).Include(x => x.Client).Include(y => y.ProjectStatus).ToListAsync())
                       : (await _context.Project.Where(predicate: predicate).OrderByDescending(x => x.ProjectName).Skip((int)((paginationParams.pageIndex - 1) * paginationParams.pageSize)).Take((int)paginationParams.pageSize).Include(x => x.Client).Include(y => y.ProjectStatus).ToListAsync());

                        }
                        else
                        {
                            return predicate == null ? (await _context.Project.OrderBy(x => x.ProjectName).Where(x => x.IsDeleted.Equals(false)).Skip((int)((paginationParams.pageIndex - 1) * paginationParams.pageSize)).Take((int)paginationParams.pageSize).Include(x => x.Client).Include(y => y.ProjectStatus).ToListAsync())
                            : (await _context.Project.Where(predicate: predicate).OrderBy(x => x.ProjectName).Skip((int)((paginationParams.pageIndex - 1) * paginationParams.pageSize)).Take((int)paginationParams.pageSize).Include(x => x.Client).Include(y => y.ProjectStatus).ToListAsync());
                        }
                    case "Client":
                        if (paginationParams.sortOrder == SharedModules.Seed.SortOrder.Descending)
                        {
                            return predicate == null ? (await _context.Project.Where(x => x.IsDeleted.Equals(false)).OrderByDescending(x => x.Client.ClientName).Skip((int)((paginationParams.pageIndex - 1) * paginationParams.pageSize)).Take((int)paginationParams.pageSize).Include(x => x.Client).Include(y => y.ProjectStatus).ToListAsync())
                       : (await _context.Project.Where(predicate: predicate).OrderByDescending(x => x.Client.ClientName).Skip((int)((paginationParams.pageIndex - 1) * paginationParams.pageSize)).Take((int)paginationParams.pageSize).Include(x => x.Client).Include(y => y.ProjectStatus).ToListAsync());

                        }
                        else
                        {
                            return predicate == null ? (await _context.Project.Where(x => x.IsDeleted.Equals(false)).OrderBy(x => x.Client.ClientName).Skip((int)((paginationParams.pageIndex - 1) * paginationParams.pageSize)).Take((int)paginationParams.pageSize).Include(x => x.Client).Include(y => y.ProjectStatus).ToListAsync())
                            : (await _context.Project.Where(predicate: predicate).OrderBy(x => x.Client.ClientName).Skip((int)((paginationParams.pageIndex - 1) * paginationParams.pageSize)).Take((int)paginationParams.pageSize).Include(x => x.Client).Include(y => y.ProjectStatus).ToListAsync());
                        }
                    case "status":
                        if (paginationParams.sortOrder == SharedModules.Seed.SortOrder.Descending)
                        {
                            return predicate == null ? (await _context.Project.Where(x => x.IsDeleted.Equals(false)).OrderByDescending(x => x.ProjectStatus.StatusName).Skip((int)((paginationParams.pageIndex - 1) * paginationParams.pageSize)).Take((int)paginationParams.pageSize).Include(x => x.Client).Include(y => y.ProjectStatus).ToListAsync())
                       : (await _context.Project.Where(predicate: predicate).OrderByDescending(x => x.ProjectStatus.StatusName).Skip((int)((paginationParams.pageIndex - 1) * paginationParams.pageSize)).Take((int)paginationParams.pageSize).Include(x => x.Client).Include(y => y.ProjectStatus).ToListAsync());

                        }
                        else
                        {
                            return predicate == null ? (await _context.Project.Where(x => x.IsDeleted.Equals(false)).OrderBy(x => x.ProjectStatus.StatusName).Skip((int)((paginationParams.pageIndex - 1) * paginationParams.pageSize)).Take((int)paginationParams.pageSize).Include(x => x.Client).Include(y => y.ProjectStatus).ToListAsync())
                            : (await _context.Project.Where(predicate: predicate).OrderBy(x => x.ProjectStatus.StatusName).Skip((int)((paginationParams.pageIndex - 1) * paginationParams.pageSize)).Take((int)paginationParams.pageSize).Include(x => x.Client).Include(y => y.ProjectStatus).ToListAsync());
                        }
                    case "supervisor":
                        if (paginationParams.sortOrder == SharedModules.Seed.SortOrder.Descending)
                        {
                            return predicate == null ? (await _context.Project.Where(x => x.IsDeleted.Equals(false)).OrderByDescending(x => x.SupervisorGuid).Skip((int)((paginationParams.pageIndex - 1) * paginationParams.pageSize)).Take((int)paginationParams.pageSize).Include(x => x.Client).Include(y => y.ProjectStatus).ToListAsync())
                       : (await _context.Project.Where(predicate: predicate).OrderByDescending(x => x.SupervisorGuid).Skip((int)((paginationParams.pageIndex - 1) * paginationParams.pageSize)).Take((int)paginationParams.pageSize).Include(x => x.Client).Include(y => y.ProjectStatus).ToListAsync());

                        }
                        else
                        {
                            return predicate == null ? (await _context.Project.Where(x => x.IsDeleted.Equals(false)).OrderBy(x => x.SupervisorGuid).Skip((int)((paginationParams.pageIndex - 1) * paginationParams.pageSize)).Take((int)paginationParams.pageSize).Include(x => x.Client).Include(y => y.ProjectStatus).ToListAsync())
                            : (await _context.Project.Where(predicate: predicate).OrderBy(x => x.SupervisorGuid).Skip((int)((paginationParams.pageIndex - 1) * paginationParams.pageSize)).Take((int)paginationParams.pageSize).Include(x => x.Client).Include(y => y.ProjectStatus).ToListAsync());
                        }
                    default:
                        return predicate == null ? (await _context.Project.Where(x => x.IsDeleted.Equals(false)).OrderByDescending(x => x.CreatedDate).Skip((int)((paginationParams.pageIndex - 1) * paginationParams.pageSize)).Take((int)paginationParams.pageSize).Include(x => x.Client).Include(y => y.ProjectStatus).ToListAsync())
                            : (await _context.Project.Where(predicate: predicate).OrderByDescending(x => x.CreatedDate).Skip((int)((paginationParams.pageIndex - 1) * paginationParams.pageSize)).Take((int)paginationParams.pageSize).Include(x => x.Client).Include(y => y.ProjectStatus).ToListAsync());


                }

            }
            else
            {
                return predicate == null ? (await _context.Project.Where(x => x.IsDeleted.Equals(false)).OrderByDescending(x => x.CreatedDate).Skip((int)((paginationParams.pageIndex - 1) * paginationParams.pageSize)).Take((int)paginationParams.pageSize).Include(x => x.Client).Include(y => y.ProjectStatus).ToListAsync())
             : (await _context.Project.Where(predicate: predicate).OrderByDescending(x => x.CreatedDate).Skip((int)((paginationParams.pageIndex - 1) * paginationParams.pageSize)).Take((int)paginationParams.pageSize).Include(x => x.Client).Include(y => y.ProjectStatus).ToListAsync());

            }
        }

        public async Task addProjectWithResources(Project project,List<AssignResourcEntity> assignResources)
        {
            try
            {
              await _unitOfWork.BeginTransaction();
                await  _context.Project.AddAsync(project);
                 await _context.SaveChangesAsync();
                foreach (var assignResource in assignResources)
                {
                    assignResource.ProjectGuid = project.Guid;
                   await  _context.AssignResources.AddAsync(assignResource);
                    await _context.SaveChangesAsync();
                }
                
              await   _unitOfWork.CommitTransaction();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollBackTransaction();
            }
               
        }
       
    }
}
