#region License and Product Information

// 
//     This file 'GetTenantSettings.cs' is part of KonfDB application - 
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
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using KonfDB.Infrastructure.Attributes;
using KonfDB.Infrastructure.Common;
using KonfDB.Infrastructure.Database.Entities.Configuration;
using KonfDB.Infrastructure.Services;
using KonfDB.Infrastructure.Shell;

namespace KonfDB.Engine.Commands.Server
{
    [Export(typeof(ICommand))]
    [IgnoreCache]
    internal class GetTenantSettings : ICommand
    {
        public string Keyword
        {
            get { return "GetTenantSettings"; }
        }

        public string Command
        {
            get { return "GetTenantSettings /tenantId={tenantId}"; }
        }

        public string Help
        {
            get { return "Gets all configuration settings for a specific tenant (suite)."; }
        }

        public bool IsValid(CommandInput input)
        {
            return input.HasArgument("tenantId");
        }

        public CommandOutput OnExecute(CommandInput arguments)
        {
            var output = new CommandOutput
            {
                PostAction = CommandOutput.PostCommandAction.None
            };

            try
            {
                // Validate tenant ID
                if (!arguments.HasArgument("tenantId"))
                {
                    output.DisplayMessage = "Error: tenantId parameter is required";
                    output.MessageType = CommandOutput.DisplayMessageType.Error;
                    return output;
                }

                long tenantId;
                if (!long.TryParse(arguments["tenantId"], out tenantId))
                {
                    output.DisplayMessage = "Error: Invalid tenantId format. Must be a numeric value.";
                    output.MessageType = CommandOutput.DisplayMessageType.Error;
                    return output;
                }

                // Get tenant settings from the database
                var settings = CurrentHostContext.Default.Provider.ConfigurationStore.GetTenantSettings(tenantId);
                
                if (settings == null)
                {
                    output.DisplayMessage = $"Error: Tenant with ID {tenantId} not found.";
                    output.MessageType = CommandOutput.DisplayMessageType.Error;
                    output.Data = null;
                    return output;
                }

                output.DisplayMessage = "Success";
                output.MessageType = CommandOutput.DisplayMessageType.Message;
                output.Data = settings;
            }
            catch (Exception ex)
            {
                output.DisplayMessage = $"Error retrieving tenant settings: {ex.Message}";
                output.MessageType = CommandOutput.DisplayMessageType.Error;
                output.Data = null;
            }

            return output;
        }

        public AppType Type
        {
            get { return AppType.Server; }
        }

        public AuditRecordModel GetAuditCommand(CommandInput input)
        {
            return new AuditRecordModel
            {
                Area = "Tenant",
                Reason = "Retrieved tenant settings",
                Message = input.HasArgument("tenantId") ? $"Retrieved settings for tenant {input["tenantId"]}" : "Retrieved tenant settings",
                Key = input.HasArgument("tenantId") ? input["tenantId"] : string.Empty,
                UserId = input.GetUserId()
            };
        }
    }
}