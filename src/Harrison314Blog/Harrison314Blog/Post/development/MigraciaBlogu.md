Published: 28.11.2024
Title: Migrácia blogu na AspNetStatic
Menu: Migrácia blogu
Cathegory: Dev
Description: Zmena notebooku ma donútila migrovať blog z Wyam na AspNetStatic.
---

Zmena notebooku ma donútila migrovať blog.

Tento blog bol pôvodne generovaný pomocou [Wyam-u](https://github.com/Wyamio/Wyam),
ale jeho posledný release bol v roku 2020.
Dlho som čakal na [Statiq.Framework](https://github.com/statiqdev/Statiq.Framework), je to duchovný nástupca _Wyam-u_, ale už roky čakám na verziu _1.0.0_.

Nakoniec ma až zmena notebooku donútila zmilovať si svoj blog na niečo iné, lebo Wyam moduly som mal rozbehané pod _.Net Core 2.1_
a ten som už na nový notebook nenainštaloval.

Nový blog som vytváral pomocou knižnice [AspNetStatic](https://github.com/ZarehD/AspNetStatic),
tá dokáže bežnú webovú aplikáciu postavenú na ASP.NET Core zmeniť na statický web.
Ja som si zvolil Blazor SSR, lebo mi sedí komponentový vývoj. 
Migrácia šla pomerne ľahko, najredšou prekážkou bolo rozhodnutie môjho minulého ja štruktúrovať blog ako html súbory nakopené vedľa seba.
Takže som musel čarovať s URL-kami.
