
# RomHack Migration Tool

## Overview

The RomHack Migration Tool is a command-line utility designed to manage and rename ROM hack files from the released RH.net repository and update the corresponding database entries. The tool is particularly useful for maintaining a consistent naming convention across ROM hack files and their corresponding records in the database.

This tool is a WIP and on its current state is just able to rename hacks. Next versions will include

- Management and renaming of Translations, Fonts, Hombrew, utilities, documents and abandoned projects

## Why bother doing this

- Well first I did it for myself. I wanted a clean and readable collection of available romhacks after the events that transpired at RH.net.
The current released zip and rar files retained the original filename used by the hack creator upon submission to RH.net so there's was no easy way to rename the files unless connecting to the SQL database to get the game that is related to the hack. 
- Second, I plan to create a self hostable web app to browse and search Hacks based on the released files and DB (currently I do not have interest/time to create and host a public alternative to rh.net)

## Prerequisites

- **Docker or Podman**: Ensure you have Docker or Podman installed on your system.
- **MySQL**: The tool interacts with a MySQL database that holds the ROM hack information.

## Running MySQL with Docker/Podman

To run a MySQL server with Docker or Podman, execute the following command and make sure to replace {your_password}:

    
    docker run --restart always -p 3307:3306 -e MYSQL_ROOT_PASSWORD={your_password} docker.io/mysql

### Using Docker Compose

Alternatively, you can use Docker Compose to run the MySQL server. Add the following `docker-compose.yml` file to your repository:

```version: '3.8'

services:
  mysql:
    image: mysql
    container_name: mysql_romhacking
    restart: always
    ports:
      - "3307:3306"
    environment:
      MYSQL_ROOT_PASSWORD: RomHacking
```
You will need to create a DB named 'romhacking' and download the `romhacking.sql` file to set up your database. Download it from [this link](https://archive.org/details/romhacking.net-20240801).
To import the SQL file into your MySQL instance, use the following command:

    mysql -h 127.0.0.1 -P 3307 -u root -p {your_password} < /path/to/romhacking.sql

Replace `/path/to/romhacking.sql` with the actual path to your downloaded SQL file.

## Building and Dependencies

### Dependencies

-   **.NET SDK**: Ensure you have the .NET SDK installed.
-   **MySQL.Data**: The tool uses the MySQL.Data library for database interactions.

### Building the Project

To build the project, navigate to the root directory and run:

    dotnet publish -c Release -r win-x64 --self-contained /p:PublishSingleFile=true

## Usage

The tool supports two primary commands: `rename` and `update-hacksdb`.

### Running the Rename Command

The `Rename Command` renames the files in the hacks directory from the RH.net repository according to the following naming convention `$"[{hackkey}] {gametitle} - {hackTitle}{extension}"`. The reasoning for this is that in its current state the zip and rar files that contain the patches do not follow any naming convention.

    ./rhackstool rename --hacks-path "/path/to/hacks/folder" --log-path "/path/to/log" --connection-string "Server={ip};Port={port};Database=romhacking;User Id=root;Password={your_password};"

### Running the Update-HacksDb Command

The `Update-HacksDb Command` updates the `filename` column in the `Hacks` table of the DB to reflect the current names of the files inside the hacks subdirectories. 

    ./rhackstool update-hacksdb --hacks-path "/path/to/hacks/folder" --log-path "/path/to/log" --connection-string "Server={ip};Port={port};Database=romhacking;User Id=root;Password={your_password};"
### Command Arguments

-   `--hacks-path`: Specifies the directory where the ROM hack files are located. If not provided, defaults to the root directory (`./`).
-   `--log-path`: Specifies the directory where log files will be saved. If not provided, defaults to the root directory (`./`).
-   `--connection-string`: The connection string for the MySQL database. Defaults to `"Server=192.168.1.56;Port=3306;Database=romhacking;User Id=root;Password=RomHacking;"`.