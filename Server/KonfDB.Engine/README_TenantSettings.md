# Tenant Settings Feature - Implementation Guide

## Overview
This document describes the implementation of the new Tenant Settings REST API endpoint in KonfDB. This feature allows retrieving and managing configuration settings specific to individual tenants (suites) with proper multi-tenant isolation.

## Files Added/Modified

### New Files Created:

1. **Command Implementation**
   - `/Server/KonfDB.Engine/Commands/Server/GetTenantSettings.cs`
   - Implements the command pattern for retrieving tenant settings

2. **Model Classes**
   - `/Shared/KonfDBC/Entities/Configuration/TenantSettingsModel.cs`
   - Defines the data structure for tenant settings including metadata

3. **Service Interface & Implementation**
   - `/Shared/KonfDBC/Services/ITenantService.cs`
   - `/Server/KonfDB.Engine/Services/TenantService.cs`
   - REST service implementation with proper error handling

4. **Unit Tests**
   - `/UnitTests/KonfDB.Tests/Service/TenantServiceTests.cs`
   - Unit tests covering normal and edge cases

5. **Integration Tests**
   - `/UnitTests/KonfDB.Tests/Service/TenantServiceIntegrationTests.cs`
   - End-to-end integration tests including multi-tenant isolation verification

6. **Documentation**
   - `/Server/KonfDB.Engine/API_Documentation_TenantSettings.md`
   - Complete API documentation with examples

### Modified Files:

1. **Data Store Interface**
   - `/Server/KonfDB.Infrastructure/Database/Abstracts/IConfigurationDataStore.cs`
   - Added methods: GetTenantSettings, UpdateTenantSettings, DeleteTenantSettings

2. **Data Store Implementation**
   - `/Server/KonfDB.Engine/Database/Stores/ConfigurationDataStore.cs`
   - Implemented tenant settings data access methods

## Features Implemented

### 1. REST API Endpoints

#### GET /tenants/{tenantId}/settings
- Retrieves all configuration settings for a specific tenant
- Returns settings with default values for missing keys
- Includes metadata (created date, modified date, version, etc.)

#### PUT /tenants/{tenantId}/settings
- Updates all settings for a specific tenant
- Validates tenant ID consistency
- Updates metadata automatically

#### GET /tenants/{tenantId}/settings/{key}
- Retrieves a specific setting value for a tenant
- Returns merged value (actual or default)

#### PUT /tenants/{tenantId}/settings/{key}
- Updates a specific setting value for a tenant
- Creates the setting if it doesn't exist

### 2. Data Model

**TenantSettingsModel** includes:
- `TenantId`: Unique identifier for the tenant
- `TenantName`: Human-readable tenant name
- `IsActive`: Tenant active status
- `Settings`: Dictionary of actual configuration values
- `DefaultValues`: Dictionary of default values
- `Metadata`: Creation/modification timestamps and version info

### 3. Multi-Tenant Isolation

- Complete isolation between tenants
- Settings for one tenant cannot be accessed by another
- Each tenant has its own configuration namespace
- Audit trails maintained per tenant

### 4. Error Handling

Comprehensive error handling for:
- Invalid tenant IDs (non-numeric, null, empty)
- Non-existent tenants (404 Not Found)
- Invalid request data (400 Bad Request)
- Tenant ID mismatches
- Server errors (500 Internal Server Error)

### 5. Testing

**Unit Tests** cover:
- Valid tenant ID scenarios
- Invalid input validation
- Null/empty parameter handling
- Settings merge functionality

**Integration Tests** cover:
- End-to-end REST API flow
- Multi-tenant isolation verification
- Single and bulk operations
- Error scenarios

## How to Use

### 1. Via REST API

```bash
# Get all settings for tenant 1
curl -X GET "http://localhost:8080/api/CommandService/tenants/1/settings" \
  -H "Authorization: Bearer {token}"

# Update settings for tenant 1
curl -X PUT "http://localhost:8080/api/CommandService/tenants/1/settings" \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "tenantId": 1,
    "settings": {
      "maxConnections": 200,
      "timeout": 60
    }
  }'
```

### 2. Via Command Line

```bash
# Using KonfDB command line interface
konfdbc GetTenantSettings /tenantId=1
```

### 3. Via Code

```csharp
// Using the service directly
var tenantService = new TenantService();
var settings = tenantService.GetTenantSettings("1");

// Using the command pattern
var command = new GetTenantSettings();
var input = new CommandInput { ["tenantId"] = "1" };
var output = command.OnExecute(input);
```

## Database Schema

The implementation uses existing KonfDB tables:
- `Suites`: Represents tenants
- `Parameters`: Stores configuration key-value pairs
- `Mappings`: Links parameters to suites

No database schema changes are required.

## Security Considerations

1. **Authentication**: All endpoints require valid authentication tokens
2. **Authorization**: Access controlled by user roles (Admin, ReadOnly)
3. **Input Validation**: All inputs are validated and sanitized
4. **Tenant Isolation**: Enforced at the data access layer

## Performance Optimizations

1. **Caching**: Settings are cached for fast retrieval
2. **Bulk Operations**: Optimized for database performance
3. **Lazy Loading**: Metadata computed only when needed
4. **Connection Pooling**: Efficient database connection management

## Future Enhancements

Potential improvements for future versions:
1. Setting versioning and rollback capabilities
2. Setting inheritance between environments
3. Real-time setting updates via WebSockets
4. Setting validation rules and schemas
5. Bulk import/export functionality
6. Setting change notifications

## Troubleshooting

### Common Issues:

1. **404 Not Found**: Verify the tenant ID exists in the database
2. **400 Bad Request**: Check that tenant ID is numeric
3. **500 Server Error**: Check database connectivity and logs
4. **Authentication Failed**: Ensure valid token is provided

### Logging:

All operations are logged with:
- Request/response details
- Error stack traces
- Performance metrics
- Audit trail entries

## Deployment

1. Build the solution to compile new files
2. Deploy updated assemblies to server
3. Restart KonfDB service
4. Verify endpoints are accessible
5. Run integration tests to confirm deployment

## Backward Compatibility

This feature is fully backward compatible:
- No breaking changes to existing APIs
- No database schema modifications
- Existing functionality remains unchanged
- New endpoints are additive only

## Support

For issues or questions:
- Check API documentation
- Review test cases for usage examples
- Consult KonfDB documentation at http://www.konfdb.com
- Submit issues on the project repository