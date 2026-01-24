# User Search API Implementation

## ✅ Implementation Complete

The user search API has been successfully implemented on the API side.

## 📋 Files Created/Modified

### 1. **SearchUsers.cs** (NEW)
- Location: `Api/Features/Users/SearchUsers.cs`
- Contains:
  - `Query` class with all search parameters
  - `Handler` to process search requests
  - `SearchUsersEndpoint` Carter module for endpoint routing

### 2. **IUserRepository.cs** (MODIFIED)
- Added `SearchUsersAsync` method signature with all filter parameters

### 3. **UserRepository.cs** (MODIFIED)
- Implemented `SearchUsersAsync` with MongoDB filtering logic
- Supports:
  - Text search across name, username, email, mobile
  - Role filtering
  - Gender filtering  
  - Package filtering
  - Email verification status
  - Date of birth range filtering

## 🔌 API Endpoint

**GET** `/users/search`

### Query Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `search` | string? | Search text (name, username, email, mobile) |
| `role` | UserType? | Filter by role (Customer=1, Staff=2) |
| `gender` | string? | Filter by gender |
| `package` | string? | Filter by package name |
| `emailVerified` | bool? | Filter by email verification status |
| `dobFrom` | DateTime? | Date of birth from date |
| `dobTo` | DateTime? | Date of birth to date |
| `pageSize` | int? | Number of results per page |
| `pageNumber` | int? | Page number (1-based) |

### Example Requests

```http
# Search by text
GET /users/search?search=john&pageSize=10&pageNumber=1

# Search by role
GET /users/search?role=1&pageSize=10&pageNumber=1

# Combined filters
GET /users/search?search=john&role=1&emailVerified=true&pageSize=10&pageNumber=1

# Date range
GET /users/search?dobFrom=1990-01-01&dobTo=2000-12-31&pageSize=10&pageNumber=1
```

### Response Format

```json
{
  "value": [
    {
      "id": "...",
      "name": "...",
      "username": "...",
      "emailAddress": "...",
      "userRole": 1,
      ...
    }
  ],
  "success": true,
  "totalPages": 5,
  "totalItems": 47,
  "pageNumber": 1,
  "pageSize": 10
}
```

## 🔒 Security

- Endpoint includes `AuthenticationFilter` - requires authentication
- Same authentication mechanism as other user endpoints

## ⚡ Performance

- MongoDB query optimization with chained `.Match()` filters
- Only active filters are applied to the query
- Server-side filtering before pagination
- Efficient list operations for paging

## 🔄 Integration with Web

The Web project is already configured to call this endpoint:
- `Web/Features/Users/UsersEffects.cs` - `HandleSearchUsers` effect
- Endpoint path: `/users/search`
- Automatic query string building from state filters

## 🧪 Testing

Build Status: ✅ **SUCCESS** (430 warnings - XML comments only)

Test the endpoint:
1. Start the API
2. Authenticate to get a valid session token
3. Use the search endpoint with various filter combinations
4. Verify results are correctly filtered and paginated

## 📝 Notes

- Date of birth filtering uses string comparison (format: yyyy-MM-dd)
- Text search is case-insensitive
- Package search uses partial match (contains)
- All filters can be combined for precise searching
- Empty/null filters are ignored
