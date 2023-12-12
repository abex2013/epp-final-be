using Excellerent.EppConfiguration.Domain.Entities;
using Excellerent.EppConfiguration.Domain.Interfaces.Repository;
using Excellerent.EppConfiguration.Domain.Interfaces.Service;
using Excellerent.EppConfiguration.Domain.Model;
using Excellerent.SharedModules.DTO;
using Excellerent.SharedModules.Seed;
using Excellerent.SharedModules.Services;
using LinqKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Excellerent.EppConfiguration.Domain.Services
{
    public class DepartmentService : CRUD<DepartmentEntity, Department>, IDepartmentService
    {
        private readonly IDepartmentRepository _departmentRepository;
       

        public DepartmentService(IDepartmentRepository departmentRepository) : base(departmentRepository)
        {
            this._departmentRepository = departmentRepository;
        }

        public async Task<ResponseDTO> Get(Guid id)
        {
            Department m = await _departmentRepository.Get(id);
            DepartmentEntity e = new DepartmentEntity(m);
            return new ResponseDTO
            {
                Data = e,
                ResponseStatus = ResponseStatus.Success
            };
        }

        public async Task<ResponseDTO> All()
        {
            IEnumerable<Department> departments = await _departmentRepository.GetAllAsync();
            return new ResponseDTO
            {
                Data = departments,
                ResponseStatus = ResponseStatus.Success
            };
        }

        public async Task<Department> FindOneAsyncForDelete(Guid id)
        {
            return await _departmentRepository.FindOneAsyncForDelete(d => d.Guid == id);
        }

        public async Task<PredicatedResponseDTO> GetWithPredicate(string searchKey, int? pageIndex, int? pageSize, string? sortBy, SortOrder? sortOrder)
        {
            int ItemsPerPage = pageSize ?? 10;
            int PageIndex = pageIndex ?? 1;
            var predicate = PredicateBuilder.True<Department>();

            predicate = string.IsNullOrEmpty(searchKey) ? null : predicate.And(p => p.Name.ToLower().Contains(searchKey.ToLower()));

            var departments = (await _departmentRepository.GetWithPredicateAsync(predicate, PageIndex, ItemsPerPage, sortBy, sortOrder))
                    .Select(d => new DepartmentEntity(d)).ToList();

            int TotalRowCount = await _departmentRepository.Count(predicate);
            return new PredicatedResponseDTO
            {
                Data = departments,
                TotalRecord = TotalRowCount,
                PageIndex = PageIndex,
                PageSize = ItemsPerPage,
                TotalPage = TotalRowCount % ItemsPerPage == 0 ? TotalRowCount / ItemsPerPage : TotalRowCount / ItemsPerPage + 1
            };

        }
    }
}
