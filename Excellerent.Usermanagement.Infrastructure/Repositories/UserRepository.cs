using Excellerent.SharedInfrastructure.Context;
using Excellerent.SharedInfrastructure.Repository;
using Excellerent.SharedModules.DTO;
using Excellerent.Usermanagement.Domain.Interfaces.RepositoryInterfaces;
using Excellerent.Usermanagement.Domain.Models;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Excellerent.Usermanagement.Domain.Enums;
using System.Data.SqlClient;
using System.Text;
using Npgsql;
using NpgsqlTypes;
using Excellerent.ResourceManagement.Domain.Models;

namespace Excellerent.Usermanagement.Infrastructure.Repositories
{
    public class UserRepository : AsyncRepository<User>, IUserRepository
    {
        private readonly EPPContext _context;

        public UserRepository(EPPContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<UserListView>> GetUserDashBoardList(Expression<Func<User, bool>> predicate, int pageIndex, int pageSize)
        {
            var users = (predicate == null ? (await _context.Users.Include(x => x.Employee).ThenInclude(eo => eo.EmployeeOrganization)
                        .ThenInclude(z=>z.Role).ThenInclude(a=>a.Department).Where(d => d.Employee.IsDeleted == false).OrderByDescending(o => o.LastActivityDate).ToListAsync())
                : (await _context.Users.Where(predicate).Include(x => x.Employee).ThenInclude(eo => eo.EmployeeOrganization).OrderByDescending(o => o.LastActivityDate).ToListAsync()));

            var paginatedUserList = users.Skip(pageIndex * pageSize).Take(pageSize);
            List<UserListView> userViewModelList = new List<UserListView>();
            if (paginatedUserList.Count() > 0)
            {
                foreach (User user in paginatedUserList)
                {
                    userViewModelList.Add(
                        new UserListView()
                        {
                            UserId = user.Guid,
                            FullName = user.FirstName + ' ' + user.MiddleName + ' ' + user.LastName,
                            JobTitle = user.Employee.EmployeeOrganization.Role == null ? string.Empty : user.Employee.EmployeeOrganization.Role.Name,
                            Department = user.Employee.EmployeeOrganization.Department == null ? string.Empty : user.Employee.EmployeeOrganization.Department.Name,
                            Status = user.Status == UserStatus.Active ? "Active" : "In-Active",
                            LastActivityDate = user.LastActivityDate != null? (DateTime)user.LastActivityDate: DateTime.MinValue
                        }
                    );
                }
            }
            else
            {
                userViewModelList = null;
            }
            return userViewModelList;
        }

        public async Task<int> GetUserDashBoardListCount(Expression<Func<User, bool>> predicate)
        {
            var users = (predicate == null ? (await _context.Users.Include(x => x.Employee).ThenInclude(eo => eo.EmployeeOrganization).OrderByDescending(o => o.LastActivityDate).ToListAsync())
                : (await _context.Users.Include(x => x.Employee).ThenInclude(eo => eo.EmployeeOrganization).Where(predicate).OrderByDescending(o => o.LastActivityDate).ToListAsync()));

            return users.Count;
        }
       
        public async Task<IEnumerable<Employee>> GetEmployeesNotInAsUser()
        {
            var quarable = from e in _context.Employees.Include(e => e.EmployeeOrganization.Role)
                           where !_context.Users.Any(u => u.EmployeeId == e.Guid)
                           select e
             ;
            return await quarable.ToListAsync();
        }
       


        public async Task<List<UsersNotGrouped>> LoadUsersNotGroupedInGroup(Guid Id)
        {
            //var groupIdParam = new NpgsqlParameter("groupIdParam", "'Id'");
            //string query =string.Format("select * from \"Users\" where \"Guid\" not in " +
            //                            "(select \"UserGuid\" from \"UserGroups\" where \"GroupSetGuid\"::text = '{0}')", Id);
            var quarable = from e in _context.Users.Include(u => u.Employee.EmployeeOrganization.Role).Where(x => x.IsActive == true)
                           where !_context.UserGroups.Any(ug => ug.UserGuid == e.Guid && ug.GroupSetGuid == Id)
                           select e
            ;
            
            List<UsersNotGrouped> notAssignedUsers = new List<UsersNotGrouped>();
            var notAssignedUsersList = await quarable.ToListAsync();
            StringBuilder builder = new StringBuilder();
            foreach (User user in notAssignedUsersList)
            {
                builder.Clear();
                builder.Append(user.FirstName);
                builder.Append(" ");
                builder.Append(user.MiddleName);
                builder.Append(" ");
                builder.Append(user.LastName);
                builder.Append(" ");
                builder.Append(user.Employee.EmployeeOrganization.Role.Name);
                notAssignedUsers.Add(new UsersNotGrouped()
                {
                    Guid = user.Guid,
                    FullName = builder.ToString()
                });
            }

            return notAssignedUsers;
        }

        public async Task<bool> CreatUser(User user, Guid [] GroupIds)
        {
            this._context.Users.Add(user);
            if(GroupIds != null)
            foreach (Guid groupId in GroupIds)
            {
                this._context.UserGroups.Add(new UserGroups { UserGuid= user.Guid, GroupSetGuid = groupId });
            }

            try
            {
               await this._context.SaveChangesAsync();
                return true;
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
                    
            
        }
    }
}
