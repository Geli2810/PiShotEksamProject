create table Games
(
GameId int identity (1,1) PRIMARY KEY,
Profile1 INT Not NULL,
Profile2 INT Not NULL,
GameWinner INT Not NULL,

Foreign KEY (Profile1) REFERENCES Profiles(Id),
Foreign KEY (Profile2) REFERENCES Profiles(Id),
)