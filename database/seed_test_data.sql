-- HomeCareManager schema and test data for MySQL.
-- Creates the database and tables if they do not exist.
-- All test users use the password: Test1234!

CREATE DATABASE IF NOT EXISTS homecaremanager
  CHARACTER SET utf8mb4
  COLLATE utf8mb4_unicode_ci;

USE homecaremanager;

CREATE TABLE IF NOT EXISTS roles (
  RoleId VARCHAR(50) NOT NULL,
  RoleName VARCHAR(100) NOT NULL,
  PRIMARY KEY (RoleId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS skills (
  SkillId VARCHAR(50) NOT NULL,
  Name VARCHAR(120) NOT NULL,
  PRIMARY KEY (SkillId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS task_status (
  StatusId VARCHAR(50) NOT NULL,
  Name VARCHAR(100) NOT NULL,
  PRIMARY KEY (StatusId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS users (
  UserId VARCHAR(50) NOT NULL,
  Name VARCHAR(150) NOT NULL,
  RoleId VARCHAR(50) NOT NULL,
  Email VARCHAR(255) NOT NULL,
  PasswordHash VARCHAR(255) NOT NULL,
  PasswordChanged TINYINT(1) NOT NULL DEFAULT 0,
  IsActive TINYINT(1) NOT NULL DEFAULT 1,
  CreatedAt DATETIME NOT NULL,
  SkillId VARCHAR(50) NOT NULL,
  PRIMARY KEY (UserId),
  INDEX IX_users_RoleId (RoleId),
  INDEX IX_users_SkillId (SkillId),
  CONSTRAINT FK_users_roles
    FOREIGN KEY (RoleId) REFERENCES roles (RoleId)
    ON UPDATE CASCADE ON DELETE RESTRICT,
  CONSTRAINT FK_users_skills
    FOREIGN KEY (SkillId) REFERENCES skills (SkillId)
    ON UPDATE CASCADE ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS patients (
  PatientId VARCHAR(50) NOT NULL,
  Name VARCHAR(150) NOT NULL,
  Address VARCHAR(255) NOT NULL,
  Phone VARCHAR(50) NOT NULL,
  Notes TEXT NOT NULL,
  Priority VARCHAR(20) NOT NULL,
  EmergencyContact VARCHAR(150) NOT NULL,
  Zone VARCHAR(100) NOT NULL,
  PRIMARY KEY (PatientId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS tasks (
  TaskId VARCHAR(50) NOT NULL,
  RequiredSkillId VARCHAR(50) NOT NULL,
  PatientId VARCHAR(50) NOT NULL,
  Description TEXT NOT NULL,
  `Date` DATETIME NOT NULL,
  Priority VARCHAR(20) NOT NULL,
  StatusId VARCHAR(50) NOT NULL,
  PRIMARY KEY (TaskId),
  INDEX IX_tasks_RequiredSkillId (RequiredSkillId),
  INDEX IX_tasks_PatientId (PatientId),
  INDEX IX_tasks_StatusId (StatusId),
  CONSTRAINT FK_tasks_skills
    FOREIGN KEY (RequiredSkillId) REFERENCES skills (SkillId)
    ON UPDATE CASCADE ON DELETE RESTRICT,
  CONSTRAINT FK_tasks_patients
    FOREIGN KEY (PatientId) REFERENCES patients (PatientId)
    ON UPDATE CASCADE ON DELETE CASCADE,
  CONSTRAINT FK_tasks_task_status
    FOREIGN KEY (StatusId) REFERENCES task_status (StatusId)
    ON UPDATE CASCADE ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS task_assignments (
  AssignmentId VARCHAR(50) NOT NULL,
  UserId VARCHAR(50) NOT NULL,
  TaskId VARCHAR(50) NOT NULL,
  AssignedDate DATETIME NOT NULL,
  StatusId VARCHAR(50) NOT NULL,
  PRIMARY KEY (AssignmentId),
  INDEX IX_task_assignments_UserId (UserId),
  INDEX IX_task_assignments_TaskId (TaskId),
  INDEX IX_task_assignments_StatusId (StatusId),
  CONSTRAINT FK_task_assignments_users
    FOREIGN KEY (UserId) REFERENCES users (UserId)
    ON UPDATE CASCADE ON DELETE CASCADE,
  CONSTRAINT FK_task_assignments_tasks
    FOREIGN KEY (TaskId) REFERENCES tasks (TaskId)
    ON UPDATE CASCADE ON DELETE CASCADE,
  CONSTRAINT FK_task_assignments_task_status
    FOREIGN KEY (StatusId) REFERENCES task_status (StatusId)
    ON UPDATE CASCADE ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS incidents (
  IncidentId VARCHAR(50) NOT NULL,
  UserId VARCHAR(50) NOT NULL,
  TaskId VARCHAR(50) NOT NULL,
  Description TEXT NOT NULL,
  CreatedAt DATETIME NOT NULL,
  Status VARCHAR(50) NOT NULL,
  Severity VARCHAR(20) NOT NULL DEFAULT 'Medium',
  ResolutionNotes TEXT NULL,
  ResolvedAt DATETIME NULL,
  AssignedToUserId VARCHAR(50) NULL,
  ReportId VARCHAR(50) NULL,
  PRIMARY KEY (IncidentId),
  INDEX IX_incidents_UserId (UserId),
  INDEX IX_incidents_TaskId (TaskId),
  CONSTRAINT FK_incidents_users
    FOREIGN KEY (UserId) REFERENCES users (UserId)
    ON UPDATE CASCADE ON DELETE CASCADE,
  CONSTRAINT FK_incidents_tasks
    FOREIGN KEY (TaskId) REFERENCES tasks (TaskId)
    ON UPDATE CASCADE ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

DROP PROCEDURE IF EXISTS add_incident_column_if_missing;
DELIMITER //
CREATE PROCEDURE add_incident_column_if_missing(
  IN column_name VARCHAR(64),
  IN column_definition TEXT
)
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'incidents'
      AND COLUMN_NAME = column_name
  ) THEN
    SET @ddl = CONCAT('ALTER TABLE incidents ADD COLUMN ', column_definition);
    PREPARE stmt FROM @ddl;
    EXECUTE stmt;
    DEALLOCATE PREPARE stmt;
  END IF;
END//
DELIMITER ;

CALL add_incident_column_if_missing('Severity', 'Severity VARCHAR(20) NOT NULL DEFAULT ''Medium'' AFTER Status');
CALL add_incident_column_if_missing('ResolutionNotes', 'ResolutionNotes TEXT NULL AFTER Severity');
CALL add_incident_column_if_missing('ResolvedAt', 'ResolvedAt DATETIME NULL AFTER ResolutionNotes');
CALL add_incident_column_if_missing('AssignedToUserId', 'AssignedToUserId VARCHAR(50) NULL AFTER ResolvedAt');
CALL add_incident_column_if_missing('ReportId', 'ReportId VARCHAR(50) NULL AFTER AssignedToUserId');

DROP PROCEDURE IF EXISTS add_incident_column_if_missing;

CREATE TABLE IF NOT EXISTS reports (
  ReportId VARCHAR(50) NOT NULL,
  UserId VARCHAR(50) NOT NULL,
  Notes TEXT NOT NULL,
  CreatedAt DATETIME NOT NULL,
  StatusBefore VARCHAR(50) NOT NULL,
  StatusAfter VARCHAR(50) NOT NULL,
  Duration VARCHAR(50) NOT NULL,
  TaskId VARCHAR(50) NOT NULL,
  PRIMARY KEY (ReportId),
  INDEX IX_reports_UserId (UserId),
  INDEX IX_reports_TaskId (TaskId),
  CONSTRAINT FK_reports_users
    FOREIGN KEY (UserId) REFERENCES users (UserId)
    ON UPDATE CASCADE ON DELETE CASCADE,
  CONSTRAINT FK_reports_tasks
    FOREIGN KEY (TaskId) REFERENCES tasks (TaskId)
    ON UPDATE CASCADE ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS availability (
  AvailabilityId VARCHAR(50) NOT NULL,
  StartTime DATETIME NOT NULL,
  Zone VARCHAR(100) NOT NULL,
  EndTime DATETIME NOT NULL,
  UserId VARCHAR(50) NOT NULL,
  PRIMARY KEY (AvailabilityId),
  INDEX IX_availability_UserId (UserId),
  CONSTRAINT FK_availability_users
    FOREIGN KEY (UserId) REFERENCES users (UserId)
    ON UPDATE CASCADE ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

START TRANSACTION;

SET @test_password_hash = 'PBKDF2$100000$ECI0RlhqfI6QorTG2Or8Dg==$Q9kdId8hJE14Cc3qXITL/SrPpjNfmXn7ezLuJKn39Xk=';

INSERT INTO roles (RoleId, RoleName) VALUES
  ('admin', 'Admin'),
  ('doctor', 'Doctor'),
  ('assistant', 'Assistant')
ON DUPLICATE KEY UPDATE
  RoleName = VALUES(RoleName);

INSERT INTO skills (SkillId, Name) VALUES
  ('skill-admin', 'Administration'),
  ('skill-doctor', 'Medical care'),
  ('skill-assistant', 'Home care assistance'),
  ('skill-nurse', 'Nursing care'),
  ('skill-physio', 'Physiotherapy')
ON DUPLICATE KEY UPDATE
  Name = VALUES(Name);

INSERT INTO task_status (StatusId, Name) VALUES
  ('pending', 'Pending'),
  ('assigned', 'Assigned'),
  ('accepted', 'Accepted'),
  ('rejected', 'Rejected'),
  ('in-progress', 'In Progress'),
  ('completed', 'Completed'),
  ('cancelled', 'Cancelled')
ON DUPLICATE KEY UPDATE
  Name = VALUES(Name);

INSERT INTO users
  (UserId, Name, RoleId, Email, PasswordHash, PasswordChanged, IsActive, CreatedAt, SkillId)
VALUES
  ('u-admin-test', 'Admin Test', 'admin', 'admin@test.local', @test_password_hash, 0, 1, '2026-05-01 09:00:00', 'skill-admin'),
  ('u-doctor-test', 'Doctor Test', 'doctor', 'doctor@test.local', @test_password_hash, 0, 1, '2026-05-01 09:05:00', 'skill-doctor'),
  ('u-assist-test', 'Assistant Test', 'assistant', 'assistant@test.local', @test_password_hash, 0, 1, '2026-05-01 09:10:00', 'skill-assistant'),
  ('u-nurse-test', 'Nurse Test', 'assistant', 'nurse@test.local', @test_password_hash, 0, 1, '2026-05-01 09:15:00', 'skill-nurse'),
  ('u-physio-test', 'Physio Test', 'assistant', 'physio@test.local', @test_password_hash, 0, 0, '2026-05-01 09:20:00', 'skill-physio')
ON DUPLICATE KEY UPDATE
  Name = VALUES(Name),
  RoleId = VALUES(RoleId),
  Email = VALUES(Email),
  PasswordHash = VALUES(PasswordHash),
  PasswordChanged = VALUES(PasswordChanged),
  IsActive = VALUES(IsActive),
  CreatedAt = VALUES(CreatedAt),
  SkillId = VALUES(SkillId);

INSERT INTO patients
  (PatientId, Name, Address, Phone, Notes, Priority, EmergencyContact, Zone)
VALUES
  ('pat-maria-lopez', 'Maria Lopez', 'Calle Mayor 12, Madrid', '+34 600 111 001', 'Diabetes follow-up and medication reminders.', 'High', 'Carlos Lopez - +34 600 111 101', 'Centro'),
  ('pat-jose-garcia', 'Jose Garcia', 'Avenida de America 44, Madrid', '+34 600 111 002', 'Reduced mobility after surgery.', 'Medium', 'Ana Garcia - +34 600 111 102', 'Chamartin'),
  ('pat-carmen-ruiz', 'Carmen Ruiz', 'Calle Alcala 88, Madrid', '+34 600 111 003', 'Daily hygiene support and meal supervision.', 'High', 'Luis Ruiz - +34 600 111 103', 'Salamanca'),
  ('pat-antonio-m', 'Antonio Martin', 'Paseo de la Castellana 143, Madrid', '+34 600 111 004', 'Weekly physiotherapy plan.', 'Low', 'Rosa Martin - +34 600 111 104', 'Tetuan'),
  ('pat-elena-torres', 'Elena Torres', 'Calle de Atocha 25, Madrid', '+34 600 111 005', 'Blood pressure control twice per week.', 'Medium', 'Miguel Torres - +34 600 111 105', 'Arganzuela')
ON DUPLICATE KEY UPDATE
  Name = VALUES(Name),
  Address = VALUES(Address),
  Phone = VALUES(Phone),
  Notes = VALUES(Notes),
  Priority = VALUES(Priority),
  EmergencyContact = VALUES(EmergencyContact),
  Zone = VALUES(Zone);

INSERT INTO tasks
  (RequiredSkillId, TaskId, PatientId, Description, `Date`, Priority, StatusId)
VALUES
  ('skill-doctor', 'task-med-001', 'pat-maria-lopez', 'Review glucose records and adjust care plan.', '2026-05-27 10:00:00', 'High', 'assigned'),
  ('skill-assistant', 'task-asst-001', 'pat-carmen-ruiz', 'Morning hygiene support and breakfast supervision.', '2026-05-27 08:30:00', 'High', 'assigned'),
  ('skill-nurse', 'task-nurse-001', 'pat-elena-torres', 'Blood pressure check and medication confirmation.', '2026-05-28 11:00:00', 'Medium', 'in-progress'),
  ('skill-physio', 'task-physio-001', 'pat-antonio-m', 'Guided lower-body mobility session.', '2026-05-29 16:00:00', 'Low', 'completed'),
  ('skill-assistant', 'task-asst-002', 'pat-jose-garcia', 'Evening meal preparation and mobility assistance.', '2026-05-30 19:00:00', 'Medium', 'pending')
ON DUPLICATE KEY UPDATE
  RequiredSkillId = VALUES(RequiredSkillId),
  PatientId = VALUES(PatientId),
  Description = VALUES(Description),
  `Date` = VALUES(`Date`),
  Priority = VALUES(Priority),
  StatusId = VALUES(StatusId);

DELETE FROM task_assignments
WHERE AssignmentId = 'assign-005';

INSERT INTO task_assignments
  (UserId, TaskId, AssignmentId, AssignedDate, StatusId)
VALUES
  ('u-doctor-test', 'task-med-001', 'assign-001', '2026-05-26 14:00:00', 'assigned'),
  ('u-assist-test', 'task-asst-001', 'assign-002', '2026-05-26 15:00:00', 'assigned'),
  ('u-nurse-test', 'task-nurse-001', 'assign-003', '2026-05-26 16:00:00', 'in-progress'),
  ('u-physio-test', 'task-physio-001', 'assign-004', '2026-05-25 09:00:00', 'completed')
ON DUPLICATE KEY UPDATE
  UserId = VALUES(UserId),
  TaskId = VALUES(TaskId),
  AssignedDate = VALUES(AssignedDate),
  StatusId = VALUES(StatusId);

INSERT INTO incidents
  (UserId, IncidentId, TaskId, Description, CreatedAt, Status, Severity, ResolutionNotes, ResolvedAt, AssignedToUserId, ReportId)
VALUES
  ('u-assist-test', 'incident-001', 'task-asst-001', 'Patient reported dizziness before breakfast.', '2026-05-27 08:45:00', 'Open', 'High', '', NULL, 'u-doctor-test', 'report-002'),
  ('u-doctor-test', 'incident-002', 'task-med-001', 'Glucose reading above expected range.', '2026-05-27 10:20:00', 'In review', 'Critical', '', NULL, 'u-doctor-test', 'report-003'),
  ('u-physio-test', 'incident-003', 'task-physio-001', 'Minor knee discomfort after exercise.', '2026-05-29 16:45:00', 'Resolved', 'Low', 'No further action required after observation.', '2026-05-29 17:10:00', 'u-physio-test', 'report-001'),
  ('u-assist-test', 'incident-004', 'task-asst-002', 'Access code did not work on first visit.', '2026-05-30 19:15:00', 'Closed', 'Medium', 'Access code updated for future visits.', '2026-05-30 19:40:00', 'u-assist-test', NULL)
ON DUPLICATE KEY UPDATE
  UserId = VALUES(UserId),
  TaskId = VALUES(TaskId),
  Description = VALUES(Description),
  CreatedAt = VALUES(CreatedAt),
  Status = VALUES(Status),
  Severity = VALUES(Severity),
  ResolutionNotes = VALUES(ResolutionNotes),
  ResolvedAt = VALUES(ResolvedAt),
  AssignedToUserId = VALUES(AssignedToUserId),
  ReportId = VALUES(ReportId);

INSERT INTO reports
  (ReportId, UserId, Notes, CreatedAt, StatusBefore, StatusAfter, Duration, TaskId)
VALUES
  ('report-001', 'u-physio-test', 'Completed planned mobility exercises without further issues.', '2026-05-29 17:00:00', 'In Progress', 'Completed', '45 min', 'task-physio-001'),
  ('report-002', 'u-assist-test', 'Breakfast completed. Dizziness reported to doctor.', '2026-05-27 09:15:00', 'Assigned', 'In Progress', '40 min', 'task-asst-001'),
  ('report-003', 'u-doctor-test', 'Reviewed records and scheduled follow-up check.', '2026-05-27 10:45:00', 'Pending', 'Assigned', '30 min', 'task-med-001')
ON DUPLICATE KEY UPDATE
  UserId = VALUES(UserId),
  Notes = VALUES(Notes),
  CreatedAt = VALUES(CreatedAt),
  StatusBefore = VALUES(StatusBefore),
  StatusAfter = VALUES(StatusAfter),
  Duration = VALUES(Duration),
  TaskId = VALUES(TaskId);

INSERT INTO availability
  (AvailabilityId, StartTime, Zone, EndTime, UserId)
VALUES
  ('avail-001', '2026-05-27 08:00:00', 'Centro', '2026-05-27 14:00:00', 'u-doctor-test'),
  ('avail-002', '2026-05-27 08:00:00', 'Salamanca', '2026-05-27 16:00:00', 'u-assist-test'),
  ('avail-003', '2026-05-28 09:00:00', 'Arganzuela', '2026-05-28 15:00:00', 'u-nurse-test'),
  ('avail-004', '2026-05-29 12:00:00', 'Tetuan', '2026-05-29 18:00:00', 'u-physio-test'),
  ('avail-005', '2026-05-30 18:00:00', 'Chamartin', '2026-05-30 22:00:00', 'u-assist-test')
ON DUPLICATE KEY UPDATE
  StartTime = VALUES(StartTime),
  Zone = VALUES(Zone),
  EndTime = VALUES(EndTime),
  UserId = VALUES(UserId);

COMMIT;
