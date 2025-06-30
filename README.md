# 🕒 Employee Time Tracker - Task 2 (C#)

This project retrieves employee work time data from a public API and generates a clean, sortable **HTML table** showing:

✅ Employee Names  
✅ Total Time Worked (aggregated in hours)  
✅ Visual highlight for employees with less than 100 total hours worked  

---

## 📦 Features

- Fetches data from a live API endpoint using C#  
- Calculates total hours worked by each employee  
- Generates an HTML table displaying:
  - Employee Name  
  - Total Time Worked (hours)  
  - Colored rows for employees with total hours < 100  
- Easy to extend into a full C# Web Application if desired  

---

## 🌐 Data Source

The application uses the following public API to retrieve employee work time data:


The API provides JSON records containing:

- `EmployeeName`  
- `StarTimeUtc` (Start time in UTC)  
- `EndTimeUtc` (End time in UTC)  
- `EntryNotes`  
- `DeletedOn`  

Only valid, non-deleted entries are considered for total hours calculation.

---

## 🚀 How to Run (Console Version)

### Prerequisites

- [.NET 6.0 or newer](https://dotnet.microsoft.com/download) installed  

### Steps
### Output
![Screenshot from 2025-06-30 17-55-25](https://github.com/user-attachments/assets/1df73ecb-72bf-42e4-9fde-ff48b94db666)
![Screenshot from 2025-06-30 17-55-35](https://github.com/user-attachments/assets/430f88f6-889d-48ba-bf70-fb12cce260fe)

1. Clone the repository:

```bash
git clone https://github.com/yourusername/EmployeeTimeTrackerCSharp.git
cd EmployeeTimeTrackerCSharp
dotnet run
