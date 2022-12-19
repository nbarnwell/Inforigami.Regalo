if not exists (select * from information_schema.tables where table_name = 'EventStream')
	create table EventStream (
		Id nvarchar(1024) not null,
		Version int not null,
		constraint PK_EventStream primary key (Id)
	);

if not exists (select * from information_schema.tables where table_name = 'EventStreamEvent')
	create table EventStreamEvent (
		EventStreamId nvarchar(1024) not null,
		Version int not null,
		Data nvarchar(max) not null,
		constraint PK_EventStreamEvent primary key (EventStreamId, Version),
		constraint FK_EventStreamEvent_EventStream foreign key (EventStreamId) references EventStream (Id)
	);