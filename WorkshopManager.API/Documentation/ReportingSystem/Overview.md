# Car Workshop Management System - Reporting System Overview

The reporting system provides comprehensive reporting capabilities for the Car Workshop Management System. It allows users to generate, manage, and retrieve various types of reports in different formats (PDF and Excel).

## Available Report Types

1. **Service Order Reports (PDF)**
   - Detailed information about specific service orders
   - Customer and vehicle details
   - List of tasks performed
   - Parts used and costs
   - Total cost calculation

2. **Vehicle History Reports (PDF)**
   - Complete service history for a specific vehicle
   - All service orders within a date range
   - Maintenance timeline
   - Parts replacement history

3. **Financial Reports (Excel)**
   - Revenue analysis
   - Labor costs breakdown
   - Parts costs analysis
   - Period-based financial summaries

4. **Customer History Reports (PDF)**
   - Complete service history for a customer
   - All vehicles owned
   - Service orders for each vehicle
   - Timeline of services

5. **Inventory Reports (Excel)**
   - Current stock levels
   - Part usage statistics
   - Value of inventory
   - Parts movement analysis

## Key Features

- **Multiple Formats**: Support for both PDF (documents) and Excel (data analysis) formats
- **Date Range Filtering**: Generate reports for specific time periods
- **Report History**: Track and manage generated reports
- **Secure Access**: Authorization required for all report operations
- **File Management**: Automatic file storage and cleanup
- **Error Handling**: Comprehensive error handling and user feedback

## Report Storage

Reports are stored in two ways:
1. **File System**: Physical report files are stored in the configured reports directory
2. **Database**: Report metadata is stored in the database for easy querying and management

## Configuration

The reporting system uses the following configuration in `appsettings.json`:

```json
{
  "FileStorage": {
    "ReportDirectory": "Reports"
  }
}
```

## Dependencies

- itext7: PDF generation
- EPPlus: Excel file generation

For more detailed information, please refer to the specific documentation files:
- [API_Endpoints.md](./API_Endpoints.md) - Detailed API documentation
- [Report_Types.md](./Report_Types.md) - Detailed description of each report type
- [Integration_Guide.md](./Integration_Guide.md) - Guide for integrating reporting in applications 