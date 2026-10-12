# StudentManager

StudentManager is a .NET Blazor Web App for managing courses and coursework. Students can enroll in courses, submit assignments, and track grades. Teachers can create courses and assignments, review submissions, and grade student work.

## Target audience

StudentManager is intended for two audiences in a school or other learning setting:

- **Students** who need a single place to find courses, manage enrollment, keep track of assignment deadlines, submit work, and follow grades and feedback.
- **Teachers** who need to organize their courses, publish assignments, monitor student enrollment and progress, and assess submitted work.

The application provides separate student and teacher experiences, with pages and actions appropriate to each account role.

## Project goals

- Make course and coursework information easy for students and teachers to find.
- Let students manage course enrollment, review upcoming work, submit assignments, and understand their progress through grades and feedback.
- Let teachers create and maintain courses and assignments, review submissions, and record grades and feedback.
- Keep each user's schoolwork scoped to their account and role.
- Provide a responsive, consistent interface that supports common course and coursework tasks.

## Requirements

- .NET 10 SDK
- The .NET Entity Framework Core command-line tool (`dotnet-ef` 10.x) for creating the local database

## Run locally

From the repository root, open a terminal and run:

```powershell
cd .\StudentManager
dotnet tool install --global dotnet-ef --version 10.0.12
dotnet ef database update
dotnet run --launch-profile http
```

If `dotnet-ef` is already installed, skip the installation command. The application is then available at <http://localhost:5219>.

The default connection string uses SQLite and creates `studentmanager.db` in the application directory. The database schema is managed by the EF Core migrations in `Data\Migrations`.

To build without starting the application, run this from the repository root:

```powershell
dotnet build .\StudentManager\StudentManager.csproj
```

## Development accounts

When the application runs in the `Development` environment, it seeds sample users, courses, assignments, and enrollments. The following accounts are available for local development:

| Role | Email |
| --- | --- |
| Teacher | `teacher1@school.test` |
| Teacher | `teacher2@school.test` |
| Teacher | `teacher3@school.test` |
| Student | `student1@school.test` |
| Student | `student2@school.test` |
| Student | `student3@school.test` |

All seeded accounts use the development password `StudentDemo123`. These accounts and this password are for local development only; do not use them in a deployed environment.

You can also create an account from the home page. Choose either the Student or Teacher role; student accounts also need a student number.

## Using the application

### Students

1. Log in with a student account or create one.
2. Use **Available courses** to search for courses and enroll.
3. Open **My courses** to view enrolled courses and their progress.
4. Open **Assignments** to see coursework and deadlines.
5. Submit or update work before it is graded, then review grades and teacher feedback.

### Teachers

1. Log in with a teacher account or create one.
2. Open **My courses** to view or create courses.
3. Select a course to manage its assignments and view enrolled students.
4. Open an assignment's submissions to review work and enter grades and feedback.

## Configuration

The default SQLite connection is configured in `StudentManager/appsettings.json`. For local development, override settings with user secrets or environment variables rather than committing credentials or production connection strings. The sample data is seeded only in the `Development` environment.

## Project structure

- `StudentManager/Areas/Identity` — registration and login pages
- `StudentManager/Areas/Identity` — registration and login pages (ASP.NET Core Identity Razor Pages)
- `StudentManager/Components` — the Blazor Web App UI (static server-side rendering with enhanced navigation and forms):
  - `App.razor`, `Routes.razor`, and `Layout/MainLayout.razor` — application shell, routing, and navigation
  - `Pages/Student` — student pages (dashboard, courses, assignments, submissions)
  - `Pages/Teacher` — teacher pages (dashboard, courses, assignments, grading, students)
- `StudentManager/Data` — EF Core database context, migrations, and development seed data
- `StudentManager/Services` — course, enrollment, assignment, submission, grading, and progress logic, plus the current-user and flash-message helpers used by the Blazor pages
- `StudentManager/wwwroot` — application styles, scripts, and static assets

## Tests and deployment

The repository currently does not include an automated test project. A successful `dotnet build` checks that the application compiles, but does not replace functional or accessibility testing.

No cloud deployment configuration is included in the repository. Before deploying, configure a production database and secrets, ensure the host uses the `Production` environment (development seed data runs only in `Development`), and deploy the application using a suitable .NET hosting service.