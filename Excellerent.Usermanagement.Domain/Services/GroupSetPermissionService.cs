

using Excellerent.SharedModules.DTO;
using Excellerent.SharedModules.Services;
using Excellerent.Usermanagement.Domain.Entities;
using Excellerent.Usermanagement.Domain.Interfaces.RepositoryInterfaces;
using Excellerent.Usermanagement.Domain.Interfaces.ServiceInterfaces;
using Excellerent.Usermanagement.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Excellerent.Usermanagement.Domain.Services
{
    public class GroupSetPermissionService : CRUD<GroupSetPermissionEntity, GroupSetPermission>, IGroupSetPermissionService
    {
        private readonly IGroupSetPermissionRepository _groupSetPermissionRepository;
        public GroupSetPermissionService(IGroupSetPermissionRepository groupSetPermissionRepository) : base(groupSetPermissionRepository)
        {
            _groupSetPermissionRepository = groupSetPermissionRepository;
        }

        public async Task<IEnumerable<PermissionEntity>> GetPermissionsByGroupId(Guid guid)
        {
            var data = await _groupSetPermissionRepository.GetPermissionByGroupId(guid);
            return data?.Select(x => new PermissionEntity(x));
        }
        public async Task<ResponseDTO> Delete(Guid id)
        {
            var data = await _groupSetPermissionRepository.FindOneAsync(x => x.Guid == id);
            if (data == null)
            {
                return new ResponseDTO(ResponseStatus.Error, "Assigned permission not found", null);
            }
            await _groupSetPermissionRepository.DeleteAsync(data);
            return new ResponseDTO(ResponseStatus.Success, "Record deleted successfully", null);
        }

        public async Task<dynamic> removePermissions(Guid guid)
        {
            var data = await _groupSetPermissionRepository.FindAsync(x=>x.GroupSetId==guid);
           
            
            return data;
        }
    }
}
