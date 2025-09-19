#region License and Product Information

// 
//     This file 'TenantService.cs' is part of KonfDB application - 
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

using System;
using System.ServiceModel;
using System.ServiceModel.Web;
using KonfDB.Infrastructure.Common;
using KonfDB.Infrastructure.Database.Entities.Configuration;
using KonfDB.Infrastructure.Services;
using KonfDB.Infrastructure.Shell;

namespace KonfDB.Engine.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class TenantService : ITenantService
    {
        private readonly ServiceCore _core = new ServiceCore();

        public TenantSettingsModel GetTenantSettings(string tenantId)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(tenantId))
                {
                    var fault = new ArgumentException("TenantId cannot be null or empty");
                    throw new WebFaultException<string>(fault.Message, System.Net.HttpStatusCode.BadRequest);
                }

                long tenantIdLong;
                if (!long.TryParse(tenantId, out tenantIdLong))
                {
                    throw new WebFaultException<string>("Invalid tenantId format. Must be a numeric value.", 
                        System.Net.HttpStatusCode.BadRequest);
                }

                // Execute the command through the command infrastructure
                var commandInput = new CommandInput
                {
                    Command = "GetTenantSettings",
                    ["tenantId"] = tenantId
                };

                var context = new ServiceRequestContext
                {
                    SessionId = Guid.NewGuid().ToString(),
                    Command = commandInput.ToString()
                };

                var result = _core.ExecuteCommand(context);
                
                if (result.IsError)
                {
                    if (result.DisplayMessage.Contains("not found"))
                    {
                        throw new WebFaultException<string>(result.DisplayMessage, 
                            System.Net.HttpStatusCode.NotFound);
                    }
                    throw new WebFaultException<string>(result.DisplayMessage, 
                        System.Net.HttpStatusCode.InternalServerError);
                }

                return result.Data as TenantSettingsModel;
            }
            catch (WebFaultException<string>)
            {
                throw; // Re-throw WebFaultExceptions as-is
            }
            catch (Exception ex)
            {
                // Log the exception here if logging is available
                throw new WebFaultException<string>($"An error occurred while retrieving tenant settings: {ex.Message}", 
                    System.Net.HttpStatusCode.InternalServerError);
            }
        }

        public ServiceCommandOutput<TenantSettingsModel> UpdateTenantSettings(string tenantId, TenantSettingsModel settings)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(tenantId))
                {
                    throw new WebFaultException<string>("TenantId cannot be null or empty", 
                        System.Net.HttpStatusCode.BadRequest);
                }

                if (settings == null)
                {
                    throw new WebFaultException<string>("Settings cannot be null", 
                        System.Net.HttpStatusCode.BadRequest);
                }

                long tenantIdLong;
                if (!long.TryParse(tenantId, out tenantIdLong))
                {
                    throw new WebFaultException<string>("Invalid tenantId format. Must be a numeric value.", 
                        System.Net.HttpStatusCode.BadRequest);
                }

                // Ensure tenant isolation - the tenantId in the URL must match the one in the settings
                if (settings.TenantId != 0 && settings.TenantId != tenantIdLong)
                {
                    throw new WebFaultException<string>("TenantId mismatch. The tenantId in the URL must match the one in the settings.", 
                        System.Net.HttpStatusCode.BadRequest);
                }

                settings.TenantId = tenantIdLong;

                // Update through the database store
                var updated = CurrentHostContext.Default.Provider.ConfigurationStore.UpdateTenantSettings(settings);
                
                return new ServiceCommandOutput<TenantSettingsModel>
                {
                    Data = updated,
                    DisplayMessage = "Tenant settings updated successfully",
                    IsError = false
                };
            }
            catch (WebFaultException<string>)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new WebFaultException<string>($"An error occurred while updating tenant settings: {ex.Message}", 
                    System.Net.HttpStatusCode.InternalServerError);
            }
        }

        public object GetTenantSetting(string tenantId, string key)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(tenantId))
                {
                    throw new WebFaultException<string>("TenantId cannot be null or empty", 
                        System.Net.HttpStatusCode.BadRequest);
                }

                if (string.IsNullOrWhiteSpace(key))
                {
                    throw new WebFaultException<string>("Setting key cannot be null or empty", 
                        System.Net.HttpStatusCode.BadRequest);
                }

                long tenantIdLong;
                if (!long.TryParse(tenantId, out tenantIdLong))
                {
                    throw new WebFaultException<string>("Invalid tenantId format. Must be a numeric value.", 
                        System.Net.HttpStatusCode.BadRequest);
                }

                var settings = GetTenantSettings(tenantId);
                if (settings == null)
                {
                    throw new WebFaultException<string>($"Tenant with ID {tenantId} not found", 
                        System.Net.HttpStatusCode.NotFound);
                }

                var mergedSettings = settings.GetMergedSettings();
                
                if (mergedSettings.ContainsKey(key))
                {
                    return mergedSettings[key];
                }

                throw new WebFaultException<string>($"Setting with key '{key}' not found for tenant {tenantId}", 
                    System.Net.HttpStatusCode.NotFound);
            }
            catch (WebFaultException<string>)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new WebFaultException<string>($"An error occurred while retrieving tenant setting: {ex.Message}", 
                    System.Net.HttpStatusCode.InternalServerError);
            }
        }

        public ServiceCommandOutput<object> UpdateTenantSetting(string tenantId, string key, object value)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(tenantId))
                {
                    throw new WebFaultException<string>("TenantId cannot be null or empty", 
                        System.Net.HttpStatusCode.BadRequest);
                }

                if (string.IsNullOrWhiteSpace(key))
                {
                    throw new WebFaultException<string>("Setting key cannot be null or empty", 
                        System.Net.HttpStatusCode.BadRequest);
                }

                long tenantIdLong;
                if (!long.TryParse(tenantId, out tenantIdLong))
                {
                    throw new WebFaultException<string>("Invalid tenantId format. Must be a numeric value.", 
                        System.Net.HttpStatusCode.BadRequest);
                }

                var settings = GetTenantSettings(tenantId);
                if (settings == null)
                {
                    throw new WebFaultException<string>($"Tenant with ID {tenantId} not found", 
                        System.Net.HttpStatusCode.NotFound);
                }

                settings.Settings[key] = value;
                settings.Metadata.ModifiedDate = DateTime.UtcNow;
                settings.Metadata.SettingsCount = settings.Settings.Count;

                var updated = CurrentHostContext.Default.Provider.ConfigurationStore.UpdateTenantSettings(settings);

                return new ServiceCommandOutput<object>
                {
                    Data = value,
                    DisplayMessage = $"Setting '{key}' updated successfully for tenant {tenantId}",
                    IsError = false
                };
            }
            catch (WebFaultException<string>)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new WebFaultException<string>($"An error occurred while updating tenant setting: {ex.Message}", 
                    System.Net.HttpStatusCode.InternalServerError);
            }
        }
    }
}