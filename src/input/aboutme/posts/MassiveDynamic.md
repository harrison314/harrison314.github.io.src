Published: 14.8.2016
Title: MassiveDynamic Proxy Generator
Menu: MassiveDynamic Proxy Generator
Description: Ľahká knižnica pre generovanie proxy objektov a dekorátorov.
---
_MassiveDynamicProxyGenerator_ je miniatúrna a ľahká knižnica pre dynamické generovanie proxy objektov a dekorátorov.

Generovanie proxy objektov ide využiť napríklad na vytváranie NULL implementácii rozhraní (vzor _Null object_), zakrytie prístupu ku vzdialenej službe (rovnako to robí WCF-ko) napríklad cez JSON-RPC, alebo REST-om. Alebo pre inú automatickú implementáciu rozhraní (logovanie volania, správanie určené mennou konvenciou metód,....).

Dynamické dekorátory sa dajú výhodne použiť ako interceptory, oddelenie infraštruktúrneho kódu od doménového (verifikácia prístupových práv, logovanie, sledovanie perfomace),...

Knižnica umožňuje vytvárať:

* Dynamické proxy objekty s interceptorom,
* Inštancie dekorátorov s interceptorom,
* Dynamické proxy objekty pre riadenie vytvárania inštancií reálneho objektu.

Vytvorený [Nuget balíček](https://www.nuget.org/packages/MassiveDynamicProxyGenerator/),
ktorý je určený pre .Net 4.0, 4.5, 4.6, 4.6.1, .NetStandart 1.6, 2.0 a .NetStandart 1.4 (UWP aplikácie).

Odkaz na zdrojové kódy spolu s ukážkami použitia [MassiveDynamicProxyGenerator](https://github.com/harrison314/MassiveDynamicProxyGenerator).

## MassiveDynamic Proxy Generator intergrácia pre SimpleInjector

K MassiveDynamicProxygenerator-u som vytvoril integračnú knižnicu pre [SimpleInjector](https://simpleinjector.org/index.html), ktorá umožňuje do IoC kontainera registrovať:

* mock (null implementácia služby),
* proxy implementácie rozhraní,
* dekorátorov pre služby v IoC konatienry,
* inštančných provaiderov.

Táto knižnica má aj „nebezpečné“ registrácie, ktoré nezodpovedajú best practise razených vývojármi SimpleInjector-u, nimi sú:

* namokovanie všetkých neregistrovaných služieb v IoC kontajnery,
* použitie vlastných inštančných providerov.

Vytvorený [Nuget balíček](https://www.nuget.org/packages/MassiveDynamicProxyGenerator.SimpleInjector/),
ktorý je určený pre .Net 4.0, 4.5, 4.6, 4.6.1, .NetStandart 1.6, 2.0 a .NetStandart 1.4 (UWP aplikácie).

Ukážky použitia a príklady so zdrojovými kódmi sa nachádzajú na [Github stránke projektu](https://github.com/harrison314/MassiveDynamicProxyGenerator#massivedynamicproxygeneratorsimpleinjector).

## MassiveDynamic Proxy Generator intergrácia pre Microsoft.Extensions.DependencyInjection
K MassiveDynamicProxygenerator-u som vytvoril integračnú knižnicu pre [Microsoft.Extensions.DependencyInjectio](https://www.nuget.org/packages/Microsoft.Extensions.DependencyInjection/),
je určená pre použitie v štandardných ASP.NET Core 2.0 projektoch.
Umožňuje do IoC kontainera pridať:

* triedu v roli dekorátora (samo o sebe to nesúvisí s MassiveDynamicProxygenerator),
* proxy implementácie rozhraní (pomocou interceptoru),
* dekorátory pre služby v IoC kontainery (pomocou interceptoru),
* inštančných provaiderov.

Táto knižnica podporuje zatiaľ iba _.NetStandard 2.0_. K tomu použitie Microsoft.Extensions.DependencyInjection prináša svoje obmedzenia,
napríklad pri použití „otvorenej generiky“ (angl: open generic) - v dekorátoroch ju nie možné použiť, vždy treba dekorovať konkretne typy alebo konkrétne generické typy.

Vytvorený [Nuget balíček](https://www.nuget.org/packages/MassiveDynamicProxyGenerator.Microsoft.DependencyInjection/),
ktorý je určený pre .NetStandart 2.0.

Ukážky použitia a príklady so zdrojovými kódmi sa nachádzajú na [Github stránke projektu](https://github.com/harrison314/MassiveDynamicProxyGenerator#massivedynamicproxygeneratormicrosoftdependencyinjection).
