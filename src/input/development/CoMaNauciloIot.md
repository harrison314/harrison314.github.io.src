Published: 13.7.2020
Updated: 14.7.2020
Title: Čo ma naučilo IoT
Menu: Čo ma naučilo IoT
Cathegory: Dev
Description: Blog o tom, čo som sa naučil pri hraní s IoT.
OgImage: images/CoMaNauciloIot/esp8266.jpg
---

Medzi moje koníčky patrí aj IoT. Predovštekým som sa pomocou neho chcel zlepšiť v C/C++ a vyskúšať si iné programovanie,
ako bežne robím v práci. Je iné mať k dispozícii 8GB a iné 80KB RAM.

Pôvodne som rozmýšľal o tom zadovážiť si _Raspberry Pi_ a vyrobiť si meteostanicu,
ale odradili ma veci ako absencia obvod reálneho času, komplikovanejšie pripojenie snímačov,
SD karta (radi odchádzajú kvôli zápisom Linuxu).
Ale náhodou som narazil na vývojovú sadu _ESP-201_ osadenú modulom _[ESP8266](https://en.wikipedia.org/wiki/ESP8266)_.

S _ESP8266_ sa začínalo ľahko, najskôr v _Arduino IDE_ (rozhodne to nie je IDE),
neskôr vo Visual Studiu s pluginom pre vývoj jedno-čipových dosiek.
To mi umožňovalo použiť C++11 a tým sa starosti zo správou pamäte stali minulosťou ([uniq_ptr](https://en.cppreference.com/w/cpp/memory/unique_ptr)).

![ESP2866](images/CoMaNauciloIot/esp8266.jpg){.img-center}

Nakoniec sa mi podarilo meteostanicu vytvoriť. _ESP8266_ zbieralo dáta každých 5 minút a raz za pol hodinu sa pripojilo na WiFi,
pomocou protokolu [MQTT](https://en.wikipedia.org/wiki/MQTT) odoslalo údaje na _Azure IoT Hub_,
kde ich jedna _Azure Function_ uložila do _Blob Storage_. Ďalšia Azure funkcia mi fungovala ako minimalistická stránka s REST API.

O asi dva roky sa mi dostala do rúk vývojová doska [Avnet MT3620 Starter Kit](https://www.avnet.com/shop/us/products/avnet-engineering-services/aes-ms-mt3620-sk-g-3074457345636825680/) s [Azure Sphere](https://azure.microsoft.com/en-us/services/azure-sphere/).
_Azure Sphere_ je systém na čipe vyvinutý Microsoftom pre moderné IoT aplikácie a snaží sa pomôcť riešiť ich najpálčivejšie problémy.
Po technickej stránke obsahuje jadro _ARM Cortex-A7_ a dve jadrá _Cortex-M4_. No hlavný rozdiel s pohľadu vývoja je, že _Azure Sphere_ je pripravený pre priemyselné použitie.

![Avnet MT3620 Starter Kit](images/CoMaNauciloIot/azure-sphere.jpg){.img-center}

Na _Azure Sphere_ je možné požiť dva procesory, jeden klasický, na ktorom beží malý Linux a druhý pre realtime proces (ak chceme využiť realtime proccesor, tak sa nasadzujúd dva programy - jeden bežiaci pod OS na jednom procesore a druhý pre realtime procesor, tieto programi môžu kominukovať pomocou zasielania správ).
Samozrejmosťou je aj vyriešená podpora aktualizácií, certificate pinning,... No najviac ma zaujalo, že s aplikáciou sa nasazduje manifest, ktorý funguje ako firewall – definujú sa na ňom, na aké domény môže doska pristupovať, ktoré hardvérové piny idú na čo použiť,...
To celé posúva bezpečnosť tejto dosky niekde úplne inde, ako je v IoT zvykom.

![Azure Sphere](images/CoMaNauciloIot/graphic-mcu.png){.img-center}  
(Zdroj: <https://docs.microsoft.com/en-us/azure-sphere/product-overview/what-is-azure-sphere>)

## Čo ma naučilo IoT?

#### Iné programovanie
Programovaním IoT zariadení sa človek dostáva do úplne iného sveta ako pri programovaní backendu.
Zrazu musí šetriť pamäť a počíta každý bajt a to nie len počas behu programu v RAM, ale aj pri prenose (_SigFox_, _Lora_).
Niekedy treba šetriť aj batériu.

Zaujímavou skúsenosťou je programovanie na niečo, čo nemá operačný systém. Ďalšími aspektami sú hardvérové prerušenia,
globálne premenné vynútené API, obtiažne logovanie,...

#### Neveriť knižniciam
Počas experimentovania s metostanicou som potreboval pre _ESP8266_ rôzne knižnice pre protokoly,
senzory a mnoho z nich malo chyby a plno z nich chýbali akékoľvek náznaky zabezpečenia.

Keďže to „I“ v IoT je internet, tak bezpečnosť by mala hrať adekvátnu rolu.

V jednom prípade šlo o _MQTT_ knižnicu, ktorá trpela _buffer owerflow_ zraniteľnosťou.
(Ak niekoho napadne, že v jeho obľúbenom jazyku by sa to nestalo, tak treba vedieť, že pri malej RAM by sa z _buffer owerflow_ stala _DoS_ zraniteľnosť.)

#### Arduino je na vzdelávanie
_Arduino_ a _Raspberry Pi_ boli vymyslené na vzdelávanie a hranie sa, nie je dobre ich používať na seriózne veci.
Na sériovú výrobu sa nehodia, _Razzbery Pi_ používa SD-kartu, ktorá rada odchádza,...

Tieto „hračky“ sa naozaj hodia na vzdelávanie a prototypovanie, no pre priemyselné použitie a domácu automatizáciu odporúčam použiť veci na to určené (kludne aj _Azure Sphere_, ten bol konštruovaný pre priemysel a používa ho napríklad [Starbucks](https://news.microsoft.com/transform/starbucks-turns-to-technology-to-brew-up-a-more-personal-connection-with-its-customers/)).

## Záver
Vďaka IoT som si vyskúšal iný pohľad na programovanie. Najmä človeka teší, že to čo vytvoril má nejakú fyzickú podobu, reaguje to v skutočnom svete a dá sa to chytiť do ruky.
