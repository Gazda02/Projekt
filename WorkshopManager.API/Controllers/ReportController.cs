using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using WorkshopManager.API.Services.Interfaces;

namespace WorkshopManager.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpPost("service-order/{serviceOrderId}")]
        public async Task<IActionResult> GenerateServiceOrderReport(int serviceOrderId, [FromQuery] string format = "PDF")
        {
            try
            {
                var report = await _reportService.GenerateServiceOrderReportAsync(serviceOrderId, format);
                return Ok(report);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("vehicle-history/{vehicleId}")]
        public async Task<IActionResult> GenerateVehicleHistoryReport(
            int vehicleId,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string format = "PDF")
        {
            try
            {
                var report = await _reportService.GenerateVehicleHistoryReportAsync(vehicleId, startDate, endDate, format);
                return Ok(report);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("financial")]
        public async Task<IActionResult> GenerateFinancialReport(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] string format = "Excel")
        {
            try
            {
                var report = await _reportService.GenerateFinancialReportAsync(startDate, endDate, format);
                return Ok(report);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("customer-history/{customerId}")]
        public async Task<IActionResult> GenerateCustomerHistoryReport(
            int customerId,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string format = "PDF")
        {
            try
            {
                var report = await _reportService.GenerateCustomerHistoryReportAsync(customerId, startDate, endDate, format);
                return Ok(report);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("inventory")]
        public async Task<IActionResult> GenerateInventoryReport([FromQuery] string format = "Excel")
        {
            try
            {
                var report = await _reportService.GenerateInventoryReportAsync(format);
                return Ok(report);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetReportHistory(
            [FromQuery] string type = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            var reports = await _reportService.GetReportHistoryAsync(type, startDate, endDate);
            return Ok(reports);
        }

        [HttpGet("{reportId}/download")]
        public async Task<IActionResult> DownloadReport(int reportId)
        {
            try
            {
                var fileBytes = await _reportService.GetReportFileAsync(reportId);
                var report = await _reportService.GetReportHistoryAsync();
                var reportInfo = report.First(r => r.Id == reportId);
                
                return File(fileBytes, GetContentType(reportInfo.Format), reportInfo.FilePath);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{reportId}")]
        public async Task<IActionResult> DeleteReport(int reportId)
        {
            try
            {
                await _reportService.DeleteReportAsync(reportId);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private string GetContentType(string format)
        {
            return format.ToUpper() switch
            {
                "PDF" => "application/pdf",
                "EXCEL" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                _ => "application/octet-stream"
            };
        }
    }
} 