# Report Types Documentation

This document provides detailed information about each report type available in the Car Workshop Management System.

## Service Order Report (PDF)

A comprehensive document detailing a single service order.

### Content
1. **Header Information**
   - Report title and generation date
   - Service order ID and date
   - Workshop details

2. **Customer Information**
   - Full name
   - Email address
   - Contact details

3. **Vehicle Information**
   - Make and model
   - VIN number
   - Registration number

4. **Service Details**
   - List of tasks performed
   - Labor costs per task
   - Parts used with quantities and costs
   - Total cost calculation

### Sample Layout
```
Service Order Report #123
Generated: 2024-03-15 10:30:00

Customer Information:
Name: John Smith
Email: john.smith@email.com

Vehicle Information:
Make: Toyota
Model: Camry
VIN: 1HGCM82633A123456
Registration: ABC-123

Service Tasks:
- Oil Change
  Labor Cost: $50.00
  Parts Used:
    • Oil Filter x1 - $15.00
    • Engine Oil 5W-30 x5 - $45.00

Total Cost: $110.00
```

## Vehicle History Report (PDF)

A chronological history of all service orders for a specific vehicle.

### Content
1. **Vehicle Details**
   - Make, model, and year
   - VIN and registration
   - Owner information

2. **Service History**
   - Chronological list of service orders
   - Date and type of service
   - Tasks performed
   - Parts replaced
   - Costs per service

3. **Maintenance Timeline**
   - Visual representation of service dates
   - Regular maintenance patterns
   - Major repairs highlighted

## Financial Report (Excel)

A detailed financial analysis report with multiple worksheets.

### Worksheets
1. **Summary**
   - Total revenue
   - Total labor costs
   - Total parts costs
   - Net profit
   - Period comparison

2. **Orders Detail**
   - Order-by-order breakdown
   - Date
   - Labor costs
   - Parts costs
   - Total per order

3. **Revenue Analysis**
   - Daily/weekly/monthly trends
   - Service type distribution
   - Revenue by service category

### Formulas Used
- Revenue calculation: `=SUM(Labor_Cost + Parts_Cost)`
- Profit margins: `=(Revenue - Costs) / Revenue`
- Growth rates: `=(Current_Period - Previous_Period) / Previous_Period`

## Customer History Report (PDF)

A comprehensive report of all services performed for a customer across all their vehicles.

### Content
1. **Customer Profile**
   - Personal information
   - Contact details
   - Customer since date

2. **Vehicles Owned**
   - List of all vehicles
   - Vehicle details
   - Purchase/registration dates

3. **Service History per Vehicle**
   - Chronological service records
   - Maintenance patterns
   - Total spending

## Inventory Report (Excel)

A detailed analysis of parts inventory and usage.

### Worksheets
1. **Current Stock**
   - Part number
   - Name
   - Quantity in stock
   - Unit price
   - Total value
   - Reorder levels

2. **Usage Analysis**
   - Usage frequency
   - Most used parts
   - Seasonal patterns
   - Cost analysis

3. **Value Summary**
   - Total inventory value
   - Value by category
   - Slow-moving items
   - Critical items

### Key Metrics
- Stock turnover rate
- Days of inventory
- Reorder points
- Value distribution

## Report Formats

### PDF Reports
- Generated using iText7
- Professional formatting
- Consistent branding
- Printer-friendly layout

### Excel Reports
- Generated using EPPlus
- Data analysis ready
- Formula-enabled
- Chart visualization
- Filtering and sorting enabled

## Best Practices

1. **Report Generation**
   - Use appropriate date ranges
   - Include all relevant details
   - Maintain consistent formatting
   - Include generation metadata

2. **Data Presentation**
   - Clear hierarchical structure
   - Logical information flow
   - Important information highlighted
   - Consistent units and formats

3. **File Management**
   - Unique file naming
   - Organized storage
   - Regular cleanup
   - Backup strategy 