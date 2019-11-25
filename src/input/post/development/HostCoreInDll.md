Published: 9.3.2019
Title: Hostovanie .Net Core assembly v natívnej DLL/SO knižnici
Menu: CoreCrl v natívnej DLL knižnici
Cathegory: Dev
Description: Popis hostovania .Net Core aplikácie v natívej knižnici.
---
Občas sa vývojár stretne z potrebou implementovať nejaké štandardné rozhranie, plugin, ovládač...
ktoré ale musí byť natívne, aby ho mohol použiť iný program alebo operačný systém.

Ak sme nechceli alebo nemohli celú knižnicu vytvoriť v C/C++, tak sa na to dala použiť buď nejaká forma inter-process komunikácie, alebo to, že si natívna knižnica naštartuje proces (napísaný v jazyku, ktorý sa na to hodí) a volania funkcií v knižnici posiela na STDIN aplikácii a návratové hodnoty príma zo STDOUT.

Z _corecrl_ ide tento problém vyriešiť elegantnejšie a multiplatformovo.
V oficiálnej dokumentácii <https://docs.microsoft.com/en-us/dotnet/core/tutorials/netcore-hosting> je návod ako si vytvoriť vlastnú aplikáciu hostujúcu _corecrl_ a cez C/C++ kód volať statické metódy v dotnet assembly.

Pre to aby sme vytvorili natívnu dynamickú knižnicu (DLL/SO) stačí upraviť kód tak aby sa _corecrl_ inicializovalo (funkcia `initializeCoreClr`) pri načítaní knižnice a pri jej uvoľnení  vyplo (funkcia `shutdownCoreClr`).  
Pre Windows to zabezpečí funkcia `BOOL APIENTRY DllMain(HMODULE hModule, DWORD  ul_reason_for_call, LPVOID lpReserved)`.  
Pre Linux a MacOS funkcie oanotované `__attribute__((constructor))` a `__attribute__((destructor))`.  
Získajú sa ukazovatele na statické metódy z dotnet assembly, ktoré sa následne použijú v exportovaných funkciách natívnej dynamickej knižnice.

To umožňuje mať relatívne jednoduchý kód v C/C++ a zložitú aplikačnú logiku dotnete, pričom odpadajú problémy z bezpečnosťou ako pri nejakej forme inter-process komunikácie alebo právami na spúšťanie procesov.

Nasledujúci sekvenčný diagram zobrazuje použitie .NET Core host v natívnej dynamickej knižnici.

![Sekvenčný diagram použitia natívnej knižnice](images/CoreCrlHost/SequenceDiagram.svg){.img-center}

## Zdroje
1. [Write a custom .NET Core host to control the .NET runtime from your native code](https://docs.microsoft.com/en-us/dotnet/core/tutorials/netcore-hosting)
1. [Ukážkové zdrojové kódy](https://github.com/dotnet/samples/tree/master/core/hosting)
1. [Embedding CoreCLR in your C/C++ application](https://yizhang82.dev/hosting-coreclr)