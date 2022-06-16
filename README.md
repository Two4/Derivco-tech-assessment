# Derivco-tech-assessment
Technical assessment for a dev position at Derivco

## Prerequisites
To run the database example, Docker is required

To build and run and/or test the Roulette example, Jetbrains Rider or MS Visual Studio is recommended.

## Troubleshooting
If you are running SQL server locally (or in a container or VM) on port 1433 already, change the host port binding in `docker-compose.yml`:
``` yaml
services:
  sql:
    image: sql-server-northwind
    stdin_open: true
    tty: true
    ports:
      - "<your chosen port>:1433"
    build:
      dockerfile: ./Dockerfile
```

## Connecting to the MS SQL Server container
Run `docker compose up` in the root directory of this repository, then open your favourite DBA tool (DataGrip, HeidiSQL, MS SMSS, NaviCat, etc.) and point it to `localhost:1433` (change the port if you have changed the host port binding) 
