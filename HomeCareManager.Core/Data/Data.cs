using System;
using System.Collections.Generic;
using System.Linq;
using MySqlConnector;
using HomeCareManager.Core.Models;
using System.Security.AccessControl;

namespace HomeCareManager.Core.Data
{
    public class Data
    {
        private const string DefaultConnectionString =
            "datasource = 127.0.0.1;" +
            "port = 3306;" +
            "username = root; password = ;" +
            "database = homecaremanager";

        private readonly string connectionString;

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

        private int ExecuteCount(string query, params (string Name, object Value)[] parameters)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    using (MySqlCommand commandDatabase = new MySqlCommand(query, connection))
                    {
                        AddParameters(commandDatabase, parameters);

                        connection.Open();
                        object? result = commandDatabase.ExecuteScalar();
                        return result == null ? 0 : Convert.ToInt32(result);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 0;
            }
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

        public bool UpdateRole(string roleId, string roleName)
        {
            string query = "UPDATE roles SET RoleName = @roleName WHERE RoleId = @roleId;";

            return UpdateIfExists(
                "roles",
                "RoleId",
                roleId,
                query,
                ("@roleId", roleId),
                ("@roleName", roleName));
        }

        public bool DeleteRole(string roleId)
        {
            string query = "DELETE FROM roles WHERE RoleId = @roleId;";

            return DeleteIfExists(
                "roles",
                "RoleId",
                roleId,
                query,
                ("@roleId", roleId));
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

        public bool UpdateSkill(string skillId, string name)
        {
            string query = "UPDATE skills SET Name = @name WHERE SkillId = @skillId;";

            return UpdateIfExists(
                "skills",
                "SkillId",
                skillId,
                query,
                ("@skillId", skillId),
                ("@name", name));
        }

        public bool DeleteSkill(string skillId)
        {
            string query = "DELETE FROM skills WHERE SkillId = @skillId;";

            return DeleteIfExists(
                "skills",
                "SkillId",
                skillId,
                query,
                ("@skillId", skillId));
        }

        public bool InsertTaskStatus(string statusId, string name)
        {
            string query = "INSERT INTO task_status(StatusId, Name) VALUES(@statusId, @name);";

            return InsertIfNotExists(
                "task_status",
                "StatusId",
                statusId,
                query,
                ("@statusId", statusId),
                ("@name", name));
        }

        public bool UpdateTaskStatus(string statusId, string name)
        {
            string query = "UPDATE task_status SET Name = @name WHERE StatusId = @statusId;";

            return UpdateIfExists(
                "task_status",
                "StatusId",
                statusId,
                query,
                ("@statusId", statusId),
                ("@name", name));
        }

        public bool DeleteTaskStatus(string statusId)
        {
            string query = "DELETE FROM task_status WHERE StatusId = @statusId;";

            return DeleteIfExists(
                "task_status",
                "StatusId",
                statusId,
                query,
                ("@statusId", statusId));
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

        public bool DeleteTask(string taskId)
        {
            string query = "DELETE FROM tasks WHERE TaskId = @taskId;";

            return DeleteIfExists(
                "tasks",
                "TaskId",
                taskId,
                query,
                ("@taskId", taskId));
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

        public bool UpdateTaskAssignment(
            string assignmentId,
            string userId,
            string taskId,
            DateTime assignedDate,
            string statusId)
        {
            string query = "UPDATE task_assignments " +
                "SET UserId = @userId, " +
                "TaskId = @taskId, " +
                "AssignedDate = @assignedDate, " +
                "StatusId = @statusId " +
                "WHERE AssignmentId = @assignmentId;";

            return UpdateIfExists(
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

        public bool DeleteTaskAssignment(string assignmentId)
        {
            string query = "DELETE FROM task_assignments WHERE AssignmentId = @assignmentId;";

            return DeleteIfExists(
                "task_assignments",
                "AssignmentId",
                assignmentId,
                query,
                ("@assignmentId", assignmentId));
        }

        public bool InsertIncident(
            string incidentId,
            string userId,
            string taskId,
            string description,
            DateTime createdAt,
            string status)
        {
            string query = "INSERT INTO incidents(UserId, IncidentId, TaskId, Description, CreatedAt, Status) " +
                "VALUES(@userId, @incidentId, @taskId, @description, @createdAt, @status);";

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
                ("@status", status));
        }

        public bool UpdateIncident(
            string incidentId,
            string userId,
            string taskId,
            string description,
            DateTime createdAt,
            string status)
        {
            string query = "UPDATE incidents " +
                "SET UserId = @userId, " +
                "TaskId = @taskId, " +
                "Description = @description, " +
                "CreatedAt = @createdAt, " +
                "Status = @status " +
                "WHERE IncidentId = @incidentId;";

            return UpdateIfExists(
                "incidents",
                "IncidentId",
                incidentId,
                query,
                ("@userId", userId),
                ("@incidentId", incidentId),
                ("@taskId", taskId),
                ("@description", description),
                ("@createdAt", createdAt),
                ("@status", status));
        }

        public bool DeleteIncident(string incidentId)
        {
            string query = "DELETE FROM incidents WHERE IncidentId = @incidentId;";

            return DeleteIfExists(
                "incidents",
                "IncidentId",
                incidentId,
                query,
                ("@incidentId", incidentId));
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

        public bool UpdateReport(
            string reportId,
            string userId,
            string notes,
            DateTime createdAt,
            string statusBefore,
            string statusAfter,
            string duration,
            string taskId)
        {
            string query = "UPDATE reports " +
                "SET UserId = @userId, " +
                "Notes = @notes, " +
                "CreatedAt = @createdAt, " +
                "StatusBefore = @statusBefore, " +
                "StatusAfter = @statusAfter, " +
                "Duration = @duration, " +
                "TaskId = @taskId " +
                "WHERE ReportId = @reportId;";

            return UpdateIfExists(
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

        public bool DeleteReport(string reportId)
        {
            string query = "DELETE FROM reports WHERE ReportId = @reportId;";

            return DeleteIfExists(
                "reports",
                "ReportId",
                reportId,
                query,
                ("@reportId", reportId));
        }

        public bool InsertAvailability(
            string availabilityId,
            string startTime,
            string zone,
            string endTime,
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

        public bool UpdateAvailability(
            string availabilityId,
            string startTime,
            string zone,
            string endTime,
            string userId)
        {
            string query = "UPDATE availability " +
                "SET StartTime = @startTime, " +
                "Zone = @zone, " +
                "EndTime = @endTime, " +
                "UserId = @userId " +
                "WHERE AvailabilityId = @availabilityId;";

            return UpdateIfExists(
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

        public List<TaskSummary> GetTaskSummaries()
        {
            const string query = "SELECT t.TaskId, t.RequiredSkillId, t.PatientId, t.Description, t.Date, t.Priority, t.StatusId, " +
                "COALESCE(p.Name, t.PatientId) AS PatientName, " +
                "COALESCE(p.Zone, '') AS PatientZone, " +
                "COALESCE(ts.Name, t.StatusId) AS StatusName " +
                "FROM tasks t " +
                "LEFT JOIN patients p ON p.PatientId = t.PatientId " +
                "LEFT JOIN task_status ts ON ts.StatusId = t.StatusId " +
                "ORDER BY t.Date DESC;";

            return ExecuteReader(query, reader => new TaskSummary
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
                StatusName = reader.GetString("StatusName")
            });
        }

        public List<IncidentSummary> GetIncidentSummaries()
        {
            const string query = "SELECT i.IncidentId, i.Status, i.CreatedAt, " +
                "COALESCE(p.Name, '') AS PatientName, " +
                "COALESCE(t.Description, i.TaskId) AS TaskDescription " +
                "FROM incidents i " +
                "LEFT JOIN tasks t ON t.TaskId = i.TaskId " +
                "LEFT JOIN patients p ON p.PatientId = t.PatientId " +
                "ORDER BY i.CreatedAt DESC;";

            return ExecuteReader(query, reader => new IncidentSummary
            {
                IncidentId = reader.GetString("IncidentId"),
                PatientName = reader.GetString("PatientName"),
                TaskDescription = reader.GetString("TaskDescription"),
                Status = reader.GetString("Status"),
                CreatedAt = reader.GetDateTime("CreatedAt")
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

        public int CountActivePatients()
        {
            return ExecuteCount("SELECT COUNT(*) FROM patients;");
        }

        public int CountPendingTasks()
        {
            return ExecuteCount("SELECT COUNT(*) FROM tasks t " +
                "LEFT JOIN task_status ts ON ts.StatusId = t.StatusId " +
                "WHERE LOWER(COALESCE(ts.Name, t.StatusId)) = 'pending';");
        }

        public int CountActiveUsers()
        {
            return ExecuteCount("SELECT COUNT(*) FROM users WHERE IsActive = 1;");
        }

        public int CountOpenIncidents()
        {
            return ExecuteCount("SELECT COUNT(*) FROM incidents " +
                "WHERE LOWER(Status) NOT IN ('closed', 'resolved');");
        }
    }
}
