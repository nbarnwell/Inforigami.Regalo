create database Regalo;
go

use Regalo;
go

create table AggregateRoot (
	Id uniqueidentifier not null,
	Version int not null
);

create table AggregateRootEvent (
	Id uniqueidentifier not null,
	AggregateId uniqueidentifier not null,
	Version int not null,
	Data nvarchar(max) not null
);