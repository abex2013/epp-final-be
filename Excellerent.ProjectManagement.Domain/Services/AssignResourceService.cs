using Excellerent.ProjectManagement.Domain.DTOs;
using Excellerent.ProjectManagement.Domain.Entities;
using Excellerent.ProjectManagement.Domain.Interfaces.RepositoryInterface;
using Excellerent.ProjectManagement.Domain.Interfaces.ServiceInterface;
using Excellerent.ProjectManagement.Domain.Models;
using Excellerent.SharedModules.DTO;
using Excellerent.SharedModules.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Excellerent.ProjectManagement.Domain.Services
{
    public class AssignResourceService : CRUD<AssignResourceEntity, AssignResourcEntity>, IAssignResourceService
    {
        private readonly IAssignResourceRepository _repository;
        public AssignResourceService(IAssignResourceRepository repository) : base(repository)
        {
            _repository = repository;
        }
        public Task<AssignResourcEntity> GetOneAssignResource(Guid id)
        {
            return _repository.GetByGuidAsync(id);
        }

        public Task<IEnumerable<AssignResourcEntity>> GetProjectIdsByEmployee(Guid empId)
        {

            return _repository.GetProjectIdsByEmployee(empId);
            
        }

      
        public async  Task<ResponseDTO> GetAssignResourceByProject(Guid projectGuid)
        {
            try
            {
                IEnumerable< AssignResourcEntity > projectresource=  await _repository.GetAssignResourceByProject(projectGuid);

                return new ResponseDTO
                {
                    Data = projectresource.Select(p => new ProjectAssignedResourceDto(p)).ToList(),
                    Message = "project's resourses",
                    Ex = null,
                    ResponseStatus = ResponseStatus.Success
                };

            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    Data = null,
                    Message = null,
                    Ex = null,
                    ResponseStatus = ResponseStatus.Error
                };

            }

        }

      
        }
}
