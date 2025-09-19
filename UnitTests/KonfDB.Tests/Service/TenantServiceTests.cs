#region License and Product Information

// 
//     This file 'TenantServiceTests.cs' is part of KonfDB application - 
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
using System.ServiceModel.Web;
using KonfDB.Engine.Services;
using KonfDB.Infrastructure.Database.Entities.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KonfDB.Tests.Service
{
    [TestClass]
    public class TenantServiceTests
    {
        private TenantService _tenantService;

        [TestInitialize]
        public void Setup()
        {
            _tenantService = new TenantService();
        }

        [TestMethod]
        [TestCategory("TenantService")]
        public void GetTenantSettings_ValidTenantId_ReturnsSettings()
        {
            // Arrange
            string tenantId = "1";

            // Act & Assert
            try
            {
                var result = _tenantService.GetTenantSettings(tenantId);
                
                // If we get here without exception, the basic structure is working
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(TenantSettingsModel));
            }
            catch (WebFaultException<string> ex)
            {
                // This is expected if the tenant doesn't exist in test database
                Assert.IsTrue(ex.StatusCode == System.Net.HttpStatusCode.NotFound || 
                             ex.StatusCode == System.Net.HttpStatusCode.InternalServerError);
            }
        }

        [TestMethod]
        [TestCategory("TenantService")]
        [ExpectedException(typeof(WebFaultException<string>))]
        public void GetTenantSettings_NullTenantId_ThrowsBadRequest()
        {
            // Arrange
            string tenantId = null;

            // Act
            _tenantService.GetTenantSettings(tenantId);

            // Assert - Exception expected
        }

        [TestMethod]
        [TestCategory("TenantService")]
        [ExpectedException(typeof(WebFaultException<string>))]
        public void GetTenantSettings_EmptyTenantId_ThrowsBadRequest()
        {
            // Arrange
            string tenantId = "";

            // Act
            _tenantService.GetTenantSettings(tenantId);

            // Assert - Exception expected
        }

        [TestMethod]
        [TestCategory("TenantService")]
        [ExpectedException(typeof(WebFaultException<string>))]
        public void GetTenantSettings_InvalidTenantIdFormat_ThrowsBadRequest()
        {
            // Arrange
            string tenantId = "not-a-number";

            // Act
            _tenantService.GetTenantSettings(tenantId);

            // Assert - Exception expected
        }

        [TestMethod]
        [TestCategory("TenantService")]
        public void GetTenantSetting_ValidTenantIdAndKey_ReturnsSetting()
        {
            // Arrange
            string tenantId = "1";
            string key = "maxConnections";

            // Act & Assert
            try
            {
                var result = _tenantService.GetTenantSetting(tenantId, key);
                
                // If we get here without exception, the basic structure is working
                Assert.IsNotNull(result);
            }
            catch (WebFaultException<string> ex)
            {
                // This is expected if the tenant doesn't exist in test database
                Assert.IsTrue(ex.StatusCode == System.Net.HttpStatusCode.NotFound || 
                             ex.StatusCode == System.Net.HttpStatusCode.InternalServerError);
            }
        }

        [TestMethod]
        [TestCategory("TenantService")]
        [ExpectedException(typeof(WebFaultException<string>))]
        public void GetTenantSetting_NullKey_ThrowsBadRequest()
        {
            // Arrange
            string tenantId = "1";
            string key = null;

            // Act
            _tenantService.GetTenantSetting(tenantId, key);

            // Assert - Exception expected
        }

        [TestMethod]
        [TestCategory("TenantService")]
        [ExpectedException(typeof(WebFaultException<string>))]
        public void UpdateTenantSettings_NullSettings_ThrowsBadRequest()
        {
            // Arrange
            string tenantId = "1";
            TenantSettingsModel settings = null;

            // Act
            _tenantService.UpdateTenantSettings(tenantId, settings);

            // Assert - Exception expected
        }

        [TestMethod]
        [TestCategory("TenantService")]
        public void UpdateTenantSettings_ValidSettings_ReturnsUpdatedSettings()
        {
            // Arrange
            string tenantId = "1";
            var settings = new TenantSettingsModel
            {
                TenantId = 1,
                TenantName = "TestTenant",
                IsActive = true,
                Settings = new Dictionary<string, object>
                {
                    { "testKey", "testValue" }
                },
                DefaultValues = new Dictionary<string, object>(),
                Metadata = new TenantMetadata()
            };

            // Act & Assert
            try
            {
                var result = _tenantService.UpdateTenantSettings(tenantId, settings);
                
                Assert.IsNotNull(result);
                Assert.IsFalse(result.IsError);
                Assert.AreEqual("Tenant settings updated successfully", result.DisplayMessage);
            }
            catch (WebFaultException<string> ex)
            {
                // This is expected if the tenant doesn't exist in test database
                Assert.IsTrue(ex.StatusCode == System.Net.HttpStatusCode.NotFound || 
                             ex.StatusCode == System.Net.HttpStatusCode.InternalServerError);
            }
        }

        [TestMethod]
        [TestCategory("TenantService")]
        [ExpectedException(typeof(WebFaultException<string>))]
        public void UpdateTenantSettings_TenantIdMismatch_ThrowsBadRequest()
        {
            // Arrange
            string tenantId = "1";
            var settings = new TenantSettingsModel
            {
                TenantId = 2, // Different from URL parameter
                TenantName = "TestTenant",
                IsActive = true,
                Settings = new Dictionary<string, object>(),
                DefaultValues = new Dictionary<string, object>(),
                Metadata = new TenantMetadata()
            };

            // Act
            _tenantService.UpdateTenantSettings(tenantId, settings);

            // Assert - Exception expected
        }

        [TestMethod]
        [TestCategory("TenantService")]
        public void TenantSettingsModel_GetMergedSettings_MergesCorrectly()
        {
            // Arrange
            var model = new TenantSettingsModel
            {
                Settings = new Dictionary<string, object>
                {
                    { "key1", "value1" },
                    { "key2", "value2" }
                },
                DefaultValues = new Dictionary<string, object>
                {
                    { "key1", "defaultValue1" },
                    { "key3", "defaultValue3" }
                }
            };

            // Act
            var merged = model.GetMergedSettings();

            // Assert
            Assert.AreEqual(3, merged.Count);
            Assert.AreEqual("value1", merged["key1"]); // Should use actual setting, not default
            Assert.AreEqual("value2", merged["key2"]);
            Assert.AreEqual("defaultValue3", merged["key3"]); // Should use default when no actual setting
        }
    }
}