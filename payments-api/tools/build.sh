#!/bin/bash

mysql -h "localhost" -u "oscar" -p < "./tools/create-database.sql"
dotnet ef dbcontext scaffold "Server=localhost;User=oscar;Password=gitCheckout2022!;Database=payments_api" Pomelo.EntityFrameworkCore.MySql -d -c PaymentsAPIDbContext --context-dir EfStructures -o Entities -f
dotnet clean payments-api.csproj
dotnet build payments-api.csproj



