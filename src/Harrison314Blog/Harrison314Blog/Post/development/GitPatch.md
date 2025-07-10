Published: 10.7.2025
Title: Ako poslať komit emailom
Menu: Ako poslať komit emailom
Cathegory: Dev
OgImage: images/InterpolatedStringAsHtmlTemplate/preview.jpg
Description: Návod ako je možné poslať git komit pomocou emailu, USB kľúča alebo holuba.
---
Občas som sa dostal do situácie, že som potreboval opraviť bug, ale na počítači, kde som pracoval som nebol prihlásený na _githube_.
Našťastie to _git_ dokáže riešiť a komity je možné poslať emailom, cez watsapp alebo uložiť na USB kľuč,...

Tu je návod.

Práca sa lokálne komitne a následne sa vytvorí patch súbor:

```
git format-patch HEAD~1
```

Je takto možné vytvoriť aj viac súborov z viacerých komitov (napr. `HEAD~3`), alebo ku konkrétnemu komitu (`git format-patch <commit hash>`).

Následne sa _*.patch_ súbory prednesú na iný počítač, napríklad spomínaným emailom.

Následne sa aplikujú na cieľový repozitár pomocou:

```
git apply file1.patch file2.patch ...
```
