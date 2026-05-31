using System;
using System.Collections.Generic;
using System.Linq;
using HomeCareManager.Core.Configuration;
using MySqlConnector;
using HomeCareManager.Core.Models;

namespace HomeCareManager.Core.Data
{
    public class Data
    {
        private static readonly string DefaultConnectionString = DatabaseConfiguration.GetConnectionString();

        private readonly string connectionString;
        private const string PendingStatusId = "pending";
        private const string RejectedStatusId = "rejected";

        public Data(string? connectionString = null)
        {
            this.connectionString = string.IsNullOrWhiteSpace(connectionString)
                ? DefaultConnectionString
                : connectionString;
        }

        private bool ExecuteNonQuery(string query, params (string Name, object Value)[] parameters)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    using (MySqlCommand commandDatabase = new MySqlCommand(query, connection))
                    {
                        AddParameters(commandDatabase, parameters);

                        connection.Open();
                        return commandDatabase.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        private List<T> ExecuteReader<T>(
            string query,
            Func<MySqlDataReader, T> map,
            params (string Name, object Value)[] parameters)
        {
            List<T> rows = new List<T>();

            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    using (MySqlCommand commandDatabase = new MySqlCommand(query, connection))
                    {
                        AddParameters(commandDatabase, parameters);

                        connection.Open();
                        using (MySqlDataReader reader = commandDatabase.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                rows.Add(map(reader));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return rows;
        }

        public bool CanConnect()
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        private bool RecordExists(string tableName, string idColumn, string idValue)
        {
            try
            {
                ValidateSqlIdentifier(tableName);
                ValidateSqlIdentifier(idColumn);

                string query = $"SELECT COUNT(*) FROM `{tableName}` WHERE `{idColumn}` = @idValue;";

                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    using (MySqlCommand commandDatabase = new MySqlCommand(query, connection))
                    {
                        AddParameters(commandDatabase, ("@idValue", idValue));

                        connection.Open();

                        object? result = commandDatabase.ExecuteScalar();
                        return result != null && Convert.ToInt32(result) > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        private bool InsertIfNotExists(
            string tableName,
            string idColumn,
            string idValue,
            string query,
            params (string Name, object Value)[] parameters)
        {
            if (RecordExists(tableName, idColumn, idValue))
            {
                Console.WriteLine($"Insert skipped: a record already exists in {tableName} with {idColumn} = {idValue}");
                return false;
            }

            return ExecuteNonQuery(query, parameters);
        }

        private bool UpdateIfExists(
            string tableName,
            string idColumn,
            string idValue,
            string query,
            params (string Name, object Value)[] parameters)
        {
            if (!RecordExists(tableName, idColumn, idValue))
            {
                Console.WriteLine($"Update skipped: no record exists in {tableName} with {idColumn} = {idValue}");
                return false;
            }

            return ExecuteNonQuery(query, parameters);
        }

        private bool DeleteIfExists(
            string tableName,
            string idColumn,
            string idValue,
            string query,
            params (string Name, object Value)[] parameters)
        {
            if (!RecordExists(tableName, idColumn, idValue))
            {
                Console.WriteLine($"Delete skipped: no record exists in {tableName} with {idColumn} = {idValue}");
                return false;
            }

            return ExecuteNonQuery(query, parameters);
        }

        private static void AddParameters(MySqlCommand command, params (string Name, object Value)[] parameters)
        {
            foreach ((string name, object value) in parameters)
            {
                command.Parameters.AddWithValue(name, value);
            }
        }

        private static void ValidateSqlIdentifier(string identifier)
        {
            if (string.IsNullOrWhiteSpace(identifier))
            {
                throw new ArgumentException("SQL identifier cannot be empty.", nameof(identifier));
            }

            foreach (char character in identifier)
            {
                if (!char.IsLetterOrDigit(character) && character != '_')
                {
                    throw new ArgumentException($"Invalid SQL identifier: {identifier}", nameof(identifier));
                }
            }
        }

        public bool InsertRole(string roleId, string roleName)
        {
            string query = "INSERT INTO roles(RoleId, RoleName) VALUES(@roleId, @roleName);";

            return InsertIfNotExists(
                "roles",
                "RoleId",
                roleId,
                query,
                ("@roleId", roleId),
                ("@roleName", roleName));
        }

        public bool InsertSkill(string skillId, string name)
        {
            string query = "INSERT INTO skills(SkillId, Name) VALUES(@skillId, @name);";

            return InsertIfNotExists(
                "skills",
                "SkillId",
                skillId,
                query,
                ("@skillId", skillId),
                ("@name", name));
        }

        public User? GetUserByEmail(string email)
        {
            const string query =
                "SELECT UserId, Name, RoleId, Email, PasswordHash, PasswordChanged, IsActive, CreatedAt, SkillId " +
                "FROM users WHERE Email = @email LIMIT 1;";

            List<User> users = ExecuteReader(query, reader => new User
            {
                UserId = reader.GetString("UserId"),
                Name = reader.GetString("Name"),
                RoleId = reader.GetString("RoleId"),
                Email = reader.GetString("Email"),
                PasswordHash = reader.GetString("PasswordHash"),
                PasswordChanged = reader.GetBoolean("PasswordChanged"),
                IsActive = reader.GetBoolean("IsActive"),
                CreatedAt = reader.GetDateTime("CreatedAt"),
                SkillId = reader.GetString("SkillId")
            }, ("@email", email));

            return users.FirstOrDefault();
        }

        public User? GetUserById(string userId)
        {
            const string query =
                "SELECT UserId, Name, RoleId, Email, PasswordHash, PasswordChanged, IsActive, CreatedAt, SkillId " +
                "FROM users WHERE UserId = @userId LIMIT 1;";

            List<User> users = ExecuteReader(query, reader => new User
            {
                UserId = reader.GetString("UserId"),
                Name = reader.GetString("Name"),
                RoleId = reader.GetString("RoleId"),
                Email = reader.GetString("Email"),
                PasswordHash = reader.GetString("PasswordHash"),
                PasswordChanged = reader.GetBoolean("PasswordChanged"),
                IsActive = reader.GetBoolean("IsActive"),
                CreatedAt = reader.GetDateTime("CreatedAt"),
                SkillId = reader.GetString("SkillId")
            }, ("@userId", userId));

            return users.FirstOrDefault();
        }

        public bool InsertUser(User user)
        {
            string query = "INSERT INTO users(UserId, Name, RoleId, Email, PasswordHash, PasswordChanged, IsActive, CreatedAt, SkillId) " +
                "VALUES(@userId, @name, @roleId, @email, @passwordHash, @passwordChanged, @isActive, @createdAt, @skillId);";

            return InsertIfNotExists(
                "users",
                "UserId",
                user.UserId,
                query,
                ("@userId", user.UserId),
                ("@name", user.Name),
                ("@roleId", user.RoleId),
                ("@email", user.Email),
                ("@passwordHash", user.PasswordHash),
                ("@passwordChanged", user.PasswordChanged),
                ("@isActive", user.IsActive),
                ("@createdAt", user.CreatedAt),
                ("@skillId", user.SkillId));
        }

        public bool UpdateUser(User user)
        {
            string query = "UPDATE users " +
                "SET Name = @name, " +
                "RoleId = @roleId, " +
                "Email = @email, " +
                "PasswordHash = @passwordHash, " +
                "PasswordChanged = @passwordChanged, " +
                "IsActive = @isActive, " +
                "CreatedAt = @createdAt, " +
                "SkillId = @skillId " +
                "WHERE UserId = @userId;";

            return UpdateIfExists(
                "users",
                "UserId",
                user.UserId,
                query,
                ("@userId", user.UserId),
                ("@name", user.Name),
                ("@roleId", user.RoleId),
                ("@email", user.Email),
                ("@passwordHash", user.PasswordHash),
                ("@passwordChanged", user.PasswordChanged),
                ("@isActive", user.IsActive),
                ("@createdAt", user.CreatedAt),
                ("@skillId", user.SkillId));
        }

        public bool UpdateUserPassword(string userId, string passwordHash)
        {
            string query = "UPDATE users " +
                "SET PasswordHash = @passwordHash, " +
                "PasswordChanged = @passwordChanged " +
                "WHERE UserId = @userId;";

            return UpdateIfExists(
                "users",
                "UserId",
                userId,
                query,
                ("@userId", userId),
                ("@passwordHash", passwordHash),
                ("@passwordChanged", true));
        }

        public bool DeleteUser(string userId)
        {
            string query = "DELETE FROM users WHERE UserId = @userId;";

            return DeleteIfExists(
                "users",
                "UserId",
                userId,
                query,
                ("@userId", userId));
        }

        public bool InsertPatient(
            string patientId,
            string name,
            string address,
            string phone,
            string notes,
            string priority,
            string emergencyContact,
            string zone)
        {
            string query = "INSERT INTO patients(PatientId, Name, Address, Phone, Notes, Priority, EmergencyContact, Zone) " +
                "VALUES(@patientId, @name, @address, @phone, @notes, @priority, @emergencyContact, @zone);";

            return InsertIfNotExists(
                "patients",
                "PatientId",
                patientId,
                query,
                ("@patientId", patientId),
                ("@name", name),
                ("@address", address),
                ("@phone", phone),
                ("@notes", notes),
                ("@priority", priority),
                ("@emergencyContact", emergencyContact),
                ("@zone", zone));
        }

        public bool UpdatePatient(
            string patientId,
            string name,
            string address,
            string phone,
            string notes,
            string priority,
            string emergencyContact,
            string zone)
        {
            string query = "UPDATE patients " +
                "SET Name = @name, " +
                "Address = @address, " +
                "Phone = @phone, " +
                "Notes = @notes, " +
                "Priority = @priority, " +
                "EmergencyContact = @emergencyContact, " +
                "Zone = @zone " +
                "WHERE PatientId = @patientId;";

            return UpdateIfExists(
                "patients",
                "PatientId",
                patientId,
                query,
                ("@patientId", patientId),
                ("@name", name),
                ("@address", address),
                ("@phone", phone),
                ("@notes", notes),
                ("@priority", priority),
                ("@emergencyContact", emergencyContact),
                ("@zone", zone));
        }

        public bool DeletePatient(string patientId)
        {
            string query = "DELETE FROM patients WHERE PatientId = @patientId;";

            return DeleteIfExists(
                "patients",
                "PatientId",
                patientId,
                query,
                ("@patientId", patientId));
        }

        public bool InsertTask(
            string taskId,
            string requiredSkillId,
            string patientId,
            string description,
            DateTime date,
            string priority,
            string statusId)
        {
            string query = "INSERT INTO tasks(RequiredSkillId, TaskId, PatientId, Description, Date, Priority, StatusId) " +
                "VALUES(@requiredSkillId, @taskId, @patientId, @description, @date, @priority, @statusId);";

            return InsertIfNotExists(
                "tasks",
                "TaskId",
                taskId,
                query,
                ("@requiredSkillId", requiredSkillId),
                ("@taskId", taskId),
                ("@patientId", patientId),
                ("@description", description),
                ("@date", date),
                ("@priority", priority),
                ("@statusId", statusId));
        }

        public bool UpdateTask(
            string taskId,
            string requiredSkillId,
            string patientId,
            string description,
            DateTime date,
            string priority,
            string statusId)
        {
            string query = "UPDATE tasks " +
                "SET RequiredSkillId = @requiredSkillId, " +
                "PatientId = @patientId, " +
                "Description = @description, " +
                "Date = @date, " +
                "Priority = @priority, " +
                "StatusId = @statusId " +
                "WHERE TaskId = @taskId;";

            return UpdateIfExists(
                "tasks",
                "TaskId",
                taskId,
                query,
                ("@requiredSkillId", requiredSkillId),
                ("@taskId", taskId),
                ("@patientId", patientId),
                ("@description", description),
                ("@date", date),
                ("@priority", priority),
                ("@statusId", statusId));
        }

        public bool UpdateTaskItemStatus(string taskId, string statusId)
        {
            string query = "UPDATE tasks SET StatusId = @statusId WHERE TaskId = @taskId;";

            return UpdateIfExists(
                "tasks",
                "TaskId",
                taskId,
                query,
                ("@taskId", taskId),
                ("@statusId", statusId));
        }

        public bool InsertTaskAssignment(
            string assignmentId,
            string userId,
            string taskId,
            DateTime assignedDate,
            string statusId)
        {
            string query = "INSERT INTO task_assignments(UserId, TaskId, AssignmentId, AssignedDate, StatusId) " +
                "VALUES(@userId, @taskId, @assignmentId, @assignedDate, @statusId);";

            return InsertIfNotExists(
                "task_assignments",
                "AssignmentId",
                assignmentId,
                query,
                ("@userId", userId),
                ("@taskId", taskId),
                ("@assignmentId", assignmentId),
                ("@assignedDate", assignedDate),
                ("@statusId", statusId));
        }

        public bool UpdateTaskAssignmentStatus(string assignmentId, string statusId)
        {
            string query = "UPDATE task_assignments SET StatusId = @statusId WHERE AssignmentId = @assignmentId;";

            return UpdateIfExists(
                "task_assignments",
                "AssignmentId",
                assignmentId,
                query,
                ("@assignmentId", assignmentId),
                ("@statusId", statusId));
        }

        public bool RejectOpenTaskAssignmentsForTask(string taskId)
        {
            string query = "UPDATE task_assignments " +
                "SET StatusId = @rejectedStatusId " +
                "WHERE TaskId = @taskId " +
                "AND LOWER(StatusId) IN ('assigned', 'accepted', 'in-progress');";

            return ExecuteNonQuery(
                query,
                ("@taskId", taskId),
                ("@rejectedStatusId", RejectedStatusId));
        }

        public bool InsertIncident(
            string incidentId,
            string userId,
            string taskId,
            string description,
            DateTime createdAt,
            string status,
            string severity = "Medium",
            string resolutionNotes = "",
            DateTime? resolvedAt = null,
            string assignedToUserId = "",
            string reportId = "")
        {
            string query = "INSERT INTO incidents(UserId, IncidentId, TaskId, Description, CreatedAt, Status, Severity, ResolutionNotes, ResolvedAt, AssignedToUserId, ReportId) " +
                "VALUES(@userId, @incidentId, @taskId, @description, @createdAt, @status, @severity, @resolutionNotes, @resolvedAt, @assignedToUserId, @reportId);";

            return InsertIfNotExists(
                "incidents",
                "IncidentId",
                incidentId,
                query,
                ("@userId", userId),
                ("@incidentId", incidentId),
                ("@taskId", taskId),
                ("@description", description),
                ("@createdAt", createdAt),
                ("@status", status),
                ("@severity", severity),
                ("@resolutionNotes", resolutionNotes),
                ("@resolvedAt", resolvedAt.HasValue ? resolvedAt.Value : DBNull.Value),
                ("@assignedToUserId", string.IsNullOrWhiteSpace(assignedToUserId) ? DBNull.Value : assignedToUserId),
                ("@reportId", string.IsNullOrWhiteSpace(reportId) ? DBNull.Value : reportId));
        }

        public bool UpdateIncidentFollowUp(string incidentId, string status, string resolutionNotes)
        {
            DateTime? resolvedAt = status.Equals("Resolved", StringComparison.OrdinalIgnoreCase)
                || status.Equals("Closed", StringComparison.OrdinalIgnoreCase)
                    ? DateTime.Now
                    : null;

            string query = "UPDATE incidents " +
                "SET Status = @status, " +
                "ResolutionNotes = @resolutionNotes, " +
                "ResolvedAt = @resolvedAt " +
                "WHERE IncidentId = @incidentId;";

            return UpdateIfExists(
                "incidents",
                "IncidentId",
                incidentId,
                query,
                ("@incidentId", incidentId),
                ("@status", status),
                ("@resolutionNotes", resolutionNotes),
                ("@resolvedAt", resolvedAt.HasValue ? resolvedAt.Value : DBNull.Value));
        }

        public bool InsertReport(
            string reportId,
            string userId,
            string notes,
            DateTime createdAt,
            string statusBefore,
            string statusAfter,
            string duration,
            string taskId)
        {
            string query = "INSERT INTO reports(ReportId, UserId, Notes, CreatedAt, StatusBefore, StatusAfter, Duration, TaskId) " +
                "VALUES(@reportId, @userId, @notes, @createdAt, @statusBefore, @statusAfter, @duration, @taskId);";

            return InsertIfNotExists(
                "reports",
                "ReportId",
                reportId,
                query,
                ("@reportId", reportId),
                ("@userId", userId),
                ("@notes", notes),
                ("@createdAt", createdAt),
                ("@statusBefore", statusBefore),
                ("@statusAfter", statusAfter),
                ("@duration", duration),
                ("@taskId", taskId));
        }

        public ReportSummary? GetReportForIncident(string incidentId)
        {
            const string query = "SELECT r.ReportId, r.Notes, r.CreatedAt, r.StatusBefore, r.StatusAfter, r.Duration, " +
                "COALESCE(u.Name, r.UserId) AS CreatedBy " +
                "FROM incidents i " +
                "INNER JOIN reports r ON r.ReportId = i.ReportId OR (i.ReportId IS NULL AND r.TaskId = i.TaskId) " +
                "LEFT JOIN users u ON u.UserId = r.UserId " +
                "WHERE i.IncidentId = @incidentId " +
                "ORDER BY CASE WHEN r.ReportId = i.ReportId THEN 0 ELSE 1 END, r.CreatedAt DESC " +
                "LIMIT 1;";

            return ExecuteReader(query, reader => new ReportSummary
            {
                ReportId = reader.GetString("ReportId"),
                CreatedBy = reader.GetString("CreatedBy"),
                Notes = reader.GetString("Notes"),
                CreatedAt = reader.GetDateTime("CreatedAt"),
                StatusBefore = reader.GetString("StatusBefore"),
                StatusAfter = reader.GetString("StatusAfter"),
                Duration = reader.GetString("Duration")
            }, ("@incidentId", incidentId)).FirstOrDefault();
        }

        public bool InsertAvailability(
            string availabilityId,
            DateTime startTime,
            string zone,
            DateTime endTime,
            string userId)
        {
            string query = "INSERT INTO availability(AvailabilityId, StartTime, Zone, EndTime, UserId) " +
                "VALUES(@availabilityId, @startTime, @zone, @endTime, @userId);";

            return InsertIfNotExists(
                "availability",
                "AvailabilityId",
                availabilityId,
                query,
                ("@availabilityId", availabilityId),
                ("@startTime", startTime),
                ("@zone", zone),
                ("@endTime", endTime),
                ("@userId", userId));
        }

        public bool DeleteAvailability(string availabilityId)
        {
            string query = "DELETE FROM availability WHERE AvailabilityId = @availabilityId;";

            return DeleteIfExists(
                "availability",
                "AvailabilityId",
                availabilityId,
                query,
                ("@availabilityId", availabilityId));
        }

        public List<AvailabilitySummary> GetAvailabilityForUser(string userId)
        {
            const string query = "SELECT AvailabilityId, StartTime, EndTime, Zone " +
                "FROM availability " +
                "WHERE UserId = @userId " +
                "ORDER BY StartTime;";

            return ExecuteReader(query, reader => new AvailabilitySummary
            {
                AvailabilityId = reader.GetString("AvailabilityId"),
                StartTime = reader.GetDateTime("StartTime"),
                EndTime = reader.GetDateTime("EndTime"),
                Zone = reader.GetString("Zone")
            }, ("@userId", userId));
        }

        public List<Patient> GetPatients()
        {
            const string query = "SELECT PatientId, Name, Address, Phone, Notes, Priority, EmergencyContact, Zone " +
                "FROM patients ORDER BY Name;";

            return ExecuteReader(query, reader => new Patient
            {
                PatientId = reader.GetString("PatientId"),
                Name = reader.GetString("Name"),
                Address = reader.GetString("Address"),
                Phone = reader.GetString("Phone"),
                Notes = reader.GetString("Notes"),
                Priority = reader.GetString("Priority"),
                EmergencyContact = reader.GetString("EmergencyContact"),
                Zone = reader.GetString("Zone")
            });
        }

        public List<UserSummary> GetUserSummaries()
        {
            const string query = "SELECT u.UserId, u.Name, u.Email, u.IsActive, COALESCE(r.RoleName, u.RoleId) AS RoleName " +
                "FROM users u " +
                "LEFT JOIN roles r ON r.RoleId = u.RoleId " +
                "ORDER BY u.Name;";

            return ExecuteReader(query, reader => new UserSummary
            {
                UserId = reader.GetString("UserId"),
                Name = reader.GetString("Name"),
                Email = reader.GetString("Email"),
                RoleName = reader.GetString("RoleName"),
                IsActive = reader.GetBoolean("IsActive")
            });
        }

        public List<UserSummary> GetEligibleWorkerSummaries(string requiredSkillId, DateTime taskDate, string patientZone)
        {
            const string query = "SELECT DISTINCT u.UserId, u.Name, u.Email, u.IsActive, COALESCE(r.RoleName, u.RoleId) AS RoleName " +
                "FROM users u " +
                "LEFT JOIN roles r ON r.RoleId = u.RoleId " +
                "INNER JOIN availability av ON av.UserId = u.UserId " +
                "WHERE u.IsActive = 1 " +
                "AND LOWER(u.RoleId) IN ('doctor', 'assistant') " +
                "AND u.SkillId = @requiredSkillId " +
                "AND @taskDate BETWEEN av.StartTime AND av.EndTime " +
                "AND (LOWER(av.Zone) = LOWER(@patientZone) OR @patientZone = '' OR av.Zone = '') " +
                "ORDER BY u.Name;";

            return ExecuteReader(query, reader => new UserSummary
            {
                UserId = reader.GetString("UserId"),
                Name = reader.GetString("Name"),
                Email = reader.GetString("Email"),
                RoleName = reader.GetString("RoleName"),
                IsActive = reader.GetBoolean("IsActive")
            }, ("@requiredSkillId", requiredSkillId), ("@taskDate", taskDate), ("@patientZone", patientZone));
        }

        public List<TaskSummary> GetTaskSummaries()
        {
            const string query = "SELECT t.TaskId, t.RequiredSkillId, t.PatientId, t.Description, t.Date, t.Priority, t.StatusId, " +
                "COALESCE(p.Name, t.PatientId) AS PatientName, " +
                "COALESCE(p.Zone, '') AS PatientZone, " +
                "COALESCE(ts.Name, t.StatusId) AS StatusName, " +
                "COALESCE(a.AssignmentCount, 0) AS AssignmentCount, " +
                "COALESCE(a.AssignedTo, '') AS AssignedTo, " +
                "'' AS CurrentUserAssignmentId, " +
                "'' AS CurrentUserAssignmentStatusId " +
                "FROM tasks t " +
                "LEFT JOIN patients p ON p.PatientId = t.PatientId " +
                "LEFT JOIN task_status ts ON ts.StatusId = t.StatusId " +
                "LEFT JOIN (" +
                "SELECT ta.TaskId, " +
                "COUNT(CASE WHEN LOWER(ta.StatusId) NOT IN ('rejected', 'cancelled') THEN 1 END) AS AssignmentCount, " +
                "GROUP_CONCAT(DISTINCT CASE WHEN LOWER(ta.StatusId) NOT IN ('rejected', 'cancelled') THEN u.Name END ORDER BY u.Name SEPARATOR ', ') AS AssignedTo " +
                "FROM task_assignments ta " +
                "LEFT JOIN users u ON u.UserId = ta.UserId " +
                "GROUP BY ta.TaskId" +
                ") a ON a.TaskId = t.TaskId " +
                "ORDER BY t.Date DESC;";

            return ExecuteReader(query, MapTaskSummary);
        }

        public List<TaskSummary> GetTaskSummariesForWorker(string userId, string skillId)
        {
            const string query = "SELECT t.TaskId, t.RequiredSkillId, t.PatientId, t.Description, t.Date, t.Priority, t.StatusId, " +
                "COALESCE(p.Name, t.PatientId) AS PatientName, " +
                "COALESCE(p.Zone, '') AS PatientZone, " +
                "COALESCE(ts.Name, t.StatusId) AS StatusName, " +
                "COALESCE(a.AssignmentCount, 0) AS AssignmentCount, " +
                "COALESCE(a.AssignedTo, '') AS AssignedTo, " +
                "COALESCE(currentAssignment.AssignmentId, '') AS CurrentUserAssignmentId, " +
                "COALESCE(currentAssignment.StatusId, '') AS CurrentUserAssignmentStatusId " +
                "FROM tasks t " +
                "LEFT JOIN patients p ON p.PatientId = t.PatientId " +
                "LEFT JOIN task_status ts ON ts.StatusId = t.StatusId " +
                "LEFT JOIN (" +
                "SELECT ta.TaskId, " +
                "COUNT(CASE WHEN LOWER(ta.StatusId) NOT IN ('rejected', 'cancelled') THEN 1 END) AS AssignmentCount, " +
                "GROUP_CONCAT(DISTINCT CASE WHEN LOWER(ta.StatusId) NOT IN ('rejected', 'cancelled') THEN u.Name END ORDER BY u.Name SEPARATOR ', ') AS AssignedTo " +
                "FROM task_assignments ta " +
                "LEFT JOIN users u ON u.UserId = ta.UserId " +
                "GROUP BY ta.TaskId" +
                ") a ON a.TaskId = t.TaskId " +
                "LEFT JOIN (" +
                "SELECT ta.TaskId, ta.AssignmentId, ta.StatusId " +
                "FROM task_assignments ta " +
                "INNER JOIN (" +
                "SELECT TaskId, MAX(AssignedDate) AS AssignedDate " +
                "FROM task_assignments " +
                "WHERE UserId = @userId " +
                "GROUP BY TaskId" +
                ") latest ON latest.TaskId = ta.TaskId AND latest.AssignedDate = ta.AssignedDate " +
                "WHERE ta.UserId = @userId" +
                ") currentAssignment ON currentAssignment.TaskId = t.TaskId " +
                "WHERE (" +
                "EXISTS (" +
                "SELECT 1 FROM task_assignments taMine " +
                "WHERE taMine.TaskId = t.TaskId " +
                "AND taMine.UserId = @userId " +
                "AND LOWER(taMine.StatusId) IN ('assigned', 'accepted', 'in-progress', 'completed')" +
                ") OR (" +
                "LOWER(t.StatusId) = @pendingStatusId " +
                "AND t.RequiredSkillId = @skillId " +
                "AND EXISTS (" +
                "SELECT 1 FROM availability av " +
                "WHERE av.UserId = @userId " +
                "AND t.Date BETWEEN av.StartTime AND av.EndTime " +
                "AND (LOWER(av.Zone) = LOWER(COALESCE(p.Zone, '')) OR COALESCE(p.Zone, '') = '' OR av.Zone = '')" +
                ") " +
                "AND NOT EXISTS (" +
                "SELECT 1 FROM task_assignments taActive " +
                "WHERE taActive.TaskId = t.TaskId " +
                "AND LOWER(taActive.StatusId) IN ('assigned', 'accepted', 'in-progress')" +
                ") " +
                "AND NOT EXISTS (" +
                "SELECT 1 FROM task_assignments taRejected " +
                "WHERE taRejected.TaskId = t.TaskId " +
                "AND taRejected.UserId = @userId " +
                "AND LOWER(taRejected.StatusId) = @rejectedStatusId" +
                ")" +
                ")" +
                ") " +
                "ORDER BY t.Date DESC;";

            return ExecuteReader(
                query,
                MapTaskSummary,
                ("@userId", userId),
                ("@skillId", skillId),
                ("@pendingStatusId", PendingStatusId),
                ("@rejectedStatusId", RejectedStatusId));
        }

        private static TaskSummary MapTaskSummary(MySqlDataReader reader)
        {
            return new TaskSummary
            {
                TaskId = reader.GetString("TaskId"),
                RequiredSkillId = reader.GetString("RequiredSkillId"),
                PatientId = reader.GetString("PatientId"),
                PatientName = reader.GetString("PatientName"),
                PatientZone = reader.GetString("PatientZone"),
                Description = reader.GetString("Description"),
                Date = reader.GetDateTime("Date"),
                Priority = reader.GetString("Priority"),
                StatusId = reader.GetString("StatusId"),
                StatusName = reader.GetString("StatusName"),
                AssignmentCount = Convert.ToInt32(reader["AssignmentCount"]),
                AssignedTo = reader.GetString("AssignedTo"),
                CurrentUserAssignmentId = reader.GetString("CurrentUserAssignmentId"),
                CurrentUserAssignmentStatusId = reader.GetString("CurrentUserAssignmentStatusId")
            };
        }

        public List<IncidentSummary> GetIncidentSummaries()
        {
            const string query = "SELECT i.IncidentId, i.Description, i.Status, i.Severity, i.CreatedAt, COALESCE(i.ResolutionNotes, '') AS ResolutionNotes, COALESCE(i.ReportId, '') AS ReportId, i.ResolvedAt, " +
                "COALESCE(p.Name, '') AS PatientName, " +
                "COALESCE(t.Description, i.TaskId) AS TaskDescription, " +
                "COALESCE(u.Name, i.UserId) AS CreatedBy " +
                "FROM incidents i " +
                "LEFT JOIN tasks t ON t.TaskId = i.TaskId " +
                "LEFT JOIN patients p ON p.PatientId = t.PatientId " +
                "LEFT JOIN users u ON u.UserId = i.UserId " +
                "ORDER BY i.CreatedAt DESC;";

            return ExecuteReader(query, reader => new IncidentSummary
            {
                IncidentId = reader.GetString("IncidentId"),
                PatientName = reader.GetString("PatientName"),
                TaskDescription = reader.GetString("TaskDescription"),
                Description = reader.GetString("Description"),
                Severity = reader.GetString("Severity"),
                CreatedBy = reader.GetString("CreatedBy"),
                ResolutionNotes = reader.GetString("ResolutionNotes"),
                ReportId = reader.GetString("ReportId"),
                Status = reader.GetString("Status"),
                CreatedAt = reader.GetDateTime("CreatedAt"),
                ResolvedAt = reader.IsDBNull(reader.GetOrdinal("ResolvedAt"))
                    ? null
                    : reader.GetDateTime("ResolvedAt")
            });
        }

        public List<Skill> GetSkills()
        {
            const string query = "SELECT SkillId, Name FROM skills ORDER BY Name;";

            return ExecuteReader(query, reader => new Skill
            {
                SkillId = reader.GetString("SkillId"),
                Name = reader.GetString("Name")
            });
        }

        public List<HomeCareManager.Core.Models.TaskStatus> GetTaskStatuses()
        {
            const string query = "SELECT StatusId, Name FROM task_status ORDER BY Name;";

            return ExecuteReader(query, reader => new HomeCareManager.Core.Models.TaskStatus
            {
                StatusId = reader.GetString("StatusId"),
                Name = reader.GetString("Name")
            });
        }

    }
}
