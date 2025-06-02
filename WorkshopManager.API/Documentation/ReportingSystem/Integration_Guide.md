# Reporting System Integration Guide

This guide provides instructions for integrating the reporting system into your application.

## Setup

### 1. Required NuGet Packages

Install the following NuGet packages:
```bash
dotnet add package itext7
dotnet add package EPPlus
```

### 2. Configuration

Add the following to your `appsettings.json`:
```json
{
  "FileStorage": {
    "ReportDirectory": "Reports"
  }
}
```

### 3. Database Setup

The reporting system requires the following database entities:

```csharp
public class Report
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Type { get; set; }
    public DateTime GeneratedDate { get; set; }
    public string FilePath { get; set; }
    public string Format { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? VehicleId { get; set; }
    public int? ServiceOrderId { get; set; }
    public int? CustomerId { get; set; }

    // Navigation properties
    public Vehicle Vehicle { get; set; }
    public ServiceOrder ServiceOrder { get; set; }
    public Customer Customer { get; set; }
}
```

### 4. Service Registration

Register the reporting service in your `Program.cs`:
```csharp
builder.Services.AddScoped<IReportService, ReportService>();
```

## Usage Examples

### 1. Generate a Service Order Report

```csharp
public class WorkshopController : ControllerBase
{
    private readonly IReportService _reportService;

    public WorkshopController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("order/{id}/report")]
    public async Task<IActionResult> GenerateOrderReport(int id)
    {
        try
        {
            var report = await _reportService.GenerateServiceOrderReportAsync(id);
            return Ok(report);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
```

### 2. Generate and Download a Financial Report

```csharp
[HttpGet("financial-report")]
public async Task<IActionResult> GetFinancialReport(
    [FromQuery] DateTime startDate,
    [FromQuery] DateTime endDate)
{
    try
    {
        // Generate the report
        var report = await _reportService.GenerateFinancialReportAsync(
            startDate, 
            endDate, 
            "Excel"
        );

        // Get the file
        var fileBytes = await _reportService.GetReportFileAsync(report.Id);

        return File(
            fileBytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            report.FilePath
        );
    }
    catch (Exception ex)
    {
        return BadRequest(ex.Message);
    }
}
```

## Error Handling

Implement proper error handling for common scenarios:

```csharp
try
{
    var report = await _reportService.GenerateServiceOrderReportAsync(orderId);
    // Process the report
}
catch (KeyNotFoundException ex)
{
    // Handle not found error
    _logger.LogWarning(ex, "Service order not found");
    return NotFound(ex.Message);
}
catch (ArgumentException ex)
{
    // Handle invalid arguments
    _logger.LogWarning(ex, "Invalid arguments");
    return BadRequest(ex.Message);
}
catch (Exception ex)
{
    // Handle unexpected errors
    _logger.LogError(ex, "Error generating report");
    return StatusCode(500, "An unexpected error occurred");
}
```

## Best Practices

### 1. Report Generation

```csharp
// DO: Use async/await consistently
public async Task<IActionResult> GenerateReport()
{
    var report = await _reportService.GenerateReportAsync();
    return Ok(report);
}

// DON'T: Mix async and sync code
public async Task<IActionResult> GenerateReport()
{
    var report = _reportService.GenerateReportAsync().Result; // Bad practice
    return Ok(report);
}
```

### 2. File Handling

```csharp
// DO: Use using statements for file operations
private async Task SaveReportFile(string path, byte[] content)
{
    using var stream = new FileStream(path, FileMode.Create);
    await stream.WriteAsync(content);
}

// DON'T: Leave file handles open
private async Task SaveReportFile(string path, byte[] content)
{
    var stream = new FileStream(path, FileMode.Create);
    await stream.WriteAsync(content);
    // Stream not disposed properly
}
```

### 3. Memory Management

```csharp
// DO: Use streams for large files
public async Task<IActionResult> DownloadLargeReport(int reportId)
{
    var report = await _reportService.GetReportAsync(reportId);
    var stream = new FileStream(report.FilePath, FileMode.Open);
    return File(stream, GetContentType(report.Format), report.FilePath);
}

// DON'T: Load large files entirely into memory
public async Task<IActionResult> DownloadLargeReport(int reportId)
{
    var report = await _reportService.GetReportAsync(reportId);
    var bytes = await File.ReadAllBytesAsync(report.FilePath); // Memory intensive
    return File(bytes, GetContentType(report.Format), report.FilePath);
}
```

## Security Considerations

1. **File Access**
   - Validate file paths
   - Use authorization
   - Implement rate limiting

```csharp
// Validate file paths
private bool IsValidFilePath(string filePath)
{
    var fullPath = Path.GetFullPath(filePath);
    var reportDirectory = Path.GetFullPath(_configuration["FileStorage:ReportDirectory"]);
    return fullPath.StartsWith(reportDirectory);
}
```

2. **Authorization**
   - Implement role-based access
   - Validate user permissions

```csharp
[Authorize(Roles = "Admin,Manager")]
[HttpGet("financial")]
public async Task<IActionResult> GetFinancialReport()
{
    // Only accessible by admins and managers
}
```

## Maintenance

1. **Cleanup Old Reports**
```csharp
public async Task CleanupOldReports(int daysToKeep)
{
    var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);
    var oldReports = await _context.Reports
        .Where(r => r.GeneratedDate < cutoffDate)
        .ToListAsync();

    foreach (var report in oldReports)
    {
        await DeleteReportAsync(report.Id);
    }
}
```

2. **Monitor Storage**
```csharp
public async Task<StorageMetrics> GetStorageMetrics()
{
    var reportDirectory = new DirectoryInfo(_reportDirectory);
    var files = reportDirectory.GetFiles();

    return new StorageMetrics
    {
        TotalFiles = files.Length,
        TotalSize = files.Sum(f => f.Length),
        OldestFile = files.Min(f => f.CreationTime),
        NewestFile = files.Max(f => f.CreationTime)
    };
}
``` 