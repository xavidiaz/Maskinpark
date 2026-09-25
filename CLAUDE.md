# CLAUDE.md

Maskinpark är en **inlärningsövning** baserad på Lexicon-labben *Industriell
maskinpark — Blazor Managementsystem*: en Blazor Web App som byggs upp genom 13
medvetna faser (se `README.md`). Poängen är att känna smärtan som motiverar
varje komponent och varje lager *innan* det införs. Tidig kod är medvetet naiv —
förbättra den inte i förväg, före sin fas.

## Gyllene regler

- **Du skriver ingen kod och kör inga terminalkommandon.** Jag (användaren) kodar
  och kör allt själv i terminalen. Din roll: förklara, föreslå, rita flöden,
  granska — som en handledare.
  - **Undantag:** `gh issue`-kommandon (t.ex. `gh issue create`, länka
      sub-issues till en parent) får du köra åt mig när jag ber om det. Gäller
      bara GitHub-issuehantering — inte kod, build, `dotnet`- eller
      `git`-kommandon.
  - **Undantag:** `.http`-filer (Kulala-testrequests) får du redigera direkt
      när jag ber om det — från Fas 13, när API:et finns. Det är testdata, inte
      inlärningsmålet. Gäller bara `.http`-filer — inte `.razor`, `.cs`,
      `.razor.css`, `.csproj`, migrations eller andra kodfiler.
  - **Undantag:** `README.md` får du redigera direkt när jag ber om det, men
      **bara metadata-korrigeringar** — projekt-/typnamn, stavfel, redan
      beslutade namnbyten. Gäller inte fasbeskrivningar, "vad jag lär mig"-
      tabellen eller annan lärandemåls-text — det är inlärningsmålet och ska jag
      skriva/besluta själv. Vid osäkerhet: föreslå diffen och fråga, redigera
      inte.
  - **Undantag:** Arbetsloggar under `docs/` (`logg-YYYY-MM-DD.md`) får du
      skriva och redigera direkt när jag ber om det — det är sessionshistorik,
      inte inlärningsmålet. Gäller bara filer med det namnmönstret i
      `docs/` — inte andra dokument som kan hamna där.

- **Ett steg åt gången.** Gör bara det aktuella steget. Nästa steg först när jag
  säger "nästa" eller "kör".
- **Fråga innan du går utanför steget.** Föreslå gärna, men implementera inte
  otillfrågat.
- **Verifiera efter varje steg** med `dotnet build`. När steget ändrar UI:
  kontrollera även i webbläsaren via `dotnet watch`.
- **Föreslå ett commit-meddelande efter varje steg** enligt mönstret nedan — jag
  commit:ar själv.
- **Efter varje refaktorering:** ge en kort flödesbeskrivning — var är vi, vem
  äger datan (vilken sida/service), hur flödar den (parameter nedåt, event
  uppåt, anrop till service).

## Projektstruktur

.NET 10 Blazor Web App. Interactive render mode **WebAssembly**, interactivity
location **Global**. Skapad med:

```bash
dotnet new blazor -n Maskinpark -f net10.0 --interactivity WebAssembly --all-interactive --use-program-main
```

Mallen skapar två projekt:

| Projekt | Körs | Innehåll |
|---|---|---|
| `Maskinpark` | på servern | Host: `App.razor`, `Program.cs`, statiska filer. Förrenderar sidorna. Refererar Client. |
| `Maskinpark.Client` | i webbläsaren (WASM) | Alla interaktiva komponenter: `Layout/`, `Pages/`, `Routes.razor` — och våra egna mappar |

- **Interaktiva komponenter måste ligga i `Maskinpark.Client`.** Servern
  refererar Client — aldrig tvärtom.
- Våra egna mappar i `Maskinpark.Client/`: `Models/`, `Services/`,
  `Components/` (icke-routbara komponenter). Routbara sidor ligger i mallens
  `Pages/`.
- Stäm av mot faktisk mallstruktur efter Fas 1. Om den skiljer sig: föreslå en
  justering av den här sektionen.

## Bygg & verifiera

- Bygg: `dotnet build` (från rotmappen, eller i `Maskinpark/` — servern
  refererar Client, så båda byggs)
- Kör: `dotnet watch` i serverprojektet `Maskinpark/`
- Felsökning:
  - Fel under **förrendering** → syns i terminalen
  - Fel i **webbläsaren** (WASM) → syns i devtools-konsolen (F12)
- Från Fas 13 (databas):
  - Migrations: `dotnet ef migrations add <Namn>` + `dotnet ef database update`
    (i serverprojektet)
  - `dotnet ef` är ett **globalt** verktyg (inget lokalt manifest i repot)
  - `Microsoft.EntityFrameworkCore.Design` i serverns `.csproj` krävs för att
    migrations ska fungera
  - Manuell API-test: `.http`-filer via Kulala.nvim
  - SQLite, code-first; `.db`-filen skapas lokalt och ligger inte i git

## Commit-konventioner

Mönster: `Fas <N> Steg <X>: <beskrivning>. Closes #<issue>`

- Svenska, imperativ verbform: *Skapa, Implementera, Registrera, Refaktorera,
  Flytta, Bryt ut, Ta bort, Byt namn*
- Versal begynnelsebokstav efter kolon, punkt före `Closes`
- En rad, ingen brödtext (om inte steget verkligen kräver förklaring)
- Flera issues: `Closes #41, closes #42`
- Undernummer (`Steg 2.2`) när ett steg delas upp
- En commit per steg
- Rena meddelanden — ingen `Co-Authored-By`-trailer

Exempel:

```
Fas 3 Steg 1: Skapa Machine-modell. Closes #5
Fas 5 Steg 2: Bryt ut MachineList-komponent. Closes #9
Fas 8 Steg 3: Registrera IMachineService i DI. Closes #17
```

## Kodkonventioner

C# / .NET 10, `Nullable` och `ImplicitUsings` på.

### C#

- **File-scoped namespaces** (`namespace Maskinpark.Client.Services;`)
- **Primära konstruktorer** för DI
  (`public class HttpMachineService(HttpClient http)`)
- **Expression-bodied members** för enradare
- **`Async`-suffix** på alla asynkrona metoder. Labbens metodnamn får suffix:
  `GetMachines()` → `GetMachinesAsync()`, `AddMachine()` → `AddMachineAsync()`
  osv.
- "Hittades inte" → returnera `null` (`Task<Machine?>`), inte exceptions
- **`Program.cs` har en `Main`-metod** (labbens val: *Do not use top-level
  statements*). Medveten avvikelse — ändra inte till top-level statements.
- Rot-namespaces: `Maskinpark` (server), `Maskinpark.Client` (klient)
- **Mappnamn = namespace-segment:** `Models/`, `Services/`, `Components/`,
  `Pages/`, `Layout/`
- En publik typ per fil, filnamn = typnamn. Interface prefixas `I`, ligger
  bredvid sin implementation

### Razor / Blazor

- En komponent per fil, PascalCase (`MachineList.razor`)
- `@code`-block i `.razor`-filen som default. Code-behind (`.razor.cs`) bara om
  jag väljer det
- Barnkomponenter får data via `[Parameter]` och **ändrar den aldrig själva**
- Events uppåt via `EventCallback<T>` (från Fas 6)
- `@inject` bara i sidor (`Pages/`), inte i presentationskomponenter (från Fas 8)
- Undvik manuellt `StateHasChanged()`. Blazor rerenderar själv efter UI-events.
  Behövs det ofta är dataflödet troligen fel — flagga det.
- Styling: mallens Bootstrap-klasser. CSS isolation (`.razor.css`) från Fas 9

### Lageransvar

- **Page** (`Pages/`, har `@page`) — container: hämtar data, äger state för
  vyn, hanterar events från barnkomponenter
- **Component** (`Components/`) — presentation: tar emot data via
  `[Parameter]`, skickar events via `EventCallback`, ingen dataåtkomst
- **Service** (`Services/`, från Fas 8) — affärslogik + data (in-memory, senare
  HTTP). Vet ingenting om UI.
- **Model** (`Models/`) — bara data, inga beroenden

## Kända fallgropar

Flagga dem när de dyker upp. Lös dem i rätt fas — inte tidigare.

- **Förrendering:** servern renderar sidan först, sedan tar WASM över →
  `OnInitialized(Async)` körs två gånger. GUID:er som genereras vid init
  "hoppar". Syns i Fas 4, förklaras och hanteras i Fas 8.
- **DI + förrendering:** en service som injiceras i en komponent måste finnas
  registrerad även där förrenderingen sker (servern), annars kraschar sidan.
  Fas 8.
- **In-memory i WASM:** datan bor i webbläsarflikens minne. Omladdning = allt
  nollställs. Det är motivationen för Fas 13.
- **WASM når inte databasen direkt:** EF Core och SQLite hör hemma på servern.
  Client pratar med servern via HTTP. Fas 13.

## Fasdisciplin

- **Följ faserna i ordning.** Inför inte en komponent, ett lager eller en teknik
  före sin fas — även om det "vore bättre".
  - Exempel: ingen `IMachineService` före Fas 8, inga `EventCallback` före
    Fas 6, ingen databas före Fas 13.
- **Föreslå inte förbättringar som hör till en senare fas.** Om du ser något som
  löses senare: nämn kort vilken fas, gå inte vidare.
- **Naiv kod är avsiktlig** tills den fas som fixar den. Ingen
  drive-by-refaktorering.
- Om ett steg känns fel eller ur ordning — säg till, gör det inte ändå.
- Fasplanen, labbsteg → fas-mappningen och "vad jag lär mig"-tabellen finns i
  `README.md`.

## Var ligger jag

- **GitHub Project:** Maskinpark, Project #`<N>`
  (<https://github.com/users/xavidiaz/projects/<N>>) — källan till vilket steg
  som är på tur
- **Issues:** ett issue per steg, mönster `Fas N Steg X: <vad>`. Aktuellt steg =
  lägsta öppna issue-numret i nuvarande fas
- **Krav:** labbinstruktionen (`Blazor_Maskinpark.pdf`). `README.md` mappar
  labbsteg → faser.
- **Historik:** `git log --oneline` visar avklarade steg (commit stänger sitt
  issue via `Closes #<nr>`)

Vid sessionsstart: kolla senaste commit + öppna issues för att avgöra var vi är.
Fråga om det är oklart.
