﻿using Excellerent.ClientManagement.Domain.Entities;
using Excellerent.ClientManagement.Domain.Models;
using Excellerent.SharedModules.DTO;
using Excellerent.SharedModules.Interface.Service;
using System;
using System.Threading.Tasks;

namespace Excellerent.ClientManagement.Domain.Interfaces.ServiceInterface
{
    public interface ICompanyContactService : ICRUD<CompanyContactEntity, CompanyContact>
    {
        Task<ResponseDTO> DeleteCompanyContact(Guid id);

    }
}