#region License and Product Information

// 
//     This file 'ITenantService.cs' is part of KonfDB application - 
//     a project perceived and developed by Punit Ganshani.
// 
//     KonfDB is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
// 
//     KonfDB is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
// 
//     You should have received a copy of the GNU General Public License
//     along with KonfDB.  If not, see <http://www.gnu.org/licenses/>.
// 
//     You can also view the documentation and progress of this project 'KonfDB'
//     on the project website, <http://www.konfdb.com> or on 
//     <http://www.ganshani.com/applications/konfdb>

#endregion

using System.ServiceModel;
using System.ServiceModel.Web;
using KonfDB.Infrastructure.Database.Entities.Configuration;

namespace KonfDB.Infrastructure.Services
{
    [ServiceContract(Namespace = ServiceConstants.Schema, Name = "ITenantService")]
    public interface ITenantService : IService
    {
        [OperationContract(Name = "GetTenantSettings")]
        [WebGet(ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = "/tenants/{tenantId}/settings")]
        TenantSettingsModel GetTenantSettings(string tenantId);

        [OperationContract(Name = "UpdateTenantSettings")]
        [WebInvoke(Method = "PUT", 
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = "/tenants/{tenantId}/settings")]
        ServiceCommandOutput<TenantSettingsModel> UpdateTenantSettings(string tenantId, TenantSettingsModel settings);

        [OperationContract(Name = "GetTenantSetting")]
        [WebGet(ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = "/tenants/{tenantId}/settings/{key}")]
        object GetTenantSetting(string tenantId, string key);

        [OperationContract(Name = "UpdateTenantSetting")]
        [WebInvoke(Method = "PUT",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = "/tenants/{tenantId}/settings/{key}")]
        ServiceCommandOutput<object> UpdateTenantSetting(string tenantId, string key, object value);
    }
}