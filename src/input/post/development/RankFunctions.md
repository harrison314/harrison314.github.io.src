Published: 26.10.2022
Title: MS SQL a RANK funkcie
Menu: MS SQL a RANK funkcie
Cathegory: Dev
Description: Ukážka RANK a Windows funkcii v MS SQL (SQL Serveru).
---
V SQL rank funkcie umožňujú učiť poradie v dopyte podľa nejakého kritéria.
Táto funkcia je veľmi užitočná pri istom type dopytov. V tomto blogu píšem aj o iných maličkostiach, uľahčujúcich život.
No nejde o žiadne novinky, plno týchto funkcionalít je dostupný od _SQL Serveru 2008 R2_.

## Príkladový problém

Keď som ešte pracoval s PHP a MySQL na sociálnej sieti pre lukostrelcov, tak som dostal úlohu,
zobraziť v detaile luku (produktu) najlepšie výsledky, ktoré tým lukom boli dosiahnuté v jednotlivých kategóriách súťaží,
kde boli použité.

Šlo to spraviť selfjoinom a vnoreným dopytom, no to MySQL zabilo, doslova, pri tabuľke s dvoma tisíckami záznamov vyletel procesor na 100% a po troch minútach MySQL zamrzla a bolo potrebné ju celú reštartovať.
Riešením bolo nakoniec použiť _N+1 dopytov_ – najskôr sa vytiahol zoznam kategórií a potom sa pre každú vytiahli najlepšie výsledky so súťažou a súťažiacim.

Presne na takýto scenár ide požiť funkcia [RANK](https://learn.microsoft.com/en-us/sql/t-sql/functions/rank-transact-sql?view=sql-server-ver16) ([DENSE_RANK](https://learn.microsoft.com/en-us/sql/t-sql/functions/dense-rank-transact-sql?view=sql-server-ver16)) v MS SQL.

Nasledujúci kód vytvorí testovaciu databázu, nad ktorou ukážem riešenie podobného problému ako vyššie a tiež použitie window funkcií.

```sql
CREATE TABLE [dbo].[Competition](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](500) NOT NULL,
 CONSTRAINT [PK_Competition] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE TABLE [dbo].[Competition_CompetitionCathegory](
	[CompetitionId] [int] NOT NULL,
	[CompetitionCathegoryId] [int] NOT NULL,
 CONSTRAINT [PK_Competition_CompetitionCathegory] PRIMARY KEY CLUSTERED 
(
	[CompetitionId] ASC,
	[CompetitionCathegoryId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE TABLE [dbo].[CompetitionCategory](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_CompetitionCategory] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE TABLE [dbo].[CompetitionResult](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [int] NOT NULL,
	[CompetitionId] [int] NOT NULL,
	[CathegoryId] [int] NOT NULL,
	[Points] [int] NULL,
	[Notes] [nvarchar](500) NULL,
 CONSTRAINT [PK_CompetitionResult] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE TABLE [dbo].[User](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[Gender] [bit] NULL,
 CONSTRAINT [PK_User] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Competition_CompetitionCathegory]  WITH CHECK ADD  CONSTRAINT [FK_Competition_CompetitionCathegory_Competition] FOREIGN KEY([CompetitionId])
REFERENCES [dbo].[Competition] ([Id])
GO
ALTER TABLE [dbo].[Competition_CompetitionCathegory] CHECK CONSTRAINT [FK_Competition_CompetitionCathegory_Competition]
GO
ALTER TABLE [dbo].[Competition_CompetitionCathegory]  WITH CHECK ADD  CONSTRAINT [FK_Competition_CompetitionCathegory_CompetitionCategory] FOREIGN KEY([CompetitionCathegoryId])
REFERENCES [dbo].[CompetitionCategory] ([Id])
GO
ALTER TABLE [dbo].[Competition_CompetitionCathegory] CHECK CONSTRAINT [FK_Competition_CompetitionCathegory_CompetitionCategory]
GO
ALTER TABLE [dbo].[CompetitionResult]  WITH CHECK ADD  CONSTRAINT [FK_CompetitionResult_Competition] FOREIGN KEY([CompetitionId])
REFERENCES [dbo].[Competition] ([Id])
GO
ALTER TABLE [dbo].[CompetitionResult] CHECK CONSTRAINT [FK_CompetitionResult_Competition]
GO
ALTER TABLE [dbo].[CompetitionResult]  WITH CHECK ADD  CONSTRAINT [FK_CompetitionResult_CompetitionCategory] FOREIGN KEY([CathegoryId])
REFERENCES [dbo].[CompetitionCategory] ([Id])
GO
ALTER TABLE [dbo].[CompetitionResult] CHECK CONSTRAINT [FK_CompetitionResult_CompetitionCategory]
GO
ALTER TABLE [dbo].[CompetitionResult]  WITH CHECK ADD  CONSTRAINT [FK_CompetitionResult_User] FOREIGN KEY([UserId])
REFERENCES [dbo].[User] ([Id])
GO
ALTER TABLE [dbo].[CompetitionResult] CHECK CONSTRAINT [FK_CompetitionResult_User]
GO

SET IDENTITY_INSERT [dbo].[CompetitionCategory] ON
GO
INSERT [dbo].[CompetitionCategory] ([Id], [Name]) VALUES (1, N'Bowhunter Freestyle Limited'), (2, N'Barebow'), (3, N'Freestyle Limited'),
(4, N'Traditional'), (5, N'NFAA'), (6, N'Indoor')
GO
SET IDENTITY_INSERT [dbo].[CompetitionCategory] OFF
GO

SET IDENTITY_INSERT [dbo].[Competition] ON 
GO
INSERT [dbo].[Competition] ([Id], [Name]) VALUES (1, N'Minnehaha Archers Summer Field League - Week 5'),
(2, N'Face2Face'),
(3, N'Minnehaha Archers Summer Field League - Week 7')
GO
SET IDENTITY_INSERT [dbo].[Competition] OFF
GO
INSERT [dbo].[Competition_CompetitionCathegory] ([CompetitionId], [CompetitionCathegoryId]) VALUES (1, 1),(1, 2),(1, 3),(2, 2),(2, 3),(2, 4),(3, 5),(3, 6)
GO
SET IDENTITY_INSERT [dbo].[User] ON 
GO
INSERT [dbo].[User] ([Id], [Name], [Gender]) VALUES (1, N'Lena', 0),
(2, N'Jano', 1),(3, N'Dano', 1),(4, N'Marianna', 0),
(5, N'Greta', 0),(6, N'Ada', 0),
(7, N'Mirka', 0),(8, N'Peter', 1),
(9, N'Martin', 1),(10, N'Martin G.', 1),
(11, N'Maria', 0),(12, N'Silvester', 1)
GO
SET IDENTITY_INSERT [dbo].[User] OFF
GO
SET IDENTITY_INSERT [dbo].[CompetitionResult] ON
INSERT [dbo].[CompetitionResult] ([Id], [UserId], [CompetitionId], [CathegoryId], [Points], [Notes]) VALUES (1, 1, 1, 1, 256, NULL),
(2, 2, 1, 1, 120, NULL),(3, 3, 1, 1, 1500, NULL),(4, 4, 1, 2, 41, NULL),
(5, 2, 1, 2, 24, NULL),(6, 5, 1, 2, 24, NULL),(7, 1, 1, 2, 0, NULL),
(8, 6, 1, 2, 15, NULL),(9, 2, 1, 3, 120, NULL),(10, 3, 1, 3, 110, NULL),
(11, 6, 1, 4, 130, NULL),(12, 7, 2, 4, 80, NULL),(13, 8, 2, 4, 114, NULL),
(14, 7, 2, 2, 85, NULL),(15, 9, 2, 2, 91, NULL),(16, 10, 2, 2, 42, NULL),
(17, 12, 2, 2, 91, NULL),(18, 6, 2, 3, 174, NULL),(19, 7, 2, 3, 200, NULL),
(20, 8, 2, 3, 195, NULL),(21, 9, 2, 3, 112, NULL),(22, 10, 2, 3, 45, NULL),
(23, 11, 2, 3, 138, NULL),(24, 12, 2, 3, 0, NULL),(25, 7, 3, 5, 47, NULL),
(26, 8, 3, 5, 39, NULL),
(27, 1, 3, 6, 266, NULL),(28, 2, 3, 6, 200, NULL),(29, 3, 3, 6, 1101, NULL)
GO
SET IDENTITY_INSERT [dbo].[CompetitionResult] OFF
GO
```

Pre vytiahnutntie súťažiacich, ktorý dosiahli najviac bodov v danej kategórii  ide použiť nasledujúce SQL.

Je v ňom využité _CTE_ a funkcia [DENSE_RANK](https://learn.microsoft.com/en-us/sql/t-sql/functions/dense-rank-transact-sql?view=sql-server-ver16), ktorá očísluje riadky podľa počtu bodov pre konkrétnu kategóriu. Následne je vonkajšou podmienkou zabezpečené aby sa vybrali len najlepšie výsledky.

Toto SQL je jednoduché upraviť tak, aby vracalo _N_ najlepších výsledkov pomocou úpravy _WHERE_ klauzuly.

```sql
WITH RankedPoints ([UserName], [Competition], [Cathegory], [Points], [Rank])
AS
(
 SELECT [u].[Name] AS [UserName],
        [c].[Name] AS [Competition],
        [cc].[Name] AS [Cathegory],
        [cr].[Points] AS [Points],
	    DENSE_RANK() OVER (PARTITION BY [cr].[CathegoryId] ORDER BY [cr].[Points] DESC) AS [Rank]
  FROM [dbo].[CompetitionResult] [cr]
  LEFT JOIN [dbo].[User] [u] ON [cr].[UserId] = [u].[Id]
  LEFT JOIN [dbo].[Competition] [c] ON [cr].[CompetitionId] = [c].[Id]
  LEFT JOIN [dbo].[CompetitionCategory] [cc] ON [cr].[CathegoryId] = [cc].[Id]
)
SELECT [UserName], [Competition], [Cathegory], [Points] FROM RankedPoints
WHERE [Rank] = 1
```

## Iné triky
Window funkcie umožňujú „naporcovať“ skúmané dáta podobne ako pri grupovaní, ale týmto spôsobom je možné vytiahnuť aj iné údaje v riadkoch a „porcovať“ ich podľa rôznych kritérií.

Nasledujúci selekt vráti medián bodov a počet súťažiacich pre kategórie.
Window funkcie sa zvyčajne používajú s `DISTINCT` kvôli opakujúcim sa riadkom vo výsledkoch.

```sql
SELECT DISTINCT [cc].[Name] AS [Cathegory],
		PERCENTILE_CONT(0.5) WITHIN GROUP (ORDER BY  [Points]) OVER (PARTITION BY [cr].[CathegoryId]) AS [PointsMedian],
		COUNT([cr].[UserId]) OVER (PARTITION BY [cr].[CathegoryId]) AS [Count]
  FROM [dbo].[CompetitionResult] [cr]
  LEFT JOIN [dbo].[User] [u] ON [cr].[UserId] = [u].[Id]
  LEFT JOIN [dbo].[CompetitionCategory] [cc] ON [cr].[CathegoryId] = [cc].[Id]
```

Do agregačných funkcií idú vpísať podmienky, to sa dá využiť pri rôznom spočítavaní. Napríklad nasledujúci dopyt vráti počet súťažiacich mužov a žien v kategóriách:

```sql
SELECT [cc].[Name] AS [Cathegory],
	   COUNT(*) AS [Total],
	   SUM(IIF([u].[Gender] = 1, 1, 0)) AS [Men],
	   SUM(IIF([u].[Gender] = 0, 1, 0)) AS [Women]
  FROM [dbo].[CompetitionResult] [cr]
  LEFT JOIN [dbo].[User] [u] ON [cr].[UserId] = [u].[Id]
  LEFT JOIN [dbo].[CompetitionCategory] [cc] ON [cr].[CathegoryId] = [cc].[Id]
  GROUP BY [cr].[CathegoryId], [cc].[Name]
```

## Záver
Tento blog nemal za cieľ  byť úplným a a vyčerpávajúcim, skôr šlo ukázať niektoré málo známe možnosti.
