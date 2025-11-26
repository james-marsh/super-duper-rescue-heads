# API Contracts: Collections API

This directory contains the API contracts for the Collections Management feature.

## Files

- **collections-api.yaml**: OpenAPI 3.0 specification for the Collections REST API

## Viewing the OpenAPI Specification

### Online Viewers

1. **Swagger Editor**: https://editor.swagger.io/
   - Copy/paste the YAML content to view and test the API

2. **Redoc**: https://redocly.github.io/redoc/
   - Paste the YAML URL to generate beautiful API documentation

### Local Tools

```bash
# Install Swagger UI locally
npm install -g swagger-ui-watcher

# View the spec
swagger-ui-watcher ./collections-api.yaml
```

## API Endpoints Summary

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| `GET` | `/api/v1/collections` | List all collections for authenticated user (paginated) | ✅ Required |
| `POST` | `/api/v1/collections` | Create a new collection | ✅ Required |
| `GET` | `/api/v1/collections/{id}` | Get a single collection by ID | ✅ Required |
| `PUT` | `/api/v1/collections/{id}` | Update collection name and/or description | ✅ Required |
| `DELETE` | `/api/v1/collections/{id}` | Delete a collection (hard delete) | ✅ Required |

## Authentication

All endpoints require authentication via Bearer token (JWT from ASP.NET Core Identity).

**Header Format**:
```
Authorization: Bearer <jwt-token>
```

## Request/Response Examples

### Create Collection

**Request**:
```bash
curl -X POST https://api.superduperrescueheads.com/api/v1/collections \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Classic Rock Vinyl",
    "description": "My favorite classic rock albums from the 70s and 80s",
    "itemTypeId": 1
  }'
```

**Response** (201 Created):
```json
{
  "collectionId": "550e8400-e29b-41d4-a716-446655440000",
  "name": "Classic Rock Vinyl",
  "description": "My favorite classic rock albums from the 70s and 80s",
  "itemType": {
    "id": 1,
    "name": "Vinyl Record"
  },
  "itemCount": 0,
  "createdAt": "2025-11-24T16:45:00Z",
  "updatedAt": "2025-11-24T16:45:00Z"
}
```

### List Collections

**Request**:
```bash
curl -X GET "https://api.superduperrescueheads.com/api/v1/collections?skip=0&take=20" \
  -H "Authorization: Bearer <token>"
```

**Response** (200 OK):
```json
{
  "data": [
    {
      "collectionId": "550e8400-e29b-41d4-a716-446655440000",
      "name": "Classic Rock Vinyl",
      "description": "My favorite classic rock albums from the 70s and 80s",
      "itemType": {
        "id": 1,
        "name": "Vinyl Record"
      },
      "itemCount": 47,
      "createdAt": "2025-11-20T14:30:00Z",
      "updatedAt": "2025-11-23T10:15:00Z"
    }
  ],
  "pagination": {
    "total": 42,
    "skip": 0,
    "take": 20,
    "hasMore": true
  }
}
```

## Error Responses

All errors follow RFC 7807 Problem Details format.

### Example: Duplicate Collection Name (409 Conflict)

```json
{
  "type": "https://api.superduperrescueheads.com/errors/duplicate-collection-name",
  "title": "Duplicate Collection Name",
  "status": 409,
  "detail": "A collection with the name 'Classic Rock Vinyl' already exists in your account. Please choose a different name.",
  "instance": "/api/v1/collections",
  "errors": {
    "name": ["Collection name must be unique"]
  }
}
```

### Example: Collection Limit Exceeded (409 Conflict)

```json
{
  "type": "https://api.superduperrescueheads.com/errors/collection-limit-exceeded",
  "title": "Collection Limit Exceeded",
  "status": 409,
  "detail": "You have reached the maximum of 100 collections. Please delete unused collections before creating new ones.",
  "instance": "/api/v1/collections",
  "extensions": {
    "currentCount": 100,
    "maxAllowed": 100
  }
}
```

### Example: Validation Failed (422 Unprocessable Entity)

```json
{
  "type": "https://api.superduperrescueheads.com/errors/validation-failed",
  "title": "Validation Failed",
  "status": 422,
  "detail": "One or more validation errors occurred.",
  "instance": "/api/v1/collections",
  "errors": {
    "name": ["Collection name must be between 1 and 100 characters"],
    "itemTypeId": ["Item type ID must be 1, 2, or 3"]
  }
}
```

## Validation Rules

### Collection Name
- ✅ Required
- ✅ Length: 1-100 characters
- ✅ Must be unique per user
- ✅ Automatically trimmed

### Description
- ✅ Optional
- ✅ Max length: 500 characters

### Item Type ID
- ✅ Required
- ✅ Must be one of: 1 (Vinyl Record), 2 (Comic Book), 3 (Puzzle)
- ✅ Cannot be changed after creation

## Rate Limiting

(To be implemented)

Planned rate limits:
- 1,000 requests per hour per user
- 100 requests per minute per user

## Versioning

API uses URL versioning: `/api/v1/collections`

Breaking changes will result in a new API version (`/api/v2/`).

## Testing Contracts

Contract tests are located in `tests/Contract/Endpoints/CollectionsApiContractTests.cs`.

Run contract tests:
```bash
dotnet test --filter "FullyQualifiedName~CollectionsApiContractTests"
```

## See Also

- [Data Model](../data-model.md) - Domain model and database schema
- [Quickstart Guide](../quickstart.md) - Implementation guide
- [Feature Specification](../spec.md) - Complete feature requirements
