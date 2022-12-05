#!/bin/bash

mysql -h "localhost" -u "oscar" -p < "./tools/create-database.sql"
dotnet ef dbcontext scaffold Name=ConnectionStrings:Default Pomelo.EntityFrameworkCore.MySql -d -c PaymentsAPIDbContext --context-dir EfStructures -o Entities -f
dotnet clean payments-api.csproj
dotnet build payments-api.csproj



