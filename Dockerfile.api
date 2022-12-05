FROM mcr.microsoft.com/dotnet/sdk:6.0

RUN mkdir -p /app
WORKDIR /app
COPY ./payments-api /app

RUN dotnet restore
RUN dotnet build "payments-api.csproj"
RUN dotnet publish "payments-api.csproj" -c Release -o /app/publish/

EXPOSE 13401

ENTRYPOINT ["dotnet", "/app/publish/payments-api.dll"]