# StudentManager

## Database configuration

Development uses SQLite through the `ConnectionStrings:DefaultConnection`
setting in `appsettings.json`.

Production uses SQL Server. Configure the connection string outside source
control using the `ConnectionStrings__ProductionConnection` environment
variable (for example, through the deployment platform's secret settings).
The application fails at startup if the required connection string is missing.

The checked-in EF Core migrations were generated for SQLite. Create and verify
a SQL Server-compatible schema/migration set before deploying to production;
do not apply the SQLite migrations to SQL Server.