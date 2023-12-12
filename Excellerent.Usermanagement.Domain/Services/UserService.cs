using Excellerent.SharedModules.DTO;
using Excellerent.SharedModules.Seed;
using Excellerent.SharedModules.Services;
using Excellerent.Usermanagement.Domain.Entities;
using Excellerent.Usermanagement.Domain.Interfaces.RepositoryInterfaces;
using Excellerent.Usermanagement.Domain.Interfaces.ServiceInterfaces;
using Excellerent.Usermanagement.Domain.Models;
using LinqKit;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading.Tasks;
using Excellerent.ResourceManagement.Domain.Entities;

namespace Excellerent.Usermanagement.Domain.Services
{
    public class UserService : CRUD<UserEntity, User>, IUserService
    {
        public IUserRepository _repository { get; }
        public UserService(IUserRepository repository) : base(repository)
        {
            _repository = repository;
        }

        public async Task<PredicatedResponseDTO> GetUserDashBoardList(string userName, int pageIndex, int pageSize)
        {
            var predicate = PredicateBuilder.True<User>();

            if (userName == "" || userName == null)
                predicate = null;
            else
            {
                predicate = predicate.And(d => d.Employee.IsDeleted == false)
                                     .And(p => p.FirstName.ToLower().Contains(userName.ToLower()))
                                     .Or(p => p.MiddleName.ToLower().Contains(userName.ToLower()))
                                     .Or(p => p.LastName.ToLower().Contains(userName.ToLower()));
            }

            var result = await _repository.GetUserDashBoardList(predicate, pageIndex - 1, pageSize);
            if (result != null)
            {
                int totalRowCount = await _repository.GetUserDashBoardListCount(predicate);
                return new PredicatedResponseDTO
                {
                    Data = result,
                    TotalRecord = totalRowCount,//total row count
                    PageIndex = pageIndex,
                    PageSize = pageSize,  // itemPerPage,
                    TotalPage = result.Count
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

        public async Task<IEnumerable<UserEntity>> GetUsers()
        {
            try
            {
                IEnumerable<User> users = await _repository.GetAllAsync();
                IEnumerable<UserEntity> mappedUsers = new List<UserEntity>() ;
                if(users.Any())
                mappedUsers = users.Select(u => new UserEntity(u));
                return mappedUsers;
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }

        public async Task<UserEntity> GetUser(Guid id)
        {
            try
            {
                User user = await _repository.FindOneAsync(u => u.Guid == id);

                return user ==null ? null:  new UserEntity(user);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }
        public async Task<IEnumerable<User>> GetUserByEmployeeId(Guid empId)
        {
            try
            {
                return await _repository.FindAsync(u => u.EmployeeId == empId);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }
        public new async Task<ResponseDTO> Update(UserEntity entity)
        {
            var data = await _repository.FindOneAsync(x => x.Guid == entity.Guid);
            if (data == null)
            {
                return new ResponseDTO(ResponseStatus.Error, "User not found", null);
            }
            var model = entity.MapToModel(data);
            await _repository.UpdateAsync(model);

            return new ResponseDTO(ResponseStatus.Success, "Record updated successfully", null);
        }
        public  async Task<ResponseDTO> CreateUser(UserEntity entity, Guid [] GroupIds)
        {
            var model = entity.MapToModel();
            model.IsPasswordGenerated = true;
            model.LastActivityDate = DateTime.Now;
            bool success =   await _repository.CreatUser(model, GroupIds);
            if(success)
            return new ResponseDTO(ResponseStatus.Success, "User is created successfully", null);
            
            return new ResponseDTO(ResponseStatus.Error, "", null);
        }

        public async Task<ResponseDTO> GetEmployeesNotInUsers()
        {
            try
            {
                var employees = await _repository.GetEmployeesNotInAsUser();
                IEnumerable<EmployeeEntity> employeeEntities = new List<EmployeeEntity>();
                if(employees != null && employees.Any())
                {
                    employeeEntities = employees.Select(e => new EmployeeEntity(e));
                }
                return new ResponseDTO(ResponseStatus.Success, "", employeeEntities);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }
       
        

        public async Task<Guid> GetUserGuidByEmail(string email)
        {
            var data = await _repository.FindOneAsync(x => x.Email == email && x.IsDeleted == false);
            return data == null ? Guid.Empty : data.Guid;
        }
        public async Task<UserEntity> GetUserByEmail(string email)
        {
            var model = await _repository.FindOneAsync(x => x.Email == email && x.IsDeleted == false);
            if (model == null) return null;
            return new UserEntity(model);

        }

        public async Task<ResponseDTO> LoadUsersNotGroupedInGroup(Guid groupSetId)
        {
            var result = await _repository.LoadUsersNotGroupedInGroup(groupSetId);
            if(result == null)
            {
                return new ResponseDTO(ResponseStatus.Error, "There are no user not assigned to this specific group", null);
            }
            return new ResponseDTO(ResponseStatus.Success, "", result);
        }

        public async Task<UserEntity> ValidateUser(string email)
        {
            try
            {
                User user = await _repository.FindOneAsync(u => u.Email == email && u.IsDeleted == false);

                return ((user == null) || (!user.IsActive)) ? null : new UserEntity(user);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }


        public async Task<ResponseDTO> RemoveUser(Guid userId)
        {
            var result = await _repository.GetByGuidAsync(userId);
            if(result == null)
            {
                return new ResponseDTO(ResponseStatus.Error, "Can not get user", null);
            }
            await _repository.DeleteAsync(result);
            return new ResponseDTO(ResponseStatus.Success, "User deleted successfully", null);
        }

        public async Task<UserEntity> Authenticate(string Email, string Password)
        {
            var data = await _repository.FindOneAsync(x => x.Email == Email && x.IsDeleted == false);
            return data != null ? new UserEntity(data) : null;
        }


        public  async Task<ResponseDTO> ChangePassword(UserEntity entity, string newPassword)
        {
            var data = await _repository.FindOneAsync(x => x.Guid == entity.Guid);
            if (data == null)
            {
                return new ResponseDTO(ResponseStatus.Error, "User not found", null);
            }
            var model = entity.MapToModel(data);
            model.Password = newPassword;
            model.IsPasswordGenerated = false;
            await _repository.UpdateAsync(model);

            return new ResponseDTO(ResponseStatus.Success, "Password updated successfully", null);
        }
        public  async Task<ResponseDTO> ResetPassword(string email, string newPassword)
        {
            try
            {
                var model = await _repository.FindOneAsync(x => x.Email == email && x.IsDeleted == false);
                if (model == null)
                {
                    return new ResponseDTO(ResponseStatus.Error, "User not found", null);
                }
                model.Password = newPassword;
                model.IsPasswordGenerated = true;
                await _repository.UpdateAsync(model);

                return new ResponseDTO(ResponseStatus.Success, "Password reseted successfully", null);
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }
    }
}
