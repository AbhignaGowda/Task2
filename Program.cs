using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.IO;
using System.Linq;

class Program
{
    static async Task Main(string[] args)
    {
        try
        {
            Console.WriteLine("🔄 Fetching employee time data...");
            
            string apiUrl = "https://rc-vault-fap-live-1.azurewebsites.net/api/gettimeentries?code=vO17RnE8vuzXzPJo5eaLLjXjmRW07law99QTD90zat9FfOQJKKUcgQ==";
            HttpClient client = new HttpClient();
            string response = await client.GetStringAsync(apiUrl);
            
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var entries = JsonSerializer.Deserialize<List<TimeEntry>>(response, options);
            
            Console.WriteLine($"📊 Processing {entries.Count} time entries...");
            
            Dictionary<string, EmployeeStats> employeeStats = new Dictionary<string, EmployeeStats>();
            
            foreach (var entry in entries)
            {
                if (!string.IsNullOrEmpty(entry.EmployeeName) && 
                    entry.StarTimeUtc != null && 
                    entry.EndTimeUtc != null && 
                    entry.DeletedOn == null)
                {
                    DateTime start = DateTime.Parse(entry.StarTimeUtc);
                    DateTime end = DateTime.Parse(entry.EndTimeUtc);
                    double hours = (end - start).TotalHours;
                    
                    if (hours > 0)
                    {
                        if (!employeeStats.ContainsKey(entry.EmployeeName))
                        {
                            employeeStats[entry.EmployeeName] = new EmployeeStats();
                        }
                        
                        employeeStats[entry.EmployeeName].TotalHours += hours;
                        employeeStats[entry.EmployeeName].TotalEntries++;
                        employeeStats[entry.EmployeeName].UpdateWorkingDays(start.Date);
                    }
                }
            }
            
            var sortedStats = employeeStats.OrderByDescending(kvp => kvp.Value.TotalHours).ToList();
            
            string htmlContent = GenerateEnhancedHtml(sortedStats);
            File.WriteAllText("EmployeeReport.html", htmlContent);
            
            Console.WriteLine("✅ Enhanced HTML report generated: EmployeeReport.html");
            Console.WriteLine($"📈 Report includes {sortedStats.Count} employees");
            
            // Optional: Open the file automatically
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
                {
                    FileName = "EmployeeReport.html",
                    UseShellExecute = true
                });
                Console.WriteLine("🌐 Report opened in browser");
            }
            catch
            {
                Console.WriteLine("💡 Please open EmployeeReport.html in your browser to view the report");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
        }
    }
    
    static string GenerateEnhancedHtml(List<KeyValuePair<string, EmployeeStats>> sortedStats)
    {
        double totalHours = sortedStats.Sum(s => s.Value.TotalHours);
        int totalEmployees = sortedStats.Count;
        double avgHours = totalEmployees > 0 ? totalHours / totalEmployees : 0;
        
        StringBuilder html = new StringBuilder();
        html.AppendLine("<!DOCTYPE html>");
        html.AppendLine("<html lang='en'>");
        html.AppendLine("<head>");
        html.AppendLine("<meta charset='UTF-8'>");
        html.AppendLine("<meta name='viewport' content='width=device-width, initial-scale=1.0'>");
        html.AppendLine("<title>Employee Time Tracking Report</title>");
        html.AppendLine(@"<style>
            * {
                margin: 0;
                padding: 0;
                box-sizing: border-box;
            }
            
            body {
                font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
                min-height: 100vh;
                padding: 20px;
            }
            
            .container {
                max-width: 1200px;
                margin: 0 auto;
                background: white;
                border-radius: 15px;
                box-shadow: 0 20px 40px rgba(0,0,0,0.1);
                overflow: hidden;
            }
            
            .header {
                background: linear-gradient(135deg, #2c3e50 0%, #34495e 100%);
                color: white;
                padding: 30px;
                text-align: center;
            }
            
            .header h1 {
                font-size: 2.5em;
                margin-bottom: 10px;
                font-weight: 300;
            }
            
            .header .subtitle {
                opacity: 0.9;
                font-size: 1.1em;
            }
            
            .stats-grid {
                display: grid;
                grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
                gap: 20px;
                padding: 30px;
                background: #f8f9fa;
            }
            
            .stat-card {
                background: white;
                padding: 25px;
                border-radius: 10px;
                text-align: center;
                box-shadow: 0 5px 15px rgba(0,0,0,0.08);
                transition: transform 0.3s ease;
            }
            
            .stat-card:hover {
                transform: translateY(-5px);
            }
            
            .stat-number {
                font-size: 2.5em;
                font-weight: bold;
                color: #2c3e50;
                margin-bottom: 10px;
            }
            
            .stat-label {
                color: #7f8c8d;
                font-size: 0.9em;
                text-transform: uppercase;
                letter-spacing: 1px;
            }
            
            .table-container {
                padding: 0 30px 30px 30px;
            }
            
            .search-container {
                margin-bottom: 20px;
                position: relative;
            }
            
            .search-input {
                width: 100%;
                padding: 15px 50px 15px 20px;
                border: 2px solid #e0e0e0;
                border-radius: 25px;
                font-size: 16px;
                outline: none;
                transition: border-color 0.3s ease;
            }
            
            .search-input:focus {
                border-color: #667eea;
            }
            
            .search-icon {
                position: absolute;
                right: 20px;
                top: 50%;
                transform: translateY(-50%);
                color: #999;
            }
            
            .table-wrapper {
                overflow-x: auto;
                border-radius: 10px;
                box-shadow: 0 5px 15px rgba(0,0,0,0.08);
            }
            
            table {
                width: 100%;
                border-collapse: collapse;
                background: white;
                min-width: 600px;
            }
            
            th {
                background: linear-gradient(135deg, #2c3e50 0%, #34495e 100%);
                color: white;
                padding: 20px;
                text-align: left;
                font-weight: 600;
                text-transform: uppercase;
                letter-spacing: 1px;
                font-size: 0.9em;
                cursor: pointer;
                transition: background 0.3s ease;
            }
            
            th:hover {
                background: linear-gradient(135deg, #34495e 0%, #2c3e50 100%);
            }
            
            td {
                padding: 18px 20px;
                border-bottom: 1px solid #eee;
                transition: background-color 0.3s ease;
            }
            
            tr:hover {
                background-color: #f8f9fa;
            }
            
            .employee-name {
                font-weight: 600;
                color: #2c3e50;
            }
            
            .hours-cell {
                font-weight: 600;
                color: #27ae60;
            }
            
            .low-hours {
                background-color: #fff5f5 !important;
                border-left: 4px solid #e74c3c;
            }
            
            .low-hours .hours-cell {
                color: #e74c3c;
            }
            
            .high-performer {
                background-color: #f0fff4 !important;
                border-left: 4px solid #27ae60;
            }
            
            .rank-badge {
                display: inline-block;
                width: 30px;
                height: 30px;
                border-radius: 50%;
                background: #667eea;
                color: white;
                text-align: center;
                line-height: 30px;
                font-weight: bold;
                font-size: 0.9em;
            }
            
            .rank-badge.top3 {
                background: linear-gradient(135deg, #ffd700 0%, #ffed4e 100%);
                color: #333;
            }
            
            .footer {
                background: #f8f9fa;
                padding: 20px 30px;
                text-align: center;
                color: #7f8c8d;
                border-top: 1px solid #eee;
            }
            
            .no-results {
                text-align: center;
                padding: 40px;
                color: #7f8c8d;
                font-style: italic;
            }
            
            @media (max-width: 768px) {
                .header h1 {
                    font-size: 2em;
                }
                
                .stats-grid {
                    grid-template-columns: 1fr;
                    padding: 20px;
                }
                
                .table-container {
                    padding: 0 20px 20px 20px;
                }
                
                td, th {
                    padding: 12px 15px;
                }
            }
        </style>");
        
        html.AppendLine("</head>");
        html.AppendLine("<body>");
        html.AppendLine("<div class='container'>");
        
        // Header
        html.AppendLine("<div class='header'>");
        html.AppendLine("<h1>Employee Time Tracking Report</h1>");
        html.AppendLine($"<div class='subtitle'>Generated on {DateTime.Now:MMMM dd, yyyy 'at' HH:mm}</div>");
        html.AppendLine("</div>");
        
        // Stats Cards
        html.AppendLine("<div class='stats-grid'>");
        html.AppendLine($"<div class='stat-card'><div class='stat-number'>{totalEmployees}</div><div class='stat-label'>Total Employees</div></div>");
        html.AppendLine($"<div class='stat-card'><div class='stat-number'>{Math.Round(totalHours, 1)}</div><div class='stat-label'>Total Hours</div></div>");
        html.AppendLine($"<div class='stat-card'><div class='stat-number'>{Math.Round(avgHours, 1)}</div><div class='stat-label'>Average Hours</div></div>");
        
        if (sortedStats.Count > 0)
        {
            html.AppendLine($"<div class='stat-card'><div class='stat-number'>{Math.Round(sortedStats.First().Value.TotalHours, 1)}</div><div class='stat-label'>Top Performer</div></div>");
        }
        html.AppendLine("</div>");
        
        // Table Section
        html.AppendLine("<div class='table-container'>");
        
        // Search Box
        html.AppendLine("<div class='search-container'>");
        html.AppendLine("<input type='text' class='search-input' id='searchInput' placeholder='Search employees...'>");
        html.AppendLine("<span class='search-icon'>🔍</span>");
        html.AppendLine("</div>");
        
        // Table
        html.AppendLine("<div class='table-wrapper'>");
        html.AppendLine("<table id='employeeTable'>");
        html.AppendLine("<thead>");
        html.AppendLine("<tr>");
        html.AppendLine("<th onclick='sortTable(0)'>Rank</th>");
        html.AppendLine("<th onclick='sortTable(1)'>Employee Name</th>");
        html.AppendLine("<th onclick='sortTable(2)'>Total Hours</th>");
        html.AppendLine("<th onclick='sortTable(3)'>Time Entries</th>");
        html.AppendLine("<th onclick='sortTable(4)'>Working Days</th>");
        html.AppendLine("<th onclick='sortTable(5)'>Avg Hours/Day</th>");
        html.AppendLine("</tr>");
        html.AppendLine("</thead>");
        html.AppendLine("<tbody>");
        
        int rank = 1;
        foreach (var kvp in sortedStats)
        {
            string employeeName = kvp.Key;
            EmployeeStats stats = kvp.Value;
            double avgHoursPerDay = stats.WorkingDays > 0 ? stats.TotalHours / stats.WorkingDays : 0;
            
            string rowClass = "";
            if (stats.TotalHours < 100)
                rowClass = "low-hours";
            else if (rank <= 3)
                rowClass = "high-performer";
            
            string rankBadgeClass = rank <= 3 ? "rank-badge top3" : "rank-badge";
            
            html.AppendLine($"<tr class='{rowClass}'>");
            html.AppendLine($"<td><span class='{rankBadgeClass}'>{rank}</span></td>");
            html.AppendLine($"<td class='employee-name'>{employeeName}</td>");
            html.AppendLine($"<td class='hours-cell'>{Math.Round(stats.TotalHours, 2)}</td>");
            html.AppendLine($"<td>{stats.TotalEntries}</td>");
            html.AppendLine($"<td>{stats.WorkingDays}</td>");
            html.AppendLine($"<td>{Math.Round(avgHoursPerDay, 2)}</td>");
            html.AppendLine("</tr>");
            
            rank++;
        }
        
        html.AppendLine("</tbody>");
        html.AppendLine("</table>");
        html.AppendLine("</div>");
        html.AppendLine("</div>");
        
        // Footer
        html.AppendLine("<div class='footer'>");
        html.AppendLine("Employee Time Tracking System | Data processed and secured");
        html.AppendLine("</div>");
        
        html.AppendLine("</div>");
        
        // JavaScript for interactivity
        html.AppendLine(@"<script>
            // Search functionality
            document.getElementById('searchInput').addEventListener('keyup', function() {
                const searchTerm = this.value.toLowerCase();
                const table = document.getElementById('employeeTable');
                const rows = table.getElementsByTagName('tr');
                
                for (let i = 1; i < rows.length; i++) {
                    const employeeName = rows[i].getElementsByTagName('td')[1].textContent.toLowerCase();
                    if (employeeName.includes(searchTerm)) {
                        rows[i].style.display = '';
                    } else {
                        rows[i].style.display = 'none';
                    }
                }
            });
            
            // Sort functionality
            function sortTable(columnIndex) {
                const table = document.getElementById('employeeTable');
                const tbody = table.getElementsByTagName('tbody')[0];
                const rows = Array.from(tbody.getElementsByTagName('tr'));
                
                rows.sort((a, b) => {
                    let aValue = a.getElementsByTagName('td')[columnIndex].textContent;
                    let bValue = b.getElementsByTagName('td')[columnIndex].textContent;
                    
                    // Handle numeric columns
                    if (columnIndex >= 2) {
                        aValue = parseFloat(aValue) || 0;
                        bValue = parseFloat(bValue) || 0;
                        return bValue - aValue;
                    }
                    
                    // Handle text columns
                    return aValue.localeCompare(bValue);
                });
                
                // Re-append sorted rows
                rows.forEach(row => tbody.appendChild(row));
                
                // Update rank numbers
                rows.forEach((row, index) => {
                    const rankCell = row.getElementsByTagName('td')[0];
                    const rankBadge = rankCell.querySelector('.rank-badge');
                    if (rankBadge) {
                        rankBadge.textContent = index + 1;
                        rankBadge.className = (index < 3) ? 'rank-badge top3' : 'rank-badge';
                    }
                });
            }
            
            // Add loading animation on page load
            window.addEventListener('load', function() {
                document.body.style.opacity = '0';
                document.body.style.transition = 'opacity 0.5s ease-in-out';
                setTimeout(() => {
                    document.body.style.opacity = '1';
                }, 100);
            });
        </script>");
        
        html.AppendLine("</body>");
        html.AppendLine("</html>");
        
        return html.ToString();
    }
}

public class EmployeeStats
{
    public double TotalHours { get; set; } = 0;
    public int TotalEntries { get; set; } = 0;
    public HashSet<DateTime> UniqueDays { get; set; } = new HashSet<DateTime>();
    public int WorkingDays => UniqueDays.Count;
    
    public void UpdateWorkingDays(DateTime date)
    {
        UniqueDays.Add(date);
    }
}

public class TimeEntry
{
    public string Id { get; set; }
    public string EmployeeName { get; set; }
    public string StarTimeUtc { get; set; }
    public string EndTimeUtc { get; set; }
    public string EntryNotes { get; set; }
    public string DeletedOn { get; set; }
}