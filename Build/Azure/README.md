﻿This directory contains test configs and setup scripts for test jobs on Azure Pipelines
- `net472` folder stores test job configs for .NET 4.7.2 Windows tests
- `netcoreapp21` folder stores test job configs for `netcoreapp2.1` test runs for Windows, Linux and MacOS
- `netcoreapp31` folder stores test job configs for `netcoreapp3.1` test runs for Windows, Linux and MacOS
- `net50` folder stores test job configs for `net5.0` test runs for Windows, Linux and MacOS
- `net60` folder stores test job configs for `net6.0` test runs for Windows, Linux and MacOS
- `scripts` folder stores test job setup scripts (`*.cmd` for Windows jobs and `*.sh` for Linux and MacOS)

## Azure Pipelines
All existing pipelines we have listed below. If you need more flexible test runs, you can request more test pipelines. E.g. to run only specific database or framework/OS tests.

#### `default` pipeline

Automatically runs for:
- PR to `release` branch: runs all tests for PR commit
- commit to `master`: runs all tests and publish nugets to [Azure Artifacts feed](https://dev.azure.com/linq2db/linq2db/_packaging?_a=feed&feed=linq2db)
- commit to `release`: publish nugets to [Nuget.org](https://www.nuget.org/profiles/LinqToDB)

#### `build` pipeline

Automatically triggered for all PR commits and runs solution build

#### `test-all` pipeline

Runs manually using `/azp run test-all` command from PR comment by team member. Currently this pipeline will skip testing targeting macos (you need to use db-specific pipeline for it) due to incredible slowness of docker for macos.

#### db-specific test pipelines

Those pipelines used to run tests only for specific databases manually by team member:
- `/azp run test-access` - MS Access tests
- `/azp run test-db2` - IBM DB2 tests
- `/azp run test-firebird` - Firebird tests
- `/azp run test-informix` - IBM Informix tests
- `/azp run test-mysql` - MySQL and MariaDB tests
- `/azp run test-oracle` - Oracle tests
- `/azp run test-postgresql` - PostgreSQL tests
- `/azp run test-saphana` - SAP HANA 2 tests
- `/azp run test-sqlce` - SQL CE tests
- `/azp run test-sqlite` - SQLite tests
- `/azp run test-sqlserver` - SQL Server tests (all versions)
- `/azp run test-sqlserver-2019` - SQL Server 2019 tests
- `/azp run test-sybase` - SAP/SYBASE ASE tests

#### `experimental` pipeline
Runs manually using `/azp run experimental` command from PR and used for development and testing of new pipelines/test providers.
Base pipeline template contains only solution build and should be reset to initial state before merge.

## Test Matrix

Following table contains information about which test jobs are awailable per:
- operating system
- target framework
- database
- database version
- database provider

Legend:
- :heavy_minus_sign: - test configuration not supported (e.g. db/provider not available for target OS/Framework)
- :heavy_check_mark: - test job implemented
- :x: - test job not implemented yet
- `netfx`: .NET Framework (4.7.2)
- `netcore`: .NET Core 2.1/3.1 OR .NET 5.0 / 6.0
- :door: - Windows 2022, 2019 or 2016 (depends on used docker image for database)
- :penguin: - Linux (Ununtu 20.04)
- :green_apple: - MacOS Catalina 10.15

| Database (version): provider \ Target framework (OS) | netfx :door: | netcore :door: | netcore :penguin: | netcore :green_apple: |
|:---|:---:|:---:|:---:|:---:|
|TestNoopProvider<sup>[1](#notes)</sup>|:heavy_check_mark:|:heavy_check_mark:|:heavy_check_mark:|:heavy_check_mark:|
|SQLite [3.13.0](https://www.sqlite.org/releaselog/3_13_0.html)<sup>[2](#notes)</sup><br>[Microsoft.Data.SQLite](https://www.nuget.org/packages/Microsoft.Data.SQLite/) 1.1.1<br>with NorthwindDB Tests|:heavy_check_mark:|:heavy_minus_sign:|:heavy_minus_sign:|:heavy_minus_sign:|
|SQLite [3.33.0](https://www.sqlite.org/releaselog/3_33_0.html) / [3.35.5](https://www.sqlite.org/releaselog/3_35_5.html)<br>[Microsoft.Data.SQLite](https://www.nuget.org/packages/Microsoft.Data.SQLite/) 5.0.12 / 6.0.0<br>with NorthwindDB Tests|:heavy_minus_sign:|:heavy_check_mark:|:heavy_check_mark:|:heavy_check_mark:|
|SQLite [3.37.0](https://www.sqlite.org/draft/releaselog/current.html)<br>[System.Data.SQLite](https://www.nuget.org/packages/System.Data.SQLite.Core/) 1.0.115.5<br>with NorthwindDB Tests|:heavy_check_mark:|:heavy_check_mark:|:heavy_check_mark:|:heavy_check_mark:|
|SQLite [3.37.0](https://www.sqlite.org/draft/releaselog/current.html)<br>[System.Data.SQLite](https://www.nuget.org/packages/System.Data.SQLite.Core/) 1.0.115.5<br>with [MiniProfiler](https://www.nuget.org/packages/MiniProfiler.Shared/) 4.2.22 (core)<br>[MiniProfiler](https://www.nuget.org/packages/MiniProfiler/) 3.2.0.157 (netfx)<br>without mappings to underlying provider|:heavy_check_mark:|:heavy_check_mark:|:heavy_check_mark:|:heavy_check_mark:|
|SQLite [3.37.0](https://www.sqlite.org/draft/releaselog/current.html)<br>[System.Data.SQLite](https://www.nuget.org/packages/System.Data.SQLite.Core/) 1.0.115.5<br>with [MiniProfiler](https://www.nuget.org/packages/MiniProfiler.Shared/) 4.2.22 (core)<br>[MiniProfiler](https://www.nuget.org/packages/MiniProfiler/) 3.2.0.157 (netfx)<br>with mappings to underlying provider|:heavy_check_mark:|:heavy_check_mark:|:heavy_check_mark:|:heavy_check_mark:|
|Access<sup>[3](#notes)</sup><br>Jet 4.0 OLE DB|:heavy_check_mark:|:x:|:heavy_minus_sign:|:heavy_minus_sign:|
|Access<sup>[3](#notes)</sup><br>ACE 12 OLE DB|:heavy_check_mark:|:heavy_check_mark:|:heavy_minus_sign:|:heavy_minus_sign:|
|Access<br>MDB ODBC|:heavy_check_mark:|:x:|:heavy_minus_sign:|:heavy_minus_sign:|
|Access<br>MDB+ACCDB ODBC|:heavy_check_mark:|:heavy_check_mark:|:heavy_minus_sign:|:heavy_minus_sign:|
|MS SQL CE<sup>[4](#notes)</sup>|:heavy_check_mark:|:heavy_check_mark:|:heavy_minus_sign:|:heavy_minus_sign:|
|MS SQL Server 2000<br>[System.Data.SqlClient](https://www.nuget.org/packages/System.Data.SqlClient/) 4.8.3|:x:|:x:|:heavy_minus_sign:|:heavy_minus_sign:|
|MS SQL Server 2000<br>[Microsoft.Data.SqlClient](https://www.nuget.org/packages/Microsoft.Data.SqlClient/) 3.0.1|:x:|:x:|:heavy_minus_sign:|:heavy_minus_sign:|
|MS SQL Server 2005<br>[System.Data.SqlClient](https://www.nuget.org/packages/System.Data.SqlClient/) 4.8.3|:heavy_check_mark:|:heavy_check_mark:|:heavy_minus_sign:|:heavy_minus_sign:|
|MS SQL Server 2005<br>[Microsoft.Data.SqlClient](https://www.nuget.org/packages/Microsoft.Data.SqlClient/) 3.0.1|:heavy_check_mark:|:heavy_check_mark:|:heavy_minus_sign:|:heavy_minus_sign:|
|MS SQL Server 2008<br>[System.Data.SqlClient](https://www.nuget.org/packages/System.Data.SqlClient/) 4.8.3|:heavy_check_mark:|:heavy_check_mark:|:heavy_minus_sign:|:heavy_minus_sign:|
|MS SQL Server 2008<br>[Microsoft.Data.SqlClient](https://www.nuget.org/packages/Microsoft.Data.SqlClient/) 3.0.1|:heavy_check_mark:|:heavy_check_mark:|:heavy_minus_sign:|:heavy_minus_sign:|
|MS SQL Server 2012<br>[System.Data.SqlClient](https://www.nuget.org/packages/System.Data.SqlClient/) 4.8.3|:heavy_check_mark:|:heavy_check_mark:|:heavy_minus_sign:|:heavy_minus_sign:|
|MS SQL Server 2012<br>[Microsoft.Data.SqlClient](https://www.nuget.org/packages/Microsoft.Data.SqlClient/) 3.0.1|:heavy_check_mark:|:heavy_check_mark:|:heavy_minus_sign:|:heavy_minus_sign:|
|MS SQL Server 2014<br>[System.Data.SqlClient](https://www.nuget.org/packages/System.Data.SqlClient/) 4.8.3|:heavy_check_mark:|:heavy_check_mark:|:heavy_minus_sign:|:heavy_minus_sign:|
|MS SQL Server 2014<br>[Microsoft.Data.SqlClient](https://www.nuget.org/packages/Microsoft.Data.SqlClient/) 3.0.1|:heavy_check_mark:|:heavy_check_mark:|:heavy_minus_sign:|:heavy_minus_sign:|
|MS SQL Server 2016<br>[System.Data.SqlClient](https://www.nuget.org/packages/System.Data.SqlClient/) 4.8.3|:heavy_check_mark:|:heavy_check_mark:|:heavy_minus_sign:|:heavy_minus_sign:|
|MS SQL Server 2016<br>[Microsoft.Data.SqlClient](https://www.nuget.org/packages/Microsoft.Data.SqlClient/) 3.0.1|:heavy_check_mark:|:heavy_check_mark:|:heavy_minus_sign:|:heavy_minus_sign:|
|MS SQL Server 2017<br>[System.Data.SqlClient](https://www.nuget.org/packages/System.Data.SqlClient/) 4.8.3<br>with FTS Tests|:heavy_check_mark:<sup>[5](#notes)</sup>|:heavy_check_mark:<sup>[5](#notes)</sup>|:heavy_check_mark:|:heavy_check_mark:|
|MS SQL Server 2017<br>[Microsoft.Data.SqlClient](https://www.nuget.org/packages/Microsoft.Data.SqlClient/) 3.0.1<br>with FTS Tests|:heavy_check_mark:<sup>[5](#notes)</sup>|:heavy_check_mark:<sup>[5](#notes)</sup>|:heavy_check_mark:|:heavy_check_mark:|
|MS SQL Server 2019<br>[System.Data.SqlClient](https://www.nuget.org/packages/System.Data.SqlClient/) 4.8.3<br>with FTS Tests|:heavy_check_mark:<sup>[5](#notes)</sup>|:heavy_check_mark:<sup>[5](#notes)</sup>|:heavy_check_mark:|:heavy_check_mark:|
|MS SQL Server 2019<br>[Microsoft.Data.SqlClient](https://www.nuget.org/packages/Microsoft.Data.SqlClient/) 3.0.1<br>with FTS Tests|:heavy_check_mark:<sup>[5](#notes)</sup>|:heavy_check_mark:<sup>[5](#notes)</sup>|:heavy_check_mark:|:heavy_check_mark:|
|Azure SQL<br>[System.Data.SqlClient](https://www.nuget.org/packages/System.Data.SqlClient/) 4.8.3|:x:|:x:|:x:|:x:|
|Azure SQL<br>[Microsoft.Data.SqlClient](https://www.nuget.org/packages/Microsoft.Data.SqlClient/) 3.0.1|:x:|:x:|:x:|:x:|
|MySQL 5.6<br>[MySql.Data](https://www.nuget.org/packages/MySql.Data/) 8.0.27|:x:|:x:|:heavy_check_mark:|:heavy_check_mark:|
|MySQL 8<br>[MySql.Data](https://www.nuget.org/packages/MySql.Data/) 8.0.27|:x:|:x:|:heavy_check_mark:|:heavy_check_mark:|
|MySQL 8<br>[MySqlConnector](https://www.nuget.org/packages/MySqlConnector/) 0.69.10/1.3.14/2.0.0|:x:|:x:|:heavy_check_mark:|:heavy_check_mark:|
|MariaDB 10<br>[MySql.Data](https://www.nuget.org/packages/MySql.Data/) 8.0.27|:heavy_check_mark:|:heavy_check_mark:|:heavy_check_mark:|:heavy_check_mark:|
|PostgreSQL 9.2<br>[Npgsql](https://www.nuget.org/packages/Npgsql/) 4.1.10 (netfx) / 5.0.11 (core2.1) / 6.0.0 (core3.1+)|:x:|:x:|:heavy_check_mark:|:heavy_check_mark:|
|PostgreSQL 9.3<br>[Npgsql](https://www.nuget.org/packages/Npgsql/) 4.1.10 (netfx) / 5.0.11 (core2.1) / 6.0.0 (core3.1+)|:x:|:x:|:heavy_check_mark:|:heavy_check_mark:|
|PostgreSQL 9.5<br>[Npgsql](https://www.nuget.org/packages/Npgsql/) 4.1.10 (netfx) / 5.0.11 (core2.1) / 6.0.0 (core3.1+)|:x:|:x:|:heavy_check_mark:|:heavy_check_mark:|
|PostgreSQL 10<br>[Npgsql](https://www.nuget.org/packages/Npgsql/) 4.1.10 (netfx) / 5.0.11 (core2.1) / 6.0.0 (core3.1+)|:heavy_check_mark:|:heavy_check_mark:|:heavy_check_mark:|:heavy_check_mark:|
|PostgreSQL 11<br>[Npgsql](https://www.nuget.org/packages/Npgsql/) 4.1.10 (netfx) / 5.0.11 (core2.1) / 6.0.0 (core3.1+)|:x:|:x:|:heavy_check_mark:|:heavy_check_mark:|
|PostgreSQL 12<br>[Npgsql](https://www.nuget.org/packages/Npgsql/) 4.1.10 (netfx) / 5.0.11 (core2.1) / 6.0.0 (core3.1+)|:x:|:x:|:heavy_check_mark:|:heavy_check_mark:|
|PostgreSQL 13<br>[Npgsql](https://www.nuget.org/packages/Npgsql/) 4.1.10 (netfx) / 5.0.11 (core2.1) / 6.0.0 (core3.1+)|:x:|:x:|:heavy_check_mark:|:heavy_check_mark:|
|DB2 LUW 11.5.0.0a<br>[IBM.Data.DB2](https://www.nuget.org/packages/IBM.Data.DB.Provider/) 11.5.5010.4 (netfx)<br>[IBM.Data.DB2.Core](https://www.nuget.org/packages/IBM.Data.DB2.Core/) 2.2.0.100 ([osx](https://www.nuget.org/packages/IBM.Data.DB2.Core-osx/) 2.0.0.100, [lin](https://www.nuget.org/packages/IBM.Data.DB2.Core-lnx/) 2.2.0.100) (core)|:x:|:x:|:heavy_check_mark:|:heavy_check_mark:|
|Informix 12.10.FC12W1DE<br>IBM.Data.Informix (SQLI) 4.0.410.10|:x:|:heavy_minus_sign:|:heavy_minus_sign:|:heavy_minus_sign:|
|Informix 14.10<br>IBM.Data.Informix (SQLI) 4.0.410.10|:x:|:heavy_minus_sign:|:heavy_minus_sign:|:heavy_minus_sign:|
|Informix 12.10.FC12W1DE<br>IBM.Data.Informix (IDS) 11.1.1010.4|:x:|:heavy_minus_sign:|:heavy_minus_sign:|:heavy_minus_sign:|
|Informix 14.10<br>IBM.Data.Informix (IDS) 11.1.1010.4|:x:|:heavy_minus_sign:|:heavy_minus_sign:|:heavy_minus_sign:|
|Informix 12.10.FC12W1DE<br>[IBM.Data.DB2](https://www.nuget.org/packages/IBM.Data.DB.Provider/) IDS 11.5.5010.4 (netfx)<br>[IBM.Data.DB2.Core](https://www.nuget.org/packages/IBM.Data.DB2.Core/) 2.2.0.100 ([osx](https://www.nuget.org/packages/IBM.Data.DB2.Core-osx/) 2.0.0.100, [lin](https://www.nuget.org/packages/IBM.Data.DB2.Core-lnx/) 2.2.0.100) (core)|:x:|:x:|:heavy_check_mark:|:heavy_check_mark:|
|Informix 14.10<br>[IBM.Data.DB2](https://www.nuget.org/packages/IBM.Data.DB.Provider/) IDS 11.5.5010.4 (netfx)<br>[IBM.Data.DB2.Core](https://www.nuget.org/packages/IBM.Data.DB2.Core/) 2.2.0.100 ([osx](https://www.nuget.org/packages/IBM.Data.DB2.Core-osx/) 2.0.0.100, [lin](https://www.nuget.org/packages/IBM.Data.DB2.Core-lnx/) 2.2.0.100) (core)|:x:|:x:|:x:|:x:|
|SAP HANA 2.0 SPS 04r45<br>Native Provider|:x:|:x:|:heavy_minus_sign:|:heavy_minus_sign:|
|SAP HANA 2.0 SPS 04r45<br>ODBC Provider|:x:|:x:|:heavy_check_mark:|:x:|
|SAP/Sybase ASE 16.2<br>[AdoNetCore.AseClient](https://www.nuget.org/packages/AdoNetCore.AseClient/) 0.19.2|:x:|:x:|:heavy_check_mark:|:heavy_check_mark:|
|SAP/Sybase ASE 16.2<br>Native Client|:x:|:x:|:x:|:x:|
|Oracle 11g XE<br>Native Client 4.122.19.1 |:x:|:heavy_minus_sign:|:heavy_minus_sign:|:heavy_minus_sign:|
|Oracle 11g XE<br>[Oracle.ManagedDataAccess](https://www.nuget.org/packages/Oracle.ManagedDataAccess/) 21.4.0 (netfx)<br>[Oracle.ManagedDataAccess.Core](https://www.nuget.org/packages/Oracle.ManagedDataAccess.Core/) 2.19.130/3.21.4 (core)|:x:|:x:|:heavy_check_mark:|:heavy_check_mark:|
|Oracle 12c<br>Native Client|:x:|:heavy_minus_sign:|:heavy_minus_sign:|:heavy_minus_sign:|
|Oracle 12c<br>[Oracle.ManagedDataAccess](https://www.nuget.org/packages/Oracle.ManagedDataAccess/) 21.4.0 (netfx)<br>[Oracle.ManagedDataAccess.Core](https://www.nuget.org/packages/Oracle.ManagedDataAccess.Core/) 2.19.130/3.21.4 (core)|:x:|:x:|:heavy_check_mark:|:heavy_check_mark:|
|Oracle 18c XE<br>Native Client|:x:|:heavy_minus_sign:|:heavy_minus_sign:|:heavy_minus_sign:|
|Oracle 18c XE<br>[Oracle.ManagedDataAccess](https://www.nuget.org/packages/Oracle.ManagedDataAccess/) 21.4.0 (netfx)<br>[Oracle.ManagedDataAccess.Core](https://www.nuget.org/packages/Oracle.ManagedDataAccess.Core/) 2.19.130/3.21.4 (core)|:x:|:x:|:x:|:x:|
|Firebird 2.1<br>[FirebirdSql.Data.FirebirdClient](https://www.nuget.org/packages/FirebirdSql.Data.FirebirdClient/) 8.5.4|:x:|:x:|:x:|:x:|
|Firebird 2.5<br>[FirebirdSql.Data.FirebirdClient](https://www.nuget.org/packages/FirebirdSql.Data.FirebirdClient/) 8.5.4|:x:|:x:|:heavy_check_mark:|:heavy_check_mark:|
|Firebird 3.0<br>[FirebirdSql.Data.FirebirdClient](https://www.nuget.org/packages/FirebirdSql.Data.FirebirdClient/) 8.5.4|:x:|:x:|:heavy_check_mark:|:heavy_check_mark:|
|Firebird 4.0 (RC1)<br>[FirebirdSql.Data.FirebirdClient](https://www.nuget.org/packages/FirebirdSql.Data.FirebirdClient/) 8.5.4|:x:|:x:|:heavy_check_mark:|:heavy_check_mark:|

###### Notes:
1. `TestNoopProvider` is a fake test provider to perform tests without database dependencies
2. `1.1.1` is the last version of `Microsoft.Data.SQLite`, that supports .NET Framework, so we use it for `net4.7.2` test configuration and recent version for `netcore`
3. needs System.Data.OleDb 4.7.1+ for .Net Core support (contains critical [issue](https://github.com/dotnet/runtime/issues/36954) that could crash application, so ODBC provider is recommended for .net core)
4. for SQL CE right now we don't run .net core tests
5. Northwind FTS SQL Server tests not enabled yet, as we need SQL Server images with full-text search included

###### Provider names in context of tests
| Name | Target Database | Extra Notes |
|:---|:---:|:---:|
|`ProviderName.Access`|Tests against Access using OLE DB JET or ACE (depends on connection string)||
|`ProviderName.AccessOdbc`|Tests against Access using ODBC MDB or MDB+ACCDB (depends on connection string)||
|`ProviderName.DB2`|tests against DB2 LUW||
|`ProviderName.DB2LUW`|not used||
|`ProviderName.DB2zOS`|not used||
|`ProviderName.Firebird`|Firebird 2.5|Should be used for latest version of FB and this one replaced with `Firebird25`|
|`TestProvName.Firebird3`|Firebird 3.0||
|`TestProvName.Firebird4`|Firebird 4.0 (RC1)||
|`ProviderName.Informix`|Informix 12 (IDS or SQLI using IBM.Data.Informix)| TODO: move to v14|
|`ProviderName.InformixDB2`|Informix 12 (IDS using IBM.Data.DB2)| TODO: move to v14|
|`TestProvName.SqlAzure`|Azure Sql||
|`ProviderName.SqlServer`|SQL Server (2008)|TODO: use it for latest|
|`ProviderName.SqlServer2000`|SQL Server 2000||
|`ProviderName.SqlServer2005`|SQL Server 2005||
|`ProviderName.SqlServer2008`|SQL Server 2008||
|`ProviderName.SqlServer2012`|SQL Server 2012||
|`ProviderName.SqlServer2014`|SQL Server 2014||
|`ProviderName.SqlServer2017`|SQL Server 2017||
|`TestProvName.Northwind`|SQL Server FTS tests||
|`ProviderName.MySql`|Latest MySQL using MySQL.Data||
|`TestProvName.MySql55`|MySQL 5.5||
|`ProviderName.MySqlOfficial`|not used||
|`ProviderName.MySqlConnector`|Latest MySQL using MySqlConnector||
|`TestProvName.MariaDB`|Latest MariaDB using MySQL.Data||
|`ProviderName.Oracle`|not used||
|`ProviderName.OracleNative`|Oracle 12c using native provider||
|`ProviderName.OracleManaged`|Oracle 12c using managed provider (core version for .net core)||
|`TestProvName.Oracle11Native`|Oracle 11g using native provider||
|`TestProvName.Oracle11Managed`|Oracle 11g using managed provider (core version for .net core)||
|`ProviderName.PostgreSQL`|Latest PostgreSQL (13)||
|`ProviderName.PostgreSQL92`|PostgreSQL 9.2-||
|`ProviderName.PostgreSQL93`|PostgreSQL [9.3-9.5)||
|`ProviderName.PostgreSQL95`|PostgreSQL 9.5+||
|`TestProvName.PostgreSQL10`|PostgreSQL 10||
|`TestProvName.PostgreSQL11`|PostgreSQL 11||
|`TestProvName.PostgreSQL12`|PostgreSQL 12||
|`TestProvName.PostgreSQL13`|PostgreSQL 13||
|`TestProvName.PostgreSQL14`|PostgreSQL 14||
|`ProviderName.SqlCe`|SQL CE||
|`ProviderName.SQLite`|not used||
|`ProviderName.SQLiteClassic`|System.Data.Sqlite||
|`ProviderName.SQLiteClassicMiniProfilerUnmapped`|System.Data.Sqlite + MiniProfiler|Tests compatibility with connection wrappers (without mappings to provider types)|
|`ProviderName.SQLiteClassicMiniProfilerMapped`|System.Data.Sqlite + MiniProfiler|Tests compatibility with connection wrappers (with mappings to provider types)|
|`TestProvName.NorthwindSQLite`|System.Data.Sqlite FTS||
|`ProviderName.SQLiteMS`|Microsoft.Data.Sqlite||
|`TestProvName.NorthwindSQLiteMS`|Microsoft.Data.Sqlite FTS||
|`ProviderName.Sybase`|Sybase ASE using official provider||
|`ProviderName.SybaseManaged`|Sybase ASE using DataAction provider||
|`ProviderName.SapHanaNative`|SAP HANA 2 using native provider||
|`ProviderName.SapHanaOdbc`|SAP HANA 2 using ODBC provider||
|`TestProvName.NoopProvider`|fake test provider to perform tests without database dependencies|
