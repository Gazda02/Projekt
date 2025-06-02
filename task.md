

# 🧑‍🔧 Final Project – ASP.NET Core

## **Car Workshop Management System 2.0**

---

## 🧠 **Project Goal**

Design and implement a web application for managing a car workshop. The application should allow:

* Managing customers and their vehicles,
* Creating service orders with tasks and parts,
* Assigning tasks to mechanics,
* Adding comments to orders,
* Generating PDF reports,
* Uploading vehicle photos,
* Filtering and reporting repairs.

The project should have a clear structure, be modular, and use modern tools such as: **EF Core**, **Dependency Injection**, **Mapperly**, **Identity**, **Swagger**, **Razor Pages, MVC (or SPA frontend)**.

---

## 🧑‍🤝‍🧑 Team

* Team size: **2 people**
* Work on a GitHub / GitLab repository (commit history required)

---

## ✅ **Functional Requirements**

| Module            | Requirements                                                                       |
| ----------------- | ---------------------------------------------------------------------------------- |
| 🔐 Authentication | Registration, login (ASP.NET Identity), roles: `Admin`, `Mechanic`, `Receptionist` |
| 👤 Customers      | Customer CRUD, search, list of customer's vehicles                                 |
| 🚘 Vehicles       | CRUD, vehicle photo (upload), VIN, registration                                    |
| 🧾 Service Orders | Creating orders, statuses, assigning mechanics                                     |
| 🔧 Service Tasks  | List of tasks: description + labor cost                                            |
| ⚙️ Parts          | Selecting parts from catalog, quantity, cost                                       |
| 💬 Comments       | Internal comments on orders (change history)                                       |
| 📦 Parts Catalog  | CRUD for parts, only available to `Admin` / `Receptionist`                         |
| 📈 Reports        | Repair costs per client / vehicle / month + export to PDF                          |

---

## ✅ **Additional Requirements**

### 🧩 **1. Indexes – Query Optimization**

#### 📌 Task:

* Identify **at least two SELECT queries** that are frequently executed and include a **WHERE** or **JOIN** on a non-key column.
* Add **non-clustered indexes** to selected columns.
* Perform performance analysis:

  * **Query Plan screenshot** before and after adding the index.
  * Brief comparison (e.g., read count, scan vs. seek).
  * Include this in a **PDF report** with description and screenshots.

#### 📎 File: `indexes-report.pdf`

---

### 📡 **2. SQL Profiler – Endpoint Monitoring**

#### 📌 Task:

* Use **SQL Server Profiler (or EF Core Logging)**.
* Choose a specific **API endpoint**.
* Run the app → trigger the endpoint → take a screenshot showing the query in Profiler.
* Add screenshots + query explanation + a brief comment.

#### 📎 File: `sql-profiler-report.pdf`

---

### ⚙️ **3. GitHub Actions – CI/CD**

#### 📌 Task:

* Set up a workflow with the following steps:

  * build (`dotnet build`)
  * test (`dotnet test`)
  * optionally: Docker image build
  * optionally: push to DockerHub (requires token)

#### 📎 File: `README.md` → CI/CD description

#### 📎 File: `dotnet-ci.yml` in the repository

---

### 📝 **4. Error Logging – NLog**

#### 📌 Task:

* Configure **NLog** to log exceptions and events:

  * logs written to a file (e.g., `/logs/errors.log`)
  * logging errors in controllers and services
  * logging via DI (`ILogger<T>`)

---

### 📤 **5. BackgroundService – Email Report**

#### 📌 Task:

* Implement a `BackgroundService` that:

  * once a day (or every 1–2 minutes for testing) generates a report of active orders
  * saves it as a PDF (e.g., `open_orders.pdf`)
  * sends it as an email attachment to the admin (e.g., via SMTP)

#### 📎 File: `open-orders-report.pdf`

#### 📎 Class: `OpenOrderReportBackgroundService.cs`

---

### 🚀 **6. NBomber – Performance Tests**

#### 📌 Task:

* Set up **NBomber** to test a selected endpoint, e.g., `GET /api/orders/active`
* Run the test with 50 concurrent users, 100 requests
* Save a **PDF report with test results**

#### 📎 File: `nbomber-report.pdf`

#### 📎 Test code: e.g. `PerformanceTests/OrdersLoadTest.cs`

---

## 🧱 **Example Data Models**

```csharp
class Customer { ... }
class Vehicle { string ImageUrl; ... }
class ServiceOrder { Status, AssignedMechanic, List<ServiceTask>, List<Comment> }
class ServiceTask { Description, LaborCost, List<UsedPart> }
class Part { Name, UnitPrice }
class UsedPart { Part, Quantity }
class Comment { Author, Content, Timestamp }
```

---

## 🛠️ **Technical Requirements**

| Area             | Details                                                        |
| ---------------- | -------------------------------------------------------------- |
| **ASP.NET Core** | Version 7 or 8                                                 |
| **EF Core**      | Code First + migrations – SQL Server                           |
| **Identity**     | Authentication, roles, authorization                           |
| **Mapperly**     | DTO ↔️ entity mapping (e.g., with Mapperly)                    |
| **DI**           | Business services (`ICustomerService`, `IOrderService`, ...)   |
| **Swagger**      | API documentation                                              |
| **File upload**  | Vehicle photo (e.g., to `/wwwroot/uploads`)                    |
| **PDF**          | Generating reports as PDF                                      |
| **Frontend**     | Razor Pages + Bootstrap (optionally SPA: React/Blazor/Angular) |
| **Testing**      | Unit tests (xUnit/NUnit)                                       |

---

## 🗂️ **Project Structure**

```
/WorkshopManager
├── Controllers/
├── DTOs/
├── Models/
├── Services/
├── Mappers/             // Mapperly mappers
├── Views/
├── wwwroot/
│   └── uploads/         // vehicle photos
├── Data/
├── Program.cs
```

---

## ✅ Deliverables

* GitHub repository with commit history
* Working ASP.NET Core application
* Migrations + seed data (or database dump)
* `README.md` with project overview, login info, and roles

---

## 📌 Tips

* Use **Mapperly** for all domain-to-DTO mappings
* Apply **DataAnnotations** for validation
* Ensure **layer separation**: logic should be in services, not in controllers
