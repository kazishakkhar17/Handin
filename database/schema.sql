-- ---------------------------------------------------------------------------
-- Assignment & Submission Management System — PostgreSQL schema
--
-- This mirrors exactly what AppDbContext (EF Core Code-First) produces.
-- The recommended setup path is to let EF Core create the database for you
-- (see README: "Database setup"), which runs this same schema via migrations.
-- This script is provided as a reference / fallback for anyone who wants to
-- inspect or create the schema without running the .NET migration tooling.
-- ---------------------------------------------------------------------------

CREATE TABLE IF NOT EXISTS "Users" (
    "Id"            SERIAL PRIMARY KEY,
    "FullName"      VARCHAR(150) NOT NULL,
    "Email"         VARCHAR(200) NOT NULL,
    "PasswordHash"  TEXT NOT NULL,
    "Role"          VARCHAR(20) NOT NULL CHECK ("Role" IN ('Admin', 'Teacher', 'Student')),
    "IsActive"      BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedAt"     TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    CONSTRAINT "UX_Users_Email" UNIQUE ("Email")
);

CREATE TABLE IF NOT EXISTS "ClassCourses" (
    "Id"            SERIAL PRIMARY KEY,
    "Name"          VARCHAR(150) NOT NULL,
    "Description"   TEXT NULL,
    "CreatedAt"     TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS "Subjects" (
    "Id"            SERIAL PRIMARY KEY,
    "Name"          VARCHAR(150) NOT NULL,
    "Code"          VARCHAR(30) NULL,
    "ClassCourseId" INT NOT NULL REFERENCES "ClassCourses"("Id") ON DELETE CASCADE,
    CONSTRAINT "UX_Subjects_Class_Name" UNIQUE ("ClassCourseId", "Name")
);

CREATE TABLE IF NOT EXISTS "TeacherSubjectClasses" (
    "Id"            SERIAL PRIMARY KEY,
    "TeacherId"     INT NOT NULL REFERENCES "Users"("Id") ON DELETE RESTRICT,
    "SubjectId"     INT NOT NULL REFERENCES "Subjects"("Id") ON DELETE RESTRICT,
    "ClassCourseId" INT NOT NULL REFERENCES "ClassCourses"("Id") ON DELETE RESTRICT,
    "AssignedAt"    TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    CONSTRAINT "UX_TeacherSubjectClass" UNIQUE ("TeacherId", "SubjectId", "ClassCourseId")
);

CREATE TABLE IF NOT EXISTS "StudentEnrollments" (
    "Id"            SERIAL PRIMARY KEY,
    "StudentId"     INT NOT NULL REFERENCES "Users"("Id") ON DELETE CASCADE,
    "ClassCourseId" INT NOT NULL REFERENCES "ClassCourses"("Id") ON DELETE RESTRICT,
    "EnrolledAt"    TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    CONSTRAINT "UX_StudentEnrollments_Student" UNIQUE ("StudentId")
);

CREATE TABLE IF NOT EXISTS "Assignments" (
    "Id"                SERIAL PRIMARY KEY,
    "Title"             VARCHAR(200) NOT NULL,
    "Description"       TEXT NOT NULL,
    "SubjectId"         INT NOT NULL REFERENCES "Subjects"("Id") ON DELETE RESTRICT,
    "ClassCourseId"     INT NOT NULL REFERENCES "ClassCourses"("Id") ON DELETE RESTRICT,
    "TeacherId"         INT NOT NULL REFERENCES "Users"("Id") ON DELETE RESTRICT,
    "Deadline"          TIMESTAMP WITH TIME ZONE NOT NULL,
    "MaxMarks"          INT NOT NULL,
    "Status"            VARCHAR(20) NOT NULL CHECK ("Status" IN ('Draft', 'Published')),
    "AllowResubmission" BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedAt"         TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    "UpdatedAt"         TIMESTAMP WITH TIME ZONE NULL
);

CREATE TABLE IF NOT EXISTS "Submissions" (
    "Id"                 SERIAL PRIMARY KEY,
    "AssignmentId"       INT NOT NULL REFERENCES "Assignments"("Id") ON DELETE CASCADE,
    "StudentId"          INT NOT NULL REFERENCES "Users"("Id") ON DELETE RESTRICT,
    "AnswerText"         TEXT NOT NULL,
    "AttachmentUrl"      TEXT NULL,
    "SubmittedAt"        TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    "LastUpdatedAt"      TIMESTAMP WITH TIME ZONE NULL,
    "Status"             VARCHAR(30) NOT NULL CHECK ("Status" IN ('Submitted', 'Late', 'Graded', 'ReturnedForRevision')),
    "MarksAwarded"       INT NULL,
    "Feedback"           TEXT NULL,
    "GradedByTeacherId"  INT NULL REFERENCES "Users"("Id") ON DELETE SET NULL,
    "GradedAt"           TIMESTAMP WITH TIME ZONE NULL,
    CONSTRAINT "UX_Submissions_Assignment_Student" UNIQUE ("AssignmentId", "StudentId")
);

CREATE INDEX IF NOT EXISTS "IX_Assignments_ClassCourseId" ON "Assignments" ("ClassCourseId");
CREATE INDEX IF NOT EXISTS "IX_Assignments_TeacherId" ON "Assignments" ("TeacherId");
CREATE INDEX IF NOT EXISTS "IX_Submissions_AssignmentId" ON "Submissions" ("AssignmentId");
