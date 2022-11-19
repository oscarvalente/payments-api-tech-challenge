# README #

This README explains how to setup the tech challenge and use it.

### How do I get set up? ###

* Required tools
* Pre-configuration
* Install
* Run
* Test

### Required tools
* dotnet
* dotnet-ef
* mysql server

#### Pre-configuration

1. run `mysql.server start`
2. in mysql environment run `GRANT ALL PRIVELEGES ON *.* TO 'oscar'@'localhost'`;

#### Install
* run `sh tools/build.sh`

#### Run
* run `dotnet run payments-api`

#### Test
* import Postman collection `tools/3rd-party-integrations/Tech Challenge.postman_collection` to Postman
    1. /sign-up
    2. /sign-in
    3. before invoke `/api/pay` please create card using script `aux/create-customer-card.sql`
    4. /pay

### API
* add swagger file
