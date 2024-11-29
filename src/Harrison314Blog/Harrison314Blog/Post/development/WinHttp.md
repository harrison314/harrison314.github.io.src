Published: 6.8.2022
Title: Volanie HTTP requestu zo 16kB binárky
Menu: Použitie WinHTTP
Cathegory: Dev
Description: Jednoduchý príklad ako použiť WinHTTP na volanie HTTP endpointu s minimálnou binárkou.
---
Občas sa stretnem s požiadavkou zavolať REST endpoint s natívneho programu. Mnoho ľudí hneď siahne po cURL knižnici, Rust-e, alebo Go, no výsledná binárka má najmenej _2-4MB_. S WinHTTP je možné sa zmestiť do _16kB_ programu (samozrejme funguje len na Windowsoch).

Prinášam malú ukážku použitia WinHTTP.

```c
#include <stdio.h>
#include <stdlib.h>
#include <windows.h>
#include <winhttp.h>

int main()
{
    DWORD dwSize = 0;
    DWORD dwDownloaded = 0;
    LPSTR pszOutBuffer;
    BOOL  bResults = FALSE;
    HINTERNET  hSession = NULL, hConnect = NULL, hRequest = NULL;

    hSession = WinHttpOpen(L"WinHTTP Example/1.0", WINHTTP_ACCESS_TYPE_DEFAULT_PROXY, WINHTTP_NO_PROXY_NAME, WINHTTP_NO_PROXY_BYPASS, 0);

    if (hSession)
    {
        hConnect = WinHttpConnect(hSession, L"harrison314.github.io", INTERNET_DEFAULT_HTTPS_PORT, 0);
    }

    if (hConnect)
    {
        hRequest = WinHttpOpenRequest(hConnect, L"GET", L"/dev_endpoints/simple.json", NULL, WINHTTP_NO_REFERER, WINHTTP_DEFAULT_ACCEPT_TYPES, WINHTTP_FLAG_SECURE);
    }

    if (hRequest)
    {
        bResults = WinHttpSendRequest(hRequest, WINHTTP_NO_ADDITIONAL_HEADERS, 0, WINHTTP_NO_REQUEST_DATA, 0, 0, 0);
    }

    if (bResults)
    {
        bResults = WinHttpReceiveResponse(hRequest, NULL);
    }

    if (bResults)
    {
        do
        {
            dwSize = 0;
            if (!WinHttpQueryDataAvailable(hRequest, &dwSize))
            {
                printf("Error %u in WinHttpQueryDataAvailable.\n", GetLastError());
            }

            pszOutBuffer = (char*) malloc(sizeof(char)*(dwSize + 1));
            if (!pszOutBuffer)
            {
                printf("Out of memory\n");
                dwSize = 0;
            }
            else
            {
                ZeroMemory(pszOutBuffer, dwSize + 1);
                if (!WinHttpReadData(hRequest, (LPVOID)pszOutBuffer, dwSize, &dwDownloaded))
                {
                    printf("Error %u in WinHttpReadData.\n", GetLastError());
                }
                else
                {
                    printf("%s", pszOutBuffer);
                }

                if (pszOutBuffer)
                {
                    free((void*)pszOutBuffer);
                }
            }
        }
        while (dwSize > 0);
    }

    if (!bResults)
    {
        printf("Error %d has occurred.\n", GetLastError());
    }

    if (hRequest) WinHttpCloseHandle(hRequest);
    if (hConnect) WinHttpCloseHandle(hConnect);
    if (hSession) WinHttpCloseHandle(hSession);

    return 0;
}
```

Kompilácia: `gcc -s main.c -lwinhttp -o main.exe`.

Samozrejme ide len o inšpiráciu, ale nemalo by sa zabúdať na to, že _WinAPI_ je plné zaujímavých funkcií, ktoré sú nám k dispozícii a dokážu zaujímavé veci a to aj z hadiska bezpečnosti.
