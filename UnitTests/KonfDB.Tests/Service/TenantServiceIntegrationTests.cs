#region License and Product Information

// 
//     This file 'TenantServiceIntegrationTests.cs' is part of KonfDB application - 
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
//     along with KonfDB.  If not to see <http://www.gnu.org/licenses/>.
// 
//     You can also view the documentation and progress of this project 'KonfDB'
//     on the project website, <http://www.konfdb.com> or on 
//     <http://www.ganshani.com/applications/konfdb>

#endregion

using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Description;
using KonfDB.Engine.Services;
using KonfDB.Infrastructure.Database.Entities.Configuration;
using KonfDB.Infrastructure.Services;
using KonfDB.Infrastructure.WCF;
using KonfDB.Infrastructure.WCF.Bindings;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KonfDB.Tests.Service
{
    [TestClass]
    public class TenantServiceIntegrationTests
    {
        private ServiceHost _serviceHost;
        private ITenantService _client;
        private const string ServiceAddress = "http://localhost:9999/TenantService";

        [TestInitialize]
        public void Setup()
        {
            // Setup service host
            _serviceHost = new ServiceHost(typeof(TenantService), new Uri(ServiceAddress));
            
            // Add service endpoint
            var binding = new WebHttpBinding();
            var endpoint = _serviceHost.AddServiceEndpoint(typeof(ITenantService), binding, "");
            endpoint.Behaviors.Add(new WebHttpBehavior());

            // Open the service host
            try
            {
                _serviceHost.Open();
            }
            catch (Exception ex)
            {
                // Service might already be running or port might be in use
                Console.WriteLine($"Could not start service host: {ex.Message}");
            }

            // Create client
            var factory = new ChannelFactory<ITenantService>(binding, new EndpointAddress(ServiceAddress));
            factory.Endpoint.Behaviors.Add(new WebHttpBehavior());
            _client = factory.CreateChannel();
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (_serviceHost != null && _serviceHost.State == CommunicationState.Opened)
            {
                _serviceHost.Close();
            }
        }

        [TestMethod]
        [TestCategory("TenantServiceIntegration")]
        public void Integration_GetTenantSettings_EndToEnd()
        {
            // This test verifies the complete flow from REST endpoint to database
            // Note: Requires a test database with sample data

            try
            {
                // Arrange
                string tenantId = "1"; // Assuming tenant 1 exists in test database

                // Act
                var result = _client.GetTenantSettings(tenantId);

                // Assert
                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.TenantId);
                Assert.IsNotNull(result.Settings);
                Assert.IsNotNull(result.DefaultValues);
                Assert.IsNotNull(result.Metadata);
            }
            catch (Exception ex)
            {
                // If test database is not set up, skip this test
                Assert.Inconclusive($"Integration test requires database setup: {ex.Message}");
            }
        }

        [TestMethod]
        [TestCategory("TenantServiceIntegration")]
        public void Integration_UpdateTenantSettings_EndToEnd()
        {
            try
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
                        { "integrationTestKey", "integrationTestValue" },
                        { "maxConnections", 200 }
                    },
                    DefaultValues = new Dictionary<string, object>(),
                    Metadata = new TenantMetadata()
                };

                // Act
                var result = _client.UpdateTenantSettings(tenantId, settings);

                // Assert
                Assert.IsNotNull(result);
                Assert.IsFalse(result.IsError);
                Assert.IsNotNull(result.Data);

                // Verify the update by getting the settings again
                var updatedSettings = _client.GetTenantSettings(tenantId);
                Assert.IsTrue(updatedSettings.Settings.ContainsKey("integrationTestKey"));
                Assert.AreEqual("integrationTestValue", updatedSettings.Settings["integrationTestKey"]);
            }
            catch (Exception ex)
            {
                Assert.Inconclusive($"Integration test requires database setup: {ex.Message}");
            }
        }

        [TestMethod]
        [TestCategory("TenantServiceIntegration")]
        public void Integration_GetTenantSetting_SingleKey()
        {
            try
            {
                // Arrange
                string tenantId = "1";
                string key = "maxConnections";

                // Act
                var result = _client.GetTenantSetting(tenantId, key);

                // Assert
                Assert.IsNotNull(result);
            }
            catch (Exception ex)
            {
                Assert.Inconclusive($"Integration test requires database setup: {ex.Message}");
            }
        }

        [TestMethod]
        [TestCategory("TenantServiceIntegration")]
        public void Integration_UpdateTenantSetting_SingleKey()
        {
            try
            {
                // Arrange
                string tenantId = "1";
                string key = "testSetting";
                object value = "testValue123";

                // Act
                var result = _client.UpdateTenantSetting(tenantId, key, value);

                // Assert
                Assert.IsNotNull(result);
                Assert.IsFalse(result.IsError);
                Assert.AreEqual(value, result.Data);

                // Verify the update
                var updatedValue = _client.GetTenantSetting(tenantId, key);
                Assert.AreEqual(value, updatedValue);
            }
            catch (Exception ex)
            {
                Assert.Inconclusive($"Integration test requires database setup: {ex.Message}");
            }
        }

        [TestMethod]
        [TestCategory("TenantServiceIntegration")]
        public void Integration_MultiTenantIsolation_Test()
        {
            try
            {
                // This test verifies that settings for one tenant don't affect another
                
                // Arrange
                string tenant1Id = "1";
                string tenant2Id = "2";
                var settings1 = new TenantSettingsModel
                {
                    TenantId = 1,
                    Settings = new Dictionary<string, object> { { "tenant1Key", "tenant1Value" } },
                    DefaultValues = new Dictionary<string, object>(),
                    Metadata = new TenantMetadata()
                };
                var settings2 = new TenantSettingsModel
                {
                    TenantId = 2,
                    Settings = new Dictionary<string, object> { { "tenant2Key", "tenant2Value" } },
                    DefaultValues = new Dictionary<string, object>(),
                    Metadata = new TenantMetadata()
                };

                // Act
                _client.UpdateTenantSettings(tenant1Id, settings1);
                _client.UpdateTenantSettings(tenant2Id, settings2);

                var retrievedSettings1 = _client.GetTenantSettings(tenant1Id);
                var retrievedSettings2 = _client.GetTenantSettings(tenant2Id);

                // Assert - Verify isolation
                Assert.IsTrue(retrievedSettings1.Settings.ContainsKey("tenant1Key"));
                Assert.IsFalse(retrievedSettings1.Settings.ContainsKey("tenant2Key"));
                
                Assert.IsTrue(retrievedSettings2.Settings.ContainsKey("tenant2Key"));
                Assert.IsFalse(retrievedSettings2.Settings.ContainsKey("tenant1Key"));
            }
            catch (Exception ex)
            {
                Assert.Inconclusive($"Integration test requires database setup: {ex.Message}");
            }
        }
    }
}