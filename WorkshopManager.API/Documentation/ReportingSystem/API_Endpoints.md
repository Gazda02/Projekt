# Reporting System API Documentation

This document details all available API endpoints for the reporting system. All endpoints require authentication and are protected by JWT Bearer token authorization.

## Generate Reports

### Generate Service Order Report
```http
POST /api/Report/service-order/{serviceOrderId}
```

**Parameters:**
- `serviceOrderId` (path, required): ID of the service order
- `format` (query, optional): Report format ("PDF" default)

**Response:** Report metadata object
```json
{
    "id": 1,
    "title": "Service Order Report - Toyota Camry",
    "type": "ServiceOrder",
    "generatedDate": "2024-03-15T10:30:00Z",
    "filePath": "ServiceOrder_123_20240315103000.pdf",
    "format": "PDF"
}
```

### Generate Vehicle History Report
```http
POST /api/Report/vehicle-history/{vehicleId}
```

**Parameters:**
- `vehicleId` (path, required): ID of the vehicle
- `startDate` (query, optional): Start date for history
- `endDate` (query, optional): End date for history
- `format` (query, optional): Report format ("PDF" default)

### Generate Financial Report
```http
POST /api/Report/financial
```

**Parameters:**
- `startDate` (query, required): Start date for financial period
- `endDate` (query, required): End date for financial period
- `format` (query, optional): Report format ("Excel" default)

### Generate Customer History Report
```http
POST /api/Report/customer-history/{customerId}
```

**Parameters:**
- `customerId` (path, required): ID of the customer
- `startDate` (query, optional): Start date for history
- `endDate` (query, optional): End date for history
- `format` (query, optional): Report format ("PDF" default)

### Generate Inventory Report
```http
POST /api/Report/inventory
```

**Parameters:**
- `format` (query, optional): Report format ("Excel" default)

## Manage Reports

### Get Report History
```http
GET /api/Report/history
```

**Parameters:**
- `type` (query, optional): Filter by report type
- `startDate` (query, optional): Filter by generation date start
- `endDate` (query, optional): Filter by generation date end

**Response:** Array of report metadata objects
```json
[
    {
        "id": 1,
        "title": "Service Order Report - Toyota Camry",
        "type": "ServiceOrder",
        "generatedDate": "2024-03-15T10:30:00Z",
        "filePath": "ServiceOrder_123_20240315103000.pdf",
        "format": "PDF"
    }
]
```

### Download Report
```http
GET /api/Report/{reportId}/download
```

**Parameters:**
- `reportId` (path, required): ID of the report to download

**Response:** File download with appropriate content type

### Delete Report
```http
DELETE /api/Report/{reportId}
```

**Parameters:**
- `reportId` (path, required): ID of the report to delete

**Response:** 204 No Content on success

## Error Responses

All endpoints may return the following error responses:

- **400 Bad Request**: Invalid parameters or request
- **401 Unauthorized**: Missing or invalid authentication
- **403 Forbidden**: Insufficient permissions
- **404 Not Found**: Report or related entity not found
- **500 Internal Server Error**: Server-side error

Example error response:
```json
{
    "message": "Service order with ID 123 not found."
}
```

## Content Types

Reports are available in the following formats:
- PDF: `application/pdf`
- Excel: `application/vnd.openxmlformats-officedocument.spreadsheetml.sheet`

## Authentication

All endpoints require a valid JWT Bearer token in the Authorization header:
```http
Authorization: Bearer <your_token_here>
``` 