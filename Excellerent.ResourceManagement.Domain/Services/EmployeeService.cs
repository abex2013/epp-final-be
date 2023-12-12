﻿using Excellerent.ResourceManagement.Domain.DTOs;
using Excellerent.ResourceManagement.Domain.Entities;
using Excellerent.ResourceManagement.Domain.Interfaces.Repository;
using Excellerent.ResourceManagement.Domain.Interfaces.Services;
using Excellerent.ResourceManagement.Domain.Models;
using Excellerent.SharedModules.DTO;
using Excellerent.SharedModules.Services;
using LinqKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Excellerent.ResourceManagement.Domain.Services
{
    public class EmployeeService : CRUD<EmployeeEntity, Employee>, IEmployeeService
    {
        IEmployeeRepository _employeeRepository;


        public EmployeeService(IEmployeeRepository employeeRepository) : base(employeeRepository)
        {
            _employeeRepository = employeeRepository;
        }

        public async Task<Employee> AddNewEmployeeEntry(Employee employee)
        {
            return await _employeeRepository.AddAsync(employee);

        }

        public async Task<List<Employee>> GetAllEmployeesAsync()
        {
            return await _employeeRepository.GetEmployeesAsync();
        }

        public async Task<Employee> GetEmployeesByEmailAsync(string email)
        {
            return await _employeeRepository.GetEmployeesByEmailAsync(email);
        }

        public async Task<bool> CheckIfEmailExists(string email)
        {
            bool returnresult = false;
            var result = await GetEmployeesByEmailAsync(email);
            if (result == null)
            {
                returnresult = true;
            }
            else
            {
                returnresult = false;
            }
            return  returnresult;
        }

        public async Task<PredicatedResponseDTO> GetAllEmployeesDashboardAsync(string searchKey, int pageIndex, int pageSize)
        {
            var predicate = PredicateBuilder.True<Employee>();

            if (searchKey == null)
                predicate = null;
            else
            {
                predicate = predicate.And(p => p.IsDeleted == false).And(p => p.FirstName.ToLower().Contains(searchKey.ToLower()))
                                      .Or(p => p.FatherName.ToLower().Contains(searchKey.ToLower()))
                                      .Or(p => p.GrandFatherName.ToLower().Contains(searchKey.ToLower()));
            }

            var result = await _employeeRepository.GetAllEmployeesDashboardAsync(predicate, pageIndex, pageSize);
            
            if (result != null)
            {
                
                List<EmployeeViewModel> employeeList = (List<EmployeeViewModel>)result;
                int totalRowCount = await _employeeRepository.AllEmployeesDashboardCountAsync(predicate);
                return new PredicatedResponseDTO
                {
                    Data = employeeList,
                    TotalRecord = totalRowCount,//total row count
                    PageIndex = pageIndex,
                    PageSize = pageSize,  // itemPerPage,
                    TotalPage = employeeList.Count
                };
            }
            else
            {
                return new PredicatedResponseDTO
                {
                    Data = null,
                    TotalRecord = 0,//total row count
                    PageIndex = 0,
                    PageSize = 0,  // itemPerPage,
                    TotalPage = 0
                };
            }
        }


        public  Employee GetEmployeesById(Guid empId)
        {
            return  _employeeRepository.GetEmployeesById(empId);
        }

        public async Task UpdateEmployee(EmployeeEntity employeeEntity)
        {
            var data = await _employeeRepository.FindOneAsync(x => x.Guid == employeeEntity.Guid);
            var model = employeeEntity.MapToModel(data);
            await _employeeRepository.UpdateAsync(model);
        }

        public  Employee UpdateEmployee(Employee employee)

        {
           return  _employeeRepository.UpdateEmployee(employee);
            
           
        }

        public async Task<IEnumerable<EmployeeDTO>> GetSelections()
        {
            return (await _employeeRepository.GetEmployeesAsync()).Select(x => new EmployeeDTO(x));
        }

        public async Task<EmployeeDTO> GetSelection(Guid employeeGuid)
        {
            return (await _employeeRepository.GetEmployeesAsync())
                .Where(x=> x.Guid == employeeGuid)
                .Select(x => new EmployeeDTO(x))
                .FirstOrDefault();
        }

        public async Task<PredicatedResponseDTO> GetAllEmployeesDashboardFilterAsync(EmployeeSpecParams paginationParams)
        {

            int itemPerPage = paginationParams.pageSize ?? 10;
            int PageIndex = paginationParams.pageIndex ?? 1;

            int TotalRowCount = 0;
            var predicate = PredicateBuilder.New<Employee>();
            var predicateclient = PredicateBuilder.New<Employee>();
            var predicatestatus = PredicateBuilder.New<Employee>();
            var predicatesupervisor = PredicateBuilder.New<Employee>();
            List<Employee> projectEntities = new List<Employee>();
            if (paginationParams.jobtype != null)
            {

                foreach (var client in paginationParams.jobtype)
                {
                    predicateclient = predicateclient.Or(c => c.EmployeeOrganization.Role.Name == client);
                }
            }

            if (paginationParams.status != null)
            {

                foreach (var client in paginationParams.status)
                {
                    predicatestatus = predicatestatus.Or(c => c.EmployeeOrganization.Status == client);
                }
            }
            if (paginationParams.location != null)
            {

                foreach (var client in paginationParams.location)
                {
                    predicatesupervisor = predicatesupervisor.Or(c => c.EmployeeOrganization.Country.Name == client);
                }
            }


            {
                if (paginationParams.jobtype == null
                    && paginationParams.status == null
                    && paginationParams.location == null)
                {
                    predicate = string.IsNullOrEmpty(paginationParams.searchKey) ? null
                     : predicate.And(p => p.FirstName.ToLower().Contains(paginationParams.searchKey.ToLower()));


                }

                if (paginationParams.jobtype != null)
                {

                    predicate = predicate.And(predicateclient);
                    predicate = string.IsNullOrEmpty(paginationParams.searchKey) ? predicate
                           : predicate.And(p => p.EmployeeOrganization.Role.Name.Contains(paginationParams.searchKey.ToLower()));
                }
                if (paginationParams.status != null)
                {
                    predicate = predicate.And(predicatestatus);
                    predicate = string.IsNullOrEmpty(paginationParams.searchKey) ? predicate
                          : predicate.And(p => p.EmployeeOrganization.Status.Contains(paginationParams.searchKey.ToLower()));

                }
                if (paginationParams.location != null)
                {
                    predicate = predicate.And(predicatesupervisor);
                    predicate = string.IsNullOrEmpty(paginationParams.searchKey) ? predicate
                          : predicate.And(p => p.EmployeeOrganization.Country.Name.Contains(paginationParams.searchKey.ToLower()));
                }

                var projectData = (await _employeeRepository.GetAllEmployeesDashboardwithSortAsync(predicate,paginationParams, PageIndex, itemPerPage)).ToList();

                if (projectData != null)
                {
                    List<EmployeeViewModel> employeeList = (List<EmployeeViewModel>)projectData;
                    int totalRowCount = await _employeeRepository.AllEmployeesDashboardCountAsync(predicate);
                    return new PredicatedResponseDTO
                    {
                        Data = employeeList,
                        TotalRecord = totalRowCount,//total row count
                        PageIndex = PageIndex,
                        PageSize = itemPerPage,  // itemPerPage,
                        TotalPage = employeeList.Count
                    };
                }
                else
                {
                    return new PredicatedResponseDTO
                    {
                        Data = null,
                        TotalRecord = 0,//total row count
                        PageIndex = 0,
                        PageSize = 0,  // itemPerPage,
                        TotalPage = 0
                    };
                }
               
            }
        }


        public Employee GetEmployeesByEmpNumber(string empId)
        {
            return _employeeRepository.GetEmployeesByEmpNumber(empId);
        }

        public async Task<ResponseDTO> ChangeIsDeletedStatus(Guid employeeId)
        {
            Employee employee = _employeeRepository.FindOneAsync(x => x.Guid == employeeId).Result;
            if(employee == null)
            {
                return new ResponseDTO(ResponseStatus.Error, "Employee does not exist", null);
            }
            else
            {
                await _employeeRepository.ChangeIsDeletedStatus(employee);
                return new ResponseDTO(ResponseStatus.Success, "Employee deleted successfully", null);
            }
        }

        public async Task<ResponseDTO> GetEmployeeListForReportingManager()
        {
            return new ResponseDTO(ResponseStatus.Info, "", await _employeeRepository.GetEmployeeListForReportingManager());
        }


        public async Task<ResponseDTO> GetFilterMenu()
        {
            List<EmployeeForFilterDTO> jobtypelist = new List<EmployeeForFilterDTO>();
            List<EmployeeForFilterDTO> locationlist = new List<EmployeeForFilterDTO>();
            List<EmployeeForFilterDTO> statusLists = new List<EmployeeForFilterDTO>();
            var statusList = await _employeeRepository.GetEmployeesAsync();
            foreach (var st in statusList)
            {
                if (st.EmployeeOrganization != null)
                {
                    var status = new EmployeeForFilterDTO
                    {
                        Id = st.EmployeeOrganization.Guid,
                        Name = st.EmployeeOrganization.Status
                    };
                    statusLists.Add(status);

                    var jobtype = new EmployeeForFilterDTO
                    {
                        Id = st.EmployeeOrganization.JobTitleId,
                        Name = st.EmployeeOrganization.Role.Name
                    };
                    jobtypelist.Add(jobtype);

                    var location = new EmployeeForFilterDTO
                    {
                        Id = st.EmployeeOrganization.Country.Guid,
                        Name = st.EmployeeOrganization.Country.Name
                    };
                    locationlist.Add(location);
                }
            }


            return new ResponseDTO(ResponseStatus.Success, "Filter Data", new
            {
                Status = statusLists,
                location = locationlist,
                jobtype = jobtypelist

            });
        }

        public async Task<bool> CheckIdNumber(string idNumber)
        {
            return await _employeeRepository.CheckIdNumber(idNumber);

        }

        public async Task<bool> CheckDeptData(Guid idNumber)
        {
            return await _employeeRepository.IsEmpForDepartment(idNumber);
        }
    }
}
