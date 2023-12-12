using Excellerent.ResourceManagement.Domain.Interfaces.Repository;
using Excellerent.ResourceManagement.Domain.Models;
using Excellerent.SharedInfrastructure.Context;
using Excellerent.SharedInfrastructure.Repository;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using System.Linq.Expressions;
using Excellerent.ResourceManagement.Domain.Entities;

using System.Collections;
using Excellerent.ResourceManagement.Infrastructure.TestData;
using Excellerent.ResourceManagement.Domain.DTOs;

namespace Excellerent.ResourceManagement.Infrastructure.Repositories
{
    public class EmployeeRepository : AsyncRepository<Employee>, IEmployeeRepository
    {
        private readonly EPPContext _context;

        public EmployeeRepository(EPPContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Employee> CreateEmployeeAsync(Employee emp)
        {
            await _context.Employees.AddAsync(emp);
            _context.SaveChanges();
            return emp;
        
        }

        public async  Task<List<Employee>> GetEmployeesAsync()
        {

            return await _context.Employees.Where(e => !e.IsDeleted).Include(nationalities => nationalities.Nationality)
                .Include(personaladdress => personaladdress.EmployeeAddress).Include(emergencycontact => emergencycontact.EmergencyContact)
                .Include(familydetail => familydetail.FamilyDetails).Include(employeeorganization => employeeorganization.EmployeeOrganization)
                .Include(employeeRole => employeeRole.EmployeeOrganization.Role)
                 .Include(country => country.EmployeeOrganization.Country)

                .ToListAsync();
        }

        public async Task<Employee> GetEmployeesByEmailAsync(string email)
        {
            var result = await _context.Employees.Include(employeeorganization => employeeorganization.EmployeeOrganization).ThenInclude(jt => jt.Role).Where(x => x.EmployeeOrganization.CompaynEmail.ToLower() == email.ToLower()).FirstOrDefaultAsync();
            return result;
        }

       


        public async Task<IEnumerable<EmployeeViewModel>> GetAllEmployeesDashboardAsync(Expression<Func<Employee, Boolean>> predicate, int pageIndex, int pageSize)
        {

            var employees = (predicate == null ? (await _context.Employees.Include(x => x.EmployeeOrganization).Include(x => x.EmployeeOrganization.Country).Include(y => y.EmployeeOrganization.Role).Where(d => d.IsDeleted == false).OrderByDescending(o => o.CreatedDate).ToListAsync())
                            : (await _context.Employees.Where(predicate).Include(x => x.EmployeeOrganization).Include(x => x.EmployeeOrganization.Country).Include(y => y.EmployeeOrganization.Role).OrderByDescending(o => o.CreatedDate).ToListAsync()));

            int pageindex = 0;
            if (pageIndex == 1) pageindex = pageIndex - 1;
            else pageindex = 1;

            var employeePaginatedList = employees.Skip(pageindex * pageSize).Take(pageSize);

            List<EmployeeViewModel> employeeViewModelList = new List<EmployeeViewModel>();
            if (employeePaginatedList.Count() > 0)
            {
                foreach (Employee employee in employeePaginatedList)
                {
                    employeeViewModelList.Add(new EmployeeViewModel()
                    {
                        EmployeeGUid = employee.Guid,
                        FullName = employee.FirstName + ' ' + employee.FatherName + ' ' + employee.GrandFatherName,
                        JobTitle = employee.EmployeeOrganization == null ? string.Empty : employee.EmployeeOrganization.Role.Name,
                        Status = employee.EmployeeOrganization ==  null? string.Empty : employee.EmployeeOrganization.Status,
                        Location = employee.EmployeeOrganization == null ? string.Empty : employee.EmployeeOrganization.Country.Name,
                        JoiningDate = employee.EmployeeOrganization == null ? new DateTime() : employee.EmployeeOrganization.JoiningDate
                    });
                }
            }
            else
            {
                employeeViewModelList = null;
            }
            return employeeViewModelList;
        }

        public async Task<IEnumerable<EmployeeViewModel>> GetAllEmployeesDashboardwithSortAsync(Expression<Func<Employee, Boolean>> predicate, EmployeeSpecParams paginationParams, int pageIndex, int pageSize)
        {
            var tmpemp = new List<Employee>();
            paginationParams.pageSize = paginationParams.pageSize ?? 10;
            paginationParams.pageIndex = paginationParams.pageIndex ?? 1;
            if (paginationParams.SortField != null)
            {
                switch (paginationParams.SortField)
                {
                    case "JobTitle":
                        if (paginationParams.sortOrder == SharedModules.Seed.SortOrder.Descending)
                        {
                            tmpemp = (predicate == null ? (await _context.Employees.OrderByDescending(x => x.EmployeeOrganization.Role.Name).Include(x => x.EmployeeOrganization).Include(x => x.EmployeeOrganization.Country).Include(y => y.EmployeeOrganization.Role).Where(d => d.IsDeleted == false)/*.OrderByDescending(o => o.CreatedDate)*/.ToListAsync())
                            : (await _context.Employees.Where(predicate).Include(x => x.EmployeeOrganization).Include(x => x.EmployeeOrganization.Country).Include(y => y.EmployeeOrganization.Role).OrderByDescending(x => x.EmployeeOrganization.Role.Name)/*.OrderByDescending(o => o.CreatedDate)*/.ToListAsync()));
                        }
                        else
                        {
                            tmpemp = (predicate == null ? (await _context.Employees.OrderBy(x => x.EmployeeOrganization.Role.Name).Include(x => x.EmployeeOrganization).Include(x => x.EmployeeOrganization.Country).Include(y => y.EmployeeOrganization.Role).Where(d => d.IsDeleted == false)/*.OrderByDescending(o => o.CreatedDate)*/.ToListAsync())
                             : (await _context.Employees.Where(predicate).Include(x => x.EmployeeOrganization).Include(x => x.EmployeeOrganization.Country).Include(y => y.EmployeeOrganization.Role).OrderBy(x => x.EmployeeOrganization.Role.Name)./*OrderByDescending(o => o.CreatedDate).*/ToListAsync()));
                        }
                        break;

                    case "FullName":
                        if (paginationParams.sortOrder == SharedModules.Seed.SortOrder.Descending)
                        {
                            tmpemp = (predicate == null ? (await _context.Employees.OrderByDescending(x => x.FirstName).Include(x => x.EmployeeOrganization).Include(x => x.EmployeeOrganization.Country).Include(y => y.EmployeeOrganization.Role).Where(d => d.IsDeleted == false)/*.OrderByDescending(o => o.CreatedDate)*/.ToListAsync())
                            : (await _context.Employees.Where(predicate).OrderByDescending(x => x.FirstName).Include(x => x.EmployeeOrganization).Include(x => x.EmployeeOrganization.Country).Include(y => y.EmployeeOrganization.Role)/*.OrderByDescending(o => o.CreatedDate)*/.ToListAsync()));
                        }
                        else
                        {
                            tmpemp = (predicate == null ? (await _context.Employees.OrderBy(x => x.FirstName).Include(x => x.EmployeeOrganization).Include(x => x.EmployeeOrganization.Country).Include(y => y.EmployeeOrganization.Role).Where(d => d.IsDeleted == false)/*.OrderByDescending(o => o.CreatedDate)*/.ToListAsync())
                             : (await _context.Employees.Where(predicate).OrderBy(x => x.FirstName).Include(x => x.EmployeeOrganization).Include(x => x.EmployeeOrganization.Country).Include(y => y.EmployeeOrganization.Role)/*.OrderByDescending(o => o.CreatedDate)*/.ToListAsync()));
                        }
                        break;

                    case "JoiningDate":
                        if (paginationParams.sortOrder == SharedModules.Seed.SortOrder.Descending)
                        {
                            tmpemp = (predicate == null ? (await _context.Employees.OrderByDescending(x => x.EmployeeOrganization.JoiningDate).Include(x => x.EmployeeOrganization).Include(x => x.EmployeeOrganization.Country).Include(y => y.EmployeeOrganization.Role).Where(d => d.IsDeleted == false)/*.OrderByDescending(o => o.CreatedDate)*/.ToListAsync())
                            : (await _context.Employees.Where(predicate).OrderByDescending(x => x.EmployeeOrganization.JoiningDate).Include(x => x.EmployeeOrganization).Include(x => x.EmployeeOrganization.Country).Include(y => y.EmployeeOrganization.Role)/*.OrderByDescending(o => o.CreatedDate)*/.ToListAsync()));
                        }
                        else
                        {
                            tmpemp = (predicate == null ? (await _context.Employees.OrderBy(x => x.EmployeeOrganization.JoiningDate).Include(x => x.EmployeeOrganization).Include(x => x.EmployeeOrganization.Country).Include(y => y.EmployeeOrganization.Role).Where(d => d.IsDeleted == false)/*.OrderByDescending(o => o.CreatedDate)*/.ToListAsync())
                             : (await _context.Employees.Where(predicate).OrderBy(x => x.EmployeeOrganization.JoiningDate).Include(x => x.EmployeeOrganization).Include(x => x.EmployeeOrganization.Country).Include(y => y.EmployeeOrganization.Role)./*OrderByDescending(o => o.CreatedDate).*/ToListAsync()));
                        }
                        break;

                    case "Location":
                        if (paginationParams.sortOrder == SharedModules.Seed.SortOrder.Descending)
                        {
                            tmpemp = (predicate == null ? (await _context.Employees.OrderByDescending(x => x.EmployeeOrganization.Country.Name).Include(x => x.EmployeeOrganization).Include(x => x.EmployeeOrganization.Country).Include(y => y.EmployeeOrganization.Role).Where(d => d.IsDeleted == false)/*.OrderByDescending(o => o.CreatedDate)*/.ToListAsync())
                            : (await _context.Employees.Where(predicate).OrderByDescending(x => x.EmployeeOrganization.Country.Name).Include(x => x.EmployeeOrganization).Include(x => x.EmployeeOrganization.Country).Include(y => y.EmployeeOrganization.Role)./*OrderByDescending(o => o.CreatedDate).*/ToListAsync()));
                        }
                        else
                        {
                            tmpemp = (predicate == null ? (await _context.Employees.OrderBy(x => x.EmployeeOrganization.Country.Name).Include(x => x.EmployeeOrganization).Include(x => x.EmployeeOrganization.Country).Include(y => y.EmployeeOrganization.Role).Where(d => d.IsDeleted == false)/*.OrderByDescending(o => o.CreatedDate)*/.ToListAsync())
                             : (await _context.Employees.Where(predicate).OrderBy(x => x.EmployeeOrganization.Country.Name).Include(x => x.EmployeeOrganization).Include(x => x.EmployeeOrganization.Country).Include(y => y.EmployeeOrganization.Role)/*.OrderByDescending(o => o.CreatedDate)*/.ToListAsync()));
                        }
                        break;

                    case "Status":
                        if (paginationParams.sortOrder == SharedModules.Seed.SortOrder.Descending)
                        {
                            tmpemp = (predicate == null ? (await _context.Employees.OrderByDescending(x => x.EmployeeOrganization.Status).Include(x => x.EmployeeOrganization).Include(x => x.EmployeeOrganization.Country).Include(y => y.EmployeeOrganization.Role).Where(d => d.IsDeleted == false)/*.OrderByDescending(o => o.CreatedDate)*/.ToListAsync())
                            : (await _context.Employees.Where(predicate).OrderByDescending(x => x.EmployeeOrganization.Status).Include(x => x.EmployeeOrganization).Include(x => x.EmployeeOrganization.Country).Include(y => y.EmployeeOrganization.Role)/*.OrderByDescending(o => o.CreatedDate)*/.ToListAsync()));
                        }
                        else
                        {
                            tmpemp = (predicate == null ? (await _context.Employees.OrderBy(x => x.EmployeeOrganization.Status).Include(x => x.EmployeeOrganization).Include(x => x.EmployeeOrganization.Country).Include(y => y.EmployeeOrganization.Role).Where(d => d.IsDeleted == false)/*.OrderByDescending(o => o.CreatedDate)*/.ToListAsync())
                             : (await _context.Employees.Where(predicate).OrderBy(x => x.EmployeeOrganization.Status).Include(x => x.EmployeeOrganization).Include(x => x.EmployeeOrganization.Country).Include(y => y.EmployeeOrganization.Role)/*.OrderByDescending(o => o.CreatedDate)*/.ToListAsync()));
                        }
                        break;

                    default:
                        tmpemp = (predicate == null ? (await _context.Employees.Include(x => x.EmployeeOrganization).Include(x => x.EmployeeOrganization.Country).Include(y => y.EmployeeOrganization.Role).Where(d => d.IsDeleted == false).OrderByDescending(o => o.CreatedDate).ToListAsync())
                                 : (await _context.Employees.Where(predicate).Include(x => x.EmployeeOrganization).Include(x => x.EmployeeOrganization.Country).Include(y => y.EmployeeOrganization.Role).OrderByDescending(o => o.CreatedDate).ToListAsync()));
                        throw new ArgumentException(nameof(predicate));

                }
            }
            else 
            {
                tmpemp = (predicate == null ? (await _context.Employees.Include(x => x.EmployeeOrganization).Include(x => x.EmployeeOrganization.Country).Include(y => y.EmployeeOrganization.Role).Where(d => d.IsDeleted == false).OrderByDescending(o => o.CreatedDate).ToListAsync())
                            : (await _context.Employees.Where(predicate).Include(x => x.EmployeeOrganization).Include(x => x.EmployeeOrganization.Country).Include(y => y.EmployeeOrganization.Role).OrderByDescending(o => o.CreatedDate).ToListAsync()));
            }

            var employees = tmpemp; //(predicate == null ? (await _context.Employees.Include(x => x.EmployeeOrganization).Include(x => x.EmployeeOrganization.Country).Include(y => y.EmployeeOrganization.Role).Where(d => d.IsDeleted == false).OrderByDescending(o => o.CreatedDate).ToListAsync())
                            //: (await _context.Employees.Where(predicate).Include(x => x.EmployeeOrganization).Include(x => x.EmployeeOrganization.Country).Include(y => y.EmployeeOrganization.Role).OrderByDescending(o => o.CreatedDate).ToListAsync()));

            int pageindex = 0;
            if (pageIndex == 1) pageindex = pageIndex - 1;
            else pageindex = 1;

            var employeePaginatedList = employees.Skip(pageindex * pageSize).Take(pageSize);

            List<EmployeeViewModel> employeeViewModelList = new List<EmployeeViewModel>();
            if (employeePaginatedList.Count() > 0)
            {
                foreach (Employee employee in employeePaginatedList)
                {
                    employeeViewModelList.Add(new EmployeeViewModel()
                    {
                        EmployeeGUid = employee.Guid,
                        FullName = employee.FirstName + ' ' + employee.FatherName + ' ' + employee.GrandFatherName,
                        JobTitle = employee.EmployeeOrganization == null ? string.Empty : employee.EmployeeOrganization.Role.Name,
                        Status = employee.EmployeeOrganization == null ? string.Empty : employee.EmployeeOrganization.Status,
                        Location = employee.EmployeeOrganization == null ? string.Empty : employee.EmployeeOrganization.Country.Name,
                        JoiningDate = employee.EmployeeOrganization == null ? new DateTime() : employee.EmployeeOrganization.JoiningDate
                    });
                }
            }
            else
            {
                employeeViewModelList = null;
            }
            return employeeViewModelList;
        }


        public Employee UpdateEmployee(Employee employee)
        {
          
            #region
             var existingEmp = _context.Employees
                .Include(n=>n.Nationality)
                 .Include(ce => ce.EmergencyContact)
                 .Include(f => f.FamilyDetails)
                 .Include(o => o.EmployeeOrganization)
                 .Include(ea => ea.EmployeeAddress)
                .FirstOrDefault(e=>e.Guid.Equals(employee.Guid));

            if (existingEmp == null)
                return null;
            else
            {
                // existingEmp.Photo = employee.Photo;
                existingEmp.EmployeeNumber = existingEmp.EmployeeNumber;
                existingEmp.FirstName = employee.FirstName;
                existingEmp.FatherName = employee.FatherName;
                existingEmp.GrandFatherName = employee.GrandFatherName;
                existingEmp.MobilePhone = employee.MobilePhone;
                existingEmp.Phone1 = employee.Phone1;
                existingEmp.Phone2 = employee.Phone2;
                existingEmp.DateofBirth = employee.DateofBirth;
                existingEmp.Gender = employee.Gender;
                existingEmp.PersonalEmail = employee.PersonalEmail;
                existingEmp.PersonalEmail = employee.PersonalEmail;
                existingEmp.PersonalEmail2 = employee.PersonalEmail2;
                existingEmp.PersonalEmail3 = employee.PersonalEmail3;

                if (existingEmp.Nationality.Count() > 0 && employee.Nationality.Count() > 0)
                {
                    for (int i = 0; i < employee.Nationality.Count(); i++)
                    {
                        existingEmp.Nationality[i].Name = employee.Nationality[i].Name;

                    }
                }

                //emergency contact
                if (existingEmp.EmergencyContact.Count() == 0)
                {
                    for (int i = 0; i < employee.EmergencyContact.Count(); i++)
                    {
                        existingEmp.EmergencyContact.Add(employee.EmergencyContact[i]);
                    }
                }
                else
                {
                    for (int i = 0; i < employee.EmergencyContact.Count(); i++)
                    {


                        for (int j = 0; j < existingEmp.EmergencyContact.Count(); j++)
                        {
                            if (existingEmp.EmergencyContact[j].email == employee.EmergencyContact[i].email)
                            {
                                existingEmp.EmergencyContact[i].city = employee.EmergencyContact[i].city;
                                existingEmp.EmergencyContact[i].Country = employee.EmergencyContact[i].Country;

                              //  existingEmp.EmergencyContact[i].email = employee.EmergencyContact[i].email;

                                existingEmp.EmergencyContact[i].email2 = employee.EmergencyContact[i].email2;

                                existingEmp.EmergencyContact[i].email3 = employee.EmergencyContact[i].email3;
                                existingEmp.EmergencyContact[i].GrandFatherName = employee.EmergencyContact[i].GrandFatherName;

                                existingEmp.EmergencyContact[i].FatherName = employee.EmergencyContact[i].FatherName;
                                existingEmp.EmergencyContact[i].FirstName = employee.EmergencyContact[i].FirstName;
                                existingEmp.EmergencyContact[i].houseNumber = employee.EmergencyContact[i].houseNumber;
                                existingEmp.EmergencyContact[i].PhoneNumber = employee.EmergencyContact[i].PhoneNumber;

                                existingEmp.EmergencyContact[i].phoneNumber2 = employee.EmergencyContact[i].phoneNumber2;

                                existingEmp.EmergencyContact[i].phoneNumber3 = employee.EmergencyContact[i].phoneNumber3;

                                existingEmp.EmergencyContact[i].postalCode = employee.EmergencyContact[i].postalCode;

                                existingEmp.EmergencyContact[i].stateRegionProvice = employee.EmergencyContact[i].stateRegionProvice;
                                existingEmp.EmergencyContact[i].Relationship = employee.EmergencyContact[i].Relationship;
                                existingEmp.EmergencyContact[i].woreda = employee.EmergencyContact[i].woreda;
                                existingEmp.EmergencyContact[i].subCityZone = employee.EmergencyContact[i].subCityZone;
                            }
                            else
                            {
                                //jobtitle.findIndex(x=>x.text.trim() === response.Data.jobtype[i].Name.trim()) === -1 
                                if (existingEmp.EmergencyContact.FindIndex(x => x.email == employee.EmergencyContact[i].email) == -1)
                                {
                                    existingEmp.EmergencyContact.Add(employee.EmergencyContact[i]);
                                }
                                //existingEmp.EmergencyContact.Add(employee.EmergencyContact[i]);
                            }
                        }


                    }
                }

                //personal address

                if (existingEmp.EmployeeAddress.Count() == 0)
                {
                    for (int i = 0; i < employee.EmployeeAddress.Count(); i++)
                    {
                        existingEmp.EmployeeAddress.Add(employee.EmployeeAddress[i]);
                    }
                }
                else
                {

                    for (int i = 0; i < employee.EmployeeAddress.Count(); i++)
                    {
                        for (int j = 0; j < existingEmp.EmployeeAddress.Count(); j++)
                        {
                            if (existingEmp.EmployeeAddress[j].Country == employee.EmployeeAddress[i].Country)
                            {
                                //existingEmp.EmployeeAddress[i].Country = employee.EmployeeAddress[i].Country;
                                existingEmp.EmployeeAddress[i].City = employee.EmployeeAddress[i].City;
                                existingEmp.EmployeeAddress[i].HouseNumber = employee.EmployeeAddress[i].HouseNumber;
                                existingEmp.EmployeeAddress[i].PhoneNumber = employee.EmployeeAddress[i].PhoneNumber;
                                existingEmp.EmployeeAddress[i].PostalCode = employee.EmployeeAddress[i].PostalCode;
                                existingEmp.EmployeeAddress[i].StateRegionProvice = employee.EmployeeAddress[i].StateRegionProvice;
                                existingEmp.EmployeeAddress[i].SubCityZone = employee.EmployeeAddress[i].SubCityZone;
                                existingEmp.EmployeeAddress[i].Woreda = employee.EmployeeAddress[i].Woreda;
                            }
                            else
                            {
                                //jobtitle.findIndex(x=>x.text.trim() === response.Data.jobtype[i].Name.trim()) === -1 
                                if (existingEmp.EmployeeAddress.FindIndex(x => x.Country == employee.EmployeeAddress[i].Country) == -1)
                                {
                                    existingEmp.EmployeeAddress.Add(employee.EmployeeAddress[i]);
                                }
                                //existingEmp.EmergencyContact.Add(employee.EmergencyContact[i]);
                            }
                        }


                    }
                }


                //familydetail
                if (existingEmp.FamilyDetails.Count() == 0)
                {
                    for (int i = 0; i < employee.FamilyDetails.Count(); i++)
                    {
                        existingEmp.FamilyDetails.Add(employee.FamilyDetails[i]);
                    }
                }
                else
                {

                    for (int i = 0; i < employee.FamilyDetails.Count(); i++)
                    {
                        for (int j = 0; j < existingEmp.FamilyDetails.Count(); j++)
                        {
                            if (existingEmp.FamilyDetails[j].FullName == employee.FamilyDetails[i].FullName)
                            {
                                //existingEmp.FamilyDetails[i].FullName = employee.FamilyDetails[i].FullName;
                                existingEmp.FamilyDetails[i].RelationshipId = employee.FamilyDetails[i].RelationshipId;
                                existingEmp.FamilyDetails[i].Gender = employee.FamilyDetails[i].Gender;
                                existingEmp.FamilyDetails[i].DoB = employee.FamilyDetails[i].DoB;
                                existingEmp.FamilyDetails[i].Remark = employee.FamilyDetails[i].Remark;
                            }
                            else
                            {
                                //jobtitle.findIndex(x=>x.text.trim() === response.Data.jobtype[i].Name.trim()) === -1 
                                if (existingEmp.FamilyDetails.FindIndex(x => x.FullName == employee.FamilyDetails[i].FullName) == -1)
                                {
                                    existingEmp.FamilyDetails.Add(employee.FamilyDetails[i]);
                                }
                                //existingEmp.EmergencyContact.Add(employee.EmergencyContact[i]);
                            }
                        }
                        
                    }
                }
            
                 
                
                 if (existingEmp.EmployeeOrganization != null)
                 {

                     // existingEmp.EmployeeOrganization.Branch.Country = employee.EmployeeOrganization.Branch.Country;
                     existingEmp.EmployeeOrganization.CompaynEmail = employee.EmployeeOrganization.CompaynEmail;
                     existingEmp.EmployeeOrganization.Country = employee.EmployeeOrganization.Country;
                     existingEmp.EmployeeOrganization.DepartmentId = employee.EmployeeOrganization.DepartmentId;
                     existingEmp.EmployeeOrganization.DutyBranch = employee.EmployeeOrganization.DutyBranch;
                     existingEmp.EmployeeOrganization.JobTitleId = employee.EmployeeOrganization.JobTitleId;
                     existingEmp.EmployeeOrganization.JoiningDate = employee.EmployeeOrganization.JoiningDate;
                     existingEmp.EmployeeOrganization.ReportingManager = employee.EmployeeOrganization.ReportingManager;
                     existingEmp.EmployeeOrganization.Status = employee.EmployeeOrganization.Status;
                     existingEmp.EmployeeOrganization.TerminationDate = employee.EmployeeOrganization.TerminationDate;
                 }

                 _context.SaveChanges(true);

               return employee;
             }
            #endregion
        }
        public async Task<int> AllEmployeesDashboardCountAsync(Expression<Func<Employee, Boolean>> predicate)
        {
            var employeeList = (predicate == null ? (await _context.Employees.Include(x => x.EmployeeOrganization).Where(d => d.IsDeleted == false).OrderByDescending(o => o.CreatedDate).ToListAsync())
                : (await _context.Employees.Include(x => x.EmployeeOrganization).Where(predicate).OrderByDescending(o => o.CreatedDate).ToListAsync()));
            
            return employeeList.Count;
        }
        public Employee GetEmployeesById(Guid empId)
        {
            var result =  _context.Employees
                .Include(x=>x.EmployeeOrganization.Country)
                .Include(x => x.EmployeeOrganization.DutyBranch)
                .Include(x => x.EmployeeOrganization.Department)
                .Include(x => x.EmployeeOrganization.Role)
                .Include(x=>x.FamilyDetails)
                .ThenInclude(r=>r.Relationship)
                .Include(x=>x.Nationality)
                .Include(x=>x.EmergencyContact)
                .Include(x=>x.EmployeeAddress)
                .Where(x => x.Guid == empId).FirstOrDefault();

            return result;

        }

        public bool UpdatePersonalInfoEmployee(Employee employeeEntity)
        {
            var existingEmp =  _context.Employees.Where(e => e.Guid == employeeEntity.Guid).FirstOrDefault();
            if (existingEmp == null)
                return false;
            else
            {
                existingEmp.FirstName = employeeEntity.FirstName;
                existingEmp.FatherName = employeeEntity.FatherName;
                existingEmp.GrandFatherName = employeeEntity.GrandFatherName;

                existingEmp.MobilePhone = employeeEntity.MobilePhone;
                existingEmp.Phone1 = employeeEntity.Phone1;
                existingEmp.Phone2 = employeeEntity.Phone2;


                existingEmp.DateofBirth = employeeEntity.DateofBirth;
                existingEmp.Gender = employeeEntity.Gender;
                existingEmp.Nationality = employeeEntity.Nationality;
                existingEmp.PersonalEmail = employeeEntity.PersonalEmail;
                existingEmp.FirstName = employeeEntity.FirstName;

                existingEmp.PersonalEmail = employeeEntity.PersonalEmail;
                existingEmp.PersonalEmail2 = employeeEntity.PersonalEmail2;
                existingEmp.PersonalEmail3 = employeeEntity.PersonalEmail3;

                 _context.SaveChanges(true);

            }
            return true;
        }
        public bool UpdatePersonalAddressEmployee(Employee employeeEntity)
        {
            var existingEmp = _context.Employees.Where(e => e.Guid == employeeEntity.Guid).Include(pa => pa.EmployeeAddress).FirstOrDefault();
            if (existingEmp == null)
                return false;
            else
            {
                existingEmp.EmployeeAddress = employeeEntity.EmployeeAddress;

                 _context.SaveChangesAsync(true);
            }
            return true;
        }

        public bool UpdateOrgDetailEmployee(Employee employeeEntity)
        {
            var existingEmp = _context.Employees.Where(e => e.Guid == employeeEntity.Guid).Include(pa => pa.EmployeeOrganization).FirstOrDefault();
            if (existingEmp == null)
                return false;
            else
            {
                existingEmp.EmployeeOrganization = employeeEntity.EmployeeOrganization;

                 _context.SaveChanges(true);
            }
            return true;
        }

        public bool UpdateFamilyDetailEmployee(Employee employeeEntity)
        {
            var existingEmp = _context.Employees.Where(e => e.Guid == employeeEntity.Guid).Include(pa => pa.FamilyDetails).FirstOrDefault();
            if (existingEmp == null)
                return false ;
            else
            {
                existingEmp.FamilyDetails = employeeEntity.FamilyDetails;

                 _context.SaveChanges(true);
            }
            return true;
        }

        public bool UpdateContactEmployee(Employee employeeEntity)
        {
            var existingEmp = _context.Employees.Where(e => e.Guid == employeeEntity.Guid).Include(pa => pa.EmergencyContact).FirstOrDefault();
            if (existingEmp == null)
                return false;
            else
            {
                existingEmp.EmergencyContact = employeeEntity.EmergencyContact;

                 _context.SaveChanges(true);
            }
            return true;
        }

        public async Task<bool> ChangeIsDeletedStatus(Employee employee)
        {
            employee.IsDeleted = true;
            _context.Employees.Update(employee);
            var userInfo = await _context.Users.Where(emp => emp.EmployeeId == employee.Guid).FirstOrDefaultAsync();
            if (userInfo != null)
            {
                userInfo.IsActive = false;
                _context.Users.Update(userInfo);
            }
            return await _context.SaveChangesAsync() > 0 ? true : false;
        }

        public async Task<List<ReportingManager>> GetEmployeeListForReportingManager()
        {
           var employeeList = await _context.Employees.Include(jt=>jt.EmployeeOrganization.Role).Where(x=>x.IsDeleted == false).ToListAsync();
           List<ReportingManager> objReportingManager = new List<ReportingManager>();
           foreach(Employee employee in employeeList)
           {
                objReportingManager.Add(new ReportingManager()
                {
                    Id = employee.Guid,
                    EmployeeName = employee.FirstName + ' ' + employee.FatherName + ' ' + employee.GrandFatherName + '(' + employee.EmployeeOrganization.Role.Name + ')'
                });
           }
           return objReportingManager;
        }

        public Employee GetEmployeesByEmpNumber(string empId)
        {
            var result = _context.Employees
              .Include(x => x.EmployeeOrganization)
              .Include(x => x.FamilyDetails)
              .ThenInclude(r => r.Relationship)
              .Include(x => x.Nationality)
              .Include(x => x.EmergencyContact)
              .Include(x => x.EmployeeAddress)
              .Where(x => x.EmployeeNumber == empId).FirstOrDefault();

            return result;
        }

        public async Task<bool> CheckIdNumber(string idNumber)
        {
            return (await GetAllAsync()).Count(e => e.EmployeeNumber.StartsWith(idNumber)) == 0;
        }

        public async Task<bool> IsEmpForDepartment(Guid id)
        {
            var employeeList = await _context.Employees.Where(x => x.EmployeeOrganization.DepartmentId == id).ToListAsync();
             bool result = false;
             result = (employeeList.Count() > 0 ? (true) :  (false));

            return result;
        }
    }
}