Published: 2.5.2016
Title: PerDia2012
Menu: PerDia2012
Description: Webová aplikácia ako osobný denník.
---
# PerDia2012

PerDia2012 je jednoduchá webová aplikácia, ktorá ma slúžiť 
ako denník a organizátor pre poznámky a pripomienky s možnosťou fulltextového vyhľadávania,
anotácií záznamov v denníku, súkromnou časovou osou, možnosťou exportu do "offline" html balíčka a iných. 

Ide o single page aplikáciu postavenú na OWIN-e, Web Api,
SimpleInjector, MS SQL. Na klientskej strane TypeScript 
prekladaný do ES5, knockout.js, Director.js, Promise.es6 a Bootstrap.

Šlo mi o to postaviť veľmi ľahkú a rýchlu aplikáciu,
pomocou technológií používaných v SPA aplikáciách,
a triku používanému v [CQRS](http://www.augi.cz/programovani/architektura-skalovatelnych-aplikaci/) architektúre – tam,
kde by dopyt do databázy používal _JOIN_ sa nahradí databázovým
pohľadom, použitie interného event publisher-a 
pre asynchrónnu aktualizáciu nepriamo súvisiacich dát 
(napríklad tabuliek pre používaných pre fulltext vyhľadávanie). 

## Použité technológie

C# 4.6, Owin, ASP.NET Web Api, SimpleInjector, Data reader a MS SQL 2016 (stored procedúry, Fulltext index, CTE, pôvodne aj SQL Service Brooker), TypeScript, knockout.js, Director.js a Bootstrap.

Pre unit testovanie: knižnice Sould, a Moq.
