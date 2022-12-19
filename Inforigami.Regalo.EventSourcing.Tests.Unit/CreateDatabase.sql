if not exists (select * from sys.databases where name = 'Inforigami_Regalo_EventSourcing_Tests_Unit')
	create database Inforigami_Regalo_EventSourcing_Tests_Unit;
