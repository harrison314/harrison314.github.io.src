Published: 25.12.2013
Title: Katalóg produktov v relačnej databáze
Menu: Katalóg produktov v relačnej DB
Cathegory: Dev
---
# Katalóg produktov v relačnej databáze
Tvorba katalógov produktov a služieb je veľmi časté, či už na webe alebo v podnikovej sfére. Požiadavkou na takýto systém je aby umožňoval ukladať tovar s rôznymi atribútmi ako rozmery, cena, pri niektorých typoch produktov farba, pri iných výkon. Predstavte si e-shop s elektronikou, pri reproduktoroch je uvedený výkon, rozmery, dostupná farba, no pri stolných počítačoch je tam procesor, veľkosť pamäte, zas pri monitore uhlopriečka...

V tomto článku chcem v krátkosti predstaviť možné riešenia.

## Naivné riešenie
Pre každý typ tovaru sa vytvorí osobitná tabuľka. Zvrátenosť tohto riešenia sa ukáže pri 18-tich druhoch tovaru a vo väzbách na 18 ďalších tabuliek (žiaľ reálna skúsenosť). Toto riešenie je absolútne nevhodné z hľadiska normalizácie a neschopnosti meniť typy produktov počas „behu“.

## Použitie ORM
Bavíme sa o použití ORM podporujúceho dedenie a polymorfizmus entít (napr. *Entity Framework* alebo *Hibernate* ), žiadne trápne *active record*. Tovar je reprezentovaný ako abstraktná entita, samotné druhy tovaru sú vytvorené dedením. Toto riešenie je pekné v tom, že ORM a polymorfizmus spraví za nás všetku špinavú prácu, no stále nie je možné po nasadení aplikácie meniť typ tovaru.

## Použitie NoSQL databázy
Bezschémové databázy (napr. *MongoDB*, [katalóg v MongoDB](http://docs.mongodb.org/ecosystem/use-cases/product-catalog/) ) umožňujú vkladať a vyťahovať tovar do a z databázy s rôznymi atribútmi. Dokonca je možné danú schému meniť po nasadení aplikácie, pridávať nové druhy tovaru.  Toto je jeden z  argumentov priaznivcov NoSQL databáz. Jednoducho navrhnete „schému“ tak ako vyzerajú najčastejšie „dopyty“.  Praktickej ukážke sa môžem  povenovať niekedy nabudúce, no v tomto článku chcem ukázať ako daný problém riešiť pomocou relačnej databázy.

## Riešenie z múdrej knihy (EAV)
Návrhové vzory sú všeobecné riešenia problémov v danom kontexte.  Niečo podobné jestvuje aj pre návrh databázových schém a volá sa to analytické vzory.  V múdrej knihe (1) som narazil na vzor *Predmet zmluvy*, ktorý spĺňa všetko to čo potrebujeme a vzory pre cenníky produktov.

Výsledok je takáto databázová schéma:

![katalog v1.jpg](images/CatalogProduktov/Eav.png){.img-center}

Ide naozaj o abstraktný návrh, ktorý treba trochu ohnúť pre každé konkrétne použitie.

_Katalog_ – táto entita predstavuje konkrétny katalóg, pravdepodobne bude mať svoje meno ( **„Katalóg jeseň-zima 2013“** ) a svoju časovú pôsobnosť. S produktom má reláciu N ku N, pretože predpokladám, že ten istý produkt sa môže vyskytovať v rôznych katalógoch.

_Produkt_ – obsahuje len meno produktu. Je otázne, či má obsahovať aj atribúty, ktoré sú pre všetky produkty spoločné (napríklad cena).

_TypProduktu_ – reprezentuje typy produktov. Produktu určuje typ a tým aj to aké má vlastnosti. (Typ môže byť: PC, televízor, mixér,....).

_TypVlastnostiProduktu_ – entita určuje typ vlastnosti produktu, môže ísť o cenu, farbu tovaru, hmotnosť... Má reláciu k *TypProduktu* typu N ku N, pretože rôzny typy produktov môžu byť opísané rovnakými vlastnosťami. Pravdepodobne každý typ produktu bude mať cenu, pri chladničkách a televízoroch to môže byť hmotnosť.

_TypHodnoty_ – reprezentuje akého typu je vlastnosť produktu. Napríklad ak ide o hmotnosť televízora, tak typ je číselný atribút, a uvádza sa v kilogramoch. Spolu s týmito informáciami je možné do tejto entity ukladať aj ohraničenia pre hodnoty vlastností produktov ako napr. číselný rozsah. Za istých okolností je vhodnejšie entitu *TypHodnoty* a *TypVlastnosťiPruktu* zlúčiť, no to záleží už od konkrétneho návrhu.

Entity *TypProduktu*, *TypVlastnostiProduktu* a *TypHodnoty*, tvoria metaúroveň produktov. Pomocou nej je možné automaticky generovať formuláre pre zadávanie nových produktov. Typ produktu je možné považovať súčasne aj za kategóriu produktu a vytvoriť z neho menu, alebo pridaním rekurzívnej relácie stromovú štruktúru typov/kategórií produktov.

_VlastnostProduktu_ – táto entita obsahuje už hodnoty vlastností produktu.

__Poznámka:__  V praxi sa používa aj riešenie, obsahujúce tabuľky profilov produktov a jednu veľkú tabuľku hodnôt, ktorá pre každý dátový typ (int, double, krátky text, dlhý text) _N_ (napr. 10) stĺpcov. V profile typu sa uchováva ktoré stĺpce reprezentujú akú položku produktu. Bližšie o tomto spôsobe dopíšem neskôr. 

## Záver
Posledné uvedené riešenie umožňuje do relačnej databázy uložiť takmer čokoľvek, čo sa aspoň trochu podobá na zadanie v úvode.  No je potrebné viac premyslieť selkty, tiež odporúčam nejaké to cachovanie.

## Zdroje
1. Šešera Ľubor RNDr. PhD., Aplikačné architektúry softvérových systémov, ISBN 978-227-3652-7
1. <http://docs.mongodb.org/ecosystem/use-cases/product-catalog/>