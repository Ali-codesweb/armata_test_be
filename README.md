## Project Instructions

- Before running this project configure appsettings.json to make sure the database connection string is based on your database connection.
- This project is wrapped inside docker container, to run this project simply run

```shell
docker compose up -d
```

- Incase the database is not created run : docker exec -it armada_db /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "Armada@Password123" -C -Q "CREATE DATABASE ArmadaTestDb"
