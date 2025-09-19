#region License and Product Information

// 
//     This file 'TenantSettingsModel.cs' is part of KonfDB application - 
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
using Newtonsoft.Json;

namespace KonfDB.Infrastructure.Database.Entities.Configuration
{
    [Serializable]
    public class TenantSettingsModel : BaseModel
    {
        [JsonProperty("tenantId")]
        public long TenantId { get; set; }

        [JsonProperty("tenantName")]
        public string TenantName { get; set; }

        [JsonProperty("isActive")]
        public bool IsActive { get; set; }

        [JsonProperty("settings")]
        public Dictionary<string, object> Settings { get; set; }

        [JsonProperty("defaultValues")]
        public Dictionary<string, object> DefaultValues { get; set; }

        [JsonProperty("metadata")]
        public TenantMetadata Metadata { get; set; }

        public TenantSettingsModel()
        {
            Settings = new Dictionary<string, object>();
            DefaultValues = new Dictionary<string, object>();
            Metadata = new TenantMetadata();
        }

        /// <summary>
        /// Merges settings with default values, giving priority to actual settings
        /// </summary>
        public Dictionary<string, object> GetMergedSettings()
        {
            var merged = new Dictionary<string, object>(DefaultValues);
            
            foreach (var setting in Settings)
            {
                merged[setting.Key] = setting.Value;
            }
            
            return merged;
        }
    }

    [Serializable]
    public class TenantMetadata
    {
        [JsonProperty("createdDate")]
        public DateTime CreatedDate { get; set; }

        [JsonProperty("modifiedDate")]
        public DateTime ModifiedDate { get; set; }

        [JsonProperty("lastAccessedDate")]
        public DateTime LastAccessedDate { get; set; }

        [JsonProperty("settingsCount")]
        public int SettingsCount { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        public TenantMetadata()
        {
            CreatedDate = DateTime.UtcNow;
            ModifiedDate = DateTime.UtcNow;
            LastAccessedDate = DateTime.UtcNow;
            Version = "1.0";
        }
    }
}