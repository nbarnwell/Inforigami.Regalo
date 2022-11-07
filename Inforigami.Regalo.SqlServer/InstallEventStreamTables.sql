if not exists (select * from information_schema.tables where table_name = 'AggregateRoot')
	create table AggregateRoot (
		Id uniqueidentifier not null,
		Version int not null
	);

if not exists (select * from information_schema.tables where table_name = 'AggregateRootEvent')
	create table AggregateRootEvent (
		Id uniqueidentifier not null,
		AggregateId uniqueidentifier not null,
		Version int not null,
		Data nvarchar(max) not null
	);