# Tenant Settings REST API Documentation

## Overview
The Tenant Settings API provides endpoints for managing configuration settings specific to individual tenants in the KonfDB multi-tenant configuration service. Each tenant (suite) can have its own isolated set of configuration parameters with default values and metadata.

## Base URL
```
http://{server}:{port}/api/CommandService/
```

## Authentication
All endpoints require authentication via a token passed in the request header or query parameter.

## Endpoints

### 1. Get Tenant Settings
Retrieves all configuration settings for a specific tenant.

**Endpoint:** `GET /tenants/{tenantId}/settings`

**Parameters:**
- `tenantId` (path, required): The numeric ID of the tenant/suite

**Response:**
```json
{
  "tenantId": 1,
  "tenantName": "Suite Name",
  "isActive": true,
  "settings": {
    "key1": "value1",
    "key2": "value2"
  },
  "defaultValues": {
    "maxConnections": 100,
    "timeout": 30,
    "retryCount": 3,
    "enableLogging": true,
    "cacheEnabled": true,
    "cacheDuration": 300
  },
  "metadata": {
    "createdDate": "2024-01-01T00:00:00Z",
    "modifiedDate": "2024-01-01T00:00:00Z",
    "lastAccessedDate": "2024-01-01T00:00:00Z",
    "settingsCount": 2,
    "version": "1.0"
  }
}
```

**Status Codes:**
- `200 OK`: Settings retrieved successfully
- `400 Bad Request`: Invalid tenant ID format
- `404 Not Found`: Tenant not found
- `500 Internal Server Error`: Server error

**Example:**
```bash
curl -X GET "http://localhost:8080/api/CommandService/tenants/1/settings" \
  -H "Authorization: Bearer {token}"
```

### 2. Update Tenant Settings
Updates all settings for a specific tenant.

**Endpoint:** `PUT /tenants/{tenantId}/settings`

**Parameters:**
- `tenantId` (path, required): The numeric ID of the tenant/suite
- Request body: TenantSettingsModel JSON

**Request Body:**
```json
{
  "tenantId": 1,
  "tenantName": "Updated Suite Name",
  "isActive": true,
  "settings": {
    "key1": "newValue1",
    "key2": "newValue2",
    "newKey": "newValue"
  },
  "defaultValues": {
    "maxConnections": 100,
    "timeout": 30
  },
  "metadata": {
    "version": "1.1"
  }
}
```

**Response:**
```json
{
  "data": {
    "tenantId": 1,
    "tenantName": "Updated Suite Name",
    "isActive": true,
    "settings": { ... },
    "defaultValues": { ... },
    "metadata": { ... }
  },
  "displayMessage": "Tenant settings updated successfully",
  "isError": false
}
```

**Status Codes:**
- `200 OK`: Settings updated successfully
- `400 Bad Request`: Invalid request (null settings, ID mismatch, etc.)
- `404 Not Found`: Tenant not found
- `500 Internal Server Error`: Server error

**Example:**
```bash
curl -X PUT "http://localhost:8080/api/CommandService/tenants/1/settings" \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "tenantId": 1,
    "settings": {
      "key1": "value1"
    }
  }'
```

### 3. Get Single Tenant Setting
Retrieves a specific configuration setting for a tenant.

**Endpoint:** `GET /tenants/{tenantId}/settings/{key}`

**Parameters:**
- `tenantId` (path, required): The numeric ID of the tenant/suite
- `key` (path, required): The setting key to retrieve

**Response:**
```json
"value"
```

**Status Codes:**
- `200 OK`: Setting retrieved successfully
- `400 Bad Request`: Invalid parameters
- `404 Not Found`: Tenant or setting key not found
- `500 Internal Server Error`: Server error

**Example:**
```bash
curl -X GET "http://localhost:8080/api/CommandService/tenants/1/settings/maxConnections" \
  -H "Authorization: Bearer {token}"
```

### 4. Update Single Tenant Setting
Updates a specific configuration setting for a tenant.

**Endpoint:** `PUT /tenants/{tenantId}/settings/{key}`

**Parameters:**
- `tenantId` (path, required): The numeric ID of the tenant/suite
- `key` (path, required): The setting key to update
- Request body: The new value (can be string, number, boolean, or object)

**Request Body:**
```json
"newValue"
```

**Response:**
```json
{
  "data": "newValue",
  "displayMessage": "Setting 'key' updated successfully for tenant 1",
  "isError": false
}
```

**Status Codes:**
- `200 OK`: Setting updated successfully
- `400 Bad Request`: Invalid parameters
- `404 Not Found`: Tenant not found
- `500 Internal Server Error`: Server error

**Example:**
```bash
curl -X PUT "http://localhost:8080/api/CommandService/tenants/1/settings/maxConnections" \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '200'
```

## Error Handling

All endpoints return error responses in the following format:

```json
{
  "error": "Error message",
  "statusCode": 400
}
```

Common error scenarios:
- **Invalid Tenant ID**: Returns 400 Bad Request
- **Non-existent Tenant**: Returns 404 Not Found
- **Missing Required Parameters**: Returns 400 Bad Request
- **Tenant ID Mismatch**: Returns 400 Bad Request (when URL tenant ID doesn't match body tenant ID)
- **Server Errors**: Returns 500 Internal Server Error

## Multi-Tenant Isolation

The API ensures complete isolation between tenants:
- Settings for one tenant cannot be accessed or modified by another tenant
- Each tenant has its own namespace for configuration keys
- Default values can be tenant-specific
- Audit trails are maintained per tenant

## Best Practices

1. **Use Merged Settings**: When retrieving settings, the API merges actual settings with default values. This ensures all expected keys are present.

2. **Validate Tenant ID**: Always validate that the tenant ID is numeric before making API calls.

3. **Handle Missing Keys**: When retrieving a single setting, handle the 404 case gracefully as the key might not exist.

4. **Batch Updates**: When updating multiple settings, use the bulk update endpoint rather than multiple single-key updates for better performance.

5. **Version Management**: Use the metadata version field to track configuration schema changes.

## Command Line Examples

### Using the KonfDB Command Line Interface:

```bash
# Get all settings for tenant 1
konfdbc GetTenantSettings /tenantId=1

# Through the generic Execute command
konfdbc Execute "GetTenantSettings /tenantId=1"
```

## Integration with Existing KonfDB Features

The Tenant Settings API integrates seamlessly with existing KonfDB features:
- **Audit Logging**: All operations are logged in the audit trail
- **Encryption**: Sensitive settings can be encrypted using KonfDB's encryption features
- **Caching**: Settings are cached for performance
- **Role-Based Access**: Access is controlled based on user roles (Admin, ReadOnly)

## Migration Guide

For existing KonfDB users migrating to use tenant-specific settings:

1. Identify suite-specific parameters currently stored as regular parameters
2. Use the bulk update endpoint to migrate these to tenant settings
3. Update client applications to use the new endpoints
4. Remove old parameter mappings once migration is complete

## Performance Considerations

- Settings are cached in memory for fast retrieval
- Bulk operations are optimized for database performance
- Default values are computed once and cached
- Metadata is updated asynchronously to avoid blocking operations