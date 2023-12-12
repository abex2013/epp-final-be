﻿using Excellerent.APIModularization.Controllers;
using Excellerent.APIModularization.Logging;
using Excellerent.ProjectManagement.Domain.DTOs;
using Excellerent.ProjectManagement.Domain.Entities;
using Excellerent.ProjectManagement.Domain.Interfaces.ServiceInterface;
using Excellerent.ProjectManagement.Domain.Services.Helpers;
using Excellerent.ProjectManagement.Presentation.Models.PostModels;
using Excellerent.ProjectManagement.Presentation.Models.UpdateModels;
using Excellerent.SharedModules.DTO;
using Excellerent.UserManagement.Presentation.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Excellerent.ProjectManagement.Presentation.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class ProjectController : AuthorizedController
    {
        private readonly IProjectService _projectService;
        private readonly IAssignResourceService _assignResourceService;

        public ProjectController(IHttpContextAccessor htttpContextAccessor, IConfiguration configuration, IBusinessLog _businessLog, IProjectService projectService, IAssignResourceService assignResourceService) : base(htttpContextAccessor, configuration, _businessLog, "Project")
        {
            _projectService = projectService;
            _assignResourceService = assignResourceService;
        }

        [HttpGet("all")]
        [TypeFilter(typeof(EPPAutorizeFilter),
            Arguments = new object[] { "View_Project" })]
        public async Task<ResponseDTO> Get()
        {
            var data = await _projectService.GetProjectFullData();
            return new ResponseDTO
            {
                Data = data,
                Message = "Project full data list.",
                ResponseStatus = data != null ? ResponseStatus.Success : ResponseStatus.Error,
                Ex = null
            };
        }
        [HttpGet("ProjectById")]
        [TypeFilter(typeof(EPPAutorizeFilter),
            Arguments = new object[] { "View_Project" })]
        public async Task<ResponseDTO> GetProjectByID(Guid projectId)
        {
            var data = await _projectService.GetProjectById(projectId);
            return new ResponseDTO
            {
                Data = data,
                Message = "Project by id",
                ResponseStatus = data != null ? ResponseStatus.Success : ResponseStatus.Error,
                Ex = null
            };
        }
        [HttpGet]
        //[TypeFilter(typeof(EPPAutorizeFilter),
        //    Arguments = new object[] { "View_Project" })]
        [AllowAnonymous]
        public async Task<PredicatedResponseDTO> Get([FromQuery]PaginationParams paginationParams)
        {
            return await _projectService.GetPaginatedProject(paginationParams);

        }
        [HttpGet("EmployeeProjects(employeeId)")]
        [TypeFilter(typeof(EPPAutorizeFilter),
            Arguments = new object[] { "View_Project" })]
        public async Task<ResponseDTO> GetEmployeeProjects(Guid employeeId)
        {
            var projects = await _projectService.GetEmployeeProjects(employeeId);
            return new ResponseDTO
            {
                Data = projects,
                Message = "Project full data list.",
                ResponseStatus = projects != null ? ResponseStatus.Success : ResponseStatus.Error,
                Ex = null
            };

        }

        [HttpGet("EmployeeClient(employeeId)")]
        [TypeFilter(typeof(EPPAutorizeFilter),
            Arguments = new object[] { "View_Project" })]
        public async Task<ResponseDTO> GetEmployeeClient(Guid employeeId)
        {
            var client = await _projectService.GetEmployeeClient(employeeId);
            return new ResponseDTO
            {
                Data = client,
                Message = "Client full data list.",
                ResponseStatus = client != null ? ResponseStatus.Success : ResponseStatus.Error,
                Ex = null
            };

        }
        [HttpGet("ProjectClient(projectId)")]
        [TypeFilter(typeof(EPPAutorizeFilter),
            Arguments = new object[] { "View_Project" })]
        public async Task<ResponseDTO> GetProjectClient(Guid projectId)
        {
            var client = await _projectService.GetProjectClient(projectId);
            return new ResponseDTO
            {
                Data = client,
                Message = "Client full data list.",
                ResponseStatus = client != null ? ResponseStatus.Success : ResponseStatus.Error,
                Ex = null
            };

        }

        [HttpGet("SupervisorProjects(supervisorGuid)")]
        [TypeFilter(typeof(EPPAutorizeFilter),
            Arguments = new object[] { "View_Project" })]
        public async Task<ResponseDTO> Get(Guid supervisorGuid)
        {
            var data = await _projectService.GetAllSupervisorProjects(supervisorGuid);

            return new ResponseDTO
            {
                Data = data,
                Message = "Project full data list.",
                ResponseStatus = data != null ? ResponseStatus.Success : ResponseStatus.Error,
                Ex = null
            };
        }
        [HttpGet("ClientProjects(clientGuid)")]
        [TypeFilter(typeof(EPPAutorizeFilter),
            Arguments = new object[] { "View_Project" })]
        public async Task<ResponseDTO> GetClientProjects(Guid clientGuid)
        {
            var data = await _projectService.GetClientProjects(clientGuid);

            return new ResponseDTO
            {
                Data = data,
                Message = "Project full data list.",
                ResponseStatus = data != null ? ResponseStatus.Success : ResponseStatus.Error,
                Ex = null
            };
        }
        [HttpPost]
        [TypeFilter(typeof(EPPAutorizeFilter),
            Arguments = new object[] { "Create_Project" })]
        public async Task<ResponseDTO> Add(ProjectPostModel model)
        {
            
      return await _projectService.AddProjectWithResources(model.MappToEntity(),
          model.AssignResource.Select(p => p.MappToEntity()).ToList());

        }
      
        [HttpPut]
        [TypeFilter(typeof(EPPAutorizeFilter),
            Arguments = new object[] { "Update_Project" })]
        public ResponseDTO Edit([FromBody] ProjectUpdateModel project)
        {  
            return  _projectService.Update(project.MappToEntity()).Result;
        }

        [HttpDelete("{guid}")]
        [TypeFilter(typeof(EPPAutorizeFilter),
            Arguments = new object[] { "Delete_Project" })]
        public async Task<ResponseDTO> Delete(Guid guid)
        {
            return await _projectService.DeleteByState(guid);
        }

        [HttpGet("FilterData")]
        [AllowAnonymous]
        public async Task<ResponseDTO> GetMenufilter()
        {
            return await _projectService.GetFilterMenu();
        }
    }
}
