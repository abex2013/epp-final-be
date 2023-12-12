﻿using Excellerent.ResourceManagement.Domain.DTOs;
using Excellerent.ResourceManagement.Domain.Entities;
using Excellerent.ResourceManagement.Domain.Models;
using Excellerent.SharedModules.Seed;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Excellerent.ResourceManagement.Domain.Interfaces.Repository
{
    public interface IEmployeeRepository : IAsyncRepository<Employee>
    {
        Task<Employee> CreateEmployeeAsync(Employee emp);
        Task<List<Employee>> GetEmployeesAsync();

        bool UpdatePersonalInfoEmployee(Employee employeeEntity);
        bool UpdatePersonalAddressEmployee(Employee employeeEntity);
        bool UpdateOrgDetailEmployee(Employee employeeEntity);

        bool UpdateFamilyDetailEmployee(Employee employeeEntity);
        bool UpdateContactEmployee(Employee employeeEntity);
        Employee GetEmployeesById(Guid empId);
        Employee GetEmployeesByEmpNumber(string empId);


        Employee UpdateEmployee(Employee employee);


        Task<Employee> GetEmployeesByEmailAsync(string email);

        Task<bool> CheckIdNumber(string idNumber);

        Task<IEnumerable<EmployeeViewModel>> GetAllEmployeesDashboardAsync(Expression<Func<Employee, Boolean>> predicate, int pageindex, int pageSize);
        Task<IEnumerable<EmployeeViewModel>> GetAllEmployeesDashboardwithSortAsync(Expression<Func<Employee, Boolean>> predicate,EmployeeSpecParams parms, int pageindex, int pageSize);

        Task<int> AllEmployeesDashboardCountAsync(Expression<Func<Employee, Boolean>> predicate);

        Task<bool> ChangeIsDeletedStatus(Employee employee);

        Task<List<ReportingManager>> GetEmployeeListForReportingManager();
        Task<bool> IsEmpForDepartment(Guid id);
    }
}
