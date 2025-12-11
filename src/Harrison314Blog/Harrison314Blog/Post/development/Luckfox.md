Published: 11.12.2025
Title: Pravdepodobne najlacnejší počítač, na ktorom beží .NET 10 
Menu: Najlacnejší počítač, na ktorom beží .NET 10 
Cathegory: Dev
Description: Najlacnejší počítač, na ktorom beží .NET 10 a návod, ako na ňom rozbehať .NET aplikácie.
OgImage: images/Luckfox/qh33s95la06g1.jpeg
---

Nedávno som narazil na jednodoskový počítač _Luckfox Pico Ultra WV1106_, ktorý stojí asi 25€, je to síce viac ako _Raspberry Pi Zero 2 W_, ale k nemu treba dokúpiť SD kartu, ktorá stojí rovnako ako Raspberry-čko a má nejaké nevýhody. 

Luckfox Pico Ultra okrem klasického vybavenia (GPIO, SPI, I2C, UART, WIFI/Bluetooth, USB-C, USB-A, Etherenet port) obsahuje 8GB EMMC úložisko,
vďaka čomu sa nemusíme naň báť zapisovať. 

 
Rozbehanie dotnetu na tejto doske je komplikovanejšie ako na Raspberry-čku. Treba postupovať podľa návodu na oficálnej stránke <https://wiki.luckfox.com/Luckfox-Pico-Ultra/Flash-image> ale ako image si zvoliť komunitou podporované _Ubuntu_ (nie _Buildroot_). Následne sfunkčniť sieťovú konektivitu a spraviť aktualizáciu systému `apt-get update & apt.get upgrade -y` (podľa oficáilneho návodu). 

Potom už ide nasadiť .NET 10 aplikáciu pomocou `dotnet publish -c Release -r linux-arm --self-contained` a všetko funguje.

![Luckfox Pico Ultra WV1106](images/Luckfox/qh33s95la06g1.jpeg)

**Poznámky**:
- _Buildroot_ je malý linux, ktorý ale nemá aplikácie.
- .NET 10 pôjde pravdepodobne skompilovať pomocou NatoveAOT pre _Buildroot_ ale treba riešiť cross-kompiláciu.
- _Buildroot_ používa _uClibc_ namiesto štandardnej _glibc_.
- Luckfox Pico Ultra používa pre ovládanie GPIO rovnaký prístup ako _Raspberry Pi_ <https://wiki.luckfox.com/Luckfox-Pico-Ultra/GPIO>.
- Pre začiatočníkov odporúčam siahnuť po _Raspberry Pi Zero 2 W_, pretože je preň viac návodov.

