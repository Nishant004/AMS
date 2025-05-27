using Dapper;
using System.Data;
using System.Text;
using System.Reflection;
using AMS.Models;
using AMS.Models.ViewModel;
using System.ComponentModel.DataAnnotations;
using NuGet.Protocol.Core.Types;

namespace AMS.Data

{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly DapperContext _context;
        private readonly string _tableName;

        public GenericRepository(DapperContext context)
        {
            _context = context;
            _tableName = typeof(T).Name; // Assumes table name = class name
        }


        public async Task<IEnumerable<T>> GetAllAsync()
        {
            
            string query;

            if (typeof(T).GetProperty("IsDelete") != null)
            {
                // Table has 'IsDelete' property => apply filter
                query = $"SELECT * FROM [{_tableName}] WHERE IsDelete = 0";
            }
            else
            {
                // Table has no 'IsDelete' => simple select


                query = $"SELECT * FROM [{_tableName}] ";


                //query = $"SELECT * FROM [{_tableName}]";
            }

            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<T>(query);



        }






        public async Task<T> GetByIdAsync(string idColumn, int id)
        {
            var query = $"SELECT * FROM [{_tableName}] WHERE {idColumn} = @Id";
            using var connection = _context.CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<T>(query, new { Id = id });
        }


        public async Task<int> InsertAsync(T entity)
        {
            //var props = typeof(T).GetProperties()
            //    .Where(p => !(p.Name.ToLower().EndsWith("id") && p.PropertyType == typeof(int))) // Exclude identity column
            //    .ToList();

            //var props = typeof(T).GetProperties()
            //.Where(p => !string.Equals(p.Name, "UserId", StringComparison.OrdinalIgnoreCase)) // or "UserId"
            //.ToList();


            //var props = typeof(T).GetProperties()
            //.Where(p => !Attribute.IsDefined(p, typeof(KeyAttribute)))
            //.ToList();


            //var identityProps = new[] { "UserId", "QuotaID", "SomeOtherId" }; // update as needed

            //return typeof(T).GetProperties()
            //    .Where(p => !identityProps.Contains(p.Name, StringComparer.OrdinalIgnoreCase))
            //    .ToList();

            var identityProp = typeof(T).GetProperties()
               .FirstOrDefault(p =>
                   (p.Name.EndsWith("Id", StringComparison.OrdinalIgnoreCase) &&
                    p.PropertyType == typeof(int)));

            // Exclude the identity property from insert
            var props = typeof(T).GetProperties()
                .Where(p => identityProp == null || p.Name != identityProp.Name)
                .ToList();



            var columnNames = string.Join(", ", props.Select(p => p.Name));
            var paramNames = string.Join(", ", props.Select(p => "@" + p.Name));

            var query = $"INSERT INTO [{_tableName}] ({columnNames}) VALUES ({paramNames}); SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using var connection = _context.CreateConnection();
            var insertedId = await connection.ExecuteScalarAsync<int>(query, entity);

            return insertedId;
        }







        public async Task<int> UpdateAsync(string idColumn, T entity)
        {
            var props = typeof(T).GetProperties().Where(p => p.Name.ToLower() != idColumn.ToLower()).ToList();
            var setClause = string.Join(", ", props.Select(p => $"{p.Name} = @{p.Name}"));
            var query = $"UPDATE [{_tableName}] SET {setClause} WHERE {idColumn} = @{idColumn}";

            using var connection = _context.CreateConnection();
            return await connection.ExecuteAsync(query, entity);
        }


     

        public async Task<int> DeleteAsync(string idColumn, int id)
        {
            var query = $"UPDATE [{_tableName}] SET IsDelete = 1 WHERE {idColumn} = @Id";
            using var connection = _context.CreateConnection();
            return await connection.ExecuteAsync(query, new { Id = id });
        }
        //show hidden deactivate employee
        public async Task<IEnumerable<T>> GetAllDeletedAsync()
        {
            var query = $"SELECT * FROM [{_tableName}] WHERE IsDelete = 1";
            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<T>(query);
        }


        public async Task<int> DeleteAsyncPermanent(string idColumn, int id)
        {
            var query = $"DELETE FROM [{_tableName}] WHERE {idColumn} = @Id";
            using var connection = _context.CreateConnection();
            return await connection.ExecuteAsync(query, new { Id = id });
        }




        // ✅ method: Login query using username and password columns
        public async Task<T> GetByCredentialsAsync(string usernameColumn, string passwordColumn, string username, string password)
        {
            var query = $"SELECT * FROM {_tableName} WHERE {usernameColumn} = @Username AND {passwordColumn} = @Password";
            using var connection = _context.CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<T>(query, new { Username = username, Password = password });
        }

  


        public async Task<(T? user, bool isDeactivated)> GetByUserCredentialsAsync<T>(
                string usernameColumn, string passwordColumn, string roleColumn,
                string username, string password, string role) where T : class
        {
            using var connection = _context.CreateConnection();

            // Single query to get both user and status
            var query = $@"
            SELECT U.*, E.Status
            FROM [{_tableName}] U
            JOIN Employees E ON U.EmployeeID = E.EmployeeID
            WHERE U.{usernameColumn} = @Username
              AND U.{passwordColumn} = @Password
              AND U.{roleColumn} = @Role";

            // Use QueryAsync to get both user data and status in one trip to the database
            var result = await connection.QueryAsync<T, string, (T user, string status)>(
                query,
                (user, status) => (user, status),
                new
                {
                    Username = username,
                    Password = password,
                    Role = role
                },
                splitOn: "Status"
            );

            var userResult = result.FirstOrDefault();

            // No matching user found
            if (userResult.user == null)
            {
                return (null, false);
            }

            // Check if user is inactive
            bool isDeactivated = !string.IsNullOrEmpty(userResult.status) &&
                                  userResult.status.Trim().Equals("Inactive", StringComparison.OrdinalIgnoreCase);

            // Return null if deactivated, otherwise return the user
            return isDeactivated ? (null, true) : (userResult.user, false);
        }





        public async Task<IEnumerable<dynamic>> GetAttendanceByMonthYearAsync(int employee, int month, int year)
        {
            using var connection = _context.CreateConnection();

            string query = @"
                -- Employee Attendance
                SELECT 
                    A.AttendanceDate,
                    A.CheckInTime,
                    A.CheckOutTime,
                    A.Status,
                    A.Remarks,
                    A.EmployeeID,
                    'Attendance' AS EntryType
                FROM Attendance A
                INNER JOIN Employees E ON A.EmployeeID = E.EmployeeID
                WHERE MONTH(A.AttendanceDate) = @Month
                  AND YEAR(A.AttendanceDate) = @Year
                  AND E.IsDelete = 0" + (employee != 0 ? " AND A.EmployeeID = @EmployeeID" : "") + @"

                UNION ALL

                -- Holidays for employees
                SELECT 
                    H.HolidayDate AS AttendanceDate,
                    NULL AS CheckInTime,
                    NULL AS CheckOutTime,
                    H.Description AS Status, -- 🛠️ fixed alias
                    H.HolidayName AS Remarks,
                    E.EmployeeID,
                    'Holiday' AS EntryType
                FROM Holidays H
                CROSS JOIN Employees E
                WHERE MONTH(H.HolidayDate) = @Month
                  AND YEAR(H.HolidayDate) = @Year
                  AND E.IsDelete = 0" + (employee != 0 ? " AND E.EmployeeID = @EmployeeID" : "");

            return await connection.QueryAsync(query, new
            {
                EmployeeID = employee,
                Month = month,
                Year = year
            });


        }





        // Get Attendance By Id or Date // unnecessary, you have used GetByIdAsync instead
        public async Task<IEnumerable<T>> GetAttendanceByIdAsync(string idColumn, object value)
        {
            var query = $"SELECT * FROM {_tableName} WHERE {idColumn} = @Value";
            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<T>(query, new { Value = value });
        }




        public async Task<Attendance?> GetAttendanceByEmployeeDateAsync(int employeeId, DateTime date)
        {
            using var connection = _context.CreateConnection();
            await connection.OpenAsync(); // Ensure the connection is open

            //var query = "SELECT * FROM Attendance WHERE EmployeeID = @EmployeeID AND AttendanceDate = @AttendanceDate";
            var query = "SELECT * FROM Attendance WHERE EmployeeID = @EmployeeID AND CONVERT(DATE, AttendanceDate) = @AttendanceDate";


        

            //return await connection.QuerySingleOrDefaultAsync<Attendance>(query, new { EmployeeID = employeeId, AttendanceDate = date.Date });
            return await connection.QueryFirstOrDefaultAsync<Attendance>(query, new { EmployeeID = employeeId, AttendanceDate = date.Date });

        }

        public async Task UpdateAttendanceAsync(Attendance attendance)
        {
       


            using var connection = _context.CreateConnection();
            await connection.OpenAsync(); // 👈 Ensure the connection is open



            var query = @"
            UPDATE Attendance 
            SET 
                CheckOutTime = @CheckOutTime,
                CheckOutLat = @CheckOutLat,
                CheckOutLong = @CheckOutLong,
                CheckoutIP=@CheckoutIP,
                FollowUpShift=@followUpShift

            WHERE EmployeeID = @EmployeeID AND AttendanceDate = @AttendanceDate";

            //await connection.ExecuteAsync(query, new { attendance.CheckOutTime, attendance.EmployeeId, attendance.AttendanceDate });

            await connection.ExecuteAsync(query, new
            {
                attendance.CheckOutTime,
                attendance.CheckOutLat,
                attendance.CheckOutLong,
                attendance.EmployeeID,
                attendance.AttendanceDate,
                attendance.CheckoutIP,
                attendance.FollowUpShift

            });
        }



        public async Task LogCheckOutAsync(
                      int attendanceId,
                      TimeSpan checkInTime,
                      TimeSpan checkOutTime,
                      double? checkInLat,
                      double? checkInLong,
                      double? checkOutLat,
                      double? checkOutLong)
        {
            using var connection = _context.CreateConnection();
            await connection.OpenAsync();

            var query = @"
                        INSERT INTO AttendanceLogs 
                        (AttendanceID, CheckInTime, CheckOutTime, CheckInLat, CheckInLong, CheckOutLat, CheckOutLong, LogDateTime)
                        VALUES 
                        (@AttendanceID, @CheckInTime, @CheckOutTime, @CheckInLat, @CheckInLong, @CheckOutLat, @CheckOutLong, GETDATE())";

            await connection.ExecuteAsync(query, new
            {
                AttendanceID = attendanceId,
                CheckInTime = checkInTime,
                CheckOutTime = checkOutTime,
                CheckInLat = checkInLat,
                CheckInLong = checkInLong,
                CheckOutLat = checkOutLat,
                CheckOutLong = checkOutLong
            });
        }




        //DATA FETCH FROM USER



        public async Task<EmployeeAttendanceDto?> GetEmployeeAttendanceByDateAsync(int employeeId)
        {
            using var connection = _context.CreateConnection();

            string query = @"
            SELECT 
                e.FirstName, 
                e.LastName, 
                e.Department, 
                e.Designation,
                COALESCE(CONVERT(VARCHAR, a.CheckInTime, 108), 'Not Available') AS CheckInTime, 
                COALESCE(CONVERT(VARCHAR, a.CheckOutTime, 108), 'Not Available') AS CheckOutTime, 
                COALESCE(a.Status, 'Not Available') AS Status, 
                a.CheckInLat,
                a.CheckInLong,
                a.CheckOutLat,
                a.CheckOutLong,
                COALESCE(a.FollowUpShift, 'No') AS FollowUpShift
            FROM Employees e
            LEFT JOIN Attendance a 
                ON e.EmployeeId = a.EmployeeId 
                AND CAST(a.AttendanceDate AS DATE) = CAST(GETDATE() AS DATE)
            WHERE e.EmployeeId = @EmployeeId;";

            return await connection.QueryFirstOrDefaultAsync<EmployeeAttendanceDto>(query, new { EmployeeId = employeeId });
        }










        public async Task<bool> CheckInAsync(int employeeId, string ip, double? checkInLat, double? checkInLong, string followUpShift)
        {
            var sql = @"
                    IF EXISTS (SELECT 1 FROM Attendance WHERE EmployeeId = @EmployeeId AND AttendanceDate = CAST(GETDATE() AS DATE))
                    BEGIN
                        UPDATE Attendance 
                        SET CheckInTime = @CheckInTime, 
                            Status = 'Present',  
                            CheckinIP = @CheckinIP,
                            CheckInLat = @CheckInLat,
                            CheckInLong = @CheckInLong,
                            FollowUpShift = @FollowUpShift
                        WHERE EmployeeId = @EmployeeId AND AttendanceDate = CAST(GETDATE() AS DATE);
                    END
                    ELSE
                    BEGIN
                        INSERT INTO Attendance 
                            (EmployeeId, AttendanceDate, CheckInTime, Status, CheckinIP, CheckInLat, CheckInLong, FollowUpShift)
                        VALUES 
                            (@EmployeeId, GETDATE(), @CheckInTime, 'Present', @CheckinIP, @CheckInLat, @CheckInLong, @FollowUpShift);
                    END";

            var parameters = new
            {
                EmployeeId = employeeId,
                CheckInTime = DateTime.Now.TimeOfDay,
                CheckinIP = ip,
                CheckInLat = checkInLat,
                CheckInLong = checkInLong,
                FollowUpShift = followUpShift ?? "No" // Default to "No" if not provided
            };

            using var connection = _context.CreateConnection();
            var result = await connection.ExecuteAsync(sql, parameters);
            return result > 0;
        }






        public async Task<IEnumerable<AttendanceLogDto>> GetAttendanceLogsAsync(int employeeId, int year, int month, int day)
        {
            using var connection = _context.CreateConnection();

            DateTime targetDate;
            try
            {
                targetDate = new DateTime(year, month, day);
            }
            catch (ArgumentOutOfRangeException)
            {
                return Enumerable.Empty<AttendanceLogDto>();
            }


            string query = @"
            SELECT 
                COALESCE(CONVERT(VARCHAR, al.LogDateTime, 120), 'Not Available') AS LogDateTime,
                COALESCE(CONVERT(VARCHAR, al.CheckInTime, 108), 'Not Available') AS CheckInTime, 
                COALESCE(CONVERT(VARCHAR, al.CheckOutTime, 108), 'Not Available') AS CheckOutTime
            FROM AttendanceLogs al
            INNER JOIN Attendance a ON a.AttendanceID = al.AttendanceID
            WHERE a.EmployeeID = @EmployeeID AND CAST(al.LogDateTime AS DATE) = @TargetDate
            ORDER BY al.LogDateTime DESC;";


            return await connection.QueryAsync<AttendanceLogDto>(query, new
            {
                EmployeeID = employeeId,
                TargetDate = targetDate.Date
            });
        }


        public async Task<IEnumerable<EmpAttendanceDto>> GetAttendanceByMonthYearAsyncById(int employeeId, int month, int year)
        {
            

            using var connection = _context.CreateConnection();

            //    string query = @"
            //SELECT 
            //    e.EmployeeId,
            //    (e.FirstName + ' ' + e.LastName) AS EmployeeName,
            //    a.AttendanceDate,
            //    a.Status
            //FROM Attendance a
            //INNER JOIN Employees e ON a.EmployeeId = e.EmployeeId
            //WHERE MONTH(a.AttendanceDate) = @Month
            //  AND YEAR(a.AttendanceDate) = @Year
            //  AND a.EmployeeId = @EmployeeId
            //ORDER BY a.AttendanceDate";

            string query = @"
                        -- Employee Attendance
                        SELECT 
                            A.AttendanceDate,
                            A.CheckInTime,
                            A.CheckOutTime,
                            A.Status,
                            A.Remarks,
                            A.EmployeeID,
                            (E.FirstName + ' ' + E.LastName) AS EmployeeName,
                            'Attendance' AS EntryType
                        FROM Attendance A
                        INNER JOIN Employees E ON A.EmployeeID = E.EmployeeID
                        WHERE MONTH(A.AttendanceDate) = @Month
                          AND YEAR(A.AttendanceDate) = @Year
                          AND E.IsDelete = 0
                          AND A.EmployeeID = @EmployeeID

                        UNION ALL

                        -- Holidays for employees
                        SELECT 
                            H.HolidayDate AS AttendanceDate,
                            NULL AS CheckInTime,
                            NULL AS CheckOutTime,
                            H.Description AS Status,
                            H.HolidayName AS Remarks,
                            E.EmployeeID,
                            (E.FirstName + ' ' + E.LastName) AS EmployeeName,
                            'Holiday' AS EntryType
                        FROM Holidays H
                        CROSS JOIN Employees E
                        WHERE MONTH(H.HolidayDate) = @Month
                          AND YEAR(H.HolidayDate) = @Year
                          AND E.IsDelete = 0
                          AND E.EmployeeID = @EmployeeID

                        ORDER BY AttendanceDate;
                    ";

            var result = await connection.QueryAsync<EmpAttendanceDto>(query, new
            {
                EmployeeId = employeeId,
                Month = month,
                Year = year
            });

            return result;
        }



        public async Task<T> GetFirstAsync(string orderByColumn, Dictionary<string, object> filters = null)
        {
            using (var connection = _context.CreateConnection())
            {
                var (whereClause, parameters) = BuildDynamicFilter(filters ?? new());
                string query = $"SELECT * FROM {_tableName} {whereClause} ORDER BY {orderByColumn} ASC LIMIT 1";
                return await connection.QueryFirstOrDefaultAsync<T>(query, parameters);
            }
        }

        private (string, DynamicParameters) BuildDynamicFilter(Dictionary<string, object> filters)
        {
            var parameters = new DynamicParameters();
            var conditions = new List<string>();

            foreach (var filter in filters)
            {
                if (filter.Value == null)
                {
                    conditions.Add($"{filter.Key} IS NULL");
                }
                else
                {
                    conditions.Add($"{filter.Key} = @{filter.Key}");
                    parameters.Add($"@{filter.Key}", filter.Value);
                }
            }

            string whereClause = conditions.Any() ? "WHERE " + string.Join(" AND ", conditions) : "";
            return (whereClause, parameters);
        }





        public async Task<T?> GetByUsernameAsync(string username)
        {


            var query = $"SELECT * FROM [{_tableName}] WHERE Username = @Username";
            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<T>(query, new { Username = username });
        }




        public async Task<IEnumerable<T>> GetLeavesByEmployeeAndMonthAsync(int employeeId, int year)
        {
            string query = $@"
        SELECT * FROM [{_tableName}]
        WHERE EmployeeId = @EmployeeId
        AND YEAR(StartDate) = @Year";

            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<T>(query, new { EmployeeId = employeeId, Year = year });
        }






        public async Task<int> CreateLeaveRequestAsync(T entity)

        {


            string query = $@"
        INSERT INTO [{_tableName}] (EmployeeId, LeaveType, StartDate, EndDate, Reason, Status, RequestedAt)
        VALUES (@EmployeeId, @LeaveType, @StartDate, @EndDate, @Reason, @Status, @RequestedAt);
        SELECT CAST(SCOPE_IDENTITY() as int);";

            using var connection = _context.CreateConnection();
            var leaveId = await connection.ExecuteScalarAsync<int>(query, entity);
            return leaveId;



        }
        


        public async Task<T?> GetByEmployeeAndYearAsync(int employeeId, int year)
        {
            var query = $@"SELECT * FROM [{_tableName}] WHERE EmployeeID = @EmployeeID AND Year = @Year";
            using var connection = _context.CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<T>(query, new { EmployeeID = employeeId, Year = year });
        }



        public async Task<bool> ExistsAsync(DateTime date)
        {
            var sql = $"SELECT COUNT(1) FROM [{_tableName}] WHERE HolidayDate = @Date";
            using var conn = _context.CreateConnection();
            return await conn.ExecuteScalarAsync<bool>(sql, new { Date = date.Date });
        }








    }




}







