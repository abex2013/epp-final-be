using Excellerent.ClientManagement.Domain.DTOs;
using Excellerent.ClientManagement.Domain.Entities;
using Excellerent.ClientManagement.Domain.Interfaces.RepositoryInterface;
using Excellerent.ClientManagement.Domain.Interfaces.ServiceInterface;
using Excellerent.ProjectManagement.Domain.DTOs;
using Excellerent.ProjectManagement.Domain.Entities;
using Excellerent.ProjectManagement.Domain.Interfaces.RepositoryInterface;
using Excellerent.ProjectManagement.Domain.Interfaces.ServiceInterface;
using Excellerent.ProjectManagement.Domain.Models;
using Excellerent.ProjectManagement.Domain.Services.Helpers;
using Excellerent.SharedModules.DTO;

using Excellerent.SharedModules.Services;
using LinqKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Excellerent.ProjectManagement.Domain.Services
{
    public class ProjectService : CRUD<ProjectEntity, Project>, IProjectService
    {
        private readonly IProjectRepository _projectRepository;
        private readonly ResourceManagement.Domain.Interfaces.Services.IEmployeeService _employeeService;
        private readonly IAssignResourceService _assignResourceService;
        private readonly IClientDetailsRepository _clientDetailsRepository;
        private readonly IProjectStatusRepository _projectStatusRepository;
        private readonly IClientDetailsService _clientDetailsService;
      
       
        public ProjectService(IProjectStatusRepository projectStatusRepository,
            IClientDetailsRepository clientDetailsRepository, IProjectRepository ProjectRepository,
            ResourceManagement.Domain.Interfaces.Services.IEmployeeService employeeService, IAssignResourceService
            assignResourceService, IClientDetailsService clientDetailsService) : base(ProjectRepository)

        {
            _projectRepository = ProjectRepository;
            _employeeService = employeeService;
            _assignResourceService = assignResourceService;
            _clientDetailsService = clientDetailsService;
            _clientDetailsRepository = clientDetailsRepository;
            _projectStatusRepository = projectStatusRepository;
        }
        public async Task<IEnumerable<ProjectEntity>> GetProjectByName(string projectName)
        {
            var data = _projectRepository.GetProjectByName(projectName);
            return (await data).Select(p => new ProjectEntity(p));
        }
        public async Task<Project> GetProjectById(Guid id)
        {
            return (await _projectRepository.GetProjectById(id));
        }
        public async Task<IEnumerable<ProjectEntity>> GetProjectFullData()
        {
            var data = _projectRepository.GetProjectFullData();
            return (await data).Select(p => new ProjectEntity(p));
        }

       
       
        public async Task<PredicatedResponseDTO> GetPaginatedProject(PaginationParams paginationParams)
        {

            int itemPerPage = paginationParams.pageSize ?? 10;
            int PageIndex = paginationParams.pageIndex ?? 1;
            
            int TotalRowCount = 0;
            var predicate = PredicateBuilder.New<Project>();
            var predicateclient = PredicateBuilder.New<Project>();
            var predicatestatus = PredicateBuilder.New<Project>();
            var predicatesupervisor = PredicateBuilder.New<Project>();
            List<ProjectEntity> projectEntities = new List<ProjectEntity>();
            if (paginationParams.client != null)
            {
              
                foreach (var client in paginationParams.client)
                {
                    predicateclient = predicateclient.Or(c => c.Client.ClientName == client && c.IsDeleted == false);
                }
            }

            if (paginationParams.status != null)
            {
                
                foreach (var client in paginationParams.status)
                {
                    predicatestatus = predicatestatus.Or(c => c.ProjectStatus.StatusName == client && c.IsDeleted == false);
                }
            }
            if (paginationParams.supervisorId != null)
            {
               
                foreach (var client in paginationParams.supervisorId)
                {
                    predicatesupervisor = predicatesupervisor.Or(c => c.SupervisorGuid == client && c.IsDeleted == false);
                }
            }
            

            if (paginationParams.id == null)
            {

                predicate = predicate.And(c => !c.IsDeleted);
                if (paginationParams.client != null)
                {

                    predicate = predicate.And(predicateclient);
                    predicate = string.IsNullOrEmpty(paginationParams.searchKey) ? predicate
                           : predicate.And(p => p.ProjectName.ToLower().Contains(paginationParams.searchKey.ToLower()) && p.IsDeleted == false);
                }
                if (paginationParams.status != null)
                {
                    predicate = predicate.And(predicatestatus);
                    predicate = string.IsNullOrEmpty(paginationParams.searchKey) ? predicate
                          : predicate.And(p => p.ProjectName.ToLower().Contains(paginationParams.searchKey.ToLower()) && p.IsDeleted == false);

                }
                if (paginationParams.supervisorId != null)
                {
                    predicate = predicate.And(predicatesupervisor);
                    predicate = string.IsNullOrEmpty(paginationParams.searchKey) ? predicate
                          : predicate.And(p => p.ProjectName.ToLower().Contains(paginationParams.searchKey.ToLower()) && p.IsDeleted == false);
                }
                
                if (((paginationParams.client == null) && (paginationParams.status == null))
                  && (paginationParams.supervisorId == null))
                {
                    predicate = string.IsNullOrEmpty(paginationParams.searchKey) ? predicate.And(p=>p.IsDeleted == false)
                     : predicate.And(p => p.ProjectName.ToLower().Contains(paginationParams.searchKey.ToLower()) && p.IsDeleted == false);


                }

                var projectData = (await _projectRepository.GetPaginatedProject(predicate, paginationParams))
                        .Select(p => new ProjectEntity(p)
                        ).ToList();
                foreach (var data in projectData)
                {
                    try
                    {
                        var supervisor = _employeeService.GetSelection(data.SupervisorGuid);

                        data.Supervisor = supervisor.Result;
                    }
                    catch (Exception ex) { }

                    projectEntities.Add(data);
                }
                TotalRowCount = (await _projectRepository.CountAsync(p => p.IsDeleted == false));
            }

            else
            {
                var eployeeProject = GetEmployeeProjects((Guid)paginationParams.id).Result.ToList();

                eployeeProject = string.IsNullOrEmpty(paginationParams.searchKey) ? eployeeProject.Where(x => x.IsDeleted == false).ToList()
                           : eployeeProject.Where(x => x.ProjectName.ToLower().Contains(paginationParams.searchKey.ToLower()) && x.IsDeleted == false).ToList();
                if (paginationParams.client != null)
                {foreach (var cleint in paginationParams.client)
                    {
                        eployeeProject = eployeeProject.FindAll(p => p.Client.ClientName == cleint && p.IsDeleted == false).ToList();
                    }
                }
                if (paginationParams.status != null)
                {
                    foreach (var status in paginationParams.status)
                    {
                        eployeeProject = eployeeProject.FindAll(p => p.ProjectStatus.StatusName == status && p.IsDeleted == false).ToList();
                    }
                }
                if (paginationParams.supervisorId != null)
                {
                    foreach (var supervisor in paginationParams.supervisorId)
                    {
                        eployeeProject = eployeeProject.FindAll(p => p.SupervisorGuid == supervisor && p.IsDeleted == false).ToList();
                    }
                }
                TotalRowCount = eployeeProject.Count(p=>p.IsDeleted==false);
                if (paginationParams.SortField != null)
                {

                    if (paginationParams.SortField == "Project") {
                        if (paginationParams.sortOrder == SharedModules.Seed.SortOrder.Descending)
                        {
                           var  eployeeProjects = eployeeProject.OrderByDescending(o => o.ProjectName).ToList();
                            eployeeProject = eployeeProjects;
                        }
                        else
                        {
                            var eployeeProjects = eployeeProject.OrderBy(o => o.ProjectName).ToList();
                            eployeeProject = eployeeProjects;
                        }
                       }
                    else if (paginationParams.SortField == "Client") {
                        if (paginationParams.sortOrder == SharedModules.Seed.SortOrder.Descending)
                        {
                            var eployeeProjects = eployeeProject.OrderByDescending(o => o.Client.ClientName).ToList();
                            eployeeProject = eployeeProjects;
                        }
                        else
                        {
                            var eployeeProjects = eployeeProject.OrderBy(o => o.Client.ClientName).ToList();
                            eployeeProject = eployeeProjects;
                        }
                    }
                    else if (paginationParams.SortField == "status") {
                        if (paginationParams.sortOrder == SharedModules.Seed.SortOrder.Descending)
                        {
                            var eployeeProjects = eployeeProject.OrderByDescending(o => o.ProjectStatus.StatusName).ToList();
                            eployeeProject = eployeeProjects;
                        }
                        else
                        {
                            var eployeeProjects = eployeeProject.OrderBy(o => o.ProjectStatus.StatusName).ToList();
                            eployeeProject = eployeeProjects;
                        }
                    }
                        else if (paginationParams.SortField== "supervisor") { 
                            if (paginationParams.sortOrder == SharedModules.Seed.SortOrder.Descending)
                        {
                            var eployeeProjects = eployeeProject.OrderByDescending(o => o.Supervisor.Name).ToList();
                            eployeeProject = eployeeProjects;
                        }
                        else
                        {
                            var eployeeProjects = eployeeProject.OrderBy(o => o.Supervisor.Name).ToList();
                            eployeeProject = eployeeProjects;
                        }
                    }
                        
                            

                    
                }
                else
                {
                    var eployeeProjects = eployeeProject.OrderByDescending(o => o.CreatedDate).ToList();
                    eployeeProject = eployeeProjects;

                }
                projectEntities = eployeeProject.Skip((PageIndex - 1) * itemPerPage).Take(itemPerPage).ToList();
                List<ProjectEntity> projects = new List<ProjectEntity>();
                foreach (var project in projectEntities)
                {

                    try
                    {
                        project.Client = await _clientDetailsRepository.GetByGuidAsync(project.ClientGuid);
                        project.ProjectStatus = await _projectStatusRepository.GetProjectStatusById(project.ProjectStatusGuid);
                        var supervisor = _employeeService.GetSelection(project.SupervisorGuid);

                        project.Supervisor = supervisor.Result;

                    }
                    catch (Exception ex) { 
                    }
                    projects.Add(project);
                }
                projectEntities = projects;
            }

            return new PredicatedResponseDTO
            {
                Data = projectEntities,
                TotalRecord = TotalRowCount,   //total row count
                PageIndex = PageIndex,
                PageSize = itemPerPage,  // itemPerPage,
                TotalPage = TotalRowCount % itemPerPage == 0 ? TotalRowCount / itemPerPage : TotalRowCount / itemPerPage + 1
            };

        }

        public async Task<IEnumerable<ProjectEntity>> GetEmployeeProjects(Guid empId)
        {
            var projIds = (await _assignResourceService.GetProjectIdsByEmployee(empId)).ToList();
            List<ProjectEntity> projects = new List<ProjectEntity>();
            foreach (var id in projIds)
            {

                var projectData = (await _projectRepository.GetByGuidAsync(id.ProjectGuid));
                ProjectEntity projEnt = new ProjectEntity(projectData);
                projects.Add(projEnt);
            }
            return projects;
        }
        public async Task<IEnumerable<ProjectDTO>> GetAllSupervisorProjects(Guid supervisorGuid)
        {
           return (await GetProjectFullData()).Where(x => x.SupervisorGuid == supervisorGuid).Select(x => new ProjectDTO(x));
        }

        public async Task<IEnumerable<ClientDTO>> GetEmployeeClient(Guid empId)
        {
            var projIds = (await _assignResourceService.GetProjectIdsByEmployee(empId)).ToList();
            List<ProjectEntity> projects = new List<ProjectEntity>();
            List<ClientDetailsEntity> client = new List<ClientDetailsEntity>();
            foreach (var id in projIds)
            {

                var projectData = (await _projectRepository.GetByGuidAsync(id.ProjectGuid));
                ProjectEntity projEnt = new ProjectEntity(projectData);
                projects.Add(projEnt);
            }
            foreach (var pro in projects)
            {
                var selectedClient = (await _clientDetailsService.GetClientById(pro.ClientGuid));
                ClientDetailsEntity clientDetailsEntity = new ClientDetailsEntity(selectedClient);
                client.Add(clientDetailsEntity);
            }
            return client.Select(p => new ClientDTO(p));
        }

        public async Task<IEnumerable<ProjectDTO>> GetClientProjects(Guid clientGuid)
        {
            return (await GetProjectFullData())
                .Where(x => x.ClientGuid == clientGuid)
                .Select(x => new ProjectDTO(x));
        }

        public async Task<IEnumerable<ClientDTO>> GetProjectClient(Guid projectID)
        {
            List<ClientDetailsEntity> client = new List<ClientDetailsEntity>();
            var projectData = (await _projectRepository.GetByGuidAsync(projectID));
            var selectedClient = (await _clientDetailsService.GetClientById(projectData.ClientGuid));
            ClientDetailsEntity clientDetailsEntity = new ClientDetailsEntity(selectedClient);
            client.Add(clientDetailsEntity);
           
            return client.Select(p => new ClientDTO(p));
        }

        Task<IEnumerable<ClientManagement.Domain.DTOs.ClientDTO>> IProjectService.GetEmployeeClient(Guid empId)
        {
            throw new NotImplementedException();
        }

        Task<IEnumerable<ClientManagement.Domain.DTOs.ClientDTO>> IProjectService.GetProjectClient(Guid projectID)
        {
            throw new NotImplementedException();
        }
  
    
        public async Task<ResponseDTO> GetFilterMenu()
        {
            
            List<ProjectForFilterDTO> superviserList = new List<ProjectForFilterDTO>();
            List<ProjectForFilterDTO> clientList = new List<ProjectForFilterDTO>();
            List<ProjectForFilterDTO> statusLists = new List<ProjectForFilterDTO>();
            
      
            var projects = GetProjectFullData().Result;

            foreach(var p in projects)
            {

                var client = new ProjectForFilterDTO
                {
                    Id =p.ClientGuid,
                    Name = p.Client.ClientName
                };

                var sup = _employeeService.GetEmployeesById(p.SupervisorGuid);
                var supervisor = new ProjectForFilterDTO
                {
                    Id = sup.Guid,
                    Name = sup.FirstName + " " + sup.FatherName
                };
                if (clientList.Where(c1 => c1.Id == client.Id).Count() == 0)
                    clientList.Add(client);
                if (superviserList.Where(c1 => c1.Id == supervisor.Id).Count() == 0)
                    superviserList.Add(supervisor);

           

            }

            var statusFilter = projects.GroupBy(x => x.ProjectStatus).Select(x => x.First())
                                             .Select(x => new { x.ProjectStatus.Guid, x.ProjectStatus.StatusName })
                                             .OrderBy(x => x.StatusName).ToList();

            foreach (var st in statusFilter)
            {
                var status = new ProjectForFilterDTO
                {
                    Id = st.Guid,
                    Name = st.StatusName
                };
                statusLists.Add(status);
            }

            return new ResponseDTO(ResponseStatus.Success, "Filter Data", new
            {
               
               Status = statusLists,
               Supervisor = superviserList,
               Clients = clientList

            });
        }

        public async Task<ResponseDTO> DeleteByState(Guid id)
        {
            var model = await _projectRepository.GetProjectById(id);

            if (model == null)
            {
                return new ResponseDTO
                {
                    ResponseStatus = ResponseStatus.Error,
                    Message = "There is no project with the given id!",
                    Data = null,
                };
            }
            else if (model.IsDeleted)
            {
                return new ResponseDTO
                {
                    ResponseStatus = ResponseStatus.Error,
                    Message = "The specified project is already deleted!",
                    Data = null,
                };
            }

            model.IsDeleted = true;
            await _projectRepository.UpdateAsync(model);
            return new ResponseDTO
            {
                ResponseStatus = ResponseStatus.Success,
                Message = "Project successfully deleted.",
                Data = null,
            };

        }

        public async Task<ResponseDTO> AddProjectWithResources(ProjectEntity projectEntity,
                                                  List<AssignResourceEntity> assignResourceEntities)
        {
            try
            {

               await _projectRepository.addProjectWithResources(projectEntity.MapToModel(),
                    assignResourceEntities.Select(p => p.MapToModel()).ToList());

                return new ResponseDTO
                {
                    ResponseStatus = ResponseStatus.Success,
                    Message = "Project successfully Saved.",
                    Data = null,
                };

            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    ResponseStatus = ResponseStatus.Error,
                    Message = "unable to save Project ",
                    Data = null,
                };
            }
         
      

        }

    }
} 

